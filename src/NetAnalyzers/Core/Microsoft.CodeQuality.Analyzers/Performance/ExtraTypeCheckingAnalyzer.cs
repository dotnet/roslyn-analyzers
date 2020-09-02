// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using Analyzer.Utilities;
    using Analyzer.Utilities.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using Microsoft.CodeAnalysis.Operations;

    /// <summary>
    /// The analyzer that generates the diagnostics for dictionary access pattern issues.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ExtraTypeCheckingAnalyzer : DiagnosticAnalyzer
    {
        private const string IsExpressionCausesDuplicateTypeCheckingFlowAnalysis = "CAxxxx";
        private const string DontCastMultipleTimesFlowAnalysis = "CAyyyy";

        /// <summary>
        /// Gets the descriptor pattern to create the TypeChecking diagnostic.
        /// </summary>
        private static readonly CodingPattern TypeCheckingPattern = new CodingPattern(
            descriptorId: IsExpressionCausesDuplicateTypeCheckingFlowAnalysis,
            descriptorTitle: "Avoid repeated type checking.",
            descriptorMessageFormat: "Avoid using the 'is' operator as it usually is followed by a casting operation that causes expensive type inspection to be done twice.\r\n\r\nCasting is usually a sign of a class (object oriented) design issue. Code outside the class should not depend on it's implementation. It may be possible to avoid casting completely by moving the code to the desired class behind a common interface.\r\n\r\nIf you must cast, save the result of the 'as' operator once in a temp variable and check for null.",
            improvementDescription: CodingPattern.CpuAndMemoryImprovement);

        /// <summary>
        /// Gets the descriptor pattern to create the DoubleCasting diagnostic.
        /// </summary>
        private static readonly CodingPattern DoubleCastingPattern = new CodingPattern(
            descriptorId: DontCastMultipleTimesFlowAnalysis,
            descriptorTitle: "Do not cast multiple times.",
            descriptorMessageFormat: "If you are using casting or the 'as' operator, save the result in a temp variable.\r\n\r\nCasting is usually a sign of a class (object oriented) design issue. Code outside the class should not depend on it's implementation. It may be possible to avoid casting completely by moving the code to the desired class behind a common interface.\r\n\r\nIf you must cast, save the result of the 'as' operator once in a temp variable, check for null, and reuse the temp variable. You may have to move the cast outside the current code block so it can be reused.",
            improvementDescription: CodingPattern.CpuAndMemoryImprovement);

        /// <summary>
        /// The diagnostic descriptor factory.
        /// </summary>
        private readonly IDiagnosticDescriptorFactory diagnosticDescriptorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalyzer" /> class.
        /// </summary>
        /// <param name="descriptorFactory">Factory to create the diagnostic descriptors.</param>
        public ExtraTypeCheckingAnalyzer(
            IDiagnosticDescriptorFactory descriptorFactory)
        {
            this.diagnosticDescriptorFactory = descriptorFactory ?? throw new ArgumentNullException(nameof(descriptorFactory));

            this.TypeCheckingRule = descriptorFactory.CreateDiagnosticDescriptor(
                TypeCheckingPattern,
                DiagnosticSeverity.Warning);

            this.DoubleCastingRule = descriptorFactory.CreateDiagnosticDescriptor(
                DoubleCastingPattern,
                DiagnosticSeverity.Warning);

            this.SupportedDiagnostics = ImmutableArray<DiagnosticDescriptor>.Empty.AddRange(
                new DiagnosticDescriptor[]
                {
                    this.TypeCheckingRule,
                    this.DoubleCastingRule,
                });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalyzer"/> class.
        /// </summary>
        /// <remarks>This constructor needed for unit testing.</remarks>
        public ExtraTypeCheckingAnalyzer()
            : this(new TextDiagnosticDescriptorFactory())
        {
        }

        /// <summary>
        /// Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        /// <summary>
        /// Gets or sets a delegate to customize properties on each diagnostic.
        /// </summary>
        public Func<SyntaxNode, SemanticModel, Dictionary<string, string>>? GetCustomPropertiesForDiagnostic { get; set; }

        /// <summary>
        /// Gets the diagnostic rule.
        /// </summary>
        internal DiagnosticDescriptor TypeCheckingRule { get; }

        /// <summary>
        /// Gets the diagnostic rule.
        /// </summary>
        internal DiagnosticDescriptor DoubleCastingRule { get; }

        /// <summary>
        /// Initializes the analyzer.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(this.OnStartCompilation);
        }

        /// <summary>
        /// Gets the list of previous locations as a list.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <returns>The list of previous locations.</returns>
        private static List<SimpleAbstractValue> GetPreviousLocationsAsList(SimpleAbstractValue inputValue)
        {
            List<SimpleAbstractValue> locations = new List<SimpleAbstractValue>(inputValue.GetPreviousLocationCount());
            HashSet<SimpleAbstractValue> uniqueLocations = new HashSet<SimpleAbstractValue>();

            GetNestedPreviousLocationsAsList(inputValue, locations, uniqueLocations);

            // Reverse the list so it is from the top of the file down.
            locations.Reverse();

            return locations;
        }

        /// <summary>
        /// Flattens the list of previous locations into a list.
        /// </summary>
        /// <param name="inputValue">The input location value.</param>
        /// <param name="locations">The list of locations.</param>
        /// <param name="uniqueLocations">The list of unique locations visited.</param>
        private static void GetNestedPreviousLocationsAsList(
            SimpleAbstractValue inputValue,
            List<SimpleAbstractValue> locations,
            HashSet<SimpleAbstractValue> uniqueLocations)
        {
            // Keep track of nodes we haven't visited yet.
            List<SimpleAbstractValue> seenFirstTime = new List<SimpleAbstractValue>(inputValue.PreviousLocations.Count);

            // Flatten the list of locations into a list.
            foreach (SimpleAbstractValue location in inputValue.PreviousLocations)
            {
                if (!uniqueLocations.Contains(location))
                {
                    uniqueLocations.Add(location);
                    locations.Add(location);
                    seenFirstTime.Add(location);
                }
            }

            // Visit sub nodes we haven't seen.
            foreach (SimpleAbstractValue location in seenFirstTime)
            {
                GetNestedPreviousLocationsAsList(location, locations, uniqueLocations);
            }
        }

        /// <summary>
        /// Executed when compilation starts.
        /// </summary>
        /// <param name="compilationContext">The compilation context.</param>
        private void OnStartCompilation(CompilationStartAnalysisContext compilationContext)
        {
            compilationContext.RegisterOperationBlockAction(this.OnOperationBlock);
        }

        /// <summary>
        /// Called when visiting each operation block.
        /// </summary>
        /// <param name="operationBlockContext">The operation context.</param>
        private void OnOperationBlock(OperationBlockAnalysisContext operationBlockContext)
        {
            if (operationBlockContext.OwningSymbol is not IMethodSymbol containingMethod)
            {
                // Not interested in proceeding further.
                return;
            }

            foreach (IOperation operationRoot in operationBlockContext.OperationBlocks)
            {
                IBlockOperation? topmostBlock = operationRoot.GetTopmostParentBlock();
                if (topmostBlock != null)
                {
                    ControlFlowGraph? cfg = topmostBlock.GetEnclosingControlFlowGraph();

                    if (cfg == null)
                    {
                        continue;
                    }

                    WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(operationBlockContext.Compilation);
                    CancellationToken cancellationToken = CancellationToken.None;

                    ExtraTypeCheckingAnalysisResult? analysisResult = ExtraTypeCheckingAnalysis.GetOrComputeResult(
                        cfg,
                        containingMethod,
                        wellKnownTypeProvider,
                        PointsToAnalysisKind.Complete,
                        operationBlockContext.Options,
                        this.TypeCheckingRule,
                        operationBlockContext.Compilation,
                        cancellationToken,
                        interproceduralAnalysisKind: InterproceduralAnalysisKind.None);

                    if (analysisResult == null)
                    {
                        continue;
                    }

                    // Tracks the nodes already reported to prevent duplicates diagnostics at the same location,
                    // and keeps track of which diagnostic the node will be reported by.
                    Dictionary<SyntaxNode, SimpleAbstractValue> reportedNodes =
                        new Dictionary<SyntaxNode, SimpleAbstractValue>(analysisResult.ExitBlockOutput.Data.Count);

                    foreach (KeyValuePair<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> kvp in analysisResult.ExitBlockOutput.Data)
                    {
                        ExtraTypeCheckingAbstractLocation location = kvp.Key;
                        SimpleAbstractValue accessValue = kvp.Value;

                        if (kvp.Key.AccessLocation == Location.None ||
                            accessValue.Kind != SimpleAbstractValueKind.Access ||
                            accessValue.AccessLocation == null ||
                            accessValue.PreviousLocations.Count == 0)
                        {
                            // Clean out house keeping entries, and the first access.
                            continue;
                        }

                        SyntaxNode node = accessValue.AccessLocation.DiagnosticLocation;
                        if (!reportedNodes.TryGetValue(node, out SimpleAbstractValue tempValue) ||
                            accessValue.GetPreviousLocationCount() > tempValue.GetPreviousLocationCount())
                        {
                            reportedNodes[node] = accessValue;
                        }
                    }

                    // Repeated operations have the original operation they are repeating
                    // in the first entry of the list of previous locations.
                    // Each diagnostic location will have different lists of previous nodes
                    // based on control flow. For example:
                    //
                    // object sb = new StringBuilder();
                    // object encoding = System.Text.Encoding.UTF8;
                    //
                    // int length = ((StringBuilder)sb).Length;           // C0
                    // StringBuilder tempBuilder = sb as StringBuilder;   // C1
                    //
                    // if (Environment.TickCount > 0)
                    // {
                    //     ((StringBuilder)sb).Append(""Howdy"");         // C2
                    // }
                    // else
                    // {
                    //     ((StringBuilder)sb).Append(""Hi"");            // C3
                    // }
                    //
                    // this.StringBuildingFunction((StringBuilder)sb);    // C4
                    //
                    // C0 is the original cast against which C1..C4 are considered duplicate and will generate diagnostics.
                    // There is no diagnostic at C0. The values at C1..C4 keep track of the previous casts found
                    // in the control flow. i.e.
                    // C2 and C3 will have C0..C1 in their previous location list, but not each other because of the control flow.
                    // C4 will have C2 or C3 and C1..C0 in its previous location list.
                    //
                    // The goal is to ensure that diagnostics for C1..C4 all have the others as additional locations,
                    // with C0 as the first additional location in the diagnostic list.
                    //
                    // The fix provider depends on C0 being in the first position in the AdditionalLocations list.
                    //
                    // Having all the locations in all the diagnostics means that "Fix All Occurances" actually does
                    // work even if you choose that option on C4.
                    Dictionary<Location, List<Location>> locationToAdditionalLocations = new Dictionary<Location, List<Location>>();
                    LocationComparer locationComparer = new LocationComparer();
                    foreach (KeyValuePair<SyntaxNode, SimpleAbstractValue> kvp in reportedNodes)
                    {
                        List<SimpleAbstractValue> previousLocations = GetPreviousLocationsAsList(kvp.Value);
                        Location? originalLocation = previousLocations[0].AccessLocation?.DiagnosticLocation.GetLocation();
                        Location? currentLocation = kvp.Value.AccessLocation?.DiagnosticLocation.GetLocation();

                        if (originalLocation == null ||
                            currentLocation == null)
                        {
                            continue;
                        }

                        if (!locationToAdditionalLocations.TryGetValue(originalLocation, out List<Location> locations))
                        {
                            locations = new List<Location>();
                            locationToAdditionalLocations.Add(originalLocation, locations);
                        }

                        HashSet<Location> uniqueLocations = new HashSet<Location>(locations);

                        // If we are on the last location in the file, it will not appear in the previous locations list.
                        if (uniqueLocations.Add(currentLocation))
                        {
                            locations.Add(currentLocation);
                        }

                        for (int index = 1; index < previousLocations.Count; index++)
                        {
                            Location? indexLocation = previousLocations[index].AccessLocation?.DiagnosticLocation.GetLocation();
                            if (indexLocation != null && uniqueLocations.Add(indexLocation))
                            {
                                locations.Add(indexLocation);
                            }
                        }
                    }

                    // Now create one diagnostic for each node.
                    // Note: This algorithm doesn't quite work if casting spans function calls.
                    foreach (KeyValuePair<SyntaxNode, SimpleAbstractValue> kvp in reportedNodes)
                    {
                        List<SimpleAbstractValue> previousLocations = GetPreviousLocationsAsList(kvp.Value);
                        if (previousLocations.Count < 1)
                        {
                            continue;
                        }

                        Location? originalLocation = previousLocations[0].AccessLocation?.DiagnosticLocation.GetLocation();
                        if (originalLocation == null)
                        {
                            continue;
                        }

                        if (!locationToAdditionalLocations.TryGetValue(originalLocation, out List<Location> locations))
                        {
                            // This should not happen, we just built this dictionary above.
                            continue;
                        }

                        // The additional locations is the original location in the first position
                        // followed by all additional locations not including the current one.
                        List<Location> additionalLocations = new List<Location>(locations.Count)
                        {
                            originalLocation
                        };

                        Location? currentLocation = kvp.Value.AccessLocation?.DiagnosticLocation.GetLocation();
                        if (currentLocation != null)
                        {
                            locations.Sort(locationComparer);
                            foreach (Location location in locations)
                            {
                                if (!location.Equals(currentLocation))
                                {
                                    additionalLocations.Add(location);
                                }
                            }
                        }

                        // Diagnostic is placed at the current location.
                        SyntaxNode? node = kvp.Value.AccessLocation?.DiagnosticLocation;
                        if (node != null)
                        {
                            Dictionary<string, string>? properties =
                                this.GetCustomPropertiesForDiagnostic?.Invoke(node, topmostBlock.SemanticModel);

                            DiagnosticDescriptor rule = this.TypeCheckingRule;

                            if (kvp.Value.AccessLocation?.Properties != null &&
                                kvp.Value.AccessLocation.Properties.TryGetValue("IsTypeChecking", out object o) &&
                                o is string isTypeChecking &&
                                isTypeChecking.Equals("False", StringComparison.OrdinalIgnoreCase))
                            {
                                rule = this.DoubleCastingRule;
                            }

                            ReportDiagnostic(operationBlockContext, node, rule, properties, additionalLocations);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reports diagnostic if this method if performance sensitive.
        /// </summary>
        /// <param name="context">the context.</param>
        /// <param name="expression">the expression.</param>
        /// <param name="descriptor">the descriptor.</param>
        /// <param name="properties">Properties.</param>
        /// <param name="additionalLocations">Additional locations to mention in the diagnostic.</param>
        private static void ReportDiagnostic(
            OperationBlockAnalysisContext context,
            SyntaxNode expression,
            DiagnosticDescriptor descriptor,
            Dictionary<string, string>? properties,
            List<Location> additionalLocations)
        {
            Location location = expression.GetLocation();

            ImmutableDictionary<string, string> immutableProperties = properties != null ? properties.ToImmutableDictionary() : ImmutableDictionary<string, string>.Empty;

            Diagnostic diagnostic = Diagnostic.Create(descriptor, location, additionalLocations, immutableProperties);
            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Because the Equals operator is overloaded, we need to write a custom comparer to use for finding
        /// unique values.
        /// </summary>
        private class ReferenceEqualityComparer : IEqualityComparer<SimpleAbstractValue>
        {
            /// <inheritdoc />
            public bool Equals(SimpleAbstractValue x, SimpleAbstractValue y)
            {
                return object.ReferenceEquals(x, y);
            }

            /// <inheritdoc />
            public int GetHashCode(SimpleAbstractValue obj)
            {
                // We want the collision so the reference equal check takes place.
                return 1;
            }
        }

        /// <summary>
        /// Sorts locations from the beginning to the end of the file.
        /// </summary>
        private class LocationComparer : IComparer<Location>
        {
            /// <summary>
            /// Compares to locations to determine which comes first in the file.
            /// </summary>
            /// <param name="x">The first location.</param>
            /// <param name="y">The second location.</param>
            /// <returns>1, 0, -1 based on the sort order.</returns>
            public int Compare(Location x, Location y)
            {
                return x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
            }
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidDuplicateElementInitialization : DiagnosticAnalyzer
    {
        // TODO: Determine which id to use.
        internal const string RuleId = "CA9999";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidDuplicateElementInitializationTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidDuplicateElementInitializationMessage), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.AvoidDuplicateElementInitializationDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(AnalyzeOperation, OperationKind.ObjectCreation);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            var objectInitializer = (IObjectCreationOperation)context.Operation;
            if (objectInitializer.Initializer == null)
            {
                return;
            }

            var initializedElementIndexes = new HashSet<object[]>(ConstantArgumentEqualityComparer.Instance);

            foreach (var initializer in objectInitializer.Initializer.Initializers)
            {
                if (initializer is ISimpleAssignmentOperation assignment &&
                    assignment.Target is IPropertyReferenceOperation propertyReference &&
                    propertyReference.Arguments.Length != 0)
                {
                    var values = GetConstantArgumentValues(propertyReference.Arguments);
                    if (values != null && !initializedElementIndexes.Add(values))
                    {
                        var indexesText = string.Join(", ", values.Select(value => value?.ToString() ?? "null"));
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, propertyReference.Syntax.GetLocation(), indexesText));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the argument values in parameter order, filling in defaults if necessary, if all
        /// arguments are constants. Otherwise, returns null.
        /// </summary>
        private static object[] GetConstantArgumentValues(ImmutableArray<IArgumentOperation> arguments)
        {
            var result = new object[arguments.Length];
            foreach(var argument in arguments)
            {
                var parameter = argument.Parameter;
                if (parameter == null ||
                    parameter.Ordinal >= result.Length ||
                    !argument.Value.ConstantValue.HasValue)
                {
                    return null;
                }

                result[parameter.Ordinal] = argument.Value.ConstantValue.Value;
            }
            return result;
        }

        private sealed class ConstantArgumentEqualityComparer : IEqualityComparer<object[]>
        {
            public static readonly ConstantArgumentEqualityComparer Instance = new ConstantArgumentEqualityComparer();

            private readonly EqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

            private ConstantArgumentEqualityComparer() { }

            bool IEqualityComparer<object[]>.Equals(object[] x, object[] y)
            {
                if (x == y)
                {
                    return true;
                }

                return x != null && y != null && x.SequenceEqual(y, _objectComparer);
            }

            int IEqualityComparer<object[]>.GetHashCode(object[] obj)
            {
                int hash = 0;
                foreach (var item in obj)
                {
                    hash = unchecked((hash * (int)0xA5555529) + _objectComparer.GetHashCode(item));
                }
                return hash;
            }
        }
    }
}
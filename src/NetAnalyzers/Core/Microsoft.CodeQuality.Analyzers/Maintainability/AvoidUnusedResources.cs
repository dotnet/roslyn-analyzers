// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidUnusedResources : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1516";
        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(AvoidUnusedResourcesTitle)),
            CreateLocalizableResourceString(nameof(AvoidUnusedResourceMessage)),
            DiagnosticCategory.Maintainability,
            RuleLevel.Disabled,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false,
            isReportedAtCompilationEnd: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationStartAction(context =>
            {
                var resourceFileNames = context.Options.AdditionalFiles
                    .Where(f => Path.GetExtension(f.Path).Equals(".resx", StringComparison.Ordinal)).Select(f => Path.GetFileNameWithoutExtension(f.Path)).ToImmutableHashSet();

                var resourceTypes = context.Compilation.GetSymbolsWithName(n => resourceFileNames.Contains(n), SymbolFilter.Type, context.CancellationToken)
                    .Where(s => s.ContainingType is null).OfType<INamedTypeSymbol>();

                var propertyToIsUsedMap = new ConcurrentDictionary<IPropertySymbol, bool>(SymbolEqualityComparer.Default);
                foreach (var resourceType in resourceTypes)
                {
                    foreach (var property in resourceType.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (property.IsStatic && property.Type.SpecialType == SpecialType.System_String)
                        {
                            propertyToIsUsedMap.TryAdd(property, false);
                        }
                    }
                }

                context.RegisterOperationAction(context =>
                {
                    var operation = (IPropertyReferenceOperation)context.Operation;
                    if (propertyToIsUsedMap.ContainsKey(operation.Property))
                    {
                        propertyToIsUsedMap.TryUpdate(operation.Property, newValue: true, comparisonValue: false);
                    }
                }, OperationKind.PropertyReference);

                context.RegisterCompilationEndAction(context =>
                {
                    foreach (var keyValuePair in propertyToIsUsedMap)
                    {
                        if (!keyValuePair.Value)
                        {
                            context.ReportDiagnostic(keyValuePair.Key.CreateDiagnostic(Rule, keyValuePair.Key.Name));
                        }
                    }
                });
            });
        }
    }
}

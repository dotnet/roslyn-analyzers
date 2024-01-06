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
            RuleLevel.IdeSuggestion,
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

                var resourceTypes = new ConcurrentBag<INamedTypeSymbol>();
                var usedProperties = new ConcurrentBag<IPropertySymbol>();

                context.RegisterSymbolAction(context =>
                {
                    if (IsResourceType(context.Symbol, resourceFileNames))
                    {
                        resourceTypes.Add((INamedTypeSymbol)context.Symbol);
                    }
                }, SymbolKind.NamedType);

                context.RegisterOperationAction(context =>
                {
                    var operation = (IPropertyReferenceOperation)context.Operation;
                    var property = operation.Property;
                    if (IsResourceType(property.ContainingType, resourceFileNames) && IsCandidateProperty(property))
                    {
                        usedProperties.Add(property);
                    }
                }, OperationKind.PropertyReference);

                context.RegisterCompilationEndAction(context =>
                {
                    foreach (var resourceType in resourceTypes)
                    {
                        foreach (var member in resourceType.GetMembers())
                        {
                            if (member is not IPropertySymbol property || !IsCandidateProperty(property))
                            {
                                continue;
                            }

                            if (!usedProperties.Contains(property))
                            {
                                context.ReportDiagnostic(property.CreateDiagnostic(Rule, property.Name));
                            }
                        }
                    }
                });
            });
        }

        private static bool IsResourceType(ISymbol type, ImmutableHashSet<string> resourceFileNames)
            => resourceFileNames.Contains(type.Name) && type.ContainingType is null;

        private static bool IsCandidateProperty(IPropertySymbol property)
            => property.IsStatic && property.Type.SpecialType == SpecialType.System_String;
    }
}

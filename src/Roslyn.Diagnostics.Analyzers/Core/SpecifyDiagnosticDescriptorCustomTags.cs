// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyDiagnosticDescriptorCustomTags : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.SpecifyDiagnosticDescriptorCustomTagsTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.SpecifyDiagnosticDescriptorCustomTagsMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RoslynDiagnosticIds.SpecifyDiagnosticDescriptorCustomTagsRuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.RoslynDiagnosticsUsage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                if (compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(typeof(DiagnosticDescriptor).FullName, out var diagnosticDescriptorType))
                {
                    compilationContext.RegisterOperationAction(opContext =>
                    {
                        var objectCreation = (IObjectCreationOperation)opContext.Operation;
                        if (objectCreation.Type.Equals(diagnosticDescriptorType) && IsMissingCustomTagsArgument(objectCreation.Arguments))
                        {
                            opContext.ReportDiagnostic(objectCreation.CreateDiagnostic(Rule));
                        }
                    }, OperationKind.ObjectCreation);
                }
            });
        }

        private static bool IsMissingCustomTagsArgument(ImmutableArray<IArgumentOperation> arguments) =>
            arguments.FirstOrDefault(x => x.Parameter?.Name == "customTags")?.Value is IArrayCreationOperation arrayCreation &&
            arrayCreation.DimensionSizes.Length == 1 &&
            arrayCreation.DimensionSizes[0].ConstantValue.HasValue &&
            arrayCreation.DimensionSizes[0].ConstantValue.Value is int size &&
            size == 0;
    }
}

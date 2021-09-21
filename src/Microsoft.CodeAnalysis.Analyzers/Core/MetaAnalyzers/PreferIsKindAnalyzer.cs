﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    using static CodeAnalysisDiagnosticsResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferIsKindAnalyzer : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor Rule = new(
            DiagnosticIds.PreferIsKindRuleId,
            CreateLocalizableResourceString(nameof(PreferIsKindTitle)),
            CreateLocalizableResourceString(nameof(PreferIsKindMessage)),
            DiagnosticCategory.MicrosoftCodeAnalysisPerformance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(PreferIsKindDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                // Kind() methods
                //  Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind
                //  Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions.Kind
                //
                // IsKind() methods
                //  Microsoft.CodeAnalysis.CSharpExtensions.IsKind
                //  Microsoft.CodeAnalysis.VisualBasicExtensions.IsKind

                Dictionary<INamedTypeSymbol, INamedTypeSymbol> containingTypeMap = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>();
                if (context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisCSharpCSharpExtensions) is { } csharpKindExtensions
                    && context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisCSharpExtensions) is { } csharpIsKindExtensions)
                {
                    containingTypeMap[csharpKindExtensions] = csharpIsKindExtensions;
                }

                if (context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisVisualBasicVisualBasicExtensions) is { } vbKindExtensions
                    && context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisVisualBasicExtensions) is { } vbIsKindExtensions)
                {
                    containingTypeMap[vbKindExtensions] = vbIsKindExtensions;
                }

                if (containingTypeMap.Count > 0)
                {
                    context.RegisterOperationAction(context => HandleBinaryOperation(context, containingTypeMap), OperationKind.Binary);
                }
            });
        }

        private static void HandleBinaryOperation(OperationAnalysisContext context, Dictionary<INamedTypeSymbol, INamedTypeSymbol> containingTypeMap)
        {
            var operation = (IBinaryOperation)context.Operation;
            if (operation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            {
                return;
            }

            var possibleInvocation = operation.LeftOperand.WalkDownConversion();
            if (possibleInvocation is IConditionalAccessOperation conditionalAccess)
            {
                // We don't currently report on Nullable<SyntaxToken>. If we'll report that in the future, the codefix must behave correctly.
                if (conditionalAccess.Operation.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    return;
                }

                possibleInvocation = conditionalAccess.WhenNotNull;
            }

            if (possibleInvocation is IInvocationOperation { TargetMethod: { Name: "Kind", ContainingType: var containingType } }
                && containingTypeMap.TryGetValue(containingType, out _))
            {
                context.ReportDiagnostic(operation.LeftOperand.CreateDiagnostic(Rule));
            }
        }
    }
}

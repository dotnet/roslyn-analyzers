// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    /// <summary>
    /// RS1034: Prefer token.IsKind(SyntaxKind.None) or token.RawKind == 0 over token == default.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed class DoNotCompareSyntaxTokenAgainstDefaultLiteralAnalyzer : DiagnosticAnalyzer
    {
        private readonly string s_syntaxTokenTypeFullName = typeof(SyntaxToken).FullName;

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotCompareSyntaxTokenAgainstDefaultLiteralTitle), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotCompareSyntaxTokenAgainstDefaultLiteralMessage), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.DoNotCompareSyntaxTokenAgainstDefaultLiteralDescription), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));

        private static readonly DiagnosticDescriptor s_rule = new(
            DiagnosticIds.DoNotCompareSyntaxTokenAgainstDefaultLiteralRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.MicrosoftCodeAnalysisPerformance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterOperationAction(
                (OperationAnalysisContext context) =>
                {
                    var operation = (IBinaryOperation)context.Operation;

                    if (operation.Syntax.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        return;
                    }

                    if (operation.OperatorMethod.Name is not (WellKnownMemberNames.EqualityOperatorName or WellKnownMemberNames.InequalityOperatorName))
                    {
                        return;
                    }

                    var syntaxKindType = context.Compilation.GetOrCreateTypeByMetadataName(s_syntaxTokenTypeFullName);
                    if (!SymbolEqualityComparer.Default.Equals(operation.LeftOperand.Type, syntaxKindType))
                    {
                        return;
                    }

                    if (operation.LeftOperand.Kind == OperationKind.DefaultValue ||
                        operation.RightOperand.Kind == OperationKind.DefaultValue)
                    {
                        context.ReportDiagnostic(operation.CreateDiagnostic(s_rule));
                    }
                }, OperationKind.Binary);
        }
    }
}

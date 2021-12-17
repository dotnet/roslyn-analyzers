// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA2259: Use null-suppression properly.
    /// </summary>
    public abstract class UseNullSuppressionCorrectly<TSyntaxKind> : DiagnosticAnalyzer where TSyntaxKind : struct
    {
        internal const string RuleId = "CA2259";

        private static readonly LocalizableString s_localizableTitle = CreateLocalizableResourceString(nameof(UseNullSuppressionCorrectlyTitle));
        private static readonly LocalizableString s_localizableDescriptionLiteralAlwaysNull = CreateLocalizableResourceString(nameof(UseNullSuppressionCorrectlyLiteralAlwaysNullDescription));
        private static readonly LocalizableString s_localizableDescriptionLiteralNeverNull = CreateLocalizableResourceString(nameof(UseNullSuppressionCorrectlyLiteralNeverNullDescription));
        private static readonly LocalizableString s_localizableMessage = CreateLocalizableResourceString(nameof(UseNullSuppressionCorrectlyMessage));

        internal static readonly DiagnosticDescriptor LiteralAlwaysNullRule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarningCandidate,
            s_localizableDescriptionLiteralAlwaysNull,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor LiteralNeverNullRule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarningCandidate,
            s_localizableDescriptionLiteralNeverNull,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(LiteralAlwaysNullRule, LiteralNeverNullRule);

        protected abstract ImmutableArray<TSyntaxKind> SyntaxKinds { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeNullSuppressedLiterals, SyntaxKinds);
        }

        protected abstract void AnalyzeNullSuppressedLiterals(SyntaxNodeAnalysisContext context);
    }
}
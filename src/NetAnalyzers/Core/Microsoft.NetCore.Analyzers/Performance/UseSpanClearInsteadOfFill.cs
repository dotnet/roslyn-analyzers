// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// CA1849: Use Span.Clear instead of Span.Fill(default)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseSpanClearInsteadOfFill : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA1849";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor s_Rule = DiagnosticDescriptorHelper.Create(
            DiagnosticId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s_Rule);

        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}

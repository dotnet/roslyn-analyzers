// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    //  Visual Basic doesn't allow ref-struct APIs
#pragma warning disable RS1004 // Recommend adding language support to diagnostic analyzer
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004 // Recommend adding language support to diagnostic analyzer
    public sealed class PreferReadOnlySpanPropertiesOverReadOnlyArrayFields: DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1850";

        private static readonly LocalizableString s_localizableTitle = Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Title));
        private static readonly LocalizableString s_localizableMessage = Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Message));
        private static readonly LocalizableString s_localizableDescription = Resx.CreateLocalizableResourceString(nameof(Resx.PreferReadOnlySpanPropertiesOverReadOnlyArrayFields_Description));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
        }
    }
}

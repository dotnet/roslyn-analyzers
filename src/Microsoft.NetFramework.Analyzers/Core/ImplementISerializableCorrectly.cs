// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetFramework.Analyzers
{
    /// <summary>
    /// CA2240: Implement ISerializable correctly
    /// </summary>
    public abstract class ImplementISerializableCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2240";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementISerializableCorrectlyTitle), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementISerializableCorrectlyMessageDefault), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeVisible = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementISerializableCorrectlyMessageMakeVisible), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeOverridable = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementISerializableCorrectlyMessageMakeOverridable), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ImplementISerializableCorrectlyDescription), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2240-implement-iserializable-correctly",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MakeVisibleRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeVisible,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2240-implement-iserializable-correctly",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MakeOverridableRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeOverridable,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2240-implement-iserializable-correctly",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;
            //DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX ? ImmutableArray.Create(DefaultRule, MakeVisibleRule, MakeOverridableRule) : ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Enable concurrent execution of analyzer actions.
            //analysisContext.EnableConcurrentExecution();

            // TODO: Configure generated code analysis.
            //analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}
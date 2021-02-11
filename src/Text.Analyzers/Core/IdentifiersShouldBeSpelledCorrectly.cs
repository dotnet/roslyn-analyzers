// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Text.Analyzers
{
    /// <summary>
    /// CA1704: Identifiers should be spelled correctly
    /// </summary>
    public abstract class IdentifiersShouldBeSpelledCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1704";

        /*private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyTitle), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageAssembly = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageAssembly), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNamespace = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageNamespace), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageType = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageType), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMember = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMember), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberParameter = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMemberParameter), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDelegateParameter = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageDelegateParameter), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeTypeParameter = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageTypeTypeParameter), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMethodTypeParameter = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMethodTypeParameter), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageAssemblyMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageAssemblyMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNamespaceMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageNamespaceMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageTypeMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMemberMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberParameterMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMemberParameterMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDelegateParameterMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageDelegateParameterMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeTypeParameterMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageTypeTypeParameterMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMethodTypeParameterMoreMeaningfulName = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyMessageMethodTypeParameterMoreMeaningfulName), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(TextAnalyzersResources.IdentifiersShouldBeSpelledCorrectlyDescription), TextAnalyzersResources.ResourceManager, typeof(TextAnalyzersResources));

        internal static DiagnosticDescriptor AssemblyRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageAssembly,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor NamespaceRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespace,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor TypeRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageType,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMember,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberParameterRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor DelegateParameterRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDelegateParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor TypeTypeParameterRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MethodTypeParameterRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMethodTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor AssemblyMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageAssemblyMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor NamespaceMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespaceMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor TypeMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberParameterMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameterMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor DelegateParameterMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDelegateParameterMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor TypeTypeParameterMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeTypeParameterMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MethodTypeParameterMoreMeaningfulNameRule = new(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMethodTypeParameterMoreMeaningfulName,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1704",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);*/

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;
        //ImmutableArray.Create(AssemblyRule, NamespaceRule, TypeRule, MemberRule, MemberParameterRule, DelegateParameterRule, TypeTypeParameterRule, MethodTypeParameterRule, AssemblyMoreMeaningfulNameRule, NamespaceMoreMeaningfulNameRule, TypeMoreMeaningfulNameRule, MemberMoreMeaningfulNameRule, MemberParameterMoreMeaningfulNameRule, DelegateParameterMoreMeaningfulNameRule, TypeTypeParameterMoreMeaningfulNameRule, MethodTypeParameterMoreMeaningfulNameRule);

#pragma warning disable RS1025 // Configure generated code analysis
        public override void Initialize(AnalysisContext context)
#pragma warning restore RS1025 // Configure generated code analysis
        {
            context.EnableConcurrentExecution();

            // TODO: Configure generated code analysis.
            //analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}
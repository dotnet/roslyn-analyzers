// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers
{                   
    /// <summary>
    /// CA2229: Implement serialization constructors
    /// </summary>
    public abstract class ImplementSerializationConstructorsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2229";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementSerializationConstructorsTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessageCreateMagicConstructor = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementSerializationConstructorsMessageCreateMagicConstructor), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeUnsealedMagicConstructorFamily = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementSerializationConstructorsMessageMakeUnsealedMagicConstructorFamily), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeSealedMagicConstructorPrivate = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementSerializationConstructorsMessageMakeSealedMagicConstructorPrivate), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementSerializationConstructorsDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        internal static DiagnosticDescriptor CreateMagicConstructorRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageCreateMagicConstructor,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeUnsealedMagicConstructorFamilyRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeUnsealedMagicConstructorFamily,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeSealedMagicConstructorPrivateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeSealedMagicConstructorPrivate,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CreateMagicConstructorRule, MakeUnsealedMagicConstructorFamilyRule, MakeSealedMagicConstructorPrivateRule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}
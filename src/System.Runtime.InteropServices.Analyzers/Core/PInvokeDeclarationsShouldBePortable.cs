// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Runtime.InteropServices.Analyzers
{                   
    /// <summary>
    /// CA1901: PInvoke declarations should be portable
    /// </summary>
    public abstract class PInvokeDeclarationsShouldBePortableAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1901";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokeDeclarationsShouldBePortableTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessageParameter = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokeDeclarationsShouldBePortableMessageParameter), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageReturn = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokeDeclarationsShouldBePortableMessageReturn), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokeDeclarationsShouldBePortableDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        internal static DiagnosticDescriptor ParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageParameter,
                                                                             DiagnosticCategory.Portability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ReturnRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageReturn,
                                                                             DiagnosticCategory.Portability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ParameterRule, ReturnRule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}
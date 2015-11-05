// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Runtime.InteropServices.Analyzers
{                   
    /// <summary>
    /// CA1401: PInvokes should not be visible
    /// </summary>
    public abstract class PInvokesShouldNotBeVisibleAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1401";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokesShouldNotBeVisibleTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokesShouldNotBeVisibleMessage), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.PInvokesShouldNotBeVisibleDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Interoperability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}
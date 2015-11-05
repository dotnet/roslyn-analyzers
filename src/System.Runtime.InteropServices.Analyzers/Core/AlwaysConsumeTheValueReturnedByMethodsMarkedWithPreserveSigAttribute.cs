// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Runtime.InteropServices.Analyzers
{                   
    /// <summary>
    /// RS0015: Always consume the value returned by methods marked with PreserveSigAttribute
    /// </summary>
    public abstract class AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0015";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeMessage), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
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
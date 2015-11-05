// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Runtime.Analyzers
{                   
    /// <summary>
    /// CA2201: Do not raise reserved exception types
    /// </summary>
    public abstract class DoNotRaiseReservedExceptionTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2201";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseReservedExceptionTypesTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessageTooGeneric = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseReservedExceptionTypesMessageTooGeneric), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageReserved = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseReservedExceptionTypesMessageReserved), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotRaiseReservedExceptionTypesDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        
        internal static DiagnosticDescriptor TooGenericRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTooGeneric,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ReservedRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageReserved,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TooGenericRule, ReservedRule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}
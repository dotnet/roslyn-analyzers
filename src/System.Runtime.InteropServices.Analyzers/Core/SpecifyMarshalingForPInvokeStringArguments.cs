// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Runtime.InteropServices.Analyzers
{                   
    /// <summary>
    /// CA2101: Specify marshaling for PInvoke string arguments
    /// </summary>
    public abstract class SpecifyMarshalingForPInvokeStringArgumentsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2101";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessageParameter = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsMessageParameter), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageField = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsMessageField), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageParameterImplicitAnsi = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsMessageParameterImplicitAnsi), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFieldImplicitAnsi = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsMessageFieldImplicitAnsi), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        
        internal static DiagnosticDescriptor ParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageParameter,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FieldRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageField,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ParameterImplicitAnsiRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageParameterImplicitAnsi,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FieldImplicitAnsiRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFieldImplicitAnsi,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ParameterRule, FieldRule, ParameterImplicitAnsiRule, FieldImplicitAnsiRule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}
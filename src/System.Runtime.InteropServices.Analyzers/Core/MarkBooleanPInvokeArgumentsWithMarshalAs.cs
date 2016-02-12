// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.InteropServices.Analyzers
{
    /// <summary>
    /// CA1414: Mark boolean PInvoke arguments with MarshalAs
    /// </summary>
    public abstract class MarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1414";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.MarkBooleanPInvokeArgumentsWithMarshalAsTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.MarkBooleanPInvokeArgumentsWithMarshalAsMessageDefault), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageReturn = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.MarkBooleanPInvokeArgumentsWithMarshalAsMessageReturn), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.MarkBooleanPInvokeArgumentsWithMarshalAsDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Interoperability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ReturnRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageReturn,
                                                                             DiagnosticCategory.Interoperability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, ReturnRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}
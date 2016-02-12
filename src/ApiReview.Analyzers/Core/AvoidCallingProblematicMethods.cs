// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace ApiReview.Analyzers
{
    /// <summary>
    /// CA2001: Avoid calling problematic methods
    /// </summary>
    public abstract class AvoidCallingProblematicMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2001";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsTitle), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageSystemGCCollect = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemGCCollect), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemThreadingThreadResume = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemThreadingThreadResume), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemThreadingThreadSuspend = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemThreadingThreadSuspend), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemTypeInvokeMember = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemTypeInvokeMember), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageCoInitializeSecurity = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageCoInitializeSecurity), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageCoSetProxyBlanket = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageCoSetProxyBlanket), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemRuntimeInteropServicesSafeHandleDangerousGetHandle = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemRuntimeInteropServicesSafeHandleDangerousGetHandle), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemReflectionAssemblyLoadFrom = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemReflectionAssemblyLoadFrom), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemReflectionAssemblyLoadFile = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemReflectionAssemblyLoadFile), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemReflectionAssemblyLoadWithPartialName = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsMessageSystemReflectionAssemblyLoadWithPartialName), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(ApiReviewAnalyzersResources.AvoidCallingProblematicMethodsDescription), ApiReviewAnalyzersResources.ResourceManager, typeof(ApiReviewAnalyzersResources));

        internal static DiagnosticDescriptor SystemGCCollectRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemGCCollect,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemThreadingThreadResumeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemThreadingThreadResume,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemThreadingThreadSuspendRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemThreadingThreadSuspend,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemTypeInvokeMemberRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemTypeInvokeMember,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor CoInitializeSecurityRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageCoInitializeSecurity,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor CoSetProxyBlanketRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageCoSetProxyBlanket,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemRuntimeInteropServicesSafeHandleDangerousGetHandleRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemRuntimeInteropServicesSafeHandleDangerousGetHandle,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemReflectionAssemblyLoadFromRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemReflectionAssemblyLoadFrom,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemReflectionAssemblyLoadFileRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemReflectionAssemblyLoadFile,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemReflectionAssemblyLoadWithPartialNameRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemReflectionAssemblyLoadWithPartialName,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SystemGCCollectRule, SystemThreadingThreadResumeRule, SystemThreadingThreadSuspendRule, SystemTypeInvokeMemberRule, CoInitializeSecurityRule, CoSetProxyBlanketRule, SystemRuntimeInteropServicesSafeHandleDangerousGetHandleRule, SystemReflectionAssemblyLoadFromRule, SystemReflectionAssemblyLoadFileRule, SystemReflectionAssemblyLoadWithPartialNameRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}
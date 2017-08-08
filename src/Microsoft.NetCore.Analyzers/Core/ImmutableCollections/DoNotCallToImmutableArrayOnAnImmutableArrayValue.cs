// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.NetCore.Analyzers.ImmutableCollections
{
    /// <summary>
    /// RS0012: Do not call ToImmutableArray on an ImmutableArray value
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer : DiagnosticAnalyzer
    {
        private const string ImmutableArrayMetadataName = "System.Collections.Immutable.ImmutableArray`1";
        internal const string RuleId = "RS0012";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueTitle), SystemCollectionsImmutableAnalyzersResources.ResourceManager, typeof(SystemCollectionsImmutableAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueMessage), SystemCollectionsImmutableAnalyzersResources.ResourceManager, typeof(SystemCollectionsImmutableAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var immutableArrayType = compilationStartContext.Compilation.GetTypeByMetadataName(ImmutableArrayMetadataName);
                if (immutableArrayType == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationActionInternal(operationContext =>
                {
                    var invocation = (IInvocationExpression)operationContext.Operation;
                    if (invocation.TargetMethod?.Name != "ToImmutableArray")
                    {
                        return;
                    }

                    var receiverType = invocation.GetReceiverType(operationContext.Compilation, beforeConversion: true, cancellationToken: operationContext.CancellationToken);
                    if (receiverType != null &&
                        receiverType.DerivesFromOrImplementsAnyConstructionOf(immutableArrayType))
                    {
                        operationContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                    }
                }, OperationKind.InvocationExpression);
            });
        }


    }
}
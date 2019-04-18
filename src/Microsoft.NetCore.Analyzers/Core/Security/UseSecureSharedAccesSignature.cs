// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseSecureSharedAccesSignature : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5375";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSecureSharedAccesSignature),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_DoNotUseAccountSASMessage = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseAccountSASMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_UseSharedAccessProtocolHttpsOnlyMessage = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSharedAccessProtocolHttpsOnlyMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_UseContainerLevelAccessPolicyMessage = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseContainerLevelAccessPolicyMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSecureSharedAccesSignatureDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor DoNotUseAccountSASRule = CreateDiagnosticDescriptor(s_DoNotUseAccountSASMessage);

        internal static DiagnosticDescriptor UseSharedAccessProtocolHttpsOnlyRule = CreateDiagnosticDescriptor(s_UseSharedAccessProtocolHttpsOnlyMessage);

        internal static DiagnosticDescriptor UseContainerLevelAccessPolicyRule = CreateDiagnosticDescriptor(s_UseContainerLevelAccessPolicyMessage);

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(
            LocalizableString message)
        {
            return new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);
        }

        internal static ImmutableArray<(string, string)> NamespaceAndpolicyIdentifierNamePairs = ImmutableArray.Create(
                                                                                                    ("Blob", "groupPolicyIdentifier"),
                                                                                                    ("File", "groupPolicyIdentifier"),
                                                                                                    ("Queue", "accessPolicyIdentifier"),
                                                                                                    ("Table", "accessPolicyIdentifier"));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
                                                                                        DoNotUseAccountSASRule,
                                                                                        UseSharedAccessProtocolHttpsOnlyRule,
                                                                                        UseContainerLevelAccessPolicyRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                compilationStartAnalysisContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    if (!(operationBlockStartContext.OwningSymbol is IMethodSymbol containingMethod))
                    {
                        return;
                    }

                    var interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                                                            operationBlockStartContext.Options,
                                                            SupportedDiagnostics,
                                                            defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.None,
                                                            cancellationToken: operationBlockStartContext.CancellationToken,
                                                            defaultMaxInterproceduralMethodCallChain: 1);
                    var lazyValueContentResult = new Lazy<(DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>, PointsToAnalysisResult)>(
                        valueFactory: ComputeValueContentAnalysisResult, isThreadSafe: true);

                    operationBlockStartContext.RegisterOperationAction(operationAnalysisContext =>
                    {
                        var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                        var methodSymbol = invocationOperation.TargetMethod;

                        if (methodSymbol.Name != "GetSharedAccessSignature")
                        {
                            return;
                        }

                        var namespaceSymbol = methodSymbol.ContainingNamespace;

                        if (namespaceSymbol == null)
                        {
                            return;
                        }

                        var namespaceQualifiedName = namespaceSymbol.ToDisplayString();

                        if (namespaceQualifiedName != "Microsoft.WindowsAzure.Storage" &&
                            !namespaceQualifiedName.StartsWith("Microsoft.WindowsAzure.Storage.", StringComparison.Ordinal))
                        {
                            return;
                        }

                        var typeSymbol = methodSymbol.ContainingType;

                        if (typeSymbol.Name == "CloudStorageAccount")
                        {
                            operationAnalysisContext.ReportDiagnostic(
                                    invocationOperation.CreateDiagnostic(
                                        DoNotUseAccountSASRule));
                        }
                        else
                        {
                            var protocalsArgumentOperation = invocationOperation.Arguments.FirstOrDefault(s => s.Parameter.Name == "protocols");

                            if (protocalsArgumentOperation != null)
                            {
                                var protocalsArgument = lazyValueContentResult.Value.Item1[protocalsArgumentOperation.Kind, protocalsArgumentOperation.Syntax];

                                if (protocalsArgument.IsLiteralState &&
                                    !protocalsArgument.LiteralValues.Contains(1))
                                {
                                    operationAnalysisContext.ReportDiagnostic(
                                        invocationOperation.CreateDiagnostic(
                                            UseSharedAccessProtocolHttpsOnlyRule));
                                }
                            }
                        }

                        foreach (var namespaceAndpolicyIdentifierNamePair in NamespaceAndpolicyIdentifierNamePairs)
                        {
                            if (namespaceQualifiedName == "Microsoft.WindowsAzure.Storage." + namespaceAndpolicyIdentifierNamePair.Item1)
                            {
                                var argumentOperation = invocationOperation.Arguments.FirstOrDefault(s => s.Parameter.Name == namespaceAndpolicyIdentifierNamePair.Item2);

                                if (argumentOperation != null)
                                {
                                    var pointsToAbstractValue = lazyValueContentResult.Value.Item2[argumentOperation.Kind, argumentOperation.Syntax];

                                    if (pointsToAbstractValue.NullState == NullAbstractValue.Null)
                                    {
                                        operationAnalysisContext.ReportDiagnostic(
                                            invocationOperation.CreateDiagnostic(
                                                UseContainerLevelAccessPolicyRule));
                                    }
                                }
                            }
                        }
                    }, OperationKind.Invocation);

                    (DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>, PointsToAnalysisResult) ComputeValueContentAnalysisResult()
                    {
                        foreach (var operationRoot in operationBlockStartContext.OperationBlocks)
                        {
                            var topmostBlock = operationRoot.GetTopmostParentBlock();

                            if (topmostBlock != null)
                            {
                                var cfg = topmostBlock.GetEnclosingControlFlowGraph();
                                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(operationBlockStartContext.Compilation);
                                var valueContentAnalysisResultOpt = ValueContentAnalysis.GetOrComputeResult(
                                                                                            cfg,
                                                                                            containingMethod,
                                                                                            wellKnownTypeProvider,
                                                                                            interproceduralAnalysisConfig,
                                                                                            out var copyAnalysisResult,
                                                                                            out var pointsToAnalysisResult);

                                return (valueContentAnalysisResultOpt, pointsToAnalysisResult);
                            }
                        }

                        return (null, null);
                    }
                });
            });
        }
    }
}

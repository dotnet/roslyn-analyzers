﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    public abstract class AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer<TSyntaxKind> : DiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        internal const string RuleId = "RS0015";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeTitle), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeMessage), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeInteropServicesAnalyzersResources.AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeDescription), SystemRuntimeInteropServicesAnalyzersResources.ResourceManager, typeof(SystemRuntimeInteropServicesAnalyzersResources));

        internal static readonly DiagnosticDescriptor ConsumePreserveSigAnalyzerDescriptor = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ConsumePreserveSigAnalyzerDescriptor);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var preserveSigType = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.PreserveSigAttribute");
                if (preserveSigType != null)
                {
                    compilationContext.RegisterSyntaxNodeAction(
                        nodeContext => AnalyzeNode(nodeContext, preserveSigType, IsExpressionStatementSyntaxKind),
                        InvocationExpressionSyntaxKind);
                }
            });
        }

        protected abstract TSyntaxKind InvocationExpressionSyntaxKind { get; }
        protected abstract bool IsExpressionStatementSyntaxKind(int rawKind);

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol preserveSigType, Func<int, bool> isExpressionStatementSyntaxKind)
        {
            SyntaxNode node = context.Node;
            if (!isExpressionStatementSyntaxKind(node.Parent.RawKind))
            {
                return;
            }

            ISymbol symbol = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol;
            if (symbol == null)
            {
                return;
            }

            foreach (AttributeData attributeData in symbol.GetAttributes())
            {
                if (attributeData.AttributeClass.Equals(preserveSigType))
                {
                    Diagnostic diagnostic = Diagnostic.Create(ConsumePreserveSigAnalyzerDescriptor, node.GetLocation(), symbol);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.InteropServices.Analyzers
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
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        private INamedTypeSymbol _lazyPreserveSigType;

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(ConsumePreserveSigAnalyzerDescriptor);
            }
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                _lazyPreserveSigType = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.PreserveSigAttribute");
                if (_lazyPreserveSigType != null)
                {
                    compilationContext.RegisterSyntaxNodeAction(AnalyzeNode, ImmutableArray.Create(InvocationExpressionSyntaxKind));
                }
            });
        }

        protected abstract TSyntaxKind InvocationExpressionSyntaxKind { get; }
        protected abstract bool IsExpressionStatementSyntaxKind(int rawKind);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (_lazyPreserveSigType == null)
            {
                return;
            }

            SyntaxNode node = context.Node;
            if (!IsExpressionStatementSyntaxKind(node.Parent.RawKind))
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
                if (attributeData.AttributeClass.Equals(_lazyPreserveSigType))
                {
                    Diagnostic diagnostic = Diagnostic.Create(ConsumePreserveSigAnalyzerDescriptor, node.GetLocation(), symbol);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
        }
    }
}

namespace ClrHeapAllocationAnalyzer
{
    using System;
    using System.Collections.Immutable;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CallSiteImplicitAllocationAnalyzer : AllocationAnalyzer
    {
        public static DiagnosticDescriptor ParamsParameterRule = new DiagnosticDescriptor("HAA0101", "Array allocation for params parameter", "This call site is calling into a function with a 'params' parameter. This results in an array allocation even if no parameter is passed in for the params parameter", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor ValueTypeNonOverridenCallRule = new DiagnosticDescriptor("HAA0102", "Non-overridden virtual method call on value type", "Non-overridden virtual method call on a value type adds a boxing or constrained instruction", "Performance", DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ParamsParameterRule, ValueTypeNonOverridenCallRule);

        protected override SyntaxKind[] Expressions => new[] { SyntaxKind.InvocationExpression };

        private static readonly object[] EmptyMessageArgs = { };

        protected override void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var semanticModel = context.SemanticModel;
            Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;
            var cancellationToken = context.CancellationToken;
            string filePath = node.SyntaxTree.FilePath;

            var invocationExpression = node as InvocationExpressionSyntax;

            if (semanticModel.GetSymbolInfo(invocationExpression, cancellationToken).Symbol is IMethodSymbol methodInfo)
            {
                if (methodInfo.IsOverride)
                {
                    CheckNonOverridenMethodOnStruct(methodInfo, reportDiagnostic, invocationExpression, filePath);
                }

                if (methodInfo.Parameters.Length > 0 && invocationExpression.ArgumentList != null)
                {
                    var lastParam = methodInfo.Parameters[methodInfo.Parameters.Length - 1];
                    if (lastParam.IsParams)
                    {
                        CheckParam(invocationExpression, methodInfo, semanticModel, reportDiagnostic, filePath, cancellationToken);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckParam(InvocationExpressionSyntax invocationExpression, IMethodSymbol methodInfo, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var arguments = invocationExpression.ArgumentList.Arguments;
            if (arguments.Count != methodInfo.Parameters.Length)
            {
                reportDiagnostic(Diagnostic.Create(ParamsParameterRule, invocationExpression.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.ParamsAllocation(filePath);
            }
            else
            {
                var lastIndex = arguments.Count - 1;
                var lastArgumentTypeInfo = semanticModel.GetTypeInfo(arguments[lastIndex].Expression, cancellationToken);
                if (lastArgumentTypeInfo.Type != null && !lastArgumentTypeInfo.Type.Equals(methodInfo.Parameters[lastIndex].Type))
                {
                    reportDiagnostic(Diagnostic.Create(ParamsParameterRule, invocationExpression.GetLocation(), EmptyMessageArgs));
                    HeapAllocationAnalyzerEventSource.Logger.ParamsAllocation(filePath);
                }
            }
        }

        private static void CheckNonOverridenMethodOnStruct(IMethodSymbol methodInfo, Action<Diagnostic> reportDiagnostic, SyntaxNode node, string filePath)
        {
            if (methodInfo.ContainingType != null)
            {
                // hack? Hmmm.
                var containingType = methodInfo.ContainingType.ToString();
                if (string.Equals(containingType, "System.ValueType", StringComparison.OrdinalIgnoreCase) || string.Equals(containingType, "System.Enum", StringComparison.OrdinalIgnoreCase))
                {
                    reportDiagnostic(Diagnostic.Create(ValueTypeNonOverridenCallRule, node.GetLocation(), EmptyMessageArgs));
                    HeapAllocationAnalyzerEventSource.Logger.NonOverridenVirtualMethodCallOnValueType(filePath);
                }
            }
        }
    }
}
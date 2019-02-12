namespace ClrHeapAllocationAnalyzer
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisplayClassAllocationAnalyzer : AllocationAnalyzer
    {
        public static DiagnosticDescriptor ClosureDriverRule = new DiagnosticDescriptor("HAA0301", "Closure Allocation Source", "Heap allocation of closure Captures: {0}", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor ClosureCaptureRule = new DiagnosticDescriptor("HAA0302", "Display class allocation to capture closure", "The compiler will emit a class that will hold this as a field to allow capturing of this closure", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor LambaOrAnonymousMethodInGenericMethodRule = new DiagnosticDescriptor("HAA0303", "Lambda or anonymous method in a generic method allocates a delegate instance", "Considering moving this out of the generic method", "Performance", DiagnosticSeverity.Warning, true);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ClosureCaptureRule, ClosureDriverRule, LambaOrAnonymousMethodInGenericMethodRule);

        protected override SyntaxKind[] Expressions => new[] { SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression };

        private static readonly object[] EmptyMessageArgs = { };

        protected override void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var semanticModel = context.SemanticModel;
            var cancellationToken = context.CancellationToken;
            Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;

            var anonExpr = node as AnonymousMethodExpressionSyntax;
            if (anonExpr?.Block?.ChildNodes() != null && anonExpr.Block.ChildNodes().Any())
            {
                GenericMethodCheck(semanticModel, node, anonExpr.DelegateKeyword.GetLocation(), reportDiagnostic, cancellationToken);
                ClosureCaptureDataFlowAnalysis(semanticModel.AnalyzeDataFlow(anonExpr.Block.ChildNodes().First(), anonExpr.Block.ChildNodes().Last()), reportDiagnostic, anonExpr.DelegateKeyword.GetLocation());
                return;
            }

            if (node is SimpleLambdaExpressionSyntax lambdaExpr)
            {
                GenericMethodCheck(semanticModel, node, lambdaExpr.ArrowToken.GetLocation(), reportDiagnostic, cancellationToken);
                ClosureCaptureDataFlowAnalysis(semanticModel.AnalyzeDataFlow(lambdaExpr), reportDiagnostic, lambdaExpr.ArrowToken.GetLocation());
                return;
            }

            if (node is ParenthesizedLambdaExpressionSyntax parenLambdaExpr)
            {
                GenericMethodCheck(semanticModel, node, parenLambdaExpr.ArrowToken.GetLocation(), reportDiagnostic, cancellationToken);
                ClosureCaptureDataFlowAnalysis(semanticModel.AnalyzeDataFlow(parenLambdaExpr), reportDiagnostic, parenLambdaExpr.ArrowToken.GetLocation());
                return;
            }
        }
        
        private static void ClosureCaptureDataFlowAnalysis(DataFlowAnalysis flow, Action<Diagnostic> reportDiagnostic, Location location)
        {
            if (flow?.Captured.Length <= 0)
            {
                return;
            }

            foreach (var capture in flow.Captured)
            {
                if (capture.Name != null && capture.Locations != null)
                {
                    foreach (var l in capture.Locations)
                     {
                        reportDiagnostic(Diagnostic.Create(ClosureCaptureRule, l, EmptyMessageArgs));
                    }
                }
            }

            reportDiagnostic(Diagnostic.Create(ClosureDriverRule, location, new[] { string.Join(",", flow.Captured.Select(x => x.Name)) }));
        }

        private static void GenericMethodCheck(SemanticModel semanticModel, SyntaxNode node, Location location, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(node, cancellationToken).Symbol != null)
            {
                var containingSymbol = semanticModel.GetSymbolInfo(node, cancellationToken).Symbol.ContainingSymbol as IMethodSymbol;
                if (containingSymbol != null && containingSymbol.Arity > 0)
                {
                    reportDiagnostic(Diagnostic.Create(LambaOrAnonymousMethodInGenericMethodRule, location, EmptyMessageArgs));
                }
            }
        }
    }
}
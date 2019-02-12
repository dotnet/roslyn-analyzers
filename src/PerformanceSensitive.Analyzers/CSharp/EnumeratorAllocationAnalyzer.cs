namespace ClrHeapAllocationAnalyzer
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EnumeratorAllocationAnalyzer : AllocationAnalyzer
    {
        public static DiagnosticDescriptor ReferenceTypeEnumeratorRule = new DiagnosticDescriptor("HAA0401", "Possible allocation of reference type enumerator", "Non-ValueType enumerator may result in a heap allocation", "Performance", DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ReferenceTypeEnumeratorRule);

        protected override SyntaxKind[] Expressions => new[] { SyntaxKind.ForEachStatement, SyntaxKind.InvocationExpression };

        private static readonly object[] EmptyMessageArgs = { };

        protected override void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var semanticModel = context.SemanticModel;
            Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;
            var cancellationToken = context.CancellationToken;
            string filePath = node.SyntaxTree.FilePath;
            var foreachExpression = node as ForEachStatementSyntax;
            if (foreachExpression != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(foreachExpression.Expression, cancellationToken);
                if (typeInfo.Type == null)
                    return;

                if (typeInfo.Type.Name == "String" && typeInfo.Type.ContainingNamespace.Name == "System")
                {
                    // Special case for System.String which is optmizined by
                    // the compiler and does not result in an allocation.
                    return;
                }

                // Regular way of getting the enumerator
                ImmutableArray<ISymbol> enumerator = typeInfo.Type.GetMembers("GetEnumerator");
                if ((enumerator == null || enumerator.Length == 0) && typeInfo.ConvertedType != null)
                {
                    // 1st we try and fallback to using the ConvertedType
                    enumerator = typeInfo.ConvertedType.GetMembers("GetEnumerator");
                }
                if ((enumerator == null || enumerator.Length == 0) && typeInfo.Type.Interfaces != null)
                {
                    // 2nd fallback, now we try and find the IEnumerable Interface explicitly
                    var iEnumerable = typeInfo.Type.Interfaces.Where(i => i.Name == "IEnumerable").ToImmutableArray();
                    if (iEnumerable != null && iEnumerable.Length > 0)
                    {
                        enumerator = iEnumerable[0].GetMembers("GetEnumerator");
                    }
                }

                if (enumerator != null && enumerator.Length > 0)
                {
                    var methodSymbol = enumerator[0] as IMethodSymbol; // probably should do something better here, hack.
                    if (methodSymbol != null)
                    {
                        if (methodSymbol.ReturnType.IsReferenceType && methodSymbol.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerator)
                        {
                            reportDiagnostic(Diagnostic.Create(ReferenceTypeEnumeratorRule, foreachExpression.InKeyword.GetLocation(), EmptyMessageArgs));
                            HeapAllocationAnalyzerEventSource.Logger.EnumeratorAllocation(filePath);
                        }
                    }
                }

                return;
            }

            var invocationExpression = node as InvocationExpressionSyntax;
            if (invocationExpression != null)
            {
                var methodInfo = semanticModel.GetSymbolInfo(invocationExpression, cancellationToken).Symbol as IMethodSymbol;
	            if (methodInfo?.ReturnType != null && methodInfo.ReturnType.IsReferenceType)
	            {
		            if (methodInfo.ReturnType.AllInterfaces != null)
		            {
			            foreach (var @interface in methodInfo.ReturnType.AllInterfaces)
			            {
				            if (@interface.SpecialType == SpecialType.System_Collections_Generic_IEnumerator_T || @interface.SpecialType == SpecialType.System_Collections_IEnumerator)
				            {
					            reportDiagnostic(Diagnostic.Create(ReferenceTypeEnumeratorRule, invocationExpression.GetLocation(), EmptyMessageArgs));
					            HeapAllocationAnalyzerEventSource.Logger.EnumeratorAllocation(filePath);
				            }
			            }
		            }
	            }
            }
        }
    }
}

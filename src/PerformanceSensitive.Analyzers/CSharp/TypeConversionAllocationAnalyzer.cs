namespace ClrHeapAllocationAnalyzer
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;


    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeConversionAllocationAnalyzer : AllocationAnalyzer
    {
        public static DiagnosticDescriptor ValueTypeToReferenceTypeConversionRule = new DiagnosticDescriptor("HAA0601", "Value type to reference type conversion causing boxing allocation", "Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor DelegateOnStructInstanceRule = new DiagnosticDescriptor("HAA0602", "Delegate on struct instance caused a boxing allocation", "Struct instance method being used for delegate creation, this will result in a boxing instruction", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor MethodGroupAllocationRule = new DiagnosticDescriptor("HAA0603", "Delegate allocation from a method group", "This will allocate a delegate instance", "Performance", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor ReadonlyMethodGroupAllocationRule = new DiagnosticDescriptor("HeapAnalyzerReadonlyMethodGroupAllocationRule", "Delegate allocation from a method group", "This will allocate a delegate instance", "Performance", DiagnosticSeverity.Info, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ValueTypeToReferenceTypeConversionRule, DelegateOnStructInstanceRule, MethodGroupAllocationRule, ReadonlyMethodGroupAllocationRule);

        protected override SyntaxKind[] Expressions => new[]
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.ReturnStatement,
            SyntaxKind.YieldReturnStatement,
            SyntaxKind.CastExpression,
            SyntaxKind.AsExpression,
            SyntaxKind.CoalesceExpression,
            SyntaxKind.ConditionalExpression,
            SyntaxKind.ForEachStatement,
            SyntaxKind.EqualsValueClause,
            SyntaxKind.Argument,
            SyntaxKind.ArrowExpressionClause,
            SyntaxKind.Interpolation
        };

        private static readonly object[] EmptyMessageArgs = { };

        protected override void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var semanticModel = context.SemanticModel;
            var cancellationToken = context.CancellationToken;
            Action<Diagnostic> reportDiagnostic = context.ReportDiagnostic;
            string filePath = node.SyntaxTree.FilePath;
            bool assignedToReadonlyFieldOrProperty = 
                (context.ContainingSymbol as IFieldSymbol)?.IsReadOnly == true ||
                (context.ContainingSymbol as IPropertySymbol)?.IsReadOnly == true;

            // this.fooObjCall(10);
            // new myobject(10);
            if (node is ArgumentSyntax)
            {
                ArgumentSyntaxCheck(node, semanticModel, assignedToReadonlyFieldOrProperty, reportDiagnostic, filePath, cancellationToken);
            }

            // object foo { get { return 0; } }
            if (node is ReturnStatementSyntax)
            {
                ReturnStatementExpressionCheck(node, semanticModel, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // yield return 0
            if (node is YieldStatementSyntax)
            {
                YieldReturnStatementExpressionCheck(node, semanticModel, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // object a = x ?? 0;
            // var a = 10 as object;
            if (node is BinaryExpressionSyntax)
            {
                BinaryExpressionCheck(node, semanticModel, assignedToReadonlyFieldOrProperty, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // for (object i = 0;;)
            if (node is EqualsValueClauseSyntax)
            {
                EqualsValueClauseCheck(node, semanticModel, assignedToReadonlyFieldOrProperty, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // object = true ? 0 : obj
            if (node is ConditionalExpressionSyntax)
            {
                ConditionalExpressionCheck(node, semanticModel, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // string a = $"{1}";
            if (node is InterpolationSyntax) {
                InterpolationCheck(node, semanticModel, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // var f = (object)
            if (node is CastExpressionSyntax)
            {
                CastExpressionCheck(node, semanticModel, reportDiagnostic, filePath, cancellationToken);
                return;
            }

            // object Foo => 1
            if (node is ArrowExpressionClauseSyntax)
            {
                ArrowExpressionCheck(node, semanticModel, assignedToReadonlyFieldOrProperty, reportDiagnostic, filePath, cancellationToken);
                return;
            }
        }

        private static void ReturnStatementExpressionCheck(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var returnStatementExpression = node as ReturnStatementSyntax;
            if (returnStatementExpression.Expression != null)
            {
                var returnConversionInfo = semanticModel.GetConversion(returnStatementExpression.Expression, cancellationToken);
                CheckTypeConversion(returnConversionInfo, reportDiagnostic, returnStatementExpression.Expression.GetLocation(), filePath);
            }
        }

        private static void YieldReturnStatementExpressionCheck(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var yieldExpression = node as YieldStatementSyntax;
            if (yieldExpression.Expression != null)
            {
                var returnConversionInfo = semanticModel.GetConversion(yieldExpression.Expression, cancellationToken);
                CheckTypeConversion(returnConversionInfo, reportDiagnostic, yieldExpression.Expression.GetLocation(), filePath);
            }
        }

        private static void ArgumentSyntaxCheck(SyntaxNode node, SemanticModel semanticModel, bool isAssignmentToReadonly, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var argument = node as ArgumentSyntax;
            if (argument.Expression != null)
            {
                var argumentTypeInfo = semanticModel.GetTypeInfo(argument.Expression, cancellationToken);
                var argumentConversionInfo = semanticModel.GetConversion(argument.Expression, cancellationToken);
                CheckTypeConversion(argumentConversionInfo, reportDiagnostic, argument.Expression.GetLocation(), filePath);
                CheckDelegateCreation(argument.Expression, argumentTypeInfo, semanticModel, isAssignmentToReadonly, reportDiagnostic, argument.Expression.GetLocation(), filePath, cancellationToken);
            }
        }

        private static void BinaryExpressionCheck(SyntaxNode node, SemanticModel semanticModel, bool isAssignmentToReadonly, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var binaryExpression = node as BinaryExpressionSyntax;

            // as expression
            if (binaryExpression.IsKind(SyntaxKind.AsExpression) && binaryExpression.Left != null && binaryExpression.Right != null)
            {
                var leftT = semanticModel.GetTypeInfo(binaryExpression.Left, cancellationToken);
                var rightT = semanticModel.GetTypeInfo(binaryExpression.Right, cancellationToken);

                if (leftT.Type?.IsValueType == true && rightT.Type?.IsReferenceType == true)
                {
                    reportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, binaryExpression.Left.GetLocation(), EmptyMessageArgs));
                    HeapAllocationAnalyzerEventSource.Logger.BoxingAllocation(filePath);
                }

                return;
            }

            if (binaryExpression.Right != null)
            {
                var assignmentExprTypeInfo = semanticModel.GetTypeInfo(binaryExpression.Right, cancellationToken);
                var assignmentExprConversionInfo = semanticModel.GetConversion(binaryExpression.Right, cancellationToken);
                CheckTypeConversion(assignmentExprConversionInfo, reportDiagnostic, binaryExpression.Right.GetLocation(), filePath);
                CheckDelegateCreation(binaryExpression.Right, assignmentExprTypeInfo, semanticModel, isAssignmentToReadonly, reportDiagnostic, binaryExpression.Right.GetLocation(), filePath, cancellationToken);
                return;
            }
        }

        private static void InterpolationCheck(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var interpolation = node as InterpolationSyntax;
            var typeInfo = semanticModel.GetTypeInfo(interpolation.Expression, cancellationToken);
            if (typeInfo.Type?.IsValueType == true) {
                reportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, interpolation.Expression.GetLocation(), EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.BoxingAllocation(filePath);
            }
        }

        private static void CastExpressionCheck(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var castExpression = node as CastExpressionSyntax;
            if (castExpression.Expression != null)
            {
                var castTypeInfo = semanticModel.GetTypeInfo(castExpression, cancellationToken);
                var expressionTypeInfo = semanticModel.GetTypeInfo(castExpression.Expression, cancellationToken);

                if (castTypeInfo.Type?.IsReferenceType == true && expressionTypeInfo.Type?.IsValueType == true)
                {
                    reportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, castExpression.Expression.GetLocation(), EmptyMessageArgs));
                }
            }
        }

        private static void ConditionalExpressionCheck(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var conditionalExpression = node as ConditionalExpressionSyntax;

            var trueExp = conditionalExpression.WhenTrue;
            var falseExp = conditionalExpression.WhenFalse;

            if (trueExp != null)
            {
                CheckTypeConversion(semanticModel.GetConversion(trueExp, cancellationToken), reportDiagnostic, trueExp.GetLocation(), filePath);
            }

            if (falseExp != null)
            {
                CheckTypeConversion(semanticModel.GetConversion(falseExp, cancellationToken), reportDiagnostic, falseExp.GetLocation(), filePath);
            }
        }

        private static void EqualsValueClauseCheck(SyntaxNode node, SemanticModel semanticModel, bool isAssignmentToReadonly, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var initializer = node as EqualsValueClauseSyntax;
            if (initializer.Value != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(initializer.Value, cancellationToken);
                var conversionInfo = semanticModel.GetConversion(initializer.Value, cancellationToken);
                CheckTypeConversion(conversionInfo, reportDiagnostic, initializer.Value.GetLocation(), filePath);
                CheckDelegateCreation(initializer.Value, typeInfo, semanticModel, isAssignmentToReadonly, reportDiagnostic, initializer.Value.GetLocation(), filePath, cancellationToken);
            }
        }


        private static void ArrowExpressionCheck(SyntaxNode node, SemanticModel semanticModel, bool isAssignmentToReadonly, Action<Diagnostic> reportDiagnostic, string filePath, CancellationToken cancellationToken)
        {
            var syntax = node as ArrowExpressionClauseSyntax;

            var typeInfo = semanticModel.GetTypeInfo(syntax.Expression, cancellationToken);
            var conversionInfo = semanticModel.GetConversion(syntax.Expression, cancellationToken);
            CheckTypeConversion(conversionInfo, reportDiagnostic, syntax.Expression.GetLocation(), filePath);
            CheckDelegateCreation(syntax, typeInfo, semanticModel, false, reportDiagnostic,
                syntax.Expression.GetLocation(), filePath, cancellationToken);
        }

        private static void CheckTypeConversion(Conversion conversionInfo, Action<Diagnostic> reportDiagnostic, Location location, string filePath)
        {
            if (conversionInfo.IsBoxing)
            {
                reportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, location, EmptyMessageArgs));
                HeapAllocationAnalyzerEventSource.Logger.BoxingAllocation(filePath);
            }
        }

        private static void CheckDelegateCreation(SyntaxNode node, TypeInfo typeInfo, SemanticModel semanticModel, bool isAssignmentToReadonly, Action<Diagnostic> reportDiagnostic, Location location, string filePath, CancellationToken cancellationToken)
        {
            // special case: method groups
            if (typeInfo.ConvertedType?.TypeKind == TypeKind.Delegate)
            {
                // new Action<Foo>(MethodGroup); should skip this one
                var insideObjectCreation = node?.Parent?.Parent?.Parent?.Kind() == SyntaxKind.ObjectCreationExpression;
                if (node is ParenthesizedLambdaExpressionSyntax || node is SimpleLambdaExpressionSyntax ||
                    node is AnonymousMethodExpressionSyntax || node is ObjectCreationExpressionSyntax ||
                    insideObjectCreation)
                {
                    // skip this, because it's intended.
                }
                else
                {
                    if (node.IsKind(SyntaxKind.IdentifierName))
                    {
                        if (semanticModel.GetSymbolInfo(node, cancellationToken).Symbol is IMethodSymbol) {
                            reportDiagnostic(Diagnostic.Create(MethodGroupAllocationRule, location, EmptyMessageArgs));
                            HeapAllocationAnalyzerEventSource.Logger.MethodGroupAllocation(filePath);
                        }
                    }
                    else if (node.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var memberAccess = node as MemberAccessExpressionSyntax;
                        if (semanticModel.GetSymbolInfo(memberAccess.Name, cancellationToken).Symbol is IMethodSymbol)
                        {
                            if (isAssignmentToReadonly)
                            {
                                reportDiagnostic(Diagnostic.Create(ReadonlyMethodGroupAllocationRule, location, EmptyMessageArgs));
                                HeapAllocationAnalyzerEventSource.Logger.ReadonlyMethodGroupAllocation(filePath);
                            }
                            else
                            {
                                reportDiagnostic(Diagnostic.Create(MethodGroupAllocationRule, location, EmptyMessageArgs));
                                HeapAllocationAnalyzerEventSource.Logger.MethodGroupAllocation(filePath);
                            }
                        }
                    } 
                    else if (node is ArrowExpressionClauseSyntax)
                    {
                        var arrowClause = node as ArrowExpressionClauseSyntax;
                        if (semanticModel.GetSymbolInfo(arrowClause.Expression, cancellationToken).Symbol is IMethodSymbol) {
                            reportDiagnostic(Diagnostic.Create(MethodGroupAllocationRule, location, EmptyMessageArgs));
                            HeapAllocationAnalyzerEventSource.Logger.MethodGroupAllocation(filePath);
                        }
                    }
                }

                var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken).Symbol;
                if (symbolInfo?.ContainingType?.IsValueType == true && !insideObjectCreation)
                {
                    reportDiagnostic(Diagnostic.Create(DelegateOnStructInstanceRule, location, EmptyMessageArgs));
                }
            }
        }
    }
}
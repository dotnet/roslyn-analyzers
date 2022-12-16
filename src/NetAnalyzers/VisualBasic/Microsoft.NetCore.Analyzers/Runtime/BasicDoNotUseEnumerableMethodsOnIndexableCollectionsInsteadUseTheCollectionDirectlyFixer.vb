' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
        Inherits DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
        Private Protected Overrides Function AdjustSyntaxNode(syntaxNode As SyntaxNode) As SyntaxNode
            Return syntaxNode
        End Function

        Private Protected Overrides Function IsConditionalAccess(syntaxNode As SyntaxNode) As Boolean
            Return TypeOf syntaxNode Is ConditionalAccessExpressionSyntax
        End Function

        Private Protected Overrides Function ConditionalElementAccessExpression(expression As SyntaxNode, whenNotNull As SyntaxNode) As SyntaxNode
            Dim arguments As IEnumerable(Of ArgumentSyntax) = {SimpleArgument(DirectCast(whenNotNull, ExpressionSyntax))}
            Return ConditionalAccessExpression(DirectCast(expression, ExpressionSyntax), InvocationExpression(Nothing, ArgumentList(SeparatedList(arguments))))
        End Function
    End Class
End Namespace
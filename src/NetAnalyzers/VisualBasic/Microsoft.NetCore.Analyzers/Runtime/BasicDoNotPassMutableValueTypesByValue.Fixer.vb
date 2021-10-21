' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    Public NotInheritable Class BasicDoNotPassMutableValueTypesByValueFixer : Inherits DoNotPassMutableValueTypesByValueFixer
        Private Protected Overrides Function AddRefKeywordToParameter(parameterNode As SyntaxNode) As SyntaxNode
            Dim cast = DirectCast(parameterNode, ParameterSyntax)
            Dim byrefModifierToken = SyntaxFactory.Token(SyntaxKind.ByRefKeyword)

            Return cast.AddModifiers(byrefModifierToken)
        End Function

        Private Protected Overrides Function GetArgumentNodes(root As SyntaxNode) As IEnumerable(Of SyntaxNode)
            Return root.DescendantNodes(Function(x) True).Where(Function(node) TryCast(node, ArgumentSyntax) IsNot Nothing)
        End Function

        Private Protected Overrides Function UpdateMatchingArgumentsAsync(solutionEditor As SolutionEditor, originalProject As Project, parameterSymbol As IParameterSymbol, token As CancellationToken) As Task
            Return Task.CompletedTask
        End Function
    End Class
End Namespace


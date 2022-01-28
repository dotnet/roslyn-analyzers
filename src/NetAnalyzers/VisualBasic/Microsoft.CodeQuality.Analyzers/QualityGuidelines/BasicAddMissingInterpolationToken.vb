' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.QualityGuidelines

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAddMissingInterpolationToken
        Inherits AbstractAddMissingInterpolationTokenAnalyzer

        Private Protected Overrides Function TryGetSpeculativeSemanticModel(operation As ILiteralOperation, dummyNode As SyntaxNode, ByRef model As SemanticModel) As Boolean
            Return operation.SemanticModel.TryGetSpeculativeSemanticModel(operation.Syntax.SpanStart, dummyNode.FirstAncestorOrSelf(Of ExecutableStatementSyntax)(), model)
        End Function

        Private Protected Overrides Function AreAllInterpolationsBindable(node As SyntaxNode, model As SemanticModel) As Boolean
            Dim interpolations = DirectCast(node, InterpolatedStringExpressionSyntax).Contents.OfType(Of InterpolationSyntax)()
            Dim hasNonConstantInterpolation = False
            For Each interpolation In interpolations
                If TypeOf interpolation.Expression Is LiteralExpressionSyntax Then
                    Continue For
                End If

                If model.GetSymbolInfo(interpolation.Expression).Symbol Is Nothing Then
                    Return False
                End If

                hasNonConstantInterpolation = True
            Next

            Return hasNonConstantInterpolation
        End Function

        Private Protected Overrides Function ParseStringLiteralAsInterpolatedString(operation As ILiteralOperation) As SyntaxNode
            Return TryCast(SyntaxFactory.ParseExpression("$" + operation.Syntax.ToString()), InterpolatedStringExpressionSyntax)
        End Function
    End Class
End Namespace

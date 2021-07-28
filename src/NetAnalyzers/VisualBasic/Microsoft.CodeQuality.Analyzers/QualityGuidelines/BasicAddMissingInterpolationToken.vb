' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

        Private Protected Overrides Function ShouldReport(operation As ILiteralOperation) As Boolean
            Dim annotation As New SyntaxAnnotation()
            Dim dummyNode = TryCast(SyntaxFactory.ParseExpression("$" + operation.Syntax.ToString()).WithAdditionalAnnotations(annotation), InterpolatedStringExpressionSyntax)
            If dummyNode Is Nothing Then
                Return False
            End If

            Dim root = operation.Syntax.SyntaxTree.GetRoot()
            root = root.ReplaceNode(operation.Syntax, dummyNode)
            dummyNode = DirectCast(root.GetAnnotatedNodes(annotation).Single(), InterpolatedStringExpressionSyntax)

            Dim model As SemanticModel = Nothing
            If Not operation.SemanticModel.TryGetSpeculativeSemanticModel(operation.Syntax.SpanStart, dummyNode.FirstAncestorOrSelf(Of ExecutableStatementSyntax)(), model) Then
                Return False
            End If

            Dim interpolations = dummyNode.Contents.OfType(Of InterpolationSyntax)()
            Dim hasNonConstantInterpolation = False
            For Each interpolation In interpolations
                If TypeOf interpolation.Expression Is LiteralExpressionSyntax Then
                    Continue For
                End If

                hasNonConstantInterpolation = True
                If model.GetSymbolInfo(interpolation.Expression).Symbol Is Nothing Then
                    Return False
                End If
            Next

            Return hasNonConstantInterpolation
        End Function
    End Class
End Namespace

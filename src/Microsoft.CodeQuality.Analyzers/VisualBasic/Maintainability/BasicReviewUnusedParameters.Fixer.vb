' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    ''' CA1801: Review unused parameters
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicReviewUnusedParametersFixer
        Inherits ReviewUnusedParametersFixer

        Protected Overrides Function GetParameterNode(node As SyntaxNode) As SyntaxNode
            Return node.Parent
        End Function

        Protected Overrides Function CanContinuouslyLeadToObjectCreationOrInvocation(node As SyntaxNode) As Boolean
            Dim kind = node.Kind()
            Return kind = SyntaxKind.QualifiedName OrElse kind = SyntaxKind.IdentifierName OrElse kind = SyntaxKind.SimpleMemberAccessExpression
        End Function
    End Class
End Namespace

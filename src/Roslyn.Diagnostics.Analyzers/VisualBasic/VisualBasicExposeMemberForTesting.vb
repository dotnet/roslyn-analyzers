﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Analyzer.Utilities
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Roslyn.Diagnostics.Analyzers

Namespace Roslyn.Diagnostics.VisualBasic.Analyzers
    <ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name:=NameOf(VisualBasicCreateTestAccessor))>
    <[Shared]>
    Public NotInheritable Class VisualBasicExposeMemberForTesting
        Inherits AbstractExposeMemberForTesting(Of TypeStatementSyntax)

        Public Sub New()
        End Sub

        Private Protected Overrides ReadOnly Property RefactoringHelpers As IRefactoringHelpers
            Get
                Return VisualBasicRefactoringHelpers.Instance
            End Get
        End Property

        Protected Overrides ReadOnly Property HasRefReturns As Boolean
            Get
                Return False
            End Get
        End Property

        Protected Overrides Function GetTypeDeclarationForNode(reportedNode As SyntaxNode) As SyntaxNode
            Return reportedNode.FirstAncestorOrSelf(Of TypeStatementSyntax)()?.Parent
        End Function

        Protected Overrides Function GetByRefType(type As SyntaxNode, refKind As RefKind) As SyntaxNode
            Return type
        End Function

        Protected Overrides Function GetByRefExpression(expression As SyntaxNode) As SyntaxNode
            Return expression
        End Function
    End Class
End Namespace


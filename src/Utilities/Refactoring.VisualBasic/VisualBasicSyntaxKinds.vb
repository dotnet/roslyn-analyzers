' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.VisualBasic

Namespace Global.Analyzer.Utilities

    Friend NotInheritable Class VisualBasicSyntaxKinds
        Implements ISyntaxKinds

        Public Shared ReadOnly Property Instance As New VisualBasicSyntaxKinds()

        Private Sub New()
        End Sub

        Public ReadOnly Property EndOfFileToken As Integer = SyntaxKind.EndOfFileToken Implements ISyntaxKinds.EndOfFileToken

        Public ReadOnly Property ExpressionStatement As Integer = SyntaxKind.ExpressionStatement Implements ISyntaxKinds.ExpressionStatement

        Public ReadOnly Property LocalDeclarationStatement As Integer = SyntaxKind.LocalDeclarationStatement Implements ISyntaxKinds.LocalDeclarationStatement

        Public ReadOnly Property VariableDeclarator As Integer = SyntaxKind.VariableDeclarator Implements ISyntaxKinds.VariableDeclarator

        Public ReadOnly Property DefaultLiteralExpression As Integer = SyntaxKind.NothingLiteralExpression Implements ISyntaxKinds.DefaultLiteralExpression
    End Class

End Namespace

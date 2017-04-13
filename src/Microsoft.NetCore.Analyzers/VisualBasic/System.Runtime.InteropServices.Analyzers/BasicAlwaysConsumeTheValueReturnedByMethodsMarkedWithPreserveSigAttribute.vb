' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.InteropServices.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic

Namespace System.Runtime.InteropServices.VisualBasic.Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicAlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer
        Inherits AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer(Of SyntaxKind)

        Protected Overrides ReadOnly Property InvocationExpressionSyntaxKind As SyntaxKind
            Get
                Return SyntaxKind.InvocationExpression
            End Get
        End Property

        Protected Overrides Function IsExpressionStatementSyntaxKind(rawKind As Integer) As Boolean
            Return CType(rawKind, SyntaxKind) = SyntaxKind.ExpressionStatement
        End Function
    End Class
End Namespace

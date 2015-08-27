' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Diagnostics

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Imports Desktop.Analyzers.Common


Namespace Desktop.Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicCA3075DiagnosticAnalyzer
        Inherits CA3075DiagnosticAnalyzer(Of SyntaxKind)
        Protected Overrides Sub RegisterAnalyzer(context As CodeBlockStartAnalysisContext(Of SyntaxKind), types As CompilationSecurityTypes, frameworkVersion As Version)
            Dim analyzer As New BasicAnalyzer(types, BasicSyntaxNodeHelper.DefaultInstance, frameworkVersion)
            context.RegisterSyntaxNodeAction(AddressOf analyzer.AnalyzeNode, SyntaxKind.InvocationExpression,
                                                                             SyntaxKind.ObjectCreationExpression,
                                                                             SyntaxKind.SimpleAssignmentStatement,
                                                                             SyntaxKind.VariableDeclarator,
                                                                             SyntaxKind.NamedFieldInitializer)
            context.RegisterCodeBlockEndAction(AddressOf analyzer.AnalyzeCodeBlockEnd)
        End Sub

        Private Class BasicAnalyzer
            Inherits Analyzer
            Public Sub New(types As CompilationSecurityTypes, helper As BasicSyntaxNodeHelper, frameworkVersion As Version)
                MyBase.New(types, helper, frameworkVersion)
            End Sub

            Protected Overrides Function IsObjectConstructionForTemporaryObject(node As SyntaxNode) As Boolean
                If node Is Nothing Then
                    Return False
                End If

                Dim kind As SyntaxKind = node.Kind()
                If kind <> SyntaxKind.ObjectCreationExpression Then
                    Return False
                End If

                For Each ancestor As SyntaxNode In node.Ancestors()
                    Dim k As SyntaxKind = ancestor.Kind()
                    If k = SyntaxKind.SimpleAssignmentStatement OrElse k = SyntaxKind.VariableDeclarator OrElse k = SyntaxKind.NamedFieldInitializer Then
                        Return False
                    End If
                Next

                Return True
            End Function
        End Class
    End Class
End Namespace

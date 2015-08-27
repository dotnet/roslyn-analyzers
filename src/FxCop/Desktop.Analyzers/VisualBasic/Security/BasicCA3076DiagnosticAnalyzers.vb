' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Diagnostics

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Imports Desktop.Analyzers.Common

Namespace Desktop.Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicCA3076DiagnosticAnalyzer
        Inherits CA3076DiagnosticAnalyzer(Of SyntaxKind)
        Protected Overrides Function GetAnalyzer(context As CodeBlockStartAnalysisContext(Of SyntaxKind), types As CompilationSecurityTypes) As Analyzer
            Dim analyzer As New Analyzer(types, BasicSyntaxNodeHelper.DefaultInstance)
            context.RegisterSyntaxNodeAction(AddressOf analyzer.AnalyzeNode, SyntaxKind.InvocationExpression,
                                                                             SyntaxKind.ObjectCreationExpression,
                                                                             SyntaxKind.SimpleAssignmentStatement,
                                                                             SyntaxKind.VariableDeclarator,
                                                                             SyntaxKind.NamedFieldInitializer)
            Return analyzer
        End Function
    End Class
End Namespace

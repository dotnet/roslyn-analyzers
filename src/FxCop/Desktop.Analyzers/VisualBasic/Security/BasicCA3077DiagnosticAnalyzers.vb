' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Diagnostics

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Imports Desktop.Analyzers.Common

Namespace Desktop.Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicCA3077DiagnosticAnalyzer
        Inherits CA3077DiagnosticAnalyzer
        Protected Overrides Function GetAnalyzer(context As CompilationStartAnalysisContext, types As CompilationSecurityTypes, targetFrameworkVersion As Version) As Analyzer
            Dim analyzer As New Analyzer(types, BasicSyntaxNodeHelper.DefaultInstance, targetFrameworkVersion)
            context.RegisterSyntaxNodeAction(AddressOf analyzer.AnalyzeNode, SyntaxKind.SubBlock, SyntaxKind.FunctionBlock, SyntaxKind.ConstructorBlock)

            Return analyzer
        End Function
    End Class
End Namespace


' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Security.Cryptography.Hashing.Algorithms.Analyzers
Imports System.Security.Cryptography.Hashing.Algorithms.Analyzers.Helpers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic

Namespace System.Security.Cryptography.Hashing.Algorithms.VisualBasic.Analyzers

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicDoNotUseInsecureCryptographicAlgorithmsAnalyzer
        Inherits DoNotUseInsecureCryptographicAlgorithmsAnalyzer

        Protected Overrides Function GetAnalyzer(context As CompilationStartAnalysisContext, cryptTypes As CompilationSecurityTypes) As SyntaxNodeAnalyzer
            Dim analyzer As SyntaxNodeAnalyzer = New SyntaxNodeAnalyzer(cryptTypes)
            context.RegisterSyntaxNodeAction(AddressOf analyzer.AnalyzeNode,
                                             SyntaxKind.InvocationExpression,
                                             SyntaxKind.ObjectCreationExpression)
            Return analyzer
        End Function
    End Class
End Namespace

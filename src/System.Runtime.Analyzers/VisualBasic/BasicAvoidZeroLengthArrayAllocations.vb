' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers
    ''' <summary>
    ''' RS0007: Avoid zero-length array allocations.
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAvoidZeroLengthArrayAllocationsAnalyzer
        Inherits AvoidZeroLengthArrayAllocationsAnalyzer

        Protected Overrides Function IsAttributeSyntax(node As SyntaxNode) As Boolean
            Return TypeOf node Is AttributeSyntax
        End Function
    End Class
End Namespace
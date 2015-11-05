' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers   
    ''' <summary>
    ''' CA1820: Test for empty strings using string length
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicTestForEmptyStringsUsingStringLengthAnalyzer
        Inherits TestForEmptyStringsUsingStringLengthAnalyzer

    End Class
End Namespace
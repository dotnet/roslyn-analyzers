' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers
    ''' <summary>
    ''' CA1816: Dispose methods should call SuppressFinalize
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicCallGCSuppressFinalizeCorrectlyAnalyzer
        Inherits CallGCSuppressFinalizeCorrectlyAnalyzer

    End Class
End Namespace
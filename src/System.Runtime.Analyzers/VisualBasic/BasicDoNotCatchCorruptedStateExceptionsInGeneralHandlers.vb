' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers   
    ''' <summary>
    ''' CA2153: Do not catch corrupted state exceptions in general handlers.
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotCatchCorruptedStateExceptionsInGeneralHandlersAnalyzer
        Inherits DoNotCatchCorruptedStateExceptionsInGeneralHandlersAnalyzer

    End Class
End Namespace
' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace XmlDocumentationComments.Analyzers
    ''' <summary>
    ''' RS0010: Avoid using cref tags with a prefix
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAvoidUsingCrefTagsWithAPrefixAnalyzer
        Inherits AvoidUsingCrefTagsWithAPrefixAnalyzer

    End Class
End Namespace
' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace XmlDocumentationComments.Analyzers     
    ''' <summary>
    ''' RS0010: Avoid using cref tags with a prefix
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicAvoidUsingCrefTagsWithAPrefixFixer
        Inherits AvoidUsingCrefTagsWithAPrefixFixer 

    End Class
End Namespace

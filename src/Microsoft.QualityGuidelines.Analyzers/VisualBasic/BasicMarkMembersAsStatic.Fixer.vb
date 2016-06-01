' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.QualityGuidelines.Analyzers
    ''' <summary>
    ''' CA1822: Mark members as static
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicMarkMembersAsStaticFixer
        Inherits MarkMembersAsStaticFixer

    End Class
End Namespace

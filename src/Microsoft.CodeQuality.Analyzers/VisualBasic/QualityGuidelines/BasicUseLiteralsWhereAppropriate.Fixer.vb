' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.QualityGuidelines.Analyzers

Namespace Microsoft.QualityGuidelines.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1802: Use literals where appropriate
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseLiteralsWhereAppropriateFixer
        Inherits UseLiteralsWhereAppropriateFixer

    End Class
End Namespace

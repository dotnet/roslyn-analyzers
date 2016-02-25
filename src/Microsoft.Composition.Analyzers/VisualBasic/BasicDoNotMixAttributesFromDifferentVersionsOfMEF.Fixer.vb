' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.Composition.Analyzers
    ''' <summary>
    ''' RS0006: Do not mix attributes from different versions of MEF
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotMixAttributesFromDifferentVersionsOfMEFFixer
        Inherits DoNotMixAttributesFromDifferentVersionsOfMEFFixer

    End Class
End Namespace

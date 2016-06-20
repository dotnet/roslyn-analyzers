' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.ApiDesignGuidelines.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers
    ''' <summary>
    ''' Async006: Don't Mix Blocking and Async
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotMixBlockingAndAsyncFixer
        Inherits DoNotMixBlockingAndAsyncFixer

    End Class
End Namespace

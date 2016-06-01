' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' Async006: Don't Mix Blocking and Async
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotMixBlockingAndAsyncAnalyzer
        Inherits DoNotMixBlockingAndAsyncAnalyzer

    End Class
End Namespace
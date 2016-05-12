' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' Async003: Don't Pass Async Lambdas as Void Returning Delegate Types
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDonTPassAsyncLambdasAsVoidReturningDelegateTypesFixer
        Inherits DonTPassAsyncLambdasAsVoidReturningDelegateTypesFixer

    End Class
End Namespace

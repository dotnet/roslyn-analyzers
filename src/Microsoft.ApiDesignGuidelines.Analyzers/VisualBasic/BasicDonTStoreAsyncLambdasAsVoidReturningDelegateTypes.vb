' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' Async004: Don't Store Async Lambdas as Void Returning Delegate Types
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDonTStoreAsyncLambdasAsVoidReturningDelegateTypesAnalyzer
        Inherits DontStoreAsyncLambdasAsVoidReturningDelegateTypesAnalyzer

    End Class
End Namespace
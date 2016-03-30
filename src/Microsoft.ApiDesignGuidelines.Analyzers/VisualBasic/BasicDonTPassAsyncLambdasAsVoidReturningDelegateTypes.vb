' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' Async003: Don't Pass Async Lambdas as Void Returning Delegate Types
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDonTPassAsyncLambdasAsVoidReturningDelegateTypesAnalyzer
        Inherits DonTPassAsyncLambdasAsVoidReturningDelegateTypesAnalyzer

    End Class
End Namespace
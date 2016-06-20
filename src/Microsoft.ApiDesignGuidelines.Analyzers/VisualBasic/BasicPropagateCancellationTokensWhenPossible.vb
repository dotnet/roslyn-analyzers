' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.ApiDesignGuidelines.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers
    ''' <summary>
    ''' Async005: Propagate CancellationTokens When Possible
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPropagateCancellationTokensWhenPossibleAnalyzer
        Inherits PropagateCancellationTokensWhenPossibleAnalyzer

    End Class
End Namespace
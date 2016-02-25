' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Collections.Immutable.Analyzers
    ''' <summary>
    ''' RS0012: Do not call ToImmutableArray on an ImmutableArray value
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer
        Inherits DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer

    End Class
End Namespace
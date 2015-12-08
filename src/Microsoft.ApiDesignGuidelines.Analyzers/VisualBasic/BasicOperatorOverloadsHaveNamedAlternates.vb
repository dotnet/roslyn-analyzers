' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' CA2225: Operator overloads have named alternates
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicOperatorOverloadsHaveNamedAlternatesAnalyzer
        Inherits OperatorOverloadsHaveNamedAlternatesAnalyzer
    End Class
End Namespace
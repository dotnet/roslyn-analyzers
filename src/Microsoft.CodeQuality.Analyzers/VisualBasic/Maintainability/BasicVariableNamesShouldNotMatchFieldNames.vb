' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.Maintainability.Analyzers

Namespace Microsoft.Maintainability.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1500: Variable names should not match field names
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicVariableNamesShouldNotMatchFieldNamesAnalyzer
        Inherits VariableNamesShouldNotMatchFieldNamesAnalyzer

    End Class
End Namespace
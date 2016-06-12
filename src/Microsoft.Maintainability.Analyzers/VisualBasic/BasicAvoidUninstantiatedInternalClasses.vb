' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.Maintainability.Analyzers

Namespace Microsoft.Maintainability.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1812: Avoid uninstantiated internal classes
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAvoidUninstantiatedInternalClassesAnalyzer
        Inherits AvoidUninstantiatedInternalClassesAnalyzer

    End Class
End Namespace
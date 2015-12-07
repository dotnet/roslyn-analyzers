' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Resources.Analyzers   
    ''' <summary>
    ''' CA1824: Mark assemblies with NeutralResourcesLanguageAttribute
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicMarkAssembliesWithNeutralResourcesLanguageAnalyzer
        Inherits MarkAssembliesWithNeutralResourcesLanguageAnalyzer

    End Class
End Namespace
' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.Composition.Analyzers
    ''' <summary>
    ''' RS0006: Do not mix attributes from different versions of MEF
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer
        Inherits DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer

    End Class
End Namespace
' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.QualityGuidelines.Analyzers
    ''' <summary>
    ''' CA2119: Seal methods that satisfy private interfaces
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicSealMethodsThatSatisfyPrivateInterfacesAnalyzer
        Inherits SealMethodsThatSatisfyPrivateInterfacesAnalyzer

    End Class
End Namespace
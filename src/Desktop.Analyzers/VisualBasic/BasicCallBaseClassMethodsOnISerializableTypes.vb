' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Desktop.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Desktop.VisualBasic.Analyzers
    ''' <summary>
    ''' CA2236: Call base class methods on ISerializable types
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicCallBaseClassMethodsOnISerializableTypesAnalyzer
        Inherits CallBaseClassMethodsOnISerializableTypesAnalyzer

    End Class
End Namespace
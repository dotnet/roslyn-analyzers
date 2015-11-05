' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Desktop.Analyzers   
    ''' <summary>
    ''' CA2237: Mark ISerializable types with serializable
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicMarkISerializableTypesWithSerializableAnalyzer
        Inherits MarkISerializableTypesWithSerializableAnalyzer

    End Class
End Namespace
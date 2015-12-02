' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Desktop.Analyzers   
    ''' <summary>
    ''' CA3063: Use XmlReader for DataSet ReadXml
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicUseXmlReaderForDataSetReadXmlAnalyzer
        Inherits UseXmlReaderForDataSetReadXmlAnalyzer

    End Class
End Namespace
' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.NetCore.Analyzers.Security

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Security

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicDoNotUseInsecureCryptographicAlgorithmsAnalyzer
        Inherits DoNotUseInsecureCryptographicAlgorithmsAnalyzer
        ' This analyzer just exists in the VB assembly so that people don't have to redo their rulesets,
        ' since DoNotUseInsecureCryptographicAlgorithmsAnalyzer started out as separated by C# and VB.
    End Class
End Namespace

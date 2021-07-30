' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeQuality.Analyzers.QualityGuidelines

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicMarkMembersAsStaticAnalyzer
        Inherits MarkMembersAsStaticAnalyzer

        Protected Overrides Function SupportStaticLocalFunctions(parseOptions As ParseOptions) As Boolean
            Return False
        End Function
    End Class
End Namespace

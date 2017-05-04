' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    ''' <summary>
    ''' CA2215: Dispose Methods Should Call Base Class Dispose
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDisposeMethodsShouldCallBaseClassDisposeAnalyzer
        Inherits DisposeMethodsShouldCallBaseClassDisposeAnalyzer

    End Class
End Namespace
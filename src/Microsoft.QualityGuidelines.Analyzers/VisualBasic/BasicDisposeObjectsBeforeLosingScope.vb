' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Microsoft.QualityGuidelines.Analyzers
    ''' <summary>
    ''' CA2000: Dispose Objects Before Losing Scope
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDisposeObjectsBeforeLosingScopeAnalyzer
        Inherits DisposeObjectsBeforeLosingScopeAnalyzer

    End Class
End Namespace
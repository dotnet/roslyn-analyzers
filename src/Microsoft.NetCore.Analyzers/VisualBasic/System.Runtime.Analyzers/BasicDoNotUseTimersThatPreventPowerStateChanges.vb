' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace System.Runtime.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1601: Do not use timers that prevent power state changes
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotUseTimersThatPreventPowerStateChangesAnalyzer
        Inherits DoNotUseTimersThatPreventPowerStateChangesAnalyzer

    End Class
End Namespace
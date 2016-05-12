' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Threading.Tasks.Analyzers
    ''' <summary>
    ''' RS0018: Do not create tasks without passing a TaskScheduler
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCreateTasksWithoutPassingATaskSchedulerFixer
        Inherits DoNotCreateTasksWithoutPassingATaskSchedulerFixer

    End Class
End Namespace

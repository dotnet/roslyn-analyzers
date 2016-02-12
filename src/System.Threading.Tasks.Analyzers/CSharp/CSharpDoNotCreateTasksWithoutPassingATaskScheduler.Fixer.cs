// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Threading.Tasks.Analyzers
{
    /// <summary>
    /// RS0018: Do not create tasks without passing a TaskScheduler
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpDoNotCreateTasksWithoutPassingATaskSchedulerFixer : DoNotCreateTasksWithoutPassingATaskSchedulerFixer
    {
    }
}
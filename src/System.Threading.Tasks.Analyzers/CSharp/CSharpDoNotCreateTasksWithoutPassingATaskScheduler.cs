// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Threading.Tasks.Analyzers
{                          
    /// <summary>
    /// RS0018: Do not create tasks without passing a TaskScheduler
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer : DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer
    {

    }
}
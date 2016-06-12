// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks.CSharp.Analyzers;
using System.Threading.Tasks.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace System.Threading.Tasks.Analyzers.UnitTests
{
    public class DoNotCreateTasksWithoutPassingATaskSchedulerFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotCreateTasksWithoutPassingATaskSchedulerFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotCreateTasksWithoutPassingATaskSchedulerFixer();
        }
    }
}
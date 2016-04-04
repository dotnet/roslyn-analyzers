// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDirectlyAwaitATaskTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDirectlyAwaitATaskAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDirectlyAwaitATaskAnalyzer();
        }

        [Fact]
        public void CSharpSimpleAwaitTask()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t;
        await t;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(9, 15));
        }

        [Fact]
        public void BasicSimpleAwaitTask()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await t
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(7, 15));
        }

        public void CSharpSimpleAwaitTaskOfT()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<int> t;
        int x = await t;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(9, 23));
        }

        [Fact]
        public void BasicSimpleAwaitTaskOfT()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task(Of Integer)
        Dim x As Integer = Await t
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(7, 34));
        }

        public void CSharpNoDiagnostic()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t;
        await t.ConfigureAwait(false);

        SomeAwaitable s;
        await s;

        await; // No Argument
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void BasicNoDiagnostic()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await t.ConfigureAwait(False)

        Dim s As SomeAwaitable
        Await s

        Await 'No Argument
    End Function
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void CSharpAwaitAwaitTask()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<Task> t;
        await await t; // both have warnings.
        await await t.ConfigureAwait(false); // outer await is wrong.
        await (await t).ConfigureAwait(false); // inner await is wrong.
    }
}
";
            VerifyCSharp(code,
                GetCSharpResultAt(9, 15),
                GetCSharpResultAt(9, 21),
                GetCSharpResultAt(10, 15),
                GetCSharpResultAt(11, 22));
        }

        [Fact]
        public void BasicAwaitAwaitTask()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task(Of Task)
        Await Await t ' both have warnings.
        Await Await t.ConfigureAwait(False) ' outer await is wrong.
        Await (Await t).ConfigureAwait(False) ' inner await is wrong.
    End Function
End Class
";
            VerifyBasic(code,
                GetBasicResultAt(7, 15),
                GetBasicResultAt(7, 21),
                GetBasicResultAt(8, 15),
                GetBasicResultAt(9, 22));
        }

        [Fact]
        public void CSharpComplexAwaitTask()
        {
            var code = @"
using System;
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        int x = 10 + await GetTask();
        Func<Task<int>> a = async () => await GetTask();
        Console.WriteLine(await GetTask());
    }

    public Task<int> GetTask() { throw new NotImplementedException(); }
}
";
            VerifyCSharp(code,
                GetCSharpResultAt(9, 28),
                GetCSharpResultAt(10, 47),
                GetCSharpResultAt(11, 33));
        }

        [Fact]
        public void BasicComplexeAwaitTask()
        {
            var code = @"
Imports System
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim x As Integer = 10 + Await GetTask()
        Dim a As Func(Of Task(Of Integer)) = Async Function() Await GetTask()
        Console.WriteLine(Await GetTask())
    End Function
    Public Function GetTask() As Task(Of Integer)
        Throw New NotImplementedException()
    End Function
End Class
";
            VerifyBasic(code,
                GetBasicResultAt(7, 39),
                GetBasicResultAt(8, 69),
                GetBasicResultAt(9, 33));
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotDirectlyAwaitATaskAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDirectlyAwaitATaskMessage);
        }

        private DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotDirectlyAwaitATaskAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDirectlyAwaitATaskMessage);
        }
    }
}
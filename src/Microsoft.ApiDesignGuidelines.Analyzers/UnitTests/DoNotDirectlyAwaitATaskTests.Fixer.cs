// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDirectlyAwaitATaskFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDirectlyAwaitATaskAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDirectlyAwaitATaskAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new DoNotDirectlyAwaitATaskFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DoNotDirectlyAwaitATaskFixer();
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
            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t;
        await t.ConfigureAwait(false);
    }
}
";
            VerifyCSharpFix(code, fixedCode);
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

            var fixedCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await t.ConfigureAwait(False)
    End Function
End Class
";
            VerifyBasicFix(code, fixedCode);
        }

        [Fact]
        public void CSharpSimpleAwaitTaskWithTrivia()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t;
        await /*leading */ t /*trailing*/; //Shouldn't matter
    }
}
";
            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t;
        await /*leading */ t.ConfigureAwait(false) /*trailing*/; //Shouldn't matter
    }
}
";
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void BasicSimpleAwaitTaskWithTrivia()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await      t ' trailing
    End Function
End Class
";

            var fixedCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await      t.ConfigureAwait(False) ' trailing
    End Function
End Class
";
            VerifyBasicFix(code, fixedCode);
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

            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<Task> t;
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // both have warnings.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // outer await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // inner await is wrong.
    }
}
";
            VerifyCSharpFix(code, fixedCode);
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
            var fixedCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task(Of Task)
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' both have warnings.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' outer await is wrong.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' inner await is wrong.
    End Function
End Class
";
            VerifyBasicFix(code, fixedCode);
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
            var fixedCode = @"
using System;
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        int x = 10 + await GetTask().ConfigureAwait(false);
        Func<Task<int>> a = async () => await GetTask().ConfigureAwait(false);
        Console.WriteLine(await GetTask().ConfigureAwait(false));
    }

    public Task<int> GetTask() { throw new NotImplementedException(); }
}
";
            VerifyCSharpFix(code, fixedCode);
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
            var fixedCode = @"
Imports System
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim x As Integer = 10 + Await GetTask().ConfigureAwait(False)
        Dim a As Func(Of Task(Of Integer)) = Async Function() Await GetTask().ConfigureAwait(False)
        Console.WriteLine(Await GetTask().ConfigureAwait(False))
    End Function
    Public Function GetTask() As Task(Of Integer)
        Throw New NotImplementedException()
    End Function
End Class
";
            VerifyBasicFix(code, fixedCode);
        }
    }
}
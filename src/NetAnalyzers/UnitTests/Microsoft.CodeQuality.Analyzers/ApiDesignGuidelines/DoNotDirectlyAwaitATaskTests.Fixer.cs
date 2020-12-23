﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotDirectlyAwaitATaskFixerTests
    {
        [Theory]
        [InlineData("Task")]
        [InlineData("Task<int>")]
        [InlineData("ValueTask")]
        [InlineData("ValueTask<int>")]
        public async Task CSharpSimpleAwaitTask(string typeName)
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        " + typeName + @" t = default;
        await [|t|];
    }
}
";
            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        " + typeName + @" t = default;
        await t.ConfigureAwait(false);
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact, WorkItem(1962, "https://github.com/dotnet/roslyn-analyzers/issues/1962")]
        public async Task CSharpSimpleAwaitTask_ConfigureAwaitTrue()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t = null;
        await [|t|];
    }
}
";
            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t = null;
        await t.ConfigureAwait(true);
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = MicrosoftCodeQualityAnalyzersResources.AppendConfigureAwaitTrue,
            }.RunAsync();
        }

        [Theory]
        [InlineData("Task")]
        [InlineData("Task(Of Integer)")]
        [InlineData("ValueTask")]
        [InlineData("ValueTask(Of Integer)")]
        public async Task BasicSimpleAwaitTask(string typeName)
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As " + typeName + @"
        Await [|t|]
    End Function
End Class
";

            var fixedCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As " + typeName + @"
        Await t.ConfigureAwait(False)
    End Function
End Class
";
            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact, WorkItem(1962, "https://github.com/dotnet/roslyn-analyzers/issues/1962")]
        public async Task BasicSimpleAwaitTask_ConfigureAwaitTrue()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await [|t|]
    End Function
End Class
";

            var fixedCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await t.ConfigureAwait(True)
    End Function
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = 1,
                CodeActionEquivalenceKey = MicrosoftCodeQualityAnalyzersResources.AppendConfigureAwaitTrue,
            }.RunAsync();
        }

        [Fact]
        public async Task CSharpSimpleAwaitTaskWithTrivia()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t = null;
        await /*leading */ [|t|] /*trailing*/; //Shouldn't matter
    }
}
";
            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t = null;
        await /*leading */ t.ConfigureAwait(false) /*trailing*/; //Shouldn't matter
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task BasicSimpleAwaitTaskWithTrivia()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task
        Await      [|t|] ' trailing
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
            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task CSharpAwaitAwaitTask()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<Task> t = null;
        await [|await [|t|]|]; // both have warnings.
        await [|await t.ConfigureAwait(false)|]; // outer await is wrong.
        await (await [|t|]).ConfigureAwait(false); // inner await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // both correct.

        ValueTask<ValueTask> vt = default;
        await [|await [|vt|]|]; // both have warnings.
        await [|await vt.ConfigureAwait(false)|]; // outer await is wrong.
        await (await [|vt|]).ConfigureAwait(false); // inner await is wrong.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // both correct.
    }
}
";

            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<Task> t = null;
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // both have warnings.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // outer await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // inner await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // both correct.

        ValueTask<ValueTask> vt = default;
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // both have warnings.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // outer await is wrong.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // inner await is wrong.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // both correct.
    }
}
";

            // 🐛 the Fix All should not be producing this invalid code
            var fixAllCode = @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task<Task> t = null;
        await {|#1:(await t.ConfigureAwait(false)).ConfigureAwait(false)|}.{|#0:ConfigureAwait|}(false); // both have warnings.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // outer await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // inner await is wrong.
        await (await t.ConfigureAwait(false)).ConfigureAwait(false); // both correct.

        ValueTask<ValueTask> vt = null;
        await {|#1:(await vt.ConfigureAwait(false)).ConfigureAwait(false)|}.{|#0:ConfigureAwait|}(false); // both have warnings.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // outer await is wrong.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // inner await is wrong.
        await (await vt.ConfigureAwait(false)).ConfigureAwait(false); // both correct.
    }
}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                BatchFixedState =
                {
                    Sources = { fixAllCode },
                    ExpectedDiagnostics =
                    {
#if !NETCOREAPP
                        // /0/Test0.cs(9,69): error CS1061: 'ConfiguredTaskAwaitable' does not contain a definition for 'ConfigureAwait' and no accessible extension method 'ConfigureAwait' accepting a first argument of type 'ConfiguredTaskAwaitable' could be found (are you missing a using directive or an assembly reference?)
                        DiagnosticResult.CompilerError("CS1061").WithLocation(0).WithArguments("System.Runtime.CompilerServices.ConfiguredTaskAwaitable", "ConfigureAwait"),
#else
                        // /0/Test0.cs(9,15): error CS1929: 'ConfiguredTaskAwaitable' does not contain a definition for 'ConfigureAwait' and the best extension method overload 'TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, bool)' requires a receiver of type 'IAsyncDisposable'
                        DiagnosticResult.CompilerError("CS1929").WithLocation(1).WithArguments("System.Runtime.CompilerServices.ConfiguredTaskAwaitable", "ConfigureAwait", "System.Threading.Tasks.TaskAsyncEnumerableExtensions.ConfigureAwait(System.IAsyncDisposable, bool)", "System.IAsyncDisposable"),
#endif
                    },
                },
                NumberOfFixAllIterations = 2,
            }.RunAsync();
        }

        [Fact]
        public async Task BasicAwaitAwaitTask()
        {
            var code = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task(Of Task)
        Await [|Await [|t|]|] ' both have warnings.
        Await [|Await t.ConfigureAwait(False)|] ' outer await is wrong.
        Await (Await [|t|]).ConfigureAwait(False) ' inner await is wrong.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.

        Dim vt As ValueTask(Of ValueTask)
        Await [|Await [|vt|]|] ' both have warnings.
        Await [|Await vt.ConfigureAwait(False)|] ' outer await is wrong.
        Await (Await [|vt|]).ConfigureAwait(False) ' inner await is wrong.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.
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
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.

        Dim vt As ValueTask(Of ValueTask)
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' both have warnings.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' outer await is wrong.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' inner await is wrong.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.
    End Function
End Class
";

            // 🐛 the Fix All should not be producing this invalid code
            var fixAllCode = @"
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim t As Task(Of Task)
        Await {|BC30456:(Await t.ConfigureAwait(False)).ConfigureAwait(False).ConfigureAwait|}(False) ' both have warnings.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' outer await is wrong.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' inner await is wrong.
        Await (Await t.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.

        Dim vt As ValueTask(Of ValueTask)
        Await {|BC30456:(Await vt.ConfigureAwait(False)).ConfigureAwait(False).ConfigureAwait|}(False) ' both have warnings.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' outer await is wrong.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' inner await is wrong.
        Await (Await vt.ConfigureAwait(False)).ConfigureAwait(False) ' both correct.
    End Function
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                BatchFixedState = { Sources = { fixAllCode } },
                NumberOfFixAllIterations = 2,
            }.RunAsync();
        }

        [Fact]
        public async Task CSharpComplexAwaitTask()
        {
            var code = @"
using System;
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        int x = 10 + await [|GetTask()|];
        Func<Task<int>> a = async () => await [|GetTask()|];
        Console.WriteLine(await [|GetTask()|]);
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
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task BasicComplexeAwaitTask()
        {
            var code = @"
Imports System
Imports System.Threading.Tasks

Public Class C
    Public Async Function M() As Task
        Dim x As Integer = 10 + Await [|GetTask()|]
        Dim a As Func(Of Task(Of Integer)) = Async Function() Await [|GetTask()|]
        Console.WriteLine(Await [|GetTask()|])
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
            await VerifyVB.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact, WorkItem(1953, "https://github.com/dotnet/roslyn-analyzers/issues/1953")]
        public async Task CSharpAsyncVoidMethod_Diagnostic()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    private Task t;
    public async void M()
    {
        await [|M1Async()|];
    }

    private async Task M1Async()
    {
        await t.ConfigureAwait(false);
    }
}";

            var fixedCode = @"
using System.Threading.Tasks;

public class C
{
    private Task t;
    public async void M()
    {
        await M1Async().ConfigureAwait(false);
    }

    private async Task M1Async()
    {
        await t.ConfigureAwait(false);
    }
}";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }
    }
}
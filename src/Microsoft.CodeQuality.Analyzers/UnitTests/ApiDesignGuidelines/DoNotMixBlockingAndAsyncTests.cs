// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotMixBlockingAndAsyncTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotMixBlockingAndAsyncAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotMixBlockingAndAsyncAnalyzer();
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, params object[] messageArguments)
        {
            return GetBasicResultAt(line, column, DoNotMixBlockingAndAsyncAnalyzer.Rule, messageArguments);
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, params object[] messageArguments)
        {
            return GetCSharpResultAt(line, column, DoNotMixBlockingAndAsyncAnalyzer.Rule, messageArguments);
        }

        private static string GetFullSource(string body)
        {
            return $@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
{{
    class Program
    {{   {body}
    }}
}}
";
        }

        // No methods - no diagnostic should show
        [Fact]
        public void NoMethodNoDiagnostics()
        {
            var body = @"";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Async with task.delay - no diagnostic
        [Fact]
        public void AsyncWithAsync()
        {
            var body = @"
        async Task SleepNowAsync()
        {
            await Task.Delay(1000);
        }";
            var test = GetFullSource(body);
            VerifyCSharp(test);
        }

        // Async method calls a function with thread.sleep in it - out of scope, no diagnostic
        [Fact]
        public void HiddenSyncCode()
        {
            var body = @"
        async Task CallAnotherMethodAsync()
        {
            SleepAlittle(5888);
        }
        public void SleepAlittle(int value)
        {
            Thread.Sleep(value);
        }";
            var test = GetFullSource(body);
            VerifyCSharp(test);
        }

        // Non async method calls thread.wait - no diagnostic
        [Fact]
        public void NonAsyncWait()
        {
            var body = @"
        Task Example()
        {
            Task.Wait();
        }";
            var test = GetFullSource(body);
            VerifyCSharp(test);
        }

        // Async method calls a different Task method - no diagnostic
        [Fact]
        public void AsyncWithOtherMethod()
        {
            var body = @"
        async Task Example()
        {
            await Task.Delay(100);
        }";
            var test = GetFullSource(body);
            VerifyCSharp(test);
        }

        // Async method calls a Wait method on a Task, but not the same Wait - no diagnostic
        [Fact]
        public void AsyncCallsOtherWait()
        {
            var test = @"
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace ConsoleApplication
{
    class Program
    {
        async Task Example(Action a, string s)
        {
            await Task.Delay(100);
            var t = new Task(a);
            t.Wait(s);
        }
    }
    public static class MyTaskExtensions
    {
        public static void Wait(this Task t, string s)
        {
            s = s + 1;
        }
    }
}";
            VerifyCSharp(test);
        }

        // Async method calls a WaitAll method on a Task, but not the same WaitAll - no diagnostic
        [Fact]
        public void AsyncCallsOtherWaitAll()
        {
            var test = @"
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace ConsoleApplication
{
    class Program
    {
        async Task Example(Action a, string s)
        {
            await Task.Delay(100);
            var t = new Task(a);
            t.WaitAll(s);
        }
    }
    public static class MyTaskExtensions
    {
        public static void WaitAll(this Task t, string s)
        {
            s = s + 1;
        }
    }
}";
            VerifyCSharp(test);
        }

        // Async method calls a WaitAny method on a Task, but not the same WaitAny - no diagnostic
        [Fact]
        public void AsyncCallsOtherWaitAny()
        {
            var test = @"
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace ConsoleApplication
{
    class Program
    {
        async Task Example(Action a, string s)
        {
            await Task.Delay(100);
            var t = new Task(a);
            t.WaitAny(s);
        }
    }
    public static class MyTaskExtensions
    {
        public static void WaitAny(this Task t, string s)
        {
            s = s + 1;
        }
    }
}";
            VerifyCSharp(test);
        }

        // Async method calls a Sleep method on a Task, but not the same Sleep - no diagnostic
        [Fact]
        public void AsyncCallsOtherSleep()
        {
            var test = @"
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace ConsoleApplication
{
    class Program
    {
        async Task Example(Action a, string s)
        {
            await Task.Delay(100);
            var t = new Task(a);
            t.Sleep(s);
        }
    }
    public static class MyTaskExtensions
    {
        public static void Sleep(this Task t, string s)
        {
            s = s + 1;
        }
    }
}";
            VerifyCSharp(test);
        }

        // Async method calls a GetResult method on a Task, but not the same GetResult - no diagnostic
        [Fact]
        public void AsyncCallsOtherGetResult()
        {
            var test = @"
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace ConsoleApplication
{
    class Program
    {
        async Task Example(Action a, string s)
        {
            await Task.Delay(100);
            var t = new Task(a);
            t.GetResult(s);
        }
    }
    public static class MyTaskExtensions
    {
        public static void GetResult(this Task t, string s)
        {
            s = s + 1;
        }
    }
}";
            VerifyCSharp(test);
        }
    }
}

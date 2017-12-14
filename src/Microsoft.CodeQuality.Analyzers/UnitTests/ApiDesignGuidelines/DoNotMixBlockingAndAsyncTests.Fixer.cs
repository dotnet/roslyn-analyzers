// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotMixBlockingAndAsyncFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotMixBlockingAndAsyncAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotMixBlockingAndAsyncAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotMixBlockingAndAsyncFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotMixBlockingAndAsyncFixer();
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, params object[] messageArguments)
        {
            return GetBasicResultAt(line, column,
                rule: DoNotMixBlockingAndAsyncAnalyzer.Rule,
                messageArguments: messageArguments);
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, params object[] messageArguments)
        {
            return GetCSharpResultAt(line, column,
                rule: DoNotMixBlockingAndAsyncAnalyzer.Rule,
                messageArguments: messageArguments);
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

        // Wait block on async code - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_WaitBlockAsyncCode()
        {
            var body = @"
        async Task<int> fooAsync(int somenumber)
        {
            foo1Async(somenumber).Wait();
            return somenumber;
        }
        async Task<int> foo1Async(int value)
        {
            await Task.Yield();
            return value;
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(16, 35));

            var fixbody = @"
        async Task<int> fooAsync(int somenumber)
        {
            await foo1Async(somenumber);
            return somenumber;
        }
        async Task<int> foo1Async(int value)
        {
            await Task.Yield();
            return value;
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Result block on async code - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_ResultBlockAsyncCode()
        {
            var body = @"
        async Task foo2Async(int somenumber)
        {
            var temp = foo3Async(somenumber).Result;
        }
        async Task<int> foo3Async(int value)
        {
            await Task.Yield();
            return value;
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(16, 46));

            var fixbody = @"
        async Task foo2Async(int somenumber)
        {
            var temp = await foo3Async(somenumber);
        }
        async Task<int> foo3Async(int value)
        {
            await Task.Yield();
            return value;
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Invocation expression assigned to variable - 1 diagnostic
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_InvocationExpressionVariable()
        {
            var body = @"
        async Task<Task<int>> SomeMethodAsync()
        {
            var t = addAsync(25, 50);
            t.Wait();
            return t;
        }
        async Task<int> addAsync(int num1, int num2)
        {
            await Task.Delay(29292);
            return (num1 + num2);
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(17, 15));

            var fixbody = @"
        async Task<Task<int>> SomeMethodAsync()
        {
            var t = addAsync(25, 50);
            await t;
            return t;
        }
        async Task<int> addAsync(int num1, int num2)
        {
            await Task.Delay(29292);
            return (num1 + num2);
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Async method using waitall on task - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_WaitAllBlockAsyncCode()
        {
            var body = @"
        async Task WaitAllAsync()
        //Source: http://msdn.microsoft.com/en-us/library/dd270695(v=vs.110).aspx
        {
            Func<object, int> action = (object obj) =>
                {
                    int i = (int)obj;
                    // The tasks that receive an argument between 2 and 5 throw exceptions 
                    if (2 <= i && i <= 5)
                    {
                        throw new InvalidOperationException();
                    }
                    int tickCount = Environment.TickCount;
                    return tickCount;
                };
            const int n = 10;
            // Construct started tasks
            Task<int>[] tasks = new Task<int>[n];
            for (int i = 0; i<n; i++)
            {
                tasks[i] = Task<int>.Factory.StartNew(action, i);
            }
            // Exceptions thrown by tasks will be propagated to the main thread 
            // while it waits for the tasks. The actual exceptions will be wrapped in AggregateException. 
            try
            {
                // Wait for all the tasks to finish.
                Task.WaitAll(tasks);
                // We should never get to this point
            }
            catch (AggregateException e)
            {
                for (int j = 0; j<e.InnerExceptions.Count; j++)
                {
                    Console.WriteLine(e.InnerExceptions[j].ToString());
                }
            }
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(46, 22));

            var fixbody = @"
        async Task WaitAllAsync()
        //Source: http://msdn.microsoft.com/en-us/library/dd270695(v=vs.110).aspx
        {
            Func<object, int> action = (object obj) =>
                {
                    int i = (int)obj;
                    // The tasks that receive an argument between 2 and 5 throw exceptions 
                    if (2 <= i && i <= 5)
                    {
                        throw new InvalidOperationException();
                    }
                    int tickCount = Environment.TickCount;
                    return tickCount;
                };
            const int n = 10;
            // Construct started tasks
            Task<int>[] tasks = new Task<int>[n];
            for (int i = 0; i<n; i++)
            {
                tasks[i] = Task<int>.Factory.StartNew(action, i);
            }
            // Exceptions thrown by tasks will be propagated to the main thread 
            // while it waits for the tasks. The actual exceptions will be wrapped in AggregateException. 
            try
            {
                // Wait for all the tasks to finish.
                await Task.WhenAll(tasks);
                // We should never get to this point
            }
            catch (AggregateException e)
            {
                for (int j = 0; j<e.InnerExceptions.Count; j++)
                {
                    Console.WriteLine(e.InnerExceptions[j].ToString());
                }
            }
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }

        // Async with thread.sleep - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_AsyncWithThreadSleep()
        {
            var body = @"
        async Task<string> SleepHeadAsync(string phrase)
        {
            Thread.Sleep(9999);
            return phrase;
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(16, 20));

            var fixbody = @"
        async Task<string> SleepHeadAsync(string phrase)
        {
            await Task.Delay(9999);
            return phrase;
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }

        // Thread.sleep within loop - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_ThreadSleepInLoop()
        {
            var body = @"
        async Task SleepAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(3000);
            }
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(18, 24));

            var fixbody = @"
        async Task SleepAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(3000);
            }
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }

        // Thread.sleep(timespan) - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_AsyncWithThreadSleepTimeSpan()
        {
            var body = @"
        async Task TimeSpanAsync()
        {
            TimeSpan time = new TimeSpan(1000);
            Thread.Sleep(time);
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(17, 20));

            var fixbody = @"
        async Task TimeSpanAsync()
        {
            TimeSpan time = new TimeSpan(1000);
            await Task.Delay(time);
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }

        // Thread.sleep(TimeSpan) within loop - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_ThreadSleepTimeSpanInForLoop()
        {
            var body = @"
        async Task TimespaninForloopAsync()
        {
            TimeSpan interval = new TimeSpan(2, 20, 55, 33, 0);
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(interval);
            }
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(20, 24));

            var fixbody = @"
        async Task TimespaninForloopAsync()
        {
            TimeSpan interval = new TimeSpan(2, 20, 55, 33, 0);
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(interval);
            }
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }

        // Thread.sleep(timespan) within while loop - 1 diagnostic - code fix
        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1477")]
        public void BlockingAndAsync_ThreadSleepTimeSpanInWhileLoop()
        {
            var body = @"
        async Task TimeSpanWhileLoop()
        {
            bool trueorfalse = true;
            TimeSpan interval = new TimeSpan(0, 0, 2);
            while (trueorfalse)
            {
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(interval);
                }
                trueorfalse = false;
            }
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(22, 28));

            var fixbody = @"
        async Task TimeSpanWhileLoop()
        {
            bool trueorfalse = true;
            TimeSpan interval = new TimeSpan(0, 0, 2);
            while (trueorfalse)
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(interval);
                }
                trueorfalse = false;
            }
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest, null, true);
        }
    }
}
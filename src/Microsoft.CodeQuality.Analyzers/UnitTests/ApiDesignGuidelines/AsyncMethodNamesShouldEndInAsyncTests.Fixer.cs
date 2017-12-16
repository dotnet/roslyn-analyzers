// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AsyncMethodNamesShouldEndInAsyncFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAsyncMethodNamesShouldEndInAsyncAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAsyncMethodNamesShouldEndInAsyncAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicAsyncMethodNamesShouldEndInAsyncFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAsyncMethodNamesShouldEndInAsyncFixer();
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, string methodName)
        {
            return GetBasicResultAt(line, column, AsyncMethodNamesShouldEndInAsyncAnalyzer.Rule, methodName);
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string methodName)
        {
            return GetCSharpResultAt(line, column, AsyncMethodNamesShouldEndInAsyncAnalyzer.Rule, methodName);
        }

        private static string GetFullSource(string body)
        {
            return $@"
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication
{{
    class Program
    {{   {body}
    }}
}}
";
        }

        // Async method does not includes "Async" - 1 diagnostic
        [Fact]
        public void AsyncMethodWithoutAsync()
        {
            var body = @"
        private async Task<int> Math(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 33, "Math"));

            var fixbody = @"
        private async Task<int> MathAsync(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Adding Async would cause conflict 
        [Fact]
        public void MethodNameConflict1()
        {
            var body = @"
        private async Task<int> DoSomeMath(int number1, int number2) 
        { 
            await Task.Delay(100); 
            return (number1 + number2); 
        } 
    
        private async Task<int> DoSomeMathAsync(int number1, int number2) 
        { 
            await Task.Delay(500); 
            return (number1 * number2); 
        }";

            var test = GetFullSource(body);
            
            VerifyCSharp(test, GetCSharpResultAt(13, 33, "DoSomeMath"));

            var fixbody = @"
        private async Task<int> DoSomeMath(int number1, int number2) 
        { 
            await Task.Delay(100); 
            return (number1 + number2); 
        } 
    
        private async Task<int> DoSomeMathAsync(int number1, int number2) 
        { 
            await Task.Delay(500); 
            return (number1 * number2); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Method name conflict - 1 diagnostic
        [Fact]
        public void MethodNameConflict2()
        {
            var body = @"
        private int Math(int num1, int num2) 
        { 
            return (num1 - num2); 
        } 
        private async Task<int> Math(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(18, 33, "Math"));

            var fixbody = @"
        private int Math(int num1, int num2) 
        { 
            return (num1 - num2); 
        } 
        private async Task<int> MathAsync(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Async keyword is at beginning - should show 1 diagnostic 
        [Fact]
        public void AsyncKeywordInBeginning()
        {
            var body = @"
        public async void AsyncVoidReturnTaskT()
        {
            Console.WriteLine(""Begin Program"");
            Console.WriteLine(await AsyncTaskReturnT());
            Console.WriteLine(""End Program"");
        }
        private async Task<string> AsyncTaskReturnT()
        {
            await Task.Delay(65656565);
            return ""Program is processing...."";
        }";
            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(20, 36, "AsyncTaskReturnT"));

            var fixbody = @"
        public async void AsyncVoidReturnTaskT()
        {
            Console.WriteLine(""Begin Program"");
            Console.WriteLine(await AsyncTaskReturnTAsync());
            Console.WriteLine(""End Program"");
        }
        private async Task<string> AsyncTaskReturnTAsync()
        {
            await Task.Delay(65656565);
            return ""Program is processing...."";
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Misspelled async in method name - 1 diagnostic
        [Fact]
        public void MisspelledAsync()
        {
            var body = @"
        private async Task<int> DoSomeMathAsnyc(int number1, int number2) 
        { 
            await Task.Delay(100); 
            return (number1 + number2); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 33, "DoSomeMathAsnyc"));

            var fixbody = @"
        private async Task<int> DoSomeMathAsync(int number1, int number2) 
        { 
            await Task.Delay(100); 
            return (number1 + number2); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // No capital A in Async method name - 1 diagnostic
        [Fact]
        public void NoCapitalAinAsync()
        {
            var body = @"
        public static Task<String> ReturnAAAasync()
        {
            return Task.Run(() =>
            { 
                return (""AAA""); 
            });
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 36, "ReturnAAAasync"));

            var fixbody = @"
        public static Task<String> ReturnAAAAsync()
        {
            return Task.Run(() =>
            { 
                return (""AAA""); 
            });
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Method name contains async but misspelled and no capitol A - 1 diagnostic
        [Fact]
        public void MisspelledAndNoCapitolA()
        {
            var body = @"
        public static async Task<int> Multiplyanysc(int factor1, int factor2)
        {
            //delay three times
            await Task.Delay(100);
            await Task.Delay(100);
            await Task.Delay(100);
            return (factor1 * factor2);
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 39, "Multiplyanysc"));

            var fixbody = @"
        public static async Task<int> MultiplyAsync(int factor1, int factor2)
        {
            //delay three times
            await Task.Delay(100);
            await Task.Delay(100);
            await Task.Delay(100);
            return (factor1 * factor2);
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Misspelled Async named Async method - diagnostic should rename to Async
        [Fact]
        public void AsyncnamedAsyncMethodMisspelled()
        {
            var body = @"
        private async Task<bool> Anysc(bool trueorfalse) 
        { 
            await Task.Delay(700); 
            return !(trueorfalse); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 34, "Anysc"));

            var fixbody = @"
        private async Task<bool> Async(bool trueorfalse) 
        { 
            await Task.Delay(700); 
            return !(trueorfalse); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Very close spelling of async in name but wrong letters - 1 diagnostic
        [Fact]
        public void CloseSpellingOfAsyncbutNotEnough()
        {
            var body = @"
        private async Task<int> MathAzync(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 33, "MathAzync"));

            var fixbody = @"
        private async Task<int> MathAzyncAsync(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Very close spelling of async in name but wrong letters - 1 diagnostic
        [Fact]
        public void CloseSpellingOfAsyncbutNotEnough2()
        {
            var body = @"
        private async Task<int> MathAsytnc(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test, GetCSharpResultAt(13, 33, "MathAsytnc"));

            var fixbody = @"
        private async Task<int> MathAsytncAsync(int num1) 
        { 
            await Task.Delay(3000); 
            return (num1); 
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }

        // Test multiple fixes in a row - should have 4 diagnostics and fixes
        [Fact]
        public void MultipleMisspellings()
        {
            var body = @"
        private async Task Wait10Asycn()
        {
            await Task.Delay(10);
        }
        private async Task Wait50Aysnc()
        {
            await Task.Delay(50);
        }
        private async Task Wait100Ayscn()
        {
            await Task.Delay(100);
        }
        private async Task Wait200asycn()
        {
            await Task.Delay(200);
        }";

            var test = GetFullSource(body);

            VerifyCSharp(test,
                GetCSharpResultAt(13, 28, "Wait10Asycn"),
                GetCSharpResultAt(17, 28, "Wait50Aysnc"),
                GetCSharpResultAt(21, 28, "Wait100Auscn"),
                GetCSharpResultAt(25, 28, "Wait200asycn"));

            var fixbody = @"
        private async Task Wait10Async()
        {
            await Task.Delay(10);
        }
        private async Task Wait50Async()
        {
            await Task.Delay(50);
        }
        private async Task Wait100Async()
        {
            await Task.Delay(100);
        }
        private async Task Wait200Async()
        {
            await Task.Delay(200);
        }";

            var fixtest = GetFullSource(fixbody);
            VerifyCSharpFix(test, fixtest);
        }
    }
}
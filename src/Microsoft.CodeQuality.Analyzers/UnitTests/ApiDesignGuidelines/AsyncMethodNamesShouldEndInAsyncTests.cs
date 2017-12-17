// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AsyncMethodNamesShouldEndInAsyncTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAsyncMethodNamesShouldEndInAsyncAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAsyncMethodNamesShouldEndInAsyncAnalyzer();
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

        // No methods - no diagnostic should show
        [Fact]
        public void NoMethodNoDiagnostics()
        {
            var body = @"";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Task<int> async method includes "Async" - no diagnostic
        [Fact]
        public void TaskIntReturnWithAsync()
        {
            var body = @"
            private async Task<int> MathAsync(int num1) 
            { 
            await Task.Delay(3000); 
            return (num1); 
            } ";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Task<bool> async method includes "async" - no diagnostic
        [Fact]
        public void TaskBoolReturnWithAsync()
        {
            var body = @"
            private async Task<bool> TrueorFalseAsync(bool trueorfalse) 
            { 
            await Task.Delay(700); 
            return !(trueorfalse); 
            } ";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Async method named "async" - no diagnostic 
        [Fact]
        public void AsyncMethodNamedAsync()
        {
            var body = @"
            private async Task<bool> Async(bool trueorfalse) 
            { 
            await Task.Delay(700); 
            return !(trueorfalse); 
            } ";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Non async method named "async" - no diagnostic
        [Fact]
        public void NonAsyncMethodnamedAsync()
        {
            var body = @"
            public string Async(int zipcode) 
            { 
            if (zipcode == 21206) 
            { 
                return ""Baltimore""; 
            } 
            else 
                return ""Unknown""; 
            } ";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Non-Async method that do not include async - no diagnostic 
        [Fact]
        public void NonAsyncWithoutAsync()
        {
            var body = @"
            private string RepeatingStrings(int num, string Message)
            {
                while (num > 0)
                {
                    return RepeatingStrings(num - 1, Message);
                }
            return Message;
            }";

            var test = GetFullSource(body);

            VerifyCSharp(test);
        }

        // Simple missing "Async" test for VB - 1 diagnostic
        // Codefix is out of scope
        [Fact]
        public void VBRenameSimple()
        {
            var body = @"
imports System
imports System.Threading.Tasks

Module Module1
    Async Function example() As Task
    End Function
End Module
";

            var test = body;

            VerifyBasic(test, GetBasicResultAt(6, 20, "example"));
        }

        // Misspelled "Async" test for VB - 1 diagnostic
        // Codefix is out of scope
        [Fact]
        public void VBRenameMisspelledAsync()
        {
            var body = @"
imports System
imports System.Threading.Tasks

Module Module1
    Async Function exampleasycn() As Task
    End Function
End Module
";
            var test = body;

            VerifyBasic(test, GetBasicResultAt(6, 20, "exampleasycn"));
        }
    }
}
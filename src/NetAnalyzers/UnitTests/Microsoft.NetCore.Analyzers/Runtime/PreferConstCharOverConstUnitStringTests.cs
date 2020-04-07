// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferConstCharOverConstUnitStringForStringBuilderAppendTests
    {
        [Fact]
        public async Task TestRegularCase()
        {
            string input = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            const string [|ch = ""a""|];
            sb.Append(ch);
        } 
    } 
}";
            string fix = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            const char ch = 'a';
            sb.Append(ch);
        } 
    } 
}";

            await VerifyCS.VerifyCodeFixAsync(input, fix);
        }


        [Fact]
        public async Task TestMultipleDeclarations()
        {
            const string multipleDeclarations = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            const string ch = ""a"", bb = ""b"";
            sb.Append(ch);
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(multipleDeclarations, VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 26).WithArguments("ch"), VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 36).WithArguments("bb"));
        }

        [Fact]
        public async Task TestClassField()
        {
            const string classFieldInAppend = @"
using System;
using System.Text;

namespace RosylnScratch
{
    public class Program
    {
        public const string SS = ""a"";

        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(SS);
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(classFieldInAppend, VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(9, 29).WithArguments("SS"));
        }


        [Fact]
        public async Task TestNonUnitString()
        {
            const string nonUnitString = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            const string ch = ""ab"";
            sb.Append(ch);
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(nonUnitString);
        }

        [Fact]
        public async Task TestNoCallToStringAppend()
        {
            const string noCallToStringAppend = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            const string ch = ""a"";
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(noCallToStringAppend);
        }

        [Fact]
        public async Task TestNonConstUnitString()
        {
            const string nonConstUnitString = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            string ch = ""ab"";
            sb.Append(ch);
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(nonConstUnitString);
        }

        [Fact]
        public async Task TestAppendLiteral()
        {
            const string appendLiteral = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append("","");
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(appendLiteral);
        }

        [Fact]
        public async Task TestMethodCallInAppend()
        {
            const string methodCallInAppend = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private string AString() => ""A"";

        private void TestMethod() 
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append(AString());
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(methodCallInAppend);
        }

        [Fact]
        public async Task TestMethodParameterInAppend()
        {
            const string methodParameterInAppend = @"
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod(int value) 
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append(value.ToString());
        } 
    } 
}";
            await VerifyCS.VerifyAnalyzerAsync(methodParameterInAppend);
        }

    }
}
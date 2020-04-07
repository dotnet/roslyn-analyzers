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
            const string ch = ""a"";
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

            await VerifyCS.VerifyCodeFixAsync(input, VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 26).WithArguments("ch"), fix);
        }

        private const string multipleDeclarations = @" 
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

        [Fact]
        public async Task TestMultipleDeclarations()
        {
            await VerifyCS.VerifyAnalyzerAsync(multipleDeclarations, VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 26).WithArguments("ch"), VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 36).WithArguments("bb"));
        }

        private const string classFieldInAppend = @"
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
        [Fact]
        public async Task TestClassField()
        {
            await VerifyCS.VerifyAnalyzerAsync(classFieldInAppend, VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(9, 29).WithArguments("SS"));
        }


        private const string nonUnitString = @" 
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


        private const string noCallToStringAppend = @" 
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

        private const string nonConstUnitString = @" 
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
        private const string appendLiteral = @" 
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

        private const string methodCallInAppend = @" 
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

        private const string methodParameterInAppend = @"
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

        [Theory]
        [InlineData(nonUnitString)]
        [InlineData(noCallToStringAppend)]
        [InlineData(nonConstUnitString)]
        [InlineData(appendLiteral)]
        [InlineData(methodCallInAppend)]
        [InlineData(methodParameterInAppend)]
        public async Task TestNonUnitString(string input)
        {
            await VerifyCS.VerifyAnalyzerAsync(input);
        }
    }
}
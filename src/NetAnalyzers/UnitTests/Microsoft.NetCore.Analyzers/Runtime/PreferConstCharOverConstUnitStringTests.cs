// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

// TODO: Add tests for VisualBasic

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferConstCharOverConstUnitStringForStringBuilderAppendTests
    {
        [Fact]
        public async Task TestRegularCase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@" 
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
}", VerifyCS.Diagnostic(PreferConstCharOverConstUnitStringAnalyzer.Rule).WithLocation(12, 26).WithArguments("ch"));
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
        public async Task TestNonUnitString(string input)
        {
            await VerifyCS.VerifyAnalyzerAsync(input);
        }

        [Fact]
        public async Task TestCrash()
        {
            await VerifyCS.VerifyAnalyzerAsync(methodParameterInAppend);
        }

    }
}
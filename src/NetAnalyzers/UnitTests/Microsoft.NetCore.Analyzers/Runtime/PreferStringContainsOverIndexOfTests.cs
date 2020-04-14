// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfFixer>;
//using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
//    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfAnalyzer,
//    Microsoft.NetCore.Analyzers.Runtime.PreferStringContainsOverIndexOfFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStringContainsOverIndexOfTests
    {
        [Fact]
        public async Task TestRegularCase()
        {
            string csInput = @" 
using System; 
using System.Text;
 
namespace TestNamespace 
{ 
    class TestClass 
    { 
        private void TestMethod() 
        { 
            const string str = ""This is a string"";
            int index = str.IndexOf(""This"");
            if ([|index == -1|])
            {

            }
        } 
    } 
}";
            //            string csFix = @" 
            //using System; 
            //using System.Text;

            //namespace TestNamespace 
            //{ 
            //    class TestClass 
            //    { 
            //        private void TestMethod() 
            //        { 
            //            StringBuilder sb = new StringBuilder();
            //            const char ch = 'a';
            //            sb.Append(ch);
            //        } 
            //    } 
            //}";

            var test = new VerifyCS.Test
            {
                TestCode = csInput,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp50,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview,
            };
            await test.RunAsync();

            //            string vbInput = @" 
            //Imports System

            //Module Program
            //    Sub Main(args As String())
            //        Const [|aa|] As String = ""a""
            //        Dim builder As New System.Text.StringBuilder
            //        builder.Append(aa)

            //    End Sub
            //End Module
            //";

            //            string vbFix = @" 
            //Imports System

            //Module Program
            //    Sub Main(args As String())
            //        Const aa As Char = ""a""c
            //        Dim builder As New System.Text.StringBuilder
            //        builder.Append(aa)

            //    End Sub
            //End Module
            //";

            //            await VerifyVB.VerifyCodeFixAsync(vbInput, vbFix);
        }
    }
}
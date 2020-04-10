﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.PreferConstCharOverConstUnitStringFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferConstCharOverConstUnitStringForStringBuilderAppendTests
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
            StringBuilder sb = new StringBuilder();
            const string [|ch|] = ""a"";
            sb.Append(ch);
        } 
    } 
}";
            string csFix = @" 
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

            await VerifyCS.VerifyCodeFixAsync(csInput, csFix);

            string vbInput = @" 
Imports System

Module Program
    Sub Main(args As String())
        Const [|aa|] As String = ""a""
        Dim builder As New System.Text.StringBuilder
        builder.Append(aa)

    End Sub
End Module
";

            string vbFix = @" 
Imports System

Module Program
    Sub Main(args As String())
        Const aa As Char = ""a""c
        Dim builder As New System.Text.StringBuilder
        builder.Append(aa)

    End Sub
End Module
";

            await VerifyVB.VerifyCodeFixAsync(vbInput, vbFix);
        }

        [Fact]
        public async Task TestMultipleDeclarations()
        {
            const string multipleDeclarations_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(multipleDeclarations_cs);
            const string multipleDeclarations_vb = @" 
Imports System

Module Program
    Class TestClass
        Public Sub Main(args As String())
            Const [|aa|] As String = ""a""
            Dim builder As New System.Text.StringBuilder
            builder.Append(aa)
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(multipleDeclarations_vb);
        }

        [Fact]
        public async Task TestClassField()
        {
            const string classFieldInAppend_cs = @"
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
            await VerifyCS.VerifyAnalyzerAsync(classFieldInAppend_cs);
            const string classFieldInAppend_vb = @"
Imports System

Module Program
    Class TestClass
        Public Const str As String = ""a""
        Public Sub Main(args As String())
            Dim builder As New System.Text.StringBuilder
            builder.Append(str)
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(classFieldInAppend_vb);
        }


        [Fact]
        public async Task TestNonUnitString()
        {
            const string nonUnitString_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(nonUnitString_cs);
            const string nonUnitString_vb = @"
Imports System

Module Program
    Class TestClass
        Public Sub Main(args As String())
            Const ch As String = ""ab""
            Dim builder As New System.Text.StringBuilder
            builder.Append(ch)
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(nonUnitString_vb);
        }

        [Fact]
        public async Task TestNoCallToStringAppend()
        {
            const string noCallToStringAppend_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(noCallToStringAppend_cs);

            const string noCallToStringAppend_vb = @"
Imports System

Module Program
    Class TestClass
        Public Sub Main(args As String())
            Const ch As String = ""a""
            Dim builder As New System.Text.StringBuilder
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(noCallToStringAppend_vb);
        }

        [Fact]
        public async Task TestNonConstUnitString()
        {
            const string nonConstUnitString_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(nonConstUnitString_cs);

            const string nonConstUnitString_vb = @"
Imports System

Module Program
    Class TestClass
        Public Sub Main(args As String())
            Dim ch As String = ""a""
            Dim builder As New System.Text.StringBuilder
            builder.Append(ch)
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(nonConstUnitString_vb);
        }

        [Fact]
        public async Task TestAppendLiteral()
        {
            const string appendLiteral_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(appendLiteral_cs);

            const string appendLiteral_vb = @"
Imports System

Module Program
    Class TestClass
        Public Sub Main(args As String())
            Dim builder As New System.Text.StringBuilder
            builder.Append("","")
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(appendLiteral_vb);
        }

        [Fact]
        public async Task TestMethodCallInAppend()
        {
            const string methodCallInAppend_cs = @" 
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
            await VerifyCS.VerifyAnalyzerAsync(methodCallInAppend_cs);

            const string methodCallInAppend_vb = @"
Imports System

Module Program
    Class TestClass
        Public Function AString() As String
            Return ""A""
        End Function

        Public Sub Main(args As String())
            Dim builder As New System.Text.StringBuilder
            builder.Append(AString())
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(methodCallInAppend_vb);
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

            const string methodParameterInAppend_vb = @"
Imports System

Module Program
    Class TestClass
        Public Sub Main(arg As Int32)
            Dim builder As New System.Text.StringBuilder
            builder.Append(arg)
        End Sub
    End Class
End Module
";
            await VerifyVB.VerifyAnalyzerAsync(methodParameterInAppend_vb);
        }

    }
}
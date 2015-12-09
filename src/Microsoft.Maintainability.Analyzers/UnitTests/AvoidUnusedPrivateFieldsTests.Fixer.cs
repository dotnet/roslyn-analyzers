// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.Maintainability.Analyzers;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUnusedPrivateFieldsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUnusedPrivateFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUnusedPrivateFieldsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new AvoidUnusedPrivateFieldsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AvoidUnusedPrivateFieldsFixer();
        }

        [Fact]
        public void CA1823CSharp()
        {
            VerifyCSharpFix(
                @"  
class C  
{  
    public int x;
    public int y;
    public int z;
    private int a;
    private int b;
    private int c;

    public int Foo()
    {
        return x + z + a + c;
    }
}  
 ",
                @"  
class C  
{  
    public int x;
    public int y;
    public int z;
    private int a;
    private int c;

    public int Foo()
    {
        return x + z + a + c;
    }
}  
 ");
        }

        [Fact]
        public void CA1823VisualBasic()
        {
            VerifyBasicFix(
                @"
Class C
    Public x As Integer
    Public y As Integer
    Public z As Integer
    Private a As Integer
    Private b As Integer
    Private c As Integer

    Public Function Foo() As Integer
        Return x + z + a + c
    End Function
End Class
 ",
                @"
Class C
    Public x As Integer
    Public y As Integer
    Public z As Integer
    Private a As Integer
    Private c As Integer

    Public Function Foo() As Integer
        Return x + z + a + c
    End Function
End Class
 ");
        }
    }
}
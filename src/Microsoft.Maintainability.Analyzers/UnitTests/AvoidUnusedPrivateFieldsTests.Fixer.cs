// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
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
    private int d, e, f;

    public int Foo()
    {
        return x + z + a + c + d + f;
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
    private int d, f;

    public int Foo()
    {
        return x + z + a + c + d + f;
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
    Private d, e, f As Integer

    Public Function Foo() As Integer
        Return x + z + a + c + d + f
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
    Private d, f As Integer

    Public Function Foo() As Integer
        Return x + z + a + c + d + f
    End Function
End Class
 ");
        }
    }
}
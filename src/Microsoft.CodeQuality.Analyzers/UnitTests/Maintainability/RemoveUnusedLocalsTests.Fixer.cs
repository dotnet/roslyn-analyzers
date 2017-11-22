// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class RemoveUnusedLocalsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveUnusedLocalsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return null;
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicRemoveUnusedLocalsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpRemoveUnusedLocalsFixer();
        }
    
        [Fact]
        public void UnusedLocal_BaseFunctionality_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0; // remove also comment
        int b = 0;
        b = 0;
        var c = 3;
        unsafe
        {
            int* p;
            p = & c;
        }
        return 0;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        var c = 3;
        unsafe
        {
            int* p;
            p = & c;
        }
        return 0;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_ParametersAndMethodCalls_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        var a = 0;
        var b = N1(a);
        var c = 0;
        c = N1(a);
        return 0;
    }

    int N1(int d)
    {
        int a = 0;
        return a;
    }

    int N2(ref int d) 
    {
        d = 1;
        return 0;
    }
}
";

            VerifyCSharpFix(code, code, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, code, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveFirst_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0, b = 0;
        return b;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        int b = 0;
        return b;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveSecond_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0, b = 0;
        return a;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        int a = 0;
        return a;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveBoth_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0, b = 0;
        return 0;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        return 0;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact(Skip ="https://github.com/dotnet/roslyn/issues/23322")]
        public void UnusedLocal_JointAssignment_RemoveFirst_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0;
        int b = 0;
        a = b = 0;
        return b;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        int b = 0;
        b = 0;
        return b;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointAssignment_RemoveSecond_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0;
        int b = 0;
        a = b = 0;
        return a;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        int a = 0;
        a = 0;
        return a;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointAssignment_RemoveBoth_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0;
        int b = 0;
        a = b = 0;
        return 0;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        return 0;
    }
}
";
            // fixAll removes unused locals found in a single iteration.
            // a is used in the first iteration.
            // https://github.com/dotnet/roslyn/issues/23322 should fix this.
            var fixAll = @"
class C
{
    int M()
    {
        int a = 0;
        a = 0;
        return 0;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fixAll, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_LocalFunction_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int L() { int a = 1; return 1; }
        return 1;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        return 1;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_VariableWithinLocalFunction_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        int a = 0;
        int L() { return a; }
        return 1;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        return 1;
    }
}
";
            // Fix all will not clear 'a' used in the original code with a single pass.
            var fixAll = @"
class C
{
    int M()
    {
        int a = 0;
        return 1;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fixAll, allowNewCompilerDiagnostics: true);
        }

        [Fact (Skip = "https://github.com/dotnet/roslyn/issues/22921")]
        public void UnusedLocal_Lambda_CSharp()
        {
            var code = @"
class C
{
    int M()
    {
        Func<int> lambda = () =>
        {
            int a = 4;
            Func<int> internalLambda = () => { int b = 4; return 2; };
            return 1;
        };
        return 1;
    }
}
";
            var fix = @"
class C
{
    int M()
    {
        return 1;
    }
}
";
            VerifyCSharpFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyCSharpFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_BaseFunctionality_Basic()
        {
            var code = @"
Public Class C
    Function F() As Integer
        Dim a As Integer = 0 ' inline comment also to be deleted. 
        Dim b As Integer
        b = 0
        Return 0
    End Function

    Function G() As Integer
        Dim a As Integer = 0
        Return a
    End Function
End Class
";
            var fix = @"
Public Class C
    Function F() As Integer
        Return 0
    End Function

    Function G() As Integer
        Dim a As Integer = 0
        Return a
    End Function
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_ParametersAndMethodCalls_Basic()
        {
            var code = @"
Public Class C
    Function F() As Integer
        Dim a As Integer = 0
        Dim b As Integer = G(a)
        Dim c As Integer
        c = G(a)
        Return 0
    End Function

    Function G(p As Integer) As Integer
        Return 1
    End Function
End Class
";
          
            VerifyBasicFix(code, code, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, code, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveFirst_Basic()
        {
            var code = @"
Public Class C
    Function F() As Integer
        Dim a As Integer, b As Integer
        Dim c = 0, d = 0
        a = 0
        b = 0
        Return b + d
    End Function
End Class
";
            var fix = @"
Public Class C
    Function F() As Integer
        Dim b As Integer
        Dim d = 0
        b = 0
        Return b + d
    End Function
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveSecond_Basic()
        {
            var code = @"
Public Class C
    Function F() As Integer
        Dim a As Integer, b As Integer
        Dim c = 0, d = 0
        a = 0
        b = 0
        Return a + c
    End Function
End Class
";
            var fix = @"
Public Class C
    Function F() As Integer
        Dim a As Integer
        Dim c = 0
        a = 0
        Return a + c
    End Function
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_JointDeclaration_RemoveBoth_Basic()
        {
            var code = @"
Public Class C
    Function F() As Integer
        Dim a As Integer, b As Integer
        Dim c = 0, d = 0
        Dim g, h As New Integer
        a = 0
        b = 0
        g = 0
        h = 0
        Return 0
    End Function
End Class
";
            var fix = @"
Public Class C
    Function F() As Integer
        Dim g, h As New Integer
        g = 0
        h = 0
        Return 0
    End Function
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_Lambda_Basic()
        {
            var code = @"
Imports System
Public Class C
    Function F() As Integer
        Dim L As Func(Of Integer) = Function()
            Dim a As Integer = 0
            Dim internalL As Func(Of Integer) = Function()
                Dim b As Integer = 0
                Return 1
            End Function
            Return 1
        End Function
        Return L()
    End Function
End Class
";
            var fix = @"
Imports System
Public Class C
    Function F() As Integer
        Dim L As Func(Of Integer) = Function()
                                        Return 1
        End Function
        Return L()
    End Function
End Class
";
            VerifyBasicFix(code, fix, allowNewCompilerDiagnostics: true);
            VerifyBasicFixAll(code, fix, allowNewCompilerDiagnostics: true);
        }
    }
}
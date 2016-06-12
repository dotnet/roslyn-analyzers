// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.UnitTests
{
    public partial class CA1012FixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AbstractTypesShouldNotHaveConstructorsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new AbstractTypesShouldNotHaveConstructorsFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AbstractTypesShouldNotHaveConstructorsAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AbstractTypesShouldNotHaveConstructorsFixer();
        }

        [Fact]
        public void TestCSPublicAbstractClass()
        {
            var code = @"
public abstract class C
{
    public C()
    {
    }
}
";
            var fix = @"
public abstract class C
{
    protected C()
    {
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void TestVBPublicAbstractClass()
        {
            var code = @"
Public MustInherit Class C
    Public Sub New()
    End Sub
End Class
";
            var fix = @"
Public MustInherit Class C
    Protected Sub New()
    End Sub
End Class
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void TestCSInternalAbstractClass()
        {
            var code = @"
abstract class C
{
    public C()
    {
    }
}
";
            var fix = @"
abstract class C
{
    protected C()
    {
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void TestVBInternalAbstractClass()
        {
            var code = @"
MustInherit Class C
    Public Sub New()
    End Sub
End Class
";
            var fix = @"
MustInherit Class C
    Protected Sub New()
    End Sub
End Class
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void TestCSNestedAbstractClassWithPublicConstructor1()
        {
            var code = @"
public struct C
{
    abstract class D
    {
        public D() { }
    }
}
";
            var fix = @"
public struct C
{
    abstract class D
    {
        protected D() { }
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void TestVBNestedAbstractClassWithPublicConstructor1()
        {
            var code = @"
Public Structure C
    MustInherit Class D
        Public Sub New()
        End Sub
    End Class
End Structure
";
            var fix = @"
Public Structure C
    MustInherit Class D
        Protected Sub New()
        End Sub
    End Class
End Structure
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void TestNestedAbstractClassWithPublicConstructor2()
        {
            var code = @"
public abstract class C
{
    public abstract class D
    {
        public D() { }
    }
}
";
            var fix = @"
public abstract class C
{
    public abstract class D
    {
        protected D() { }
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void TestVBNestedAbstractClassWithPublicConstructor2()
        {
            var code = @"
Public MustInherit Class C
   Protected Friend MustInherit Class D
        Sub New()
        End Sub
    End Class
End Class
";
            var fix = @"
Public MustInherit Class C
   Protected Friend MustInherit Class D
        Protected Sub New()
        End Sub
    End Class
End Class
";
            VerifyBasicFix(code, fix);
        }

        [Fact]
        public void TestNestedAbstractClassWithPublicConstructor3()
        {
            var code = @"
internal abstract class C
{
    public abstract class D
    {
        public D() { }
    }
}
";
            var fix = @"
internal abstract class C
{
    public abstract class D
    {
        protected D() { }
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void TestVBNestedAbstractClassWithPublicConstructor3()
        {
            var code = @"
MustInherit Class C
   Public MustInherit Class D
        Sub New()
        End Sub
    End Class
End Class
";
            var fix = @"
MustInherit Class C
   Public MustInherit Class D
        Protected Sub New()
        End Sub
    End Class
End Class
";
            VerifyBasicFix(code, fix);
        }
    }
}

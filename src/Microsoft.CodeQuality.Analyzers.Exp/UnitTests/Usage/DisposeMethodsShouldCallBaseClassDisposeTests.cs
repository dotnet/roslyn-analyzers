﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.Exp.Usage;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Usage
{
    using Verify = CSharpCodeFixVerifier<DisposeMethodsShouldCallBaseClassDispose, EmptyCodeFixProvider>;
    using VerifyVB = VisualBasicCodeFixVerifier<DisposeMethodsShouldCallBaseClassDispose, EmptyCodeFixProvider>;

    public partial class DisposeMethodsShouldCallBaseClassDisposeTests
    {
        private DiagnosticResult GetCSharpResultAt(int line, int column, string containingMethod, string baseDisposeSignature) =>
            Verify.Diagnostic(DisposeMethodsShouldCallBaseClassDispose.Rule).WithLocation(line, column).WithArguments(containingMethod, baseDisposeSignature);

        private DiagnosticResult GetBasicResultAt(int line, int column, string containingMethod, string baseDisposeSignature) =>
            VerifyVB.Diagnostic(DisposeMethodsShouldCallBaseClassDispose.Rule).WithLocation(line, column).WithArguments(containingMethod, baseDisposeSignature);

        [Fact]
        public async Task NoBaseDisposeImplementation_NoBaseDisposeCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A 
{
}

class B : A, IDisposable
{
    public void Dispose()
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
End Class

Class B
    Inherits A
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class");
        }

        [Fact]
        public async Task NoBaseDisposeImplementation_NoBaseDisposeCall_02_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A 
{
    public virtual void Dispose()
    {
    }
}

class B : A, IDisposable
{
    public override void Dispose()
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Public Overridable Sub Dispose()
    End Sub
End Class

Class B
    Inherits A
    Implements IDisposable

    Public Overrides Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class");
        }

        [Fact]
        public async Task BaseDisposeCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    public override void Dispose()
    {
        base.Dispose();
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        MyBase.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public async Task NoBaseDisposeCall_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    public override void Dispose()
    {
    }
}
",
            // Test0.cs(13,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(13, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
    End Sub
End Class",
            // Test0.vb(14,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(14, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact]
        public async Task BaseDisposeCall_IgnoreCase_VB_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        myBasE.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public async Task BaseDisposeBoolCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose(bool b)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

class B : A
{
    public override void Dispose(bool b)
    {
        base.Dispose(b);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose(b As Boolean)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose(b As Boolean)
        MyBase.Dispose(b)
    End Sub
End Class");
        }

        [Fact]
        public async Task NoBaseDisposeBoolCall_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose(bool b)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

class B : A
{
    public override void Dispose(bool b)
    {
    }
}
",
            // Test0.cs(19,26): warning CA2215: Ensure that method 'void B.Dispose(bool b)' calls 'base.Dispose(bool)' in all possible control flow paths.
            GetCSharpResultAt(19, 26, "void B.Dispose(bool b)", "base.Dispose(bool)"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose(b As Boolean)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose(b As Boolean)
    End Sub
End Class",
            // Test0.vb(19,26): warning CA2215: Ensure that method 'Sub B.Dispose(b As Boolean)' calls 'MyBase.Dispose(Boolean)' in all possible control flow paths.
            GetBasicResultAt(19, 26, "Sub B.Dispose(b As Boolean)", "MyBase.Dispose(Boolean)"));
        }

        [Fact]
        public async Task NoBaseDisposeCloseCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Close()
    {
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}

class B : A
{
    public override void Close()
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Close()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Close()
        GC.SuppressFinalize(Me)
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Close()
    End Sub
End Class");
        }

        [Fact]
        public async Task AbstractBaseDisposeMethod_NoBaseDisposeCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

abstract class A : IDisposable
{
    public abstract void Dispose();
}

class B : A
{
    public override void Dispose()
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

MustInherit Class A
    Implements IDisposable

    Public MustOverride Sub Dispose() Implements IDisposable.Dispose
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
    End Sub
End Class");
        }

        [Fact]
        public async Task ShadowsBaseDisposeMethod_NoBaseDisposeCall_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    public new void Dispose()
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Shadows Sub Dispose()
    End Sub
End Class");
        }

        [Fact]
        public async Task Multiple_BaseDisposeCalls_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    public override void Dispose()
    {
        base.Dispose();
        base.Dispose();
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        MyBase.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public async Task BaseDisposeCalls_AllPaths_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    private readonly bool flag;
    public override void Dispose()
    {
        if (flag)
        {
            base.Dispose();
        }
        else
        {
            base.Dispose();
        }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Private ReadOnly flag As Boolean
    Public Overrides Sub Dispose()
        If flag Then
            MyBase.Dispose()
        Else 
            MyBase.Dispose()
        End If
    End Sub
End Class");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1654"), WorkItem(1654, "https://github.com/dotnet/roslyn-analyzers/issues/1654")]
        public async Task BaseDisposeCalls_SomePaths_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    private readonly A a;
    public override void Dispose()
    {
        if (a != null)
        {
            base.Dispose();
        }
    }
}
",
            // Test0.cs(14,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(14, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Private ReadOnly a As A
    Public Overrides Sub Dispose()
        If a IsNot Nothing Then
            MyBase.Dispose()
        End If
    End Sub
End Class",
            // Test0.vb(15,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(15, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact]
        public async Task BaseDisposeCall_GuardedWithBoolField_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    private bool disposed;

    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        base.Dispose();
        disposed = true;
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Private disposed As Boolean

    Public Overrides Sub Dispose()
        If disposed Then
            Return
        End If

        MyBase.Dispose()
        disposed = True
    End Sub
End Class");
        }

        [Fact]
        public async Task BaseDisposeCall_DifferentOverload_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }

    public void Dispose(int i)
    {
    }
}

class B : A
{
    public override void Dispose()
    {
        Dispose(0);
    }
}
",
            // Test0.cs(17,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(17, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub

    Public Overridable Sub Dispose(i As Integer)
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        Dispose(0)
    End Sub
End Class",
            // Test0.vb(17,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(17, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact]
        public async Task DisposeCall_DifferentInstance_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    private readonly A a;
    public override void Dispose()
    {
        a.Dispose();
    }
}
",
            // Test0.cs(14,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(14, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Private ReadOnly a As A
    Public Overrides Sub Dispose()
        a.Dispose()
    End Sub
End Class",
            // Test0.vb(15,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(15, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact]
        public async Task DisposeCall_StaticMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }

    public static void Dispose(bool b)
    {
    }
}

class B : A
{
    public override void Dispose()
    {
        A.Dispose(true);
    }
}
",
            // Test0.cs(17,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(17, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub

    Public Shared Sub Dispose(b As Boolean)
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        A.Dispose(True)
    End Sub
End Class",
            // Test0.vb(17,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(17, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact]
        public async Task DisposeCall_ThisOrMeInstance_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
using System;

class A : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class B : A
{
    public override void Dispose()
    {
        this.Dispose();
    }
}
",
            // Test0.cs(13,26): warning CA2215: Ensure that method 'void B.Dispose()' calls 'base.Dispose()' in all possible control flow paths.
            GetCSharpResultAt(13, 26, "void B.Dispose()", "base.Dispose()"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Class A
    Implements IDisposable

    Public Overridable Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class

Class B
    Inherits A

    Public Overrides Sub Dispose()
        Me.Dispose()
    End Sub
End Class",
            // Test0.vb(14,26): warning CA2215: Ensure that method 'Sub B.Dispose()' calls 'MyBase.Dispose()' in all possible control flow paths.
            GetBasicResultAt(14, 26, "Sub B.Dispose()", "MyBase.Dispose()"));
        }

        [Fact, WorkItem(1671, "https://github.com/dotnet/roslyn-analyzers/issues/1671")]
        public async Task ErrorCase_NoDiagnostic()
        {
            // Missing "using System;" causes "Equals" method be marked as IsOverride but with null OverriddenMethod.
            await Verify.VerifyAnalyzerAsync(@"
public class BaseClass<T> : IComparable<T>
     where T : IComparable<T>
{
    public T Value { get; set; }


    public int CompareTo(T other)
    {
        return Value.CompareTo(other);
    }

    public override bool Equals(object obj)
    {
        if (obj is BaseClass<T> other)
        {
            return Value.Equals(other.Value);
        }

        return false;
    }

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}

public class DerivedClass<T> : BaseClass<T>
    where T : IComparable<T>
{
}
",
            new[]
            {
                // Test0.cs(2,29): error CS0246: The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)
                DiagnosticResult.CompilerError("CS0246").WithLocation(2,29).WithMessage("The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)"),
                // Test0.cs(3,16): error CS0246: The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)
                DiagnosticResult.CompilerError("CS0246").WithLocation(3,16).WithMessage("The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)"),
                // Test0.cs(10,22): error CS1061: 'T' does not contain a definition for 'CompareTo' and no extension method 'CompareTo' accepting a first argument of type 'T' could be found (are you missing a using directive or an assembly reference?)
                DiagnosticResult.CompilerError("CS1061").WithLocation(10,22).WithMessage("'T' does not contain a definition for 'CompareTo' and no extension method 'CompareTo' accepting a first argument of type 'T' could be found (are you missing a using directive or an assembly reference?)"),
                // Test0.cs(13,26): error CS0115: 'BaseClass<T>.Equals(object)': no suitable method found to override
                DiagnosticResult.CompilerError("CS0115").WithLocation(13,26).WithMessage("'BaseClass<T>.Equals(object)': no suitable method found to override"),
                // Test0.cs(23,25): error CS0115: 'BaseClass<T>.GetHashCode()': no suitable method found to override
                DiagnosticResult.CompilerError("CS0115").WithLocation(23,25).WithMessage("'BaseClass<T>.GetHashCode()': no suitable method found to override"),
                // Test0.cs(26,14): error CS0314: The type 'T' cannot be used as type parameter 'T' in the generic type or method 'BaseClass<T>'. There is no boxing conversion or type parameter conversion from 'T' to 'IComparable<T>'.
                DiagnosticResult.CompilerError("CS0314").WithLocation(26,14).WithMessage("The type 'T' cannot be used as type parameter 'T' in the generic type or method 'BaseClass<T>'. There is no boxing conversion or type parameter conversion from 'T' to 'IComparable<T>'."),
                // Test0.cs(27,15): error CS0246: The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)
                DiagnosticResult.CompilerError("CS0246").WithLocation(27,15).WithMessage("The type or namespace name 'IComparable<>' could not be found (are you missing a using directive or an assembly reference?)"),
            });
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.Exp.Reliability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Reliability
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.DisposeAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PointsToAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.NullAnalysis)]
    public partial class DisposeObjectsBeforeLosingScopeTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer() => new DisposeObjectsBeforeLosingScope();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DisposeObjectsBeforeLosingScope();

        private new DiagnosticResult GetCSharpResultAt(int line, int column, string containingMethod, string allocationText) =>
            GetCSharpResultAt(line, column, DisposeObjectsBeforeLosingScope.Rule, containingMethod, allocationText);

        private new DiagnosticResult GetBasicResultAt(int line, int column, string invokedSymbol, string containingMethod) =>
            GetBasicResultAt(line, column, DisposeObjectsBeforeLosingScope.Rule, invokedSymbol, containingMethod);

        [Fact]
        public void LocalWithDisposableInitializer_DisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
        a.Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As New A()
        a.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableInitializer_NoDisposeCall_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
    }
}
",
            // Test0.cs(16,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 17, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As New A()
    End Sub
End Class",
            // Test0.vb(13,18): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 18, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void LocalWithDisposableAssignment_DisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();
        a.Dispose();

        A b = new A();
        a = b;
        a.Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A
        a = New A()
        a.Dispose()

        Dim b As New A()
        a = b
        a.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableAssignment_NoDisposeCall_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();
    }
}
",
            // Test0.cs(17,13): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(17, 13, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A
        a = New A()
    End Sub
End Class",
            // Test0.vb(14,13): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(14, 13, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void ParameterWithDisposableAssignment_DisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A a)
    {
        a = new A();
        a.Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(a As A)
        a = New A()
        a.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void ParameterWithDisposableAssignment_NoDisposeCall_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A a)
    {
        a = new A();
    }
}
",
            // Test0.cs(16,13): warning CA2000: In method 'void Test.M1(A a)', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 13, "void Test.M1(A a)", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(a As A)
        a = New A()
    End Sub
End Class",
            // Test0.vb(13,13): warning CA2000: In method 'Sub Test.M1(a As A)', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 13, "Sub Test.M1(a As A)", "New A()"));
        }

        [Fact]
        public void OutAndRefParametersWithDisposableAssignment_NoDisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(ref A a1, out A a2)
    {
        a1 = new A();
        a2 = new A();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(ByRef a As A)
        a = New A()
    End Sub
End Class");
        }

        [Fact]
        public void OutDisposableArgument_NoDisposeCall_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(out A param)
    {
        param = new A();
    }

    void M2(out A param2)
    {
        M3(out param2);
    }

    void M3(out A param3)
    {
        param3 = new A();
    }

    void Method()
    {
        A a;
        M1(out a);
        A local = a;
        M1(out a);

        M1(out var a2);

        A a3;
        M2(out a3);
    }
}
",
            // Test0.cs(32,12): warning CA2000: In method 'void Test.Method()', call System.IDisposable.Dispose on object created by 'out a' before all references to it are out of scope.
            GetCSharpResultAt(32, 12, "void Test.Method()", "out a"),
            // Test0.cs(34,12): warning CA2000: In method 'void Test.Method()', call System.IDisposable.Dispose on object created by 'out a' before all references to it are out of scope.
            GetCSharpResultAt(34, 12, "void Test.Method()", "out a"),
            // Test0.cs(36,12): warning CA2000: In method 'void Test.Method()', call System.IDisposable.Dispose on object created by 'out var a2' before all references to it are out of scope.
            GetCSharpResultAt(36, 12, "void Test.Method()", "out var a2"),
            // Test0.cs(39,12): warning CA2000: In method 'void Test.Method()', call System.IDisposable.Dispose on object created by 'out a3' before all references to it are out of scope.
            GetCSharpResultAt(39, 12, "void Test.Method()", "out a3"));
        }

        [Fact]
        public void OutDisposableArgument_DisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(out A param)
    {
        param = new A();
    }

    void M2(out A param2)
    {
        M3(out param2);
    }

    void M3(out A param3)
    {
        param3 = new A();
    }

    void Method()
    {
        A a;
        M1(out a);
        A local = a;
        M1(out a);

        M1(out var a2);

        A a3;
        M2(out a3);

        local.Dispose();
        a.Dispose();
        a2.Dispose();
        a3.Dispose();
    }
}
");
        }

        [Fact]
        public void TryGetSpecialCase_OutDisposableArgument_NoDisposeCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class MyCollection
{
    private readonly Dictionary<int, A> _map;
    public MyCollection(Dictionary<int, A> map)
    {
        _map = map;
    }

    public bool ValueExists(int i)
    {
        return _map.TryGetValue(i, out var value);
    }
}
");
        }

        [Fact]
        public void LocalWithMultipleDisposableAssignment_DisposeCallOnSome_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();
        a = new A();
        a.Dispose();
        a = new A();
    }
}
",
            // Test0.cs(17,13): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(17, 13, "void Test.M1()", "new A()"),
            // Test0.cs(20,13): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 13, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A
        a = New A()
        a = New A()
        a.Dispose()
        a = New A()
    End Sub
End Class",
            // Test0.vb(14,13): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(14, 13, "Sub Test.M1()", "New A()"),
            // Test0.vb(17,13): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(17, 13, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void FieldWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    public A a;
    void M1(Test p)
    {
        p.a = new A();

        Test l = new Test();
        l.a = new A();

        this.a = new A();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public a As A
    Sub M1(p As Test)
        p.a = New A()

        Dim l As New Test()
        l.a = New A()

        Me.a = New A()
    End Sub
End Class");
        }

        [Fact]
        public void PropertyWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    public A a { get; set; }
    void M1(Test p)
    {
        p.a = new A();

        Test l = new Test();
        l.a = new A();

        this.a = new A();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public Property a As A
    Sub M1(p As Test)
        p.a = New A()

        Dim l As New Test()
        l.a = New A()

        Me.a = New A()
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableAssignment_DisposeBoolCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Dispose(bool b)
    {
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();
        a.Dispose(true);

        A b = new A();
        a = b;
        a.Dispose(true);
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Sub Dispose(b As Boolean)
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A
        a = New A()
        a.Dispose(true)

        Dim b As New A()
        a = b
        a.Dispose(true)
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableAssignment_CloseCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();
        a.Close();

        A b = new A();
        a = b;
        a.Close();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Sub Close()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A
        a = New A()
        a.Close()

        Dim b As New A()
        a = b
        a.Close()
    End Sub
End Class");
        }

        [Fact, WorkItem(1796, "https://github.com/dotnet/roslyn-analyzers/issues/1796")]
        public void LocalWithDisposableAssignment_DisposeAsyncCall_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Threading.Tasks;

class A : IDisposable
{
    public void Dispose() => DisposeAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}

class Test
{
    async Task M1()
    {
        A a;
        a = new A();
        await a.DisposeAsync();

        A b = new A();
        a = b;
        await a.DisposeAsync();
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks

Class A
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        DisposeAsync()
    End Sub

    Public Function DisposeAsync() As Task
        Return Task.CompletedTask
    End Function
End Class

Class Test
    Async Function M1() As Task
        Dim a As A
        a = New A()
        Await a.DisposeAsync()

        Dim b As New A()
        a = b
        Await a.DisposeAsync()
    End Function
End Class");
        }

        [Fact]
        public void ArrayElementWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A[] a)
    {
        a[0] = new A();     // TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public Property a As A
    Sub M1(a As A())
        a(0) = New A()     ' TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    End Sub
End Class");
        }

        [Fact]
        public void ArrayElementWithDisposableAssignment_ConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A[] a)
    {
        a[0] = new A();
        a[0].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public Property a As A
    Sub M1(a As A())
        a(0) = New A()
        a(0).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void ArrayElementWithDisposableAssignment_NonConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A[] a, int i)
    {
        a[i] = new A();
        a[i].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public Property a As A
    Sub M1(a As A(), i As Integer)
        a(i) = New A()
        a(i).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void ArrayElementWithDisposableAssignment_NonConstantIndex_02_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A[] a, int i, int j)
    {
        a[i] = new A();
        i = j;              // Value of i is now unknown
        a[i].Dispose();     // We don't know the points to value of a[i], so don't flag 'new A()'
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public Property a As A
    Sub M1(a As A(), i As Integer, j As Integer)
        a(i) = New A()
        i = j               ' Value of i is now unknown
        a(i).Dispose()      ' We don't know the points to value of a(i), so don't flag 'New A()'
    End Sub
End Class");
        }

        [Fact]
        public void ArrayInitializer_ElementWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A[] a = new A[] { new A() };   // TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A() = New A() {New A()}    ' TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    End Sub
End Class");
        }

        [Fact]
        public void ArrayInitializer_ElementWithDisposableAssignment_ConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A[] a = new A[] { new A() };
        a[0].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As A() = New A() {New A()}
        a(0).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void ArrayInitializer_ElementWithDisposableAssignment_NonConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(int i)
    {
        A[] a = new A[] { new A() };
        a[i].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(i As Integer)
        Dim a As A() = New A() {New A()}
        a(i).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void CollectionInitializer_ElementWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        List<A> a = new List<A>() { new A() };   // TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As List(Of A) = New List(Of A) From {New A()}    ' TODO: https://github.com/dotnet/roslyn-analyzers/issues/1577
    End Sub
End Class");
        }

        [Fact]
        public void CollectionInitializer_ElementWithDisposableAssignment_ConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        List<A> a = new List<A>() { new A() };
        a[0].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As List(Of A) = New List(Of A) From {New A()}
        a(0).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void CollectionInitializer_ElementWithDisposableAssignment_NonConstantIndex_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(int i)
    {
        List<A> a = new List<A>() { new A() };
        a[i].Dispose();
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(i As Integer)
        Dim a As List(Of A) = New List(Of A) From {New A()}
        a(i).Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void CollectionAdd_SpecialCases_ElementWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class NonGenericList : ICollection
{
    public void Add(A item)
    {
    }

    public int Count => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    public bool IsSynchronized => throw new NotImplementedException();

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        List<A> a = new List<A>();
        a.Add(new A());

        A b = new A();
        a.Add(b);

        NonGenericList l = new NonGenericList();
        l.Add(new A());

        b = new A();
        l.Add(b);
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class NonGenericList
    Implements ICollection

    Public Sub Add(item As A)
    End Sub

    Public ReadOnly Property Count As Integer Implements ICollection.Count
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException()
    End Sub

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function
End Class

Class Test
    Private Sub M1()
        Dim a As New List(Of A)()
        a.Add(New A())

        Dim b As A = New A()
        a.Add(b)

        Dim l As New NonGenericList()
        l.Add(New A())

        b = New A()
        l.Add(b)
    End Sub
End Class");
        }

        [Fact]
        public void CollectionAdd_IReadOnlyCollection_SpecialCases_ElementWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class MyReadOnlyCollection : IReadOnlyCollection<A>
{
    public void Add(A item)
    {
    }
    
    public int Count => throw new NotImplementedException();

    public IEnumerator<A> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var myReadOnlyCollection = new MyReadOnlyCollection();
        myReadOnlyCollection.Add(new A());
        A a = new A();
        myReadOnlyCollection.Add(a);

        var builder = ImmutableArray.CreateBuilder<A>();
        builder.Add(new A());
        A a2 = new A();
        builder.Add(a2);

        var bag = new ConcurrentBag<A>();
        builder.Add(new A());
        A a3 = new A();
        builder.Add(a3);
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class MyReadOnlyCollection
    Implements IReadOnlyCollection(Of A)

    Public Sub Add(ByVal item As A)
    End Sub

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of A).Count
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Function GetEnumerator() As IEnumerator(Of A) Implements IEnumerable(Of A).GetEnumerator
        Throw New NotImplementedException()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function
End Class

Class Test
    Private Sub M1()
        Dim myReadOnlyCollection = New MyReadOnlyCollection()
        myReadOnlyCollection.Add(New A())
        Dim a As A = New A()
        myReadOnlyCollection.Add(a)

        Dim builder = ImmutableArray.CreateBuilder(Of A)()
        builder.Add(New A())
        Dim a2 As A = New A()
        builder.Add(a2)

        Dim bag = New ConcurrentBag(Of A)()
        builder.Add(New A())
        Dim a3 As A = New A()
        builder.Add(a3)
    End Sub
End Class");
        }

        [Fact]
        public void MemberInitializerWithDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public int X;
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    public A a;
    void M1()
    {
        var a = new Test { a = { X = 0 } };
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public X As Integer
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Public a As A
    Sub M1()
        Dim a = New Test With {.a = New A() With { .X = 1 }}
    End Sub
End Class");
        }

        [Fact]
        public void StructImplementingIDisposable_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

struct A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
    }
}
");

            VerifyBasic(@"
Imports System

Structure A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Structure

Class Test
    Sub M1()
        Dim a As New A()
    End Sub
End Class");
        }

        [Fact]
        public void NonUserDefinedConversions_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class Test
{
    void M1()
    {
        object obj = new A();   // Implicit conversion from A to object
        ((A)obj).Dispose();     // Explicit conversion from object to A

        A a = new B();          // Implicit conversion from B to A     
        a.Dispose();        
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class Test
    Sub M1()
        Dim obj As Object = New A()             ' Implicit conversion from A to object
        DirectCast(obj, A).Dispose()            ' Explicit conversion from object to A
        
        Dim a As A = new B()                    ' Implicit conversion from B to A
        a.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void NonUserDefinedConversions_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class Test
{
    void M1()
    {
        object obj = new A();   // Implicit conversion from A to object
        A a = (A)new B();       // Explicit conversion from B to A
    }
}
",
            // Test0.cs(20,22): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 22, "void Test.M1()", "new A()"),
            // Test0.cs(21,18): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new B()' before all references to it are out of scope.
            GetCSharpResultAt(21, 18, "void Test.M1()", "new B()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class Test
    Sub M1()
        Dim obj As Object = New A()             ' Implicit conversion from A to object        
        Dim a As A = DirectCast(New B(), A)     ' Explicit conversion from B to A
    End Sub
End Class",
            // Test0.vb(17,29): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(17, 29, "Sub Test.M1()", "New A()"),
            // Test0.vb(18,33): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New B()' before all references to it are out of scope.
            GetBasicResultAt(18, 33, "Sub Test.M1()", "New B()"));
        }

        [Fact]
        public void UserDefinedConversions_NoDiagnostic()
        {
            VerifyCSharp(@"

using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public static implicit operator A(B value)
    {
        value.Dispose();
        return null;
    }

    public static explicit operator B(A value)
    {
        value.Dispose();
        return null;
    }
}

class B : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    Test(string s)
    {
    }

    void M1()
    {
        A a = new B();      // Implicit user defined conversion
        B b = (B)new A();   // Explicit user defined conversion
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Shared Widening Operator CType(ByVal value As A) As B
        value.Dispose()
        Return Nothing
    End Operator

    Public Shared Widening Operator CType(ByVal value As B) As A
        value.Dispose()
        Return Nothing
    End Operator
End Class

Class B
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Private Sub M1()
        Dim a As A = New B()            ' Implicit user defined conversion
        Dim b As B = CType(New A(), B)  ' Explicit user defined conversion
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableAssignment_ByRefEscape_NoDiagnostic()
        {
            // Local/parameter passed by ref is escaped.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();
        M2(ref a);
    }

    void M2(ref A a)
    {
        a.Dispose();
        a = null;
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim a As New A()
        M2(a)
    End Sub

    Sub M2(ByRef a as A)
        a.Dispose()
        a = Nothing
    End Sub
End Class");
        }

        [Fact]
        public void LocalWithDisposableAssignment_OutRefKind_NotDisposed_Diagnostic()
        {
            // Local/parameter passed as out is not considered escaped.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();
        M2(out a);
    }

    void M2(out A a)
    {
        a = new A();
    }
}
",
            // Test0.cs(16,15): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 15, "void Test.M1()", "new A()"),
            // Test0.cs(17,12): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'out a' before all references to it are out of scope.
            GetCSharpResultAt(17, 12, "void Test.M1()", "out a"));
        }

        [Fact]
        public void LocalWithDefaultOfDisposableAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = default(A);
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As A = Nothing
    End Sub
End Module");
        }

        [Fact]
        public void NullCoalesce_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A a)
    {
        A b = a ?? new A();
        b.Dispose();

        A c = new A();
        A d = c ?? a;
        d.Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(a As A)
        Dim b As A = If(a, New A())
        b.Dispose()

        Dim c As New A()
        Dim d As A = If(c, a)
        d.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void NullCoalesce_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(A a)
    {
        A b = a ?? new A();
        a.Dispose();

        a = new A();
        A c = a ?? new A();
        c.Dispose();
    }
}
",
            // Test0.cs(16,20): warning CA2000: In method 'void Test.M1(A a)', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 20, "void Test.M1(A a)", "new A()"),
            // Test0.cs(20,20): warning CA2000: In method 'void Test.M1(A a)', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 20, "void Test.M1(A a)", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(a As A)
        Dim b As A = If(a, New A())
        a.Dispose()

        a = New A()
        Dim c As A = If(a, New A())
        c.Dispose()
    End Sub
End Class",
            // Test0.vb(13,28): warning CA2000: In method 'Sub Test.M1(a As A)', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 28, "Sub Test.M1(a As A)", "New A()"),
            // Test0.vb(17,28): warning CA2000: In method 'Sub Test.M1(a As A)', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(17, 28, "Sub Test.M1(a As A)", "New A()"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void WhileLoop_DisposeOnBackEdge_NoDiagnostic()
        {
            // Need precise CFG to avoid false reports.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(bool flag)
    {
        A a = new A();
        while (true)
        {
            a.Dispose();
            if (flag)
            {
                break;  // All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            }
            a = new A();
        }
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1(flag As Boolean)
        Dim a As New A()
        While True
            a.Dispose()
            If flag Then
                Exit While    ' All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            End If
            a = New A()
        End While
    End Sub
End Module");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1648")]
        public void WhileLoop_MissingDisposeOnExit_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();
        while (true)
        {
            a.Dispose();
            a = new A();   // This instance will not be disposed on loop exit.
        }
    }
}
",
            // Test0.cs(20,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 17, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()
        While True
            a.Dispose()
            a = New A()   ' This instance will not be disposed on loop exit.
        End While
    End Sub
End Module",
            // Test0.vb(16,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(16, 17, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void WhileLoop_MissingDisposeOnEntry_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;      
        while ((a = new A()) != null)   // This instance will never be disposed.
        {
            a = new A();
            a.Dispose();
        }
    }
}
",
            // Test0.cs(17,21): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(17, 21, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()    ' This instance will never be disposed.
        While True
            a = New A()
            a.Dispose()
        End While
    End Sub
End Module",
            // Test0.vb(13,18): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 18, "Sub Test.M1()", "New A()"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void DoWhileLoop_DisposeOnBackEdge_NoDiagnostic()
        {
            // Need precise CFG to avoid false reports.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(bool flag)
    {
        A a = new A();
        do
        {
            a.Dispose();
            if (flag)
            {
                break;  // All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            }
            a = new A();
        } while (true);
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1(flag As Boolean)
        Dim a As New A()
        Do Until True
            a.Dispose()
            If flag Then
                Exit Do    ' All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            End If
            a = New A()
        Loop
    End Sub
End Module");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1648")]
        public void DoWhileLoop_MissingDisposeOnExit_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();
        do
        {
            a.Dispose();
            a = new A();   // This instance will not be disposed on loop exit.
        } while (true);
    }
}
",
            // Test0.cs(20,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 17, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()
        Do While True
            a.Dispose()
            a = New A()   ' This instance will not be disposed on loop exit.
        Loop
    End Sub
End Module",
            // Test0.vb(16,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(16, 17, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void DoWhileLoop_MissingDisposeOnEntry_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;      
        do
        {
            a = new A();
            a.Dispose();
        } while ((a = new A()) != null);   // This instance will never be disposed.
    }
}
",
            // Test0.cs(21,23): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(21, 23, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()    ' This instance will never be disposed.
        Do While True
            a = New A()
            a.Dispose()
        Loop
    End Sub
End Module",
            // Test0.vb(13,18): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 18, "Sub Test.M1()", "New A()"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1567")]
        public void ForLoop_DisposeOnBackEdge_NoDiagnostic()
        {
            // Need precise CFG to avoid false reports.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(bool flag)
    {
        A a = new A();
        for (int i = 0; i < 10; i++)
        {
            a.Dispose();
            if (flag)
            {
                break;  // All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            }

            a = new A();
        }
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1(flag As Boolean)
        Dim a As New A()
        For i As Integer = 0 To 10
            a.Dispose()
            If flag Then
                Exit For    ' All 'A' instances have been disposed on this path, so no diagnostic should be reported.
            End If
            a = New A()
        Next
    End Sub
End Module");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1648")]
        public void ForLoop_MissingDisposeOnExit_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();
        for (int i = 0; i < 10; i++)
        {
            a.Dispose();
            a = new A();   // This instance will not be disposed on loop exit.
        }
    }
}
",
            // Test0.cs(20,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 17, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()
        For i As Integer = 0 To 10
            a.Dispose()
            a = New A()   ' This instance will not be disposed on loop exit.
        Next
    End Sub
End Module",
            // Test0.vb(16,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(16, 17, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void ForLoop_MissingDisposeOnEntry_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        int i;
        for (i = 0, a = new A(); i < 10; i++)   // This 'A' instance will never be disposed.
        {
            a = new A();
            a.Dispose();
        }
    }
}
",
            // Test0.cs(18,25): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(18, 25, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()    ' This instance will never be disposed.
        For i As Integer = 0 To 10
            a = New A()
            a.Dispose()
        Next
    End Sub
End Module",
            // Test0.vb(13,18): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 18, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void IfStatement_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class Test
{
    void M1(A a, string param)
    {
        A a1 = new A();
        B a2 = new B();
        A b;
        if (param != null)
        {
            a = a1;
            b = new B();
        }
        else 
        {
            a = a2;
            b = new A();
        }
        
        a.Dispose();         // a points to either a1 or a2.
        b.Dispose();         // b points to either instance created in if or else.
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class Test
    Private Sub M1(a As A, param As String)
        Dim a1 As New A()
        Dim a2 As B = new B()
        Dim b As A
        If param IsNot Nothing Then
            a = a1
            b = new B()
        Else
            a = a2
            b = new A()
        End If
        
        a.Dispose()          ' a points to either a1 or a2.
        b.Dispose()          ' b points to either instance created in if or else.
    End Sub
End Class");
        }

        [Fact]
        public void IfStatement_02_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class Test
{
    void M1(A a, string param, string param2)
    {
        A a1 = new A();
        B a2 = new B();
        A b;
        if (param != null)
        {
            a = a1;
            b = new B();

            if (param == """")
            {
                a = new B();
            }
            else
            {
                if (param2 != null)
                {
                    b = new A();
                }
                else
                {
                    b = new B();
                }
            }
        }
        else 
        {
            a = a2;
            b = new A();
        }
        
        a.Dispose();         // a points to either a1 or a2 or instance created in 'if(param == """")'.
        b.Dispose();         // b points to either instance created in outer if or outer else or innermost if or innermost else.
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class Test
    Private Sub M1(a As A, param As String, param2 As String)
        Dim a1 As New A()
        Dim a2 As B = new B()
        Dim b As A
        If param IsNot Nothing Then
            a = a1
            b = new B()
            If param = """" Then
                a = new B()
            Else
                If param2 IsNot Nothing Then
                    b = new A()
                Else
                    b = new B()
                End If
            End If
        Else
            a = a2
            b = new A()
        End If

        a.Dispose()          ' a points to either a1 or a2 or instance created in 'if(param == """")'.
        b.Dispose()          ' b points to either instance created in outer if or outer else or innermost if or innermost else.
    End Sub
End Class");
        }

        [Fact]
        public void IfStatement_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class C : B
{
}

class D : C
{
}

class E : D
{
}

class Test
{
    void M1(A a, string param, string param2)
    {
        A a1 = new A();     // Maybe disposed.
        B a2 = new B();     // Never disposed.
        A b;
        if (param != null)
        {
            a = a1;
            b = new C();     // Never disposed.
        }
        else
        {
            a = a2;
            b = new D();     // Never disposed.
        }
        
        // a points to either a1 or a2.
        // b points to either instance created in if or else.

        if (param != null)
        {
            A c = new A();
            a = c;
            b = a1;
        }
        else 
        {
            C d = new E();
            b = d;
            a = b;
        }

        a.Dispose();         // a points to either c or d.
        b.Dispose();         // b points to either a1 or d.
    }
}
",
            // Test0.cs(33,16): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new B()' before all references to it are out of scope.
            GetCSharpResultAt(33, 16, "void Test.M1(A a, string param, string param2)", "new B()"),
            // Test0.cs(38,17): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new C()' before all references to it are out of scope.
            GetCSharpResultAt(38, 17, "void Test.M1(A a, string param, string param2)", "new C()"),
            // Test0.cs(43,17): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new D()' before all references to it are out of scope.
            GetCSharpResultAt(43, 17, "void Test.M1(A a, string param, string param2)", "new D()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class C
    Inherits B
End Class

Class D
    Inherits C
End Class

Class E
    Inherits D
End Class

Class Test
    Private Sub M1(ByVal a As A, ByVal param As String, ByVal param2 As String)
        Dim a1 As A = New A()   ' Maybe disposed.
        Dim a2 As B = New B()   ' Never disposed.
        Dim b As A

        If param IsNot Nothing Then
            a = a1
            b = New C()     ' Never disposed.
        Else
            a = a2
            b = New D()     ' Never disposed.
        End If

        ' a points to either a1 or a2.
        ' b points to either instance created in if or else.

        If param IsNot Nothing Then
            Dim c As A = New A()
            a = c
            b = a1
        Else
            Dim d As C = New E()
            b = d
            a = b
        End If

        a.Dispose()         ' a points to either c or d.
        b.Dispose()         ' b points to either a1 or d.
    End Sub
End Class",
            // Test0.vb(30,23): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New B()' before all references to it are out of scope.
            GetBasicResultAt(30, 23, "Sub Test.M1(a As A, param As String, param2 As String)", "New B()"),
            // Test0.vb(35,17): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New C()' before all references to it are out of scope.
            GetBasicResultAt(35, 17, "Sub Test.M1(a As A, param As String, param2 As String)", "New C()"),
            // Test0.vb(38,17): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New D()' before all references to it are out of scope.
            GetBasicResultAt(38, 17, "Sub Test.M1(a As A, param As String, param2 As String)", "New D()"));
        }

        [Fact]
        public void IfStatement_02_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

class C : B
{
}

class D : C
{
}

class E : D
{
}

class Test
{
    void M1(A a, string param, string param2)
    {
        A a1 = new B();     // Never disposed
        B a2 = new C();     // Never disposed
        A b;
        if (param != null)
        {
            a = a1;
            b = new A();     // Maybe disposed

            if (param == """")
            {
                a = new D();     // Never disposed
            }
            else
            {
                if (param2 != null)
                {
                    b = new A();    // Maybe disposed
                }
                else
                {
                    b = new A();    // Maybe disposed
                    if (param == """")
                    {
                        b = new A();    // Maybe disposed
                    }
                }
                
                if (param2 == """")
                {
                    b.Dispose();    // b points to one of the three instances of A created above.
                    b = new A();    // Always disposed
                }
            }
        }
        else 
        {
            a = a2;
            b = new A();        // Maybe disposed
            if (param2 != null)
            {
                a = new A();    // Always disposed
            }
            else
            {
                a = new A();    // Always disposed
                b = new A();    // Always disposed
            }

            a.Dispose();
        }

        b.Dispose();         
    }
}
",
            // Test0.cs(32,16): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new B()' before all references to it are out of scope.
            GetCSharpResultAt(32, 16, "void Test.M1(A a, string param, string param2)", "new B()"),
            // Test0.cs(33,16): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new C()' before all references to it are out of scope.
            GetCSharpResultAt(33, 16, "void Test.M1(A a, string param, string param2)", "new C()"),
            // Test0.cs(42,21): warning CA2000: In method 'void Test.M1(A a, string param, string param2)', call System.IDisposable.Dispose on object created by 'new D()' before all references to it are out of scope.
            GetCSharpResultAt(42, 21, "void Test.M1(A a, string param, string param2)", "new D()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Class C
    Inherits B
End Class

Class D
    Inherits C
End Class

Class E
    Inherits D
End Class

Class Test

    Private Sub M1(ByVal a As A, ByVal param As String, ByVal param2 As String)
        Dim a1 As A = New B()       ' Never disposed
        Dim a2 As B = New C()       ' Never disposed
        Dim b As A
        If param IsNot Nothing Then
            a = a1
            b = New A()       ' Always disposed
            If param = """" Then
                a = New D()       ' Never disposed
            Else
                If param2 IsNot Nothing Then
                    b = New A()       ' Maybe disposed
                Else
                    b = New A()       ' Maybe disposed
                    If param = """" Then
                        b = New A()   ' Maybe disposed
                    End If
                End If

                If param2 = """" Then
                    b.Dispose()     ' b points to one of the three instances of A created above.
                    b = New A()     ' Always disposed
                End If
            End If
        Else
            a = a2
            b = New A()       ' Maybe disposed
            If param2 IsNot Nothing Then
                a = New A()       ' Always disposed
            Else
                a = New A()       ' Always disposed
                b = New A()       ' Always disposed
            End If

            a.Dispose()
        End If

        b.Dispose()
    End Sub
End Class",
                // Test0.vb(30,23): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New B()' before all references to it are out of scope.
                GetBasicResultAt(30, 23, "Sub Test.M1(a As A, param As String, param2 As String)", "New B()"),
                // Test0.vb(31,23): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New C()' before all references to it are out of scope.
                GetBasicResultAt(31, 23, "Sub Test.M1(a As A, param As String, param2 As String)", "New C()"),
                // Test0.vb(37,21): warning CA2000: In method 'Sub Test.M1(a As A, param As String, param2 As String)', call System.IDisposable.Dispose on object created by 'New E()' before all references to it are out of scope.
                GetBasicResultAt(37, 21, "Sub Test.M1(a As A, param As String, param2 As String)", "New D()"));
        }

        [Fact]
        public void UsingStatement_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        using (var a = new A())
        {
        }

        A b;
        using (b = new A())
        {
        }

        using (A c = new A(), d = new A())
        {
        }

        A e = new A();
        using (e)
        {
        }

        using (A f = null)
        {
        }
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Private Sub M1()
        Using a As New A()
        End Using

        Dim b As A = New A()
        Using b
        End Using

        Using c As New A(), d = New A()
        End Using

        Using a As A = Nothing
        End Using
    End Sub
End Class");
        }

        [Fact]
        public void ReturnStatement_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    A M1()
    {
        return new A();
    }

    A M2(A a)
    {
        a = new A();
        return a;
    }

    A M3(A a)
    {
        a = new A();
        A b = a;
        return b;
    }

    A M4(A a) => new A();

    IEnumerable<A> M5()
    {
        yield return new A();
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Private Function M1() As A
        Return New A()
    End Function

    Private Function M2(a As A) As A
        a = New A()
        Return a
    End Function

    Private Function M3(a As A) As A
        a = New A()
        Dim b = a
        Return b
    End Function

    Public Iterator Function M4() As IEnumerable(Of A)
        Yield New A
    End Function
End Class");
        }

        [Fact]
        public void ReturnStatement_02_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : I, IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

interface I
{
}

class Test
{
    I M1()
    {
        return new A();
    }

    I M2()
    {
        return new A() as I;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections.Generic

Class A
    Implements I, IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Interface I
End Interface

Class Test

    Private Function M1() As I
        Return New A()
    End Function

    Private Function M2() As I
        Return TryCast(New A(), I)
    End Function
End Class");
        }

        [Fact]
        public void LocalFunctionInvocation_EmptyBody_Diagnostic()
        {
            // Currently we do not generate a diagnostic as we do not analyze local function invocations and pessimistically assume it invalidates all saved state.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();

        void MyLocalFunction()
        {
        };

        MyLocalFunction();    // This should not change state of 'a' if we analyzed the local function.
    }
}
");

            // VB has no local functions.
        }

        [Fact]
        public void LocalFunctionInvocation_DisposesCapturedValue_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a = new A();

        void MyLocalFunction()
        {
            a.Dispose();
        };

        MyLocalFunction();    // This should change state of 'a' to be Disposed.
    }
}
");

            // VB has no local functions.
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1571")]
        public void LocalFunctionInvocation_CapturedValueAssignedNewDisposable_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;

        void MyLocalFunction()
        {
            a = new A();
        };

        MyLocalFunction();    // If we analyzed the local function, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    }
}
");

            // VB has no local functions.
        }

        [Fact]
        public void LocalFunctionInvocation_ChangesCapturedValueContextSensitive_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;

        void MyLocalFunction(A b)
        {
            a = b;
        };

        MyLocalFunction(new A());    // If we analyzed the local function, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    }
}
");

            // VB has no local functions.
        }

        [Fact]
        public void LambdaInvocation_EmptyBody_Diagnostic()
        {
            // Currently we do not generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;
        a = new A();

        System.Action myLambda = () =>
        {
        };

        myLambda();    // This should not change state of 'a' if we analyzed the lambda invocation.
    }
}
");

            // Currently we do not generate a diagnostic as we do not analyze lambda invocations and pessimistically assume it invalidates all saved state.
            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As A
        a = new A()

        Dim myLambda As System.Action = Sub()
                                        End Sub

        myLambda()      ' This should not change state of 'a' if we analyzed the lambda invocation.
    End Sub
End Module");
        }

        [Fact]
        public void LambdaInvocation_DisposesCapturedValue_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{

    void M1()
    {
        A a = new A();

        System.Action myLambda = () =>
        {
            a.Dispose();
        };

        myLambda();    // This should change state of 'a' to be Disposed.
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As New A()

        Dim myLambda As System.Action = Sub()
                                            a.Dispose()
                                        End Sub

        myLambda()      '  This should change state of 'a' to be Disposed.
    End Sub
End Module");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1571")]
        public void LambdaInvocation_CapturedValueAssignedNewDisposable_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{

    void M1()
    {
        A a;

        System.Action myLambda = () =>
        {
            a = new A();
        };

        myLambda();    // If we analyzed the lambda invocation, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As A

        Dim myLambda As System.Action = Sub()
                                            a = New A()
                                        End Sub

        myLambda()      '  If we analyzed the lambda invocation, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    End Sub
End Module");
        }

        [Fact]
        public void LambdaInvocation_ChangesCapturedValueContextSensitive_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        A a;

        System.Action<A> myLambda = b =>
        {
            a = b;
        };

        myLambda(new A());    // If we analyzed the lambda invocation, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Module Test
    Sub M1()
        Dim a As A

        Dim myLambda As System.Action(Of A) = Sub(b As A)
                                                a = b
                                              End Sub

        myLambda(New A())      '  If we analyzed the lambda invocation, this should change state of 'a' to be NotDisposed and fire a diagnostic.
    End Sub
End Module");
        }

        [Fact]
        public void PointsTo_ReferenceTypeCopyDisposed_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    public string Field;
    void M1(A a)
    {
        a = new A();
        A b = a;
        b.Dispose();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1(a As A)
        a = New A()
        Dim b As A = a
        b.Dispose()
    End Sub
End Class");
        }

        [Fact]
        public void DynamicObjectCreation_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public A(int i)
    {
    }
    public A(string s)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(dynamic d)
    {
        A a = new A(d);
    }
}
",
            // Test0.cs(23,15): warning CA2000: In method 'void Test.M1(dynamic d)', call System.IDisposable.Dispose on object created by 'new A(d)' before all references to it are out of scope.
            GetCSharpResultAt(23, 15, "void Test.M1(dynamic d)", "new A(d)"));
        }

        [Fact]
        public void DynamicObjectCreation_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public A(int i)
    {
    }
    public A(string s)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(dynamic d)
    {
        A a = new A(d);
        a.Dispose();
    }
}
");
        }

        [Fact]
        public void SpecialDisposableObjectCreationApis_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;

class Test
{
    void M1(string filePath, FileMode fileMode)
    {
        var file = File.Open(filePath, fileMode);
        var file2 = File.CreateText(filePath);
    }
}
",
            // Test0.cs(9,20): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'File.Open(filePath, fileMode)' before all references to it are out of scope.
            GetCSharpResultAt(9, 20, "void Test.M1(string filePath, FileMode fileMode)", "File.Open(filePath, fileMode)"),
            // Test0.cs(10,21): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'File.CreateText(filePath)' before all references to it are out of scope.
            GetCSharpResultAt(10, 21, "void Test.M1(string filePath, FileMode fileMode)", "File.CreateText(filePath)"));

            VerifyBasic(@"
Imports System
Imports System.IO

Class Test
    Private Sub M1(filePath As String, fileMode As FileMode)
        Dim f = File.Open(filePath, fileMode)
        Dim f2 = File.CreateText(filePath)
    End Sub
End Class
",
            // Test0.vb(7,17): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'File.Open(filePath, fileMode)' before all references to it are out of scope.
            GetBasicResultAt(7, 17, "Sub Test.M1(filePath As String, fileMode As FileMode)", "File.Open(filePath, fileMode)"),
            // Test0.vb(8,18): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'File.CreateText(filePath)' before all references to it are out of scope.
            GetBasicResultAt(8, 18, "Sub Test.M1(filePath As String, fileMode As FileMode)", "File.CreateText(filePath)"));
        }

        [Fact]
        public void SpecialDisposableObjectCreationApis_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;

class Test
{
    void M1(string filePath, FileMode fileMode)
    {
        var file = File.Open(filePath, fileMode);
        file.Dispose();

        using (var file2 = File.CreateText(filePath))
        {
        }
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.IO

Class Test
    Private Sub M1(filePath As String, fileMode As FileMode)
        Dim f = File.Open(filePath, fileMode)
        f.Dispose()

        Using f2 = File.CreateText(filePath)
        End Using
    End Sub
End Class
");
        }

        [Fact]
        public void InvocationInstanceReceiverOrArgument_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void M()
    {
    }
}

class Test
{
    void M1(string param)
    {
        A a = new A();
        a.M();      // Invoking a method on disposable instance doesn't invalidate Dispose state.

        M2(a);      // Passing the disposable instance as an argument doesn't invalidate Dispose state.
    }

    void M2(A a)
    {
    }
}
",

            // Test0.cs(20,15): warning CA2000: In method 'void Test.M1(string param)', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(20, 15, "void Test.M1(string param)", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Sub M()
    End Sub
End Class

Class Test
    Private Sub M1(ByVal param As String)
        Dim a = New A()
        a.M()       ' Invoking a method on disposable instance doesn't invalidate Dispose state.

        M2(a)       ' Passing the disposable instance as an argument doesn't invalidate Dispose state.
    End Sub

    Public Sub M2(a As A)
    End Sub
End Class",
            // Test0.vb(16,17): warning CA2000: In method 'Sub Test.M1(param As String)', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(16, 17, "Sub Test.M1(param As String)", "New A()"));
        }

        [Fact]
        public void DisposableCreationInArgument_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        M2(new A());      // Passing the disposable instance as an argument doesn't invalidate Dispose state.
    }

    void M2(A a)
    {
    }
}
",

            // Test0.cs(16,12): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 12, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Private Sub M1()
        M2(New A())       ' Passing the disposable instance as an argument doesn't invalidate Dispose state.
    End Sub

    Public Sub M2(a As A)
    End Sub
End Class",
            // Test0.vb(13,12): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(13, 12, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void DisposableCreationNotAssignedToAVariable_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public int X;
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void M()
    {
    }
}

class Test
{
    void M1()
    {
        new A();
        new A().M();
        var x = new A().X;
    }
}
",
            // Test0.cs(21,9): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(21, 9, "void Test.M1()", "new A()"),
            // Test0.cs(22,9): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(22, 9, "void Test.M1()", "new A()"),
            // Test0.cs(23,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(23, 17, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public X As Integer
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

    Public Sub M()
    End Sub
End Class

Class Test
    Private Sub M1()
        New A()
        New A().M()
        Dim x = New A().X
    End Sub
End Class", TestValidationMode.AllowCompileErrors,
            // Test0.vb(19,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(19, 17, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void DisposableCreationPassedToDisposableConstructor_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : IDisposable
{
    private readonly A _a;
    public B(A a)
    {
        _a = a;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var b = new B(new A());
        b.Dispose();

        var a = new A();
        B b2 = null;
        try
        {
            b2 = new B(a);
        }
        finally
        {
            if (b2 != null)
            {
                b2.Dispose();
            }
        }

        var a2 = new A();
        B b3 = null;
        try
        {
            b3 = new B(a2);
        }
        finally
        {
            if (b3 != null)
            {
                b3.Dispose();
            }
        }
    }
}
",
            // Test0.cs(30,23): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(30, 23, "void Test.M1()", "new A()"),
            // Test0.cs(33,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(33, 17, "void Test.M1()", "new A()"),
            // Test0.cs(47,18): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(47, 18, "void Test.M1()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable

    Public X As Integer
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Implements IDisposable

    Private ReadOnly _a As A
    Public Sub New(ByVal a As A)
        _a = a
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Private Sub M1()
        Dim b = New B(New A())
        b.Dispose()
        Dim a = New A()
        Dim b2 As B = Nothing
        Try
            b2 = New B(a)
        Finally
            If b2 IsNot Nothing Then
                b2.Dispose()
            End If
        End Try

        Dim a2 = New A()
        Dim b3 As B = Nothing
        Try
            b3 = New B(a2)
        Finally
            If b3 IsNot Nothing Then
                b3.Dispose()
            End If
        End Try
    End Sub
End Class
",
            // Test0.vb(28,23): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(28, 23, "Sub Test.M1()", "New A()"),
            // Test0.vb(30,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(30, 17, "Sub Test.M1()", "New A()"),
            // Test0.vb(40,18): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(40, 18, "Sub Test.M1()", "New A()"));
        }

        [Fact]
        public void DisposableCreationPassedToDisposableConstructor_SpecialCases_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Resources;

class A : IDisposable
{
    public A(Stream a)
    {
    }

    public A(TextReader t)
    {
    }

    public A(TextWriter t)
    {
    }

    public A(IResourceReader r)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(string filePath, FileMode fileMode)
    {
        Stream stream = new FileStream(filePath, fileMode);
        A a = null;
        try
        {
            a = new A(stream);
        }
        catch(IOException)
        {
            stream.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        TextReader reader = File.OpenText(filePath);
        a = null;
        try
        {
            a = new A(reader);
        }
        catch (IOException)
        {
            reader.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        TextWriter writer = File.CreateText(filePath);
        a = null;
        try
        {
            a = new A(writer);
        }
        catch (IOException)
        {
            writer.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        stream = new FileStream(filePath, fileMode);
        ResourceReader resourceReader = null;
        a = null;
        try
        {
            resourceReader = new ResourceReader(stream);
            a = new A(resourceReader);
        }
        catch (IOException)
        {
            if (resourceReader != null)
            {
                resourceReader.Dispose();
            }
            else
            {
                stream.Dispose();
            }
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }
    }
}
");

            VerifyBasic(@"

Imports System
Imports System.IO
Imports System.Resources

Class A
    Implements IDisposable

    Public Sub New(a As Stream)
    End Sub

    Public Sub New(t As TextReader)
    End Sub

    Public Sub New(t As TextWriter)
    End Sub

    Public Sub New(r As IResourceReader)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test

    Private Sub M1(filePath As String, fileMode As FileMode)
        Dim stream As Stream = New FileStream(filePath, fileMode)
        Dim a As A = Nothing
        Try
            a = New A(stream)
        Catch ex As IOException
            stream.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        Dim reader As TextReader = File.OpenText(filePath)
        a = Nothing
        Try
            a = New A(reader)
        Catch ex As IOException
            reader.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        Dim writer As TextWriter = File.CreateText(filePath)
        a = Nothing
        Try
            a = New A(writer)
        Catch ex As IOException
            writer.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        stream = New FileStream(filePath, fileMode)
        Dim resourceReader As ResourceReader = Nothing
        a = Nothing
        Try
            resourceReader = New ResourceReader(stream)
            a = New A(resourceReader)
        Catch ex As IOException
            If resourceReader IsNot Nothing Then
                resourceReader.Dispose()
            Else
                stream.Dispose()
            End If

        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try
    End Sub
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1580")]
        public void DisposableCreationPassedToDisposableConstructor_SpecialCases_ExceptionPath_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Resources;

class A : IDisposable
{
    public A(Stream a)
    {
    }

    public A(TextReader t)
    {
    }

    public A(TextWriter t)
    {
    }

    public A(IResourceReader r)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1(string filePath, FileMode fileMode)
    {
        Stream stream = new FileStream(filePath, fileMode);
        A a = null;
        try
        {
            a = new A(stream);
        }
        catch(IOException)
        {
            stream.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        TextReader reader = File.OpenText(filePath);
        a = null;
        try
        {
            a = new A(reader);
        }
        catch (IOException)
        {
            reader.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        TextWriter writer = File.CreateText(filePath);
        a = null;
        try
        {
            a = new A(writer);
        }
        catch (IOException)
        {
            writer.Dispose();
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }

        stream = new FileStream(filePath, fileMode);
        ResourceReader resourceReader = null;
        a = null;
        try
        {
            resourceReader = new ResourceReader(stream);
            a = new A(resourceReader);
        }
        catch (IOException)
        {
            if (resourceReader != null)
            {
                resourceReader.Dispose();
            }
            else
            {
                stream.Dispose();
            }
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }
    }
}
",
            // Test0.cs(34,25): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'new FileStream(filePath, fileMode)' before all references to it are out of scope.
            GetCSharpResultAt(34, 25, "void Test.M1(string filePath, FileMode fileMode)", "new FileStream(filePath, fileMode)"),
            // Test0.cs(52,29): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'File.OpenText(filePath)' before all references to it are out of scope.
            GetCSharpResultAt(52, 29, "void Test.M1(string filePath, FileMode fileMode)", "File.OpenText(filePath)"),
            // Test0.cs(70,29): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'File.CreateText(filePath)' before all references to it are out of scope.
            GetCSharpResultAt(70, 29, "void Test.M1(string filePath, FileMode fileMode)", "File.CreateText(filePath)"),
            // Test0.cs(88,18): warning CA2000: In method 'void Test.M1(string filePath, FileMode fileMode)', call System.IDisposable.Dispose on object created by 'new FileStream(filePath, fileMode)' before all references to it are out of scope.
            GetCSharpResultAt(88, 18, "void Test.M1(string filePath, FileMode fileMode)", "new FileStream(filePath, fileMode)"));

            VerifyBasic(@"

Imports System
Imports System.IO
Imports System.Resources

Class A
    Implements IDisposable

    Public Sub New(a As Stream)
    End Sub

    Public Sub New(t As TextReader)
    End Sub

    Public Sub New(t As TextWriter)
    End Sub

    Public Sub New(r As IResourceReader)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test

    Private Sub M1(filePath As String, fileMode As FileMode)
        Dim stream As Stream = New FileStream(filePath, fileMode)
        Dim a As A = Nothing
        Try
            a = New A(stream)
        Catch ex As IOException
            stream.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        Dim reader As TextReader = File.OpenText(filePath)
        a = Nothing
        Try
            a = New A(reader)
        Catch ex As IOException
            reader.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        Dim writer As TextWriter = File.CreateText(filePath)
        a = Nothing
        Try
            a = New A(writer)
        Catch ex As IOException
            writer.Dispose()
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try

        stream = New FileStream(filePath, fileMode)
        Dim resourceReader As ResourceReader = Nothing
        a = Nothing
        Try
            resourceReader = New ResourceReader(stream)
            a = New A(resourceReader)
        Catch ex As IOException
            If resourceReader IsNot Nothing Then
                resourceReader.Dispose()
            Else
                stream.Dispose()
            End If

        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try
    End Sub
End Class
",
            // Test0.vb(30,32): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'New FileStream(filePath, fileMode)' before all references to it are out of scope.
            GetBasicResultAt(30, 32, "Sub Test.M1(filePath As String, fileMode As FileMode)", "New FileStream(filePath, fileMode)"),
            // Test0.vb(42,36): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'File.OpenText(filePath)' before all references to it are out of scope.
            GetBasicResultAt(42, 36, "Sub Test.M1(filePath As String, fileMode As FileMode)", "File.OpenText(filePath)"),
            // Test0.vb(54,36): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'File.CreateText(filePath)' before all references to it are out of scope.
            GetBasicResultAt(54, 36, "Sub Test.M1(filePath As String, fileMode As FileMode)", "File.CreateText(filePath)"),
            // Test0.vb(66,18): warning CA2000: In method 'Sub Test.M1(filePath As String, fileMode As FileMode)', call System.IDisposable.Dispose on object created by 'New FileStream(filePath, fileMode)' before all references to it are out of scope.
            GetBasicResultAt(66, 18, "Sub Test.M1(filePath As String, fileMode As FileMode)", "New FileStream(filePath, fileMode)"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1580")]
        public void DisposableObjectNotDisposed_ExceptionPath_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
        ThrowException();   // a not disposed on exception path.
        a.Dispose();
    }

    void M2()
    {
        var a = new A();
        try
        {
            ThrowException();
            a.Dispose();
        }
        catch (Exception)
        {
            // a not disposed on this path.
        }
    }

    void M3()
    {
        var a = new A();
        try
        {
            ThrowException();
            a.Dispose();
        }
        catch (System.IO.IOException)
        {
            a.Dispose();
            // a not disposed on path with other exceptions.
        }
    }

    void ThrowException()
    {
        throw new NotImplementedException();
    }
}",
            // Test0.cs(16,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 17, "void Test.M1()", "new A()"),
            // Test0.cs(23,17): warning CA2000: In method 'void Test.M2()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(23, 17, "void Test.M2()", "new A()"),
            // Test0.cs(37,17): warning CA2000: In method 'void Test.M3()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(37, 17, "void Test.M3()", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test

    Private Sub M1()
        Dim a = New A()
        ThrowException()    ' a not disposed on exception path.
        a.Dispose()
    End Sub

    Private Sub M2()
        Dim a = New A()
        Try
            ThrowException()
            a.Dispose()
        Catch ex As Exception
            ' a not disposed on this path.
        End Try
    End Sub

    Private Sub M3()
        Dim a = New A()
        Try
            ThrowException()
            a.Dispose()
        Catch ex As System.IO.IOException
            a.Dispose()
            ' a not disposed on path with other exceptions.
        End Try
    End Sub

    Private Sub ThrowException()
        Throw New NotImplementedException()
    End Sub
End Class
",
            // Test0.vb(15,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(15, 17, "Sub Test.M1()", "New A()"),
            // Test0.vb(21,17): warning CA2000: In method 'Sub Test.M2()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(21, 17, "Sub Test.M2()", "New A()"),
            // Test0.vb(31,17): warning CA2000: In method 'Sub Test.M3()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(31, 17, "Sub Test.M3()", "New A()"));
        }

        [Fact]
        public void DisposableObjectOnlyDisposedOnExceptionPath_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        catch (Exception)
        {
            a.Dispose();
        }
    }

    void M2()
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        catch (System.IO.IOException)
        {
            a.Dispose();
        }
    }

    void M3()
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        catch (System.IO.IOException)
        {
            a.Dispose();
        }
        catch (Exception)
        {
            a.Dispose();
        }
    }

    void M4(bool flag)
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        catch (System.IO.IOException)
        {
            if (flag)
            {
                a.Dispose();
            }
        }
    }

    void ThrowException()
    {
        throw new NotImplementedException();
    }
}",
            // Test0.cs(16,17): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 17, "void Test.M1()", "new A()"),
            // Test0.cs(29,17): warning CA2000: In method 'void Test.M2()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(29, 17, "void Test.M2()", "new A()"),
            // Test0.cs(42,17): warning CA2000: In method 'void Test.M3()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(42, 17, "void Test.M3()", "new A()"),
            // Test0.cs(59,17): warning CA2000: In method 'void Test.M4(bool flag)', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(59, 17, "void Test.M4(bool flag)", "new A()"));

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test

    Private Sub M1()
        Dim a = New A()
        Try
            ThrowException()
        Catch ex As Exception
            a.Dispose()
        End Try
    End Sub

    Private Sub M2()
        Dim a = New A()
        Try
            ThrowException()
        Catch ex As System.IO.IOException
            a.Dispose()
        End Try
    End Sub

    Private Sub M3()
        Dim a = New A()
        Try
            ThrowException()
        Catch ex As System.IO.IOException
            a.Dispose()
        Catch ex As Exception
            a.Dispose()
        End Try
    End Sub

    Private Sub M4(flag As Boolean)
        Dim a = New A()
        Try
            ThrowException()
        Catch ex As System.IO.IOException
            If flag Then
                a.Dispose()
            End If
        End Try
    End Sub

    Private Sub ThrowException()
        Throw New NotImplementedException()
    End Sub
End Class
",
            // Test0.vb(15,17): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(15, 17, "Sub Test.M1()", "New A()"),
            // Test0.vb(24,17): warning CA2000: In method 'Sub Test.M2()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(24, 17, "Sub Test.M2()", "New A()"),
            // Test0.vb(33,17): warning CA2000: In method 'Sub Test.M3()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(33, 17, "Sub Test.M3()", "New A()"),
            // Test0.vb(44,17): warning CA2000: In method 'Sub Test.M4(flag As Boolean)', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(44, 17, "Sub Test.M4(flag As Boolean)", "New A()"));
        }

        [Fact]
        public void DisposableObjectDisposed_FinallyPath_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        finally
        {
            a.Dispose();
        }
    }

    void M2()
    {
        var a = new A();
        try
        {
            ThrowException();
        }
        catch (Exception)
        {
        }
        finally
        {
            a.Dispose();
        }
    }

    void M3()
    {
        var a = new A();
        try
        {
            ThrowException();   
            a.Dispose();
            a = null;
        }
        catch (System.IO.IOException)
        {
        }
        finally
        {
            if (a != null)
            {
                a.Dispose();
            }
        }
    }

    void ThrowException()
    {
        throw new NotImplementedException();
    }
}");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test

    Private Sub M1()
        Dim a = New A()
        Try
            ThrowException()
        Finally
            a.Dispose()
        End Try
    End Sub

    Private Sub M2()
        Dim a = New A()
        Try
            ThrowException()
        Catch ex As Exception
        Finally
            a.Dispose()
        End Try
    End Sub

    Private Sub M3()
        Dim a = New A()
        Try
            ThrowException()
            a.Dispose()
            a = Nothing
        Catch ex As System.IO.IOException
        Finally
            If a IsNot Nothing Then
                a.Dispose()
            End If
        End Try
    End Sub

    Private Sub ThrowException()
        Throw New NotImplementedException()
    End Sub
End Class
");
        }

        [Fact, WorkItem(1597, "https://github.com/dotnet/roslyn-analyzers/issues/1597")]
        public void DisposableObjectInErrorCode_NotDisposed_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : IDisposable
{
    public void Dispose()
    {
        A x = new A();
        = x
    }
}
", TestValidationMode.AllowCompileErrors,
            // Test0.cs(16,15): warning CA2000: In method 'void B.Dispose()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 15, "void B.Dispose()", "new A()"));
        }

        [Fact, WorkItem(1597, "https://github.com/dotnet/roslyn-analyzers/issues/1597")]
        public void DisposableObjectInErrorCode_02_NotDisposed_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Text;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        var builder = new StringBuilder();
        using ()        // This erroneous code used to cause a null reference exception in the analysis.
        this.WriteTo(new StringWriter(builder));
        return;
    }

    void WriteTo(StringWriter x)
    {
    }
}
", TestValidationMode.AllowCompileErrors,
            // Test0.cs(20,22): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new StringWriter(builder)' before all references to it are out of scope.
            GetCSharpResultAt(20, 22, "void Test.M1()", "new StringWriter(builder)"));
        }

        [Fact]
        public void DelegateCreation_Disposed_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class Test
{
    void M1()
    {
        Func<A> createA = M2;
        A a = createA();
        a.Dispose();
    }

    A M2()
    {
        return new A();
    }
}
");

            VerifyBasic(@"
Imports System

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class Test
    Sub M1()
        Dim createA As Func(Of A) = AddressOf M2
        Dim a As A = createA()
        a.Dispose()
    End Sub

    Function M2() As A
        Return New A()
    End Function
End Class");
        }

        [Fact, WorkItem(1602, "https://github.com/dotnet/roslyn-analyzers/issues/1602")]
        public void MemberReferenceInQueryFromClause_Disposed_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Immutable;
using System.Linq;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B: IDisposable
{
    public C C { get; }
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class C
{
    public ImmutableArray<A> ArrayOfA { get; }
}

class Test
{
    void M1(ImmutableArray<B> arrayOfB)
    {
        var x = from b in arrayOfB
            from a in b.C.ArrayOfA
            select a;
        var y = new A();
        y.Dispose();
    }
}
");
        }

        [Fact]
        public void SystemThreadingTask_SpecialCase_NotDisposed_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;

public class A
{
    void M()
    {
        Task t = new Task(null);
        M1(out var t2);
    }

    void M1(out Task<int> t)
    {
        t = null;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Public Class A

    Private Sub M()
        Dim t As Task = New Task(Nothing)
        Dim t2 As Task = Nothing
        M1(t2)
    End Sub

    Private Sub M1(<Out> ByRef t As Task(Of Integer))
        t = Nothing
    End Sub
End Class");
        }

        [Fact]
        public void MultipleReturnStatements_AllInstancesReturned_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class Test
{
    A M1(bool flag)
    {
        A a;
        if (flag)
        {
            A a2 = new A();
            a = a2;
            return a;
        }

        A a3 = new A();
        a = a3;
        return a;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Public Class Test
    Private Function M1(ByVal flag As Boolean) As A
        Dim a As A
        If flag Then
            Dim a2 As New A()
            a = a2
            Return a
        End If

        Dim a3 As New A()
        a = a3
        Return a
    End Function
End Class
");
        }

        [Fact]
        public void MultipleReturnStatements_AllInstancesEscapedWithOutParameter_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class Test
{
    void M1(bool flag, out A a)
    {
        if (flag)
        {
            A a2 = new A();
            a = a2;
            return;
        }

        A a3 = new A();
        a = a3;
        return;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Public Class Test
    Private Sub M1(ByVal flag As Boolean, <Out> ByRef a As A)
        If flag Then
            Dim a2 As New A()
            a = a2
            Return
        End If

        Dim a3 As New A()
        a = a3
        Return
    End Sub
End Class
");
        }

        [Fact]
        public void MultipleReturnStatements_AllButOneInstanceReturned_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

public class Test
{
    A M1(int flag, bool flag2, bool flag3)
    {
        A a = null;
        if (flag == 0)
        {
            A a2 = new A();        // Escaped with return inside below nested 'if'.
            a = a2;

            if (!flag2)
            {
                if (flag3)
                {
                    return a;
                }
            }
        }
        else
        {
            a = new A();        // Escaped with return inside below nested 'else'.
            if (flag == 1)
            {
                a = new B();    // Never disposed.
            }
            else
            {
                if (flag3)
                {
                    a = new A();    // Escaped with return inside below 'else'.
                }

                if (flag2)
                {
                }
                else
                {
                    return a;
                }
            }
        }

        A a3 = new A();     // Escaped with below return.
        a = a3;
        return a;
    }
}
",
            // Test0.cs(39,21): warning CA2000: In method 'A Test.M1(int flag, bool flag2, bool flag3)', call System.IDisposable.Dispose on object created by 'new B()' before all references to it are out of scope.
            GetCSharpResultAt(39, 21, "A Test.M1(int flag, bool flag2, bool flag3)", "new B()"));

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Public Class Test
    Private Function M1(flag As Integer, flag2 As Boolean, flag3 As Boolean) As A
        Dim a As A = Nothing
        If flag = 0 Then
            Dim a2 As A = New A()   ' Escaped with return inside below nested 'if'.
            a = a2
            If Not flag2 Then
                If flag3 Then
                    Return a
                End If
            End If
        Else
            a = New A()     ' Escaped with return inside below nested 'else'.
            If flag = 1 Then
                a = New B()     ' Never disposed
            Else
                If flag3 Then
                    a = New A()     ' Escaped with return inside below 'else'.
                End If

                If flag2 Then
                Else
                    Return a
                End If
            End If
        End If

        Dim a3 As A = New A()     ' Escaped with below return.
        a = a3
        Return a
    End Function
End Class
",
            // Test0.vb(31,21): warning CA2000: In method 'Function Test.M1(flag As Integer, flag2 As Boolean, flag3 As Boolean) As A', call System.IDisposable.Dispose on object created by 'New B()' before all references to it are out of scope.
            GetBasicResultAt(31, 21, "Function Test.M1(flag As Integer, flag2 As Boolean, flag3 As Boolean) As A", "New B()"));
        }

        [Fact]
        public void MultipleReturnStatements_AllButOneInstanceEscapedWithOutParameter_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

class B : A
{
}

public class Test
{
    void M1(int flag, bool flag2, bool flag3, out A a)
    {
        a = null;
        if (flag == 0)
        {
            A a2 = new A();        // Escaped with return inside below nested 'if'.
            a = a2;

            if (!flag2)
            {
                if (flag3)
                {
                    return;
                }
            }
        }
        else
        {
            a = new A();        // Escaped with return inside below nested 'else'.
            if (flag == 1)
            {
                a = new B();    // Never disposed.
            }
            else
            {
                if (flag3)
                {
                    a = new A();    // Escaped with return inside below 'else'.
                }

                if (flag2)
                {
                }
                else
                {
                    return;
                }
            }
        }

        A a3 = new A();     // Escaped with below return.
        a = a3;
        return;
    }
}
",
            // Test0.cs(39,21): warning CA2000: In method 'void Test.M1(int flag, bool flag2, bool flag3, out A a)', call System.IDisposable.Dispose on object created by 'new B()' before all references to it are out of scope.
            GetCSharpResultAt(39, 21, "void Test.M1(int flag, bool flag2, bool flag3, out A a)", "new B()"));

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Class B
    Inherits A
End Class

Public Class Test
    Private Sub M1(flag As Integer, flag2 As Boolean, flag3 As Boolean, <Out> ByRef a As A)
        a = Nothing
        If flag = 0 Then
            Dim a2 As A = New A()   ' Escaped with return inside below nested 'if'.
            a = a2
            If Not flag2 Then
                If flag3 Then
                    Return
                End If
            End If
        Else
            a = New A()     ' Escaped with return inside below nested 'else'.
            If flag = 1 Then
                a = New B()     ' Never disposed
            Else
                If flag3 Then
                    a = New A()     ' Escaped with return inside below 'else'.
                End If

                If flag2 Then
                Else
                    Return
                End If
            End If
        End If

        Dim a3 As A = New A()     ' Escaped with below return.
        a = a3
        Return
    End Sub
End Class
",
            // Test0.vb(31,21): warning CA2000: In method 'Sub Test.M1(flag As Integer, flag2 As Boolean, flag3 As Boolean, ByRef a As A)', call System.IDisposable.Dispose on object created by 'New B()' before all references to it are out of scope.
            GetBasicResultAt(31, 21, "Sub Test.M1(flag As Integer, flag2 As Boolean, flag3 As Boolean, ByRef a As A)", "New B()"));
        }

        [Fact]
        public void DisposableAllocation_AssignedToTuple_Escaped_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class Test
{
    (A, int) M1()
    {
        A a = new A();
        return (a, 0);
    }

    (A, int) M2()
    {
        A a = new A();
        (A, int) b = (a, 0);
        return b;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Public Class Test
    Private Function M1() As (a As A, i As Integer)
        Dim a As A = New A()
        Return (a, 0)
    End Function

    Private Function M2() As (a As A, i As Integer)
        Dim a As A = New A()
        Dim b As (a As A, i As Integer) = (a, 0)
        Return b
    End Function
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1571"), WorkItem(1571, "https://github.com/dotnet/roslyn-analyzers/issues/1571")]
        public void DisposableAllocation_AssignedToTuple_NotDisposed_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class Test
{
    void M1()
    {
        A a = new A();
        var b = (a, 0);
    }

    void M2()
    {
        A a = new A();
        (A, int) b = (a, 0);
    }
}",
            // Test0.cs(16,15): warning CA2000: In method 'void Test.M1()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(16, 15, "void Test.M1()", "new A()"),
            // Test0.cs(22,15): warning CA2000: In method 'void Test.M2()', call System.IDisposable.Dispose on object created by 'new A()' before all references to it are out of scope.
            GetCSharpResultAt(22, 15, "void Test.M2()", "new A()"));

            VerifyBasic(@"
Imports System
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Class A
    Implements IDisposable
    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class

Public Class Test
    Private Sub M1()
        Dim a As A = New A()
        Dim b = (a, 0)
    End Sub

    Private Sub M2()
        Dim a As A = New A()
        Dim b As (a As A, i As Integer) = (a, 0)
    End Sub
End Class
",
            // Test0.vb(15,22): warning CA2000: In method 'Sub Test.M1()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(15, 22, "Sub Test.M1()", "New A()"),
            // Test0.vb(20,22): warning CA2000: In method 'Sub Test.M2()', call System.IDisposable.Dispose on object created by 'New A()' before all references to it are out of scope.
            GetBasicResultAt(20, 22, "Sub Test.M2()", "New A()"));
        }

        [Fact]
        public void DisposableAllocation_IncrementOperator_RegressionTest()
        {
            VerifyCSharp(@"
using System;

class A : IDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class Test
{
    private int i;
    void M()
    {
        var a = new A();
        i++;
        a.Dispose();
    }
}
");
        }
    }
}

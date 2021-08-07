// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ConsiderPassingBaseTypesAsParameters,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ConsiderPassingBaseTypesAsParametersTests
    {
        [Fact]
        public async Task Unused_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A {}
public class B : A {}

public class Z
{
    public void M(B b)
    {
    }
}");
        }

        [Fact]
        public async Task MethodCall_TypeWithBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int GetSomething() => 42;
}

public class B : A
{
    public int GetSomethingElse() => 42;
}

public class Z
{
    public void UsesOnlyBaseType(B {|#0:b|})
    {
        b.GetSomething();
    }

    public void UsesDeclaredType(B b)
    {
        b.GetSomethingElse();
    }

    public void UsesDeclaredAndBaseType(B b)
    {
        b.GetSomething();
        b.GetSomethingElse();
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"));
        }

        [Fact]
        public async Task MethodCall_TypeWithInterface()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public interface IA
{
    int GetSomething();
}

public class A : IA
{
    public int GetSomething() => 42;
    public int GetSomethingElse() => 42;
}

public class Z
{
    public void UsesOnlyInterface(A {|#0:a|})
    {
        a.GetSomething();
    }

    public void UsesDeclaredType(A a)
    {
        a.GetSomethingElse();
    }

    public void UsesDeclaredAndInterfaceTypes(A a)
    {
        a.GetSomething();
        a.GetSomethingElse();
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"));
        }

        [Fact]
        public async Task EventLikeMethod_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class ThresholdReachedEventArgs : EventArgs
{
    public int P => 42;
}

public class NewThresholdReachedEventArgs : ThresholdReachedEventArgs
{
}

public class C
{
    public void M(object sender, NewThresholdReachedEventArgs e)
    {
        var x = e.P;
    }
}");
        }

        [Fact]
        public async Task MethodCall_MultipleBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public interface IA
{
    int GetSomething();
}

public interface IB
{
    int GetSomethingElse();
}

public interface IAll : IA, IB {}

public class A : IA
{
    public int GetSomething() => 42;
}

public class B : A, IB
{
    public int GetSomethingElse() => 42;
}

public class C : B {}

public class D : C, IAll {}

public class Z
{
    public void UsesOnlyInterfaceIA(C {|#0:c|})
    {
        c.GetSomething();
    }

    public void UsesOnlyInterfaceIB(C {|#1:c|})
    {
        c.GetSomethingElse();
    }

    public void CouldBeDeclaredAsB(C {|#2:c|})
    {
        c.GetSomething();
        c.GetSomethingElse();
    }

    public void CouldBeDeclaredAsIAll(D {|#3:d|})
    {
        d.GetSomething();
        d.GetSomethingElse();
    }
}
",
    VerifyCS.Diagnostic().WithLocation(0).WithArguments("c", "C", "IA"),
    VerifyCS.Diagnostic().WithLocation(1).WithArguments("c", "C", "IB"),
    VerifyCS.Diagnostic().WithLocation(2).WithArguments("c", "C", "B"),
    VerifyCS.Diagnostic().WithLocation(3).WithArguments("d", "D", "IAll"));
        }

        [Fact]
        public async Task PropertyCall_TypeWithBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int Age => 42;
}

public class B : A
{
    public int OtherAge => 42;
}

public class Z
{
    public void UsesOnlyBaseType(B {|#0:b|})
    {
        var x = b.Age;
    }

    public void UsesDeclaredType(B b)
    {
        var x = b.OtherAge;
    }

    public void UsesDeclaredAndBaseType(B b)
    {
        var x = b.Age;
        var y = b.OtherAge;
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"));
        }

        [Fact]
        public async Task PropertyCall_TypeWithInterface()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public interface IA
{
    int Age { get; }
}

public class A : IA
{
    public int Age => 42;
    public int OtherAge => 42;
}

public class Z
{
    public void UsesOnlyInterface(A {|#0:a|})
    {
        var x = a.Age;
    }

    public void UsesDeclaredType(A a)
    {
        var x = a.OtherAge;
    }

    public void UsesDeclaredAndInterfaceTypes(A a)
    {
        var x = a.Age;
        var y = a.OtherAge;
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"));
        }

        [Fact]
        public async Task FieldCall_TypeWithBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int Age = 42;
}

public class B : A
{
    public int OtherAge = 42;
}

public class Z
{
    public void UsesOnlyBaseType(B {|#0:b|})
    {
        var x = b.Age;
    }

    public void UsesDeclaredType(B b)
    {
        var x = b.OtherAge;
    }

    public void UsesDeclaredAndBaseType(B b)
    {
        var x = b.Age;
        var y = b.OtherAge;
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"));
        }

        [Fact]
        public async Task EventCall_TypeWithBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class A
{
    public event EventHandler Changed;
}

public class B : A
{
    public event EventHandler OtherChanged;
}

public class Z
{
    public void UsesOnlyBaseType(B {|#0:b|})
    {
        b.Changed += (s, e) => {};
    }

    public void UsesDeclaredType(B b)
    {
        b.OtherChanged += (s, e) => {};
    }

    public void UsesDeclaredAndBaseType(B b)
    {
        b.Changed += (s, e) => {};
        b.OtherChanged += (s, e) => {};
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"));
        }

        [Fact]
        public async Task EventCall_TypeWithInterface()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IA
{
    event EventHandler Changed;
}

public class A : IA
{
    public event EventHandler Changed;
    public event EventHandler OtherChanged;
}

public class Z
{
    public void UsesOnlyInterface(A {|#0:a|})
    {
        a.Changed += (s, e) => {};
    }

    public void UsesDeclaredType(A a)
    {
        a.OtherChanged += (s, e) => {};
    }

    public void UsesDeclaredAndInterfaceTypes(A a)
    {
        a.Changed += (s, e) => {};
        a.OtherChanged += (s, e) => {};
    }
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"));
        }

        [Fact]
        public async Task TypeInstance_TypeWithBase()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
}

public class B : A
{
}

public class Z
{
    public void UsesOnlyBaseType(B {|#0:b|})
    {
        NeedA(b);
    }

    public void UsesDeclaredType(B b)
    {
        NeedB(b);
    }

    public void UsesDeclaredAndBaseType(B b)
    {
        NeedA(b);
        NeedB(b);
    }

    private void NeedA(A a) {}
    private void NeedB(B b) {}
}
", VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"));
        }

        [Fact]
        public async Task Assignment_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A {}
public class B : A {}

public class Z
{
    public void M1(B b)
    {
        b = new B();
    }
}");
        }
    }
}
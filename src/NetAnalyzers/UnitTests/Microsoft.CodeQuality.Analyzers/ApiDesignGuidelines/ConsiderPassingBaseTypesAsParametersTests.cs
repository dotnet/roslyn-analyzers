// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
        public async Task Unused()
        {
            var src = @"
public class A {}
public class B : A {}

public class Z
{
    public void M(B b)
    {
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task MethodCall_TypeWithBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"), src);
        }

        [Fact]
        public async Task MethodCall_TypeWithInterface()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"), src);
        }

        [Fact]
        public async Task MethodCall_TypeWithGenericInterface()
        {
            var src = @"
using System.Collections.Generic;

public class Z
{
    public void UsesOnlyInterface(List<int> {|#0:l|})
    {
        var x = l.Count;
    }

    public void UsesDeclaredType(List<int> l)
    {
        l.BinarySearch(0);
    }

    public void UsesDeclaredAndInterfaceTypes(List<int> l)
    {
        var x = l.Count;
        l.BinarySearch(0);
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("l", "List<int>", "ICollection<int>"), src);
        }

        [Fact]
        public async Task EventLikeMethod_NoDiagnostic()
        {
            var src = @"
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
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task MethodCall_MultipleBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(
                src,
                new[]
                {
                    VerifyCS.Diagnostic().WithLocation(0).WithArguments("c", "C", "IA"),
                    VerifyCS.Diagnostic().WithLocation(1).WithArguments("c", "C", "IB"),
                    VerifyCS.Diagnostic().WithLocation(2).WithArguments("c", "C", "B"),
                    VerifyCS.Diagnostic().WithLocation(3).WithArguments("d", "D", "IAll"),
                }, src);
        }

        [Fact]
        public async Task PropertyCall_TypeWithBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"), src);
        }

        [Fact]
        public async Task PropertyCall_TypeWithInterface()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"), src);
        }

        [Fact]
        public async Task FieldCall_TypeWithBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"), src);
        }

        [Fact]
        public async Task EventCall_TypeWithBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"), src);
        }

        [Fact]
        public async Task EventCall_TypeWithInterface()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("a", "A", "IA"), src);
        }

        [Fact]
        public async Task TypeInstance_TypeWithBase()
        {
            var src = @"
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
";
            await VerifyCS.VerifyCodeFixAsync(src, VerifyCS.Diagnostic().WithLocation(0).WithArguments("b", "B", "A"), src);
        }

        [Fact]
        public async Task Assignment_NoDiagnostic()
        {
            var src = @"
public class A {}
public class B : A {}

public class Z
{
    public void M1(B b)
    {
        b = new B();
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task InterfaceImplementation_NoDiagnostic()
        {
            var src = @"
public class A
{
    public void Run() {}
}

public class B : A {}

public interface IZ
{
    void M(B b);
}

public class Z : IZ
{
    public void M(B b)
    {
        b.Run();
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task MethodOverride_NoDiagnostic()
        {
            var src = @"
public class A
{
    public void Run() {}
}

public class B : A {}

public class Y
{
    public virtual void M(B [|b|])
    {
        b.Run();
    }
}

public class Z : Y
{
    public override void M(B b)
    {
        base.M(b);
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task OverloadMoreGenericExists_NoDiagnostic()
        {
            var src = @"
using System.IO;

public class Z
{
    // Could be flagged but overload with Stream already exists.
    public void ReadNextByte(FileStream stream)
    {
        while (stream.ReadByte() != -1) { /*...*/ }
    }

    public void ReadNextByte(Stream anyStream)
    {
        while (anyStream.ReadByte() != -1) { /*...*/ }
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task OverloadMultiParameters_FalseNegative()
        {
            var src = @"
using System.IO;

public class A
{
    public void Run() {}
}
public class B : A {}

public class Z
{
    // Overload with both 'Stream' and 'A' does not exist so we could report but
    // if the user does not fix all we will end up in a conflicting overload.
    public void M1(FileStream stream, B b)
    {
        b.Run();
        while (stream.ReadByte() != -1) { /*...*/ }
    }

    public void M1(FileStream [|stream|], A a)
    {
        a.Run();
        while (stream.ReadByte() != -1) { /*...*/ }
    }

    public void M1(Stream stream, B [|b|])
    {
        b.Run();
        while (stream.ReadByte() != -1) { /*...*/ }
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task TypeIsReferredThroughVariableCreationAsync()
        {
            var src = @"
using System.IO;

public class Base
{
}

public class Derived : Base
{
    public void SomeDerivedTypeMethod() {}
}

public class C
{
    void M(Derived d)
    {
        Derived d2 = d;
        d2.SomeDerivedTypeMethod();
    }

    void M2(Derived d)
    {
        Derived d2 = d;
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }

        [Fact]
        public async Task DownCastAsync()
        {
            var src = @"
using System.IO;

public class Base
{
}

public class Derived : Base
{
}

public class FurtherDerived : Derived
{
    public void MethodOnFurtherDerived() {}
}

public class C
{
    void M(Derived d)
    {
        if (true)
        {
            ((FurtherDerived)d).MethodOnFurtherDerived();
        }
    }
}";
            await VerifyCS.VerifyCodeFixAsync(src, src);
        }
    }
}
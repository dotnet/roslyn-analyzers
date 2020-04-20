// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AvoidNonPureMethodCallOnReadonlyValueSymbol,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public class AvoidNonPureMethodCallOnReadonlyValueSymbolTests
    {
        [Fact]
        public async Task CSharpInvokingNonPureMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Mutable
{
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        a.A();
    }
}
",
            GetCSharpResultAt(13, 9, "a", "A"));
        }

        [Fact]
        public async Task CSharpInvokingNonPureMethodInCoustructor()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Mutable
{
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public Test()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingNonPureMethodInStaticCoustructor()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Mutable
{
    public void A() { }
}

public class Test
{
    static readonly Mutable a;

    static Test()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpReferencingNonPureMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public struct Mutable
{
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        Action b = a.A;
        b();
    }
}
",
            GetCSharpResultAt(15, 20, "a", "A"));
        }

        [Fact(Skip = "C# 8 is needed")]
        public async Task CSharpInvokingPureMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Mutable
{
    public readonly void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingMethodWithPureAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Diagnostics.Contracts;

public struct Mutable
{
    [Pure]
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingMethodFromImmutableType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public readonly struct Immutable
{
    public void A() { }
}

public class Test
{
    readonly Immutable a;

    public void Method()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingNonPureMethodFromReferenceType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Mutable
{
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        a.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpReferencingNonPureMethodFromReferenceType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class Mutable
{
    public void A() { }
}

public class Test
{
    readonly Mutable a;

    public void Method()
    {
        Action b = a.A;
        b();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingNonPureStaticMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Mutable
{
    public static void A() { }
}

public class Test
{
    public void Method()
    {
        Mutable.A();
    }
}
");
        }

        [Fact]
        public async Task CSharpInvokingRegularMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Test
{
    public void Method()
    {
        Method2();
    }

    public void Method2() { }
}
");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}

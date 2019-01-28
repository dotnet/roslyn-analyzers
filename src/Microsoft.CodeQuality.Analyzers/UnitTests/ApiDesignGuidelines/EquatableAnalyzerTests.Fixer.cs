using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class EquatableFixerTests : CodeFixTestBase
    {
        [Fact]
        public void CodeFixForStructWithEqualsOverrideButNoIEquatableImplementation()
        {
            VerifyCSharpFix(@"
using System;

struct S
{
    public override bool Equals(object other)
    {
        return true;
    }

    public override int GetHashCode() => 0;
}
", @"
using System;

struct S : IEquatable<S>
{
    public override bool Equals(object other)
    {
        return true;
    }

    public override int GetHashCode() => 0;

    public bool Equals(S other)
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public void CodeFixForStructWithIEquatableImplementationButNoEqualsOverride()
        {
            VerifyCSharpFix(@"
using System;

struct S : IEquatable<S>
{
    public bool Equals(S other)
    {
        return true;
    }
}
", @"
using System;

struct S : IEquatable<S>
{
    public bool Equals(S other)
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is S && Equals((S)obj);
    }
}
",
            // warning CS0659: 'S' overrides Object.Equals(object o) but does not override Object.GetHashCode()
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CodeFixForClassWithIEquatableImplementationButNoEqualsOverride()
        {
            VerifyCSharpFix(@"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
    {
        return true;
    }
}
", @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as C);
    }
}
",
            // warning CS0659: 'C' overrides Object.Equals(object o) but does not override Object.GetHashCode()
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CodeFixForClassWithExplicitIEquatableImplementationAndNoEqualsOverride()
        {
            VerifyCSharpFix(@"
using System;

class C : IEquatable<C>
{
    bool IEquatable<C>.Equals(C other)
    {
        return true;
    }
}
", @"
using System;

class C : IEquatable<C>
{
    bool IEquatable<C>.Equals(C other)
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        return ((IEquatable<C>)this).Equals(obj as C);
    }
}
",
            // warning CS0659: 'C' overrides Object.Equals(object o) but does not override Object.GetHashCode()
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CodeFixForStructWithExplicitIEquatableImplementationAndNoEqualsOverride()
        {
            VerifyCSharpFix(@"
using System;

struct S : IEquatable<S>
{
    bool IEquatable<S>.Equals(S other)
    {
        return true;
    }
}
", @"
using System;

struct S : IEquatable<S>
{
    bool IEquatable<S>.Equals(S other)
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is S && ((IEquatable<S>)this).Equals((S)obj);
    }
}
",
            // warning CS0659: 'S' overrides Object.Equals(object o) but does not override Object.GetHashCode()
            allowNewCompilerDiagnostics: true);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new EquatableAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EquatableAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new EquatableFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new EquatableFixer();
        }
    }
}
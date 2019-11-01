// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class EquatableAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void NoDiagnosticForStructWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
struct S
{
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void NoDiagnosticForClassWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
class C
{
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void DiagnosticForStructWithEqualsOverrideButNoIEquatableImplementation()
        {
            var code = @"
struct S
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "S");
            VerifyCSharp(code,
                GetCSharpResultAt(2, 8, EquatableAnalyzer.ImplementIEquatableRuleId, expectedMessage));
        }

        [Fact]
        public void NoDiagnosticForClassWithEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
class C
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void DiagnosticForStructWithIEquatableImplementationButNoEqualsOverride()
        {
            var code = @"
using System;

struct S : IEquatable<S>
{
    public bool Equals(S other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "S");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationButNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithNoParameterListAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride2()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals)
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithNoParametersAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals()
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterDeclarationAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(x)
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithWrongReturnTypeAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public int Equals(C x)
    {
        return 1;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithNoBodyAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void NoDiagnosticForClassWithIEquatableImplementationWithNoReturnTypeAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public Equals(C other)
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void NoDiagnosticForClassWithEqualsOverrideWithWrongSignatureAndNoIEquatableImplementation()
        {
            var code = @"
using System;

class C
{
    public override bool Equals(object other, int n)
    {
        return true;
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void DiagnosticForClassWithExplicitIEquatableImplementationAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    bool IEquatable<C>.Equals(C other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForDerivedStructWithEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
using System;

struct B
{
    public override bool Equals(object other)
    {
        return false;
    }
}

struct C : B
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            string expectedMessage1 = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "B");
            string expectedMessage2 = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "C");
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.ImplementIEquatableRuleId, expectedMessage1),
                GetCSharpResultAt(12, 8, EquatableAnalyzer.ImplementIEquatableRuleId, expectedMessage2));
        }

        [Fact, WorkItem(1914, "https://github.com/dotnet/roslyn-analyzers/issues/1914")]
        public void NoDiagnosticForParentClassWithIEquatableImplementation()
        {
            var code = @"
using System;

public interface IValueObject<T> : IEquatable<T> { }

public struct S : IValueObject<S>
{
    private readonly int value;

    public override bool Equals(object obj) => obj is S other && Equals(other);

    public bool Equals(S other) => value == other.value;

    public override int GetHashCode() => value;
}";
            VerifyCSharp(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public void NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_01()
        {
            var code = @"
using System;

public class A<T> : IEquatable<T>
    where T : A<T>
{
    public virtual bool Equals(T other) => false;

    public override bool Equals(object obj) => Equals(obj as T);
}

public class B : A<B>
{
}";
            VerifyCSharp(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public void NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_02()
        {
            var code = @"
using System;

public class A<T> : IEquatable<T>
    where T: class
{
    public virtual bool Equals(T other) => false;

    public override bool Equals(object obj) => Equals(obj as T);
}

public class B : A<B>
{
}

public class C<T> : A<T>
    where T : class
{
}

public class D : C<D>
{
}";
            VerifyCSharp(code);
        }

        [Fact, WorkItem(2324, "https://github.com/dotnet/roslyn-analyzers/issues/2324")]
        public void CA1066_CSharp_RefStruct_NoDiagnostic()
        {
            VerifyCSharp(@"
public ref struct S
{
    public override bool Equals(object other)
    {
        return false;
    }
}
",
            parseOptions: new CSharpParseOptions(LanguageVersion.CSharp8));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
            => new EquatableAnalyzer();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new EquatableAnalyzer();
    }
}

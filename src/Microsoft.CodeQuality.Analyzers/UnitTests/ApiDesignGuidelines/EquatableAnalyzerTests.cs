// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.EquatableAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.EquatableFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class EquatableAnalyzerTests
    {
        [Fact]
        public async Task NoDiagnosticForStructWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
struct S
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
class C
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task DiagnosticForStructWithEqualsOverrideButNoIEquatableImplementation()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(2, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithEqualsOverrideAndNoIEquatableImplementation()
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
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task DiagnosticForStructWithIEquatableImplementationButNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task DiagnosticForClassWithIEquatableImplementationButNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoParameterListAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11),
                // Test0.cs(6,17): error CS0548: 'C.Equals': property or indexer must have at least one accessor
                new DiagnosticResult("CS0548", DiagnosticSeverity.Error).WithLocation(6, 17),
                // Test0.cs(8,9): error CS1014: A get or set accessor expected
                new DiagnosticResult("CS1014", DiagnosticSeverity.Error).WithLocation(8, 9),
                // Test0.cs(8,20): error CS1014: A get or set accessor expected
                new DiagnosticResult("CS1014", DiagnosticSeverity.Error).WithLocation(8, 20));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11),
                // Test0.cs(6,24): error CS1026: ) expected
                new DiagnosticResult("CS1026", DiagnosticSeverity.Error).WithLocation(6, 24));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride2()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11),
                // Test0.cs(6,23): error CS1003: Syntax error, ',' expected
                new DiagnosticResult("CS1003", DiagnosticSeverity.Error).WithLocation(6, 23),
                // Test0.cs(10,1): error CS1022: Type or namespace definition, or end-of-file expected
                new DiagnosticResult("CS1022", DiagnosticSeverity.Error).WithLocation(10, 1));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoParametersAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                    // Test0.cs(4,11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                    new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterDeclarationAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4, 11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11),
                // Test0.cs(6,24): error CS0246: The type or namespace name 'x' could not be found (are you missing a using directive or an assembly reference?)
                new DiagnosticResult("CS0246", DiagnosticSeverity.Error).WithLocation(6, 24),
                // Test0.cs(6,25): error CS1001: Identifier expected
                new DiagnosticResult("CS1001", DiagnosticSeverity.Error).WithLocation(6, 25));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithWrongReturnTypeAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,11): error CS0738: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'. 'C.Equals(C)' cannot implement 'IEquatable<C>.Equals(C)' because it does not have the matching return type of 'bool'.
                new DiagnosticResult("CS0738", DiagnosticSeverity.Error).WithLocation(4, 11));
        }

        [Fact]
        public async Task DiagnosticForClassWithIEquatableImplementationWithNoBodyAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,7): warning CA1067: Type C should override Equals because it implements IEquatable<T>
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage),
                // Test0.cs(6,17): error CS0501: 'C.Equals(C)' must declare a body because it is not marked abstract, extern, or partial
                new DiagnosticResult("CS0501", DiagnosticSeverity.Error).WithLocation(6, 17),
                // Test0.cs(6,32): error CS1002: ; expected
                new DiagnosticResult("CS1002", DiagnosticSeverity.Error).WithLocation(6, 32));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoReturnTypeAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,11): error CS0535: 'C' does not implement interface member 'IEquatable<C>.Equals(C)'
                new DiagnosticResult("CS0535", DiagnosticSeverity.Error).WithLocation(4, 11),
                // Test0.cs(6,12): error CS1520: Method must have a return type
                new DiagnosticResult("CS1520", DiagnosticSeverity.Error).WithLocation(6, 12),
                // Test0.cs(8,9): error CS0127: Since 'C.C(C)' returns void, a return keyword must not be followed by an object expression
                new DiagnosticResult("CS0127", DiagnosticSeverity.Error).WithLocation(8, 9));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithEqualsOverrideWithWrongSignatureAndNoIEquatableImplementation()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                    // Test0.cs(6,26): error CS0115: 'C.Equals(object, int)': no suitable method found to override
                    new DiagnosticResult("CS0115", DiagnosticSeverity.Error).WithLocation(6, 26));
        }

        [Fact]
        public async Task DiagnosticForClassWithExplicitIEquatableImplementationAndNoEqualsOverride()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task DiagnosticForDerivedStructWithEqualsOverrideAndNoIEquatableImplementation()
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
            await VerifyCS.VerifyAnalyzerAsync(code,
                // Test0.cs(4,8): warning CA1066: Implement IEquatable when overriding Object.Equals
                GetCSharpResultAt(4, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage1),
                // Test0.cs(12,8): warning CA1066: Implement IEquatable when overriding Object.Equals
                GetCSharpResultAt(12, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage2),
                // Test0.cs(12,12): error CS0527: Type 'B' in interface list is not an interface
                new DiagnosticResult("CS0527", DiagnosticSeverity.Error).WithLocation(12, 12));
        }

        [Fact, WorkItem(1914, "https://github.com/dotnet/roslyn-analyzers/issues/1914")]
        public async Task NoDiagnosticForParentClassWithIEquatableImplementation()
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
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public async Task NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_01()
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
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public async Task NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_02()
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
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2324, "https://github.com/dotnet/roslyn-analyzers/issues/2324")]
        public async Task CA1066_CSharp_RefStruct_NoDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
public ref struct S
{
    public override bool Equals(object other)
    {
        return false;
    }
}
",
                LanguageVersion = LanguageVersion.CSharp8
            }.RunAsync();
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, string message)
            => new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithMessage(message);
    }
}

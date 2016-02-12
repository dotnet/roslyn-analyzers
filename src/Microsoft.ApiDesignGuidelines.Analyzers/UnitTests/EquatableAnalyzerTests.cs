// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.UnitTests
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "S");
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "S");
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithNoParameterListAndNoEqualsOverride()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride2()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithNoParametersAndNoEqualsOverride()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithMalformedParameterDeclarationAndNoEqualsOverride()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithWrongReturnTypeAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public int Equals(C x)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void DiagnosticForClassWithIEquatableImplementationWithNoReturnTypeAndNoEqualsOverride()
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
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverrideObjectEqualsRuleId, expectedMessage));
        }

        [Fact]
        public void NoDiagnosticForClassWithEqualsOverrideWithWrongSignatureAndNoIEquatableImplementation()
        {
            var code = @"
using System;

class C
{
    public override Equals(object other, int n)
    {
        return true;
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void DiagnosticForClassWithExplicitIEquatableImplementationAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool IEquatable<C>.Equals(object other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage, "C");
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
            string expectedMessage1 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "B");
            string expectedMessage2 = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "C");
            VerifyCSharp(code,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.ImplementIEquatableRuleId, expectedMessage1),
                GetCSharpResultAt(12, 8, EquatableAnalyzer.ImplementIEquatableRuleId, expectedMessage2));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
            => new EquatableAnalyzer();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new EquatableAnalyzer();
    }
}

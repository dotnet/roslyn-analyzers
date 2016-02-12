// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OperatorsShouldHaveSymmetricalOverloadsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpOperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        [Fact]
        public void CSharpTestMissingEquality()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator==(A a1, A a2) { return false; }
}"
},
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "==", "!="));
        }

        [Fact]
        public void CSharpTestMissingInequality()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator!=(A a1, A a2) { return false; }
}"
},
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "!=", "=="));
        }

        [Fact]
        public void CSharpTestBothEqualityOperators()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator==(A a1, A a2) { return false; }
    public static bool operator!=(A a1, A a2) { return false; }
}"
});
        }

        [Fact]
        public void CSharpTestMissingLessThan()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator<(A a1, A a2) { return false; }
}"
},
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "<", ">"));
        }

        [Fact]
        public void CSharpTestNotMissingLessThan()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator<(A a1, A a2) { return false; }
    public static bool operator>(A a1, A a2) { return false; }
}"
});
        }

        [Fact]
        public void CSharpTestMissingLessThanOrEqualTo()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }
}"
},
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "<=", ">="));
        }

        [Fact]
        public void CSharpTestNotMissingLessThanOrEqualTo()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }
    public static bool operator>=(A a1, A a2) { return false; }
}"
});
        }

        [Fact]
        public void CSharpTestOperatorType()
        {
            VerifyCSharp(new[] {
                @"
class A
{
    public static bool operator==(A a1, int a2) { return false; }
    public static bool operator!=(A a1, string a2) { return false; }
}"
},
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "==", "!="),
GetCSharpResultAt(5, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "!=", "=="));
        }
    }
}
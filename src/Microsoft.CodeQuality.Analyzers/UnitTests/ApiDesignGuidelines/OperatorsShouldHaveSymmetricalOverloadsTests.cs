// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class OperatorsShouldHaveSymmetricalOverloadsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        [Fact]
        public void CSharpTestMissingEquality()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator==(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '!=' to also be defined
}", TestValidationMode.AllowCompileErrors,
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "==", "!="));
        }

        [Fact]
        public void CSharpTestMissingInequality()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator!=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '==' to also be defined
}", TestValidationMode.AllowCompileErrors,
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "!=", "=="));
        }

        [Fact]
        public void CSharpTestBothEqualityOperators()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator==(A a1, A a2) { return false; }
    public static bool operator!=(A a1, A a2) { return false; }
}");
        }

        [Fact]
        public void CSharpTestMissingLessThan()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator<(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>' to also be defined
}", TestValidationMode.AllowCompileErrors,
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "<", ">"));
        }

        [Fact]
        public void CSharpTestNotMissingLessThan()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator<(A a1, A a2) { return false; }
    public static bool operator>(A a1, A a2) { return false; }
}");
        }

        [Fact]
        public void CSharpTestMissingLessThanOrEqualTo()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>=' to also be defined
}", TestValidationMode.AllowCompileErrors,
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "<=", ">="));
        }

        [Fact]
        public void CSharpTestNotMissingLessThanOrEqualTo()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }
    public static bool operator>=(A a1, A a2) { return false; }
}");
        }

        [Fact]
        public void CSharpTestOperatorType()
        {
            VerifyCSharp(@"
class A
{
    public static bool operator==(A a1, int a2) { return false; }
    public static bool operator!=(A a1, string a2) { return false; }
}", TestValidationMode.AllowCompileErrors,
GetCSharpResultAt(4, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "==", "!="),
GetCSharpResultAt(5, 32, OperatorsShouldHaveSymmetricalOverloadsAnalyzer.Rule, "A", "!=", "=="));
        }
    }
}
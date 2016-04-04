// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OperatorsShouldHaveSymmetricalOverloadsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpOperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOperatorsShouldHaveSymmetricalOverloadsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOperatorsShouldHaveSymmetricalOverloadsFixer();
        }

        [Fact]
        public void CSharpTestEquality()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator==(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator==(A a1, A a2) { return false; }

    public static bool operator !=(A a1, A a2)
    {
        return !(a1 == a2);
    }
}");
        }

        [Fact]
        public void CSharpTestInequality()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator!=(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator!=(A a1, A a2) { return false; }

    public static bool operator ==(A a1, A a2)
    {
        return !(a1 != a2);
    }
}");
        }

        [Fact]
        public void CSharpTestLessThan()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator<(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator<(A a1, A a2) { return false; }

    public static bool operator >(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CSharpTestLessThanOrEqual()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator<=(A a1, A a2) { return false; }

    public static bool operator >=(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CSharpTestGreaterThan()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator>(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator>(A a1, A a2) { return false; }

    public static bool operator <(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CSharpTestGreaterThanOrEqual()
        {
            VerifyCSharpFix(
                @"
class A
{
    public static bool operator>=(A a1, A a2) { return false; }
}", @"
class A
{
    public static bool operator>=(A a1, A a2) { return false; }

    public static bool operator <=(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void VisualBasicTestEquality()
        {
            VerifyBasicFix(
                @"
class A
    public shared operator =(a1 as A, a2 as A) as boolean
        return false
    end operator
end class", @"
class A
    public shared operator =(a1 as A, a2 as A) as boolean
        return false
    end operator

    Public Shared Operator <>(a1 As A, a2 As A) As Boolean
        Return Not a1 = a2
    End Operator
end class");
        }

        [Fact]
        public void VisualBasicTestInequality()
        {
            VerifyBasicFix(
                @"
class A
    public shared operator <>(a1 as A, a2 as A) as boolean
        return false
    end operator
end class", @"
class A
    public shared operator <>(a1 as A, a2 as A) as boolean
        return false
    end operator

    Public Shared Operator =(a1 As A, a2 As A) As Boolean
        Return Not a1 <> a2
    End Operator
end class");
        }

        [Fact]
        public void VisualBasicTestLessThan()
        {
            VerifyBasicFix(
                @"
class A
    public shared operator <(a1 as A, a2 as A) as boolean
        return false
    end operator
end class", @"
class A
    public shared operator <(a1 as A, a2 as A) as boolean
        return false
    end operator

    Public Shared Operator >(a1 As A, a2 As A) As Boolean
        Throw New System.NotImplementedException()
    End Operator
end class");
        }
    }
}
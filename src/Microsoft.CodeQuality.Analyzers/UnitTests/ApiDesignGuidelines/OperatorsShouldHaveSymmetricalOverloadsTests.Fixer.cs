// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class OperatorsShouldHaveSymmetricalOverloadsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OperatorsShouldHaveSymmetricalOverloadsFixer();
        }

        [Fact]
        public void CSharpTestEquality()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator==(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '!=' to also be defined
}", @"
public class A
{
    public static bool operator==(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '!=' to also be defined

    public static bool operator !=(A a1, A a2)
    {
        return !(a1 == a2);
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestOverloads1()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator==(A a1, A a2) { return false; }      // error CS0216: The operator requires a matching operator '!=' to also be defined
    public static bool operator==(A a1, bool a2) { return false; }   // error CS0216: The operator requires a matching operator '!=' to also be defined
}", @"
public class A
{
    public static bool operator==(A a1, A a2) { return false; }      // error CS0216: The operator requires a matching operator '!=' to also be defined

    public static bool operator !=(A a1, A a2)
    {
        return !(a1 == a2);
    }

    public static bool operator==(A a1, bool a2) { return false; }   // error CS0216: The operator requires a matching operator '!=' to also be defined

    public static bool operator !=(A a1, bool a2)
    {
        return !(a1 == a2);
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestInequality()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator!=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '==' to also be defined
}", @"
public class A
{
    public static bool operator!=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '==' to also be defined

    public static bool operator ==(A a1, A a2)
    {
        return !(a1 != a2);
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestLessThan()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator<(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>' to also be defined
}", @"
public class A
{
    public static bool operator<(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>' to also be defined

    public static bool operator >(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestLessThanOrEqual()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator<=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>=' to also be defined
}", @"
public class A
{
    public static bool operator<=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '>=' to also be defined

    public static bool operator >=(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestGreaterThan()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator>(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '<' to also be defined
}", @"
public class A
{
    public static bool operator>(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '<' to also be defined

    public static bool operator <(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpTestGreaterThanOrEqual()
        {
            VerifyCSharpFix(
                @"
public class A
{
    public static bool operator>=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '<=' to also be defined
}", @"
public class A
{
    public static bool operator>=(A a1, A a2) { return false; }   // error CS0216: The operator requires a matching operator '<=' to also be defined

    public static bool operator <=(A a1, A a2)
    {
        throw new System.NotImplementedException();
    }
}", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void VisualBasicTestEquality()
        {
            VerifyBasicFix(
                @"
Public class A
    public shared operator =(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '<>' operator is required
        return false
    end operator
end class", @"
Public class A
    public shared operator =(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '<>' operator is required
        return false
    end operator

    Public Shared Operator <>(a1 As A, a2 As A) As Boolean
        Return Not a1 = a2
    End Operator
end class", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void VisualBasicTestInequality()
        {
            VerifyBasicFix(
                @"
Public class A
    public shared operator <>(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '=' operator is required
        return false
    end operator
end class", @"
Public class A
    public shared operator <>(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '=' operator is required
        return false
    end operator

    Public Shared Operator =(a1 As A, a2 As A) As Boolean
        Return Not a1 <> a2
    End Operator
end class", validationMode: TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void VisualBasicTestLessThan()
        {
            VerifyBasicFix(
                @"
Public class A
    public shared operator <(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '>' operator is required
        return false
    end operator
end class", @"
Public class A
    public shared operator <(a1 as A, a2 as A) as boolean   ' error BC33033: Matching '>' operator is required
        return false
    end operator

    Public Shared Operator >(a1 As A, a2 As A) As Boolean
        Throw New System.NotImplementedException()
    End Operator
end class", validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}
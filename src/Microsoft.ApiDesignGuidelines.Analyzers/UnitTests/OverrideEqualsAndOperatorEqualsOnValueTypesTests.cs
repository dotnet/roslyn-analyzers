// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OverrideEqualsAndOperatorEqualsOnValueTypesTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void CSharpDiagnosticForBothEqualsAndOperatorEqualsOnStruct()
        {
            VerifyCSharp(@"
public struct A
{
}",
                GetCSharpOverrideEqualsDiagnostic(2, 15, "A"),
                GetCSharpOperatorEqualsDiagnostic(2, 15, "A"));
        }

        [Fact]
        public void CSharpNoDiagnosticForEqualsOrOperatorEqualsOnClass()
        {
            VerifyCSharp(@"
public class A
{
}");
        }

        [Fact]
        public void CSharpNoDiagnosticWhenStructImplementsEqualsAndOperatorEquals()
        {
            VerifyCSharp(@"
public struct A
{
    public override bool Equals(object other)
    {
        return true;
    }

    public static bool operator==(A left, A right)
    {
        return true;
    }

    public static bool operator!=(A left, A right)
    {
        return true;
    }
}");
        }

        [Fact]
        public void CSharpDiagnosticWhenEqualsHasWrongSignature()
        {
            VerifyCSharp(@"
public struct A
{
    public override bool Equals(A other)
    {
        return true;
    }

    public static bool operator==(A left, A right)
    {
        return true;
    }

    public static bool operator!=(A left, A right)
    {
        return true;
    }
}",
                GetCSharpOverrideEqualsDiagnostic(2, 15, "A"));
        }

        [Fact]
        public void CSharpDiagnosticWhenEqualsIsNotAnOverride()
        {
            VerifyCSharp(@"
public struct A
{
    public new bool Equals(obj other)
    {
        return true;
    }

    public static bool operator==(A left, A right)
    {
        return true;
    }

    public static bool operator!=(A left, A right)
    {
        return true;
    }
}",
                GetCSharpOverrideEqualsDiagnostic(2, 15, "A"));
        }

        [Fact]
        public void BasicDiagnosticsForEqualsOnStructure()
        {
            VerifyBasic(@"
Public Structure A
End Structure
",
                GetBasicOverrideEqualsDiagnostic(2, 18, "A"),
                GetBasicOperatorEqualsDiagnostic(2, 18, "A"));
        }

        [Fact]
        public void BasicNoDiagnosticForEqualsOnClass()
        {
            VerifyBasic(@"
Public Class A
End Class
");
        }

        [Fact]
        public void BasicNoDiagnosticWhenStructureImplementsEqualsAndOperatorEquals()
        {
            VerifyBasic(@"
Public Structure A
    Public Overrides Overloads Function Equals(obj As Object) As Boolean
        Return True
     End Function

    Public Shared Operator =(left As A, right As A)
        Return True
    End Operator

    Public Shared Operator <>(left As A, right As A)
        Return False
    End Operator
End Structure
");
        }

        [Fact]
        public void BasicDiagnosticWhenEqualsHasWrongSignature()
        {
            VerifyBasic(@"
Public Structure A
    Public Overrides Overloads Function Equals(obj As A) As Boolean
        Return True
    End Function

    Public Shared Operator =(left As A, right As A)
        Return True
    End Operator

    Public Shared Operator <>(left As A, right As A)
        Return False
    End Operator
End Structure
",
            GetBasicOverrideEqualsDiagnostic(2, 18, "A"));
        }

        [Fact]
        public void BasicDiagnosticWhenEqualsIsNotAnOverride()
        {
            VerifyBasic(@"
Public Structure A
   Public Shadows Function Equals(obj As Object) As Boolean
      Return True
   End Function

    Public Shared Operator =(left As A, right As A)
        Return True
    End Operator

    Public Shared Operator <>(left As A, right As A)
        Return False
    End Operator
End Structure
",
                GetBasicOverrideEqualsDiagnostic(2, 18, "A"));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer();
        }

        private static DiagnosticResult GetCSharpOverrideEqualsDiagnostic(int line, int column, string typeName)
        {
            return GetExpectedDiagnostic(LanguageNames.CSharp, line, column, typeName, MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageEquals);
        }

        private static DiagnosticResult GetCSharpOperatorEqualsDiagnostic(int line, int column, string typeName)
        {
            return GetExpectedDiagnostic(LanguageNames.CSharp, line, column, typeName, MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageOpEquality);
        }

        private static DiagnosticResult GetBasicOverrideEqualsDiagnostic(int line, int column, string typeName)
        {
            return GetExpectedDiagnostic(LanguageNames.VisualBasic, line, column, typeName, MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageEquals);
        }

        private static DiagnosticResult GetBasicOperatorEqualsDiagnostic(int line, int column, string typeName)
        {
            return GetExpectedDiagnostic(LanguageNames.VisualBasic, line, column, typeName, MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageOpEquality);
        }

        private static DiagnosticResult GetExpectedDiagnostic(string language, int line, int column, string typeName, string messageFormat)
        {
            string fileName = language == LanguageNames.CSharp ? "Test0.cs" : "Test0.vb";
            return new DiagnosticResult
            {
                Id = OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer.RuleId,
                Message = string.Format(messageFormat, typeName),
                Severity = OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer.EqualsRule.DefaultSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation(fileName, line, column)
                }
            };
        }
    }
}
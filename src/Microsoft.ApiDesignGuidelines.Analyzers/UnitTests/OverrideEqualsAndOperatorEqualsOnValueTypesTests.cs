// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;
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
    public int X;
}",
                GetCSharpOverrideEqualsDiagnostic(2, 15, "A"),
                GetCSharpOperatorEqualsDiagnostic(2, 15, "A"));
        }

        [WorkItem(895, "https://github.com/dotnet/roslyn-analyzers/issues/895")]
        [Fact]
        public void CSharpNoDiagnosticForInternalAndPrivateStruct()
        {
            VerifyCSharp(@"
internal struct A
{
    public int X;
}

public class B
{
    private struct C
    {
        public int X;
    }
}
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact]
        public void CSharpNoDiagnosticForEnum()
        {
            VerifyCSharp(@"
public enum E
{
    F = 0
}
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact]
        public void CSharpNoDiagnosticForStructsWithoutMembers()
        {
            VerifyCSharp(@"
public struct EmptyStruct
{
}
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact(Skip = "899")]
        public void CSharpNoDiagnosticForEnumerators()
        {
            VerifyCSharp(@"
using System;

public struct MyEnumerator : System.Collections.IEnumerator
{
    public object Current
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}

public struct MyGenericEnumerator<T> : System.Collections.Generic.IEnumerator<T>
{
    public T Current
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    object IEnumerator.Current
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public void CSharpNoDiagnosticForEqualsOrOperatorEqualsOnClass()
        {
            VerifyCSharp(@"
public class A
{
    public int X;
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
    Public X As Integer
End Structure
",
                GetBasicOverrideEqualsDiagnostic(2, 18, "A"),
                GetBasicOperatorEqualsDiagnostic(2, 18, "A"));
        }

        [WorkItem(895, "https://github.com/dotnet/roslyn-analyzers/issues/895")]
        [Fact]
        public void BasicNoDiagnosticsForInternalAndPrivateStructure()
        {
            VerifyBasic(@"
Friend Structure A
    Public X As Integer
End Structure

Public Class B
    Private Structure C
        Public X As Integer
    End Structure
End Class
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact]
        public void BasicNoDiagnosticForEnum()
        {
            VerifyBasic(@"
Public Enum E
    F = 0
End Enum
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact]
        public void BasicNoDiagnosticForStructsWithoutMembers()
        {
            VerifyBasic(@"
Public Structure EmptyStruct
End Structure
");
        }

        [WorkItem(899, "https://github.com/dotnet/roslyn-analyzers/issues/899")]
        [Fact(Skip = "899")]
        public void BasicNoDiagnosticForEnumerators()
        {
            VerifyBasic(@"
Imports System

Public Structure MyEnumerator
	Implements System.Collections.IEnumerator
	Public ReadOnly Property Current() As Object
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Function MoveNext() As Boolean
		Throw New NotImplementedException()
	End Function

	Public Sub Reset()
		Throw New NotImplementedException()
	End Sub
End Structure

Public Structure MyGenericEnumerator(Of T)
	Implements System.Collections.Generic.IEnumerator(Of T)
	Public ReadOnly Property Current() As T
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Private ReadOnly Property IEnumerator_Current() As Object Implements IEnumerator.Current
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Sub Dispose()
		Throw New NotImplementedException()
	End Sub

	Public Function MoveNext() As Boolean
		Throw New NotImplementedException()
	End Function

	Public Sub Reset()
		Throw New NotImplementedException()
	End Sub
End Structure
");
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
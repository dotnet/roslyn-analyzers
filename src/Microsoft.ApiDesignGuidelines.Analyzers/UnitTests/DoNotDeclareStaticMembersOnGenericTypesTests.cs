// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotDeclareStaticMembersOnGenericTypesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDeclareStaticMembersOnGenericTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDeclareStaticMembersOnGenericTypesAnalyzer();
        }

        [Fact]
        public void CSharp_CA1000_ShouldGenerate()
        {
            VerifyCSharp(@"public class GenericType1<T>
    {
        private GenericType1()
        {
        }
 
        public static void Output(T data)
        {
            Console.Write(data);
        }
 
        public static string Test
        {
            get { return string.Empty; }
        }        
    }
 
    public static class GenericType2<T>
    {
        public static void Output(T data)
        {
            Console.Write(data);
        }
 
        public static string Test
        {
            get { return string.Empty; }
        }
    }",
    GetCSharpResultAt(7, 28, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetCSharpResultAt(12, 30, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetCSharpResultAt(20, 28, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetCSharpResultAt(25, 30, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString())
    );
        }

        [Fact]
        public void Basic_CA1000_ShouldGenerate()
        {
            VerifyBasic(@"Imports System
Public Class GenericType1(Of T)
    Private Sub New()
    End Sub

    Public Shared Sub Output(data As T)
        Console.Write(data)
    End Sub

    Public Shared ReadOnly Property Test() As String
        Get
            Return String.Empty
        End Get
    End Property
End Class

Public NotInheritable Class GenericType2(Of T)
    Public Shared Sub Output(data As T)
        Console.Write(data)
    End Sub

    Public Shared ReadOnly Property Test() As String
        Get
            Return String.Empty
        End Get
    End Property
End Class",
    GetBasicResultAt(6, 23, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetBasicResultAt(10, 37, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetBasicResultAt(18, 23, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString()),
    GetBasicResultAt(22, 37, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.RuleId, DoNotDeclareStaticMembersOnGenericTypesAnalyzer.Rule.MessageFormat.ToString())
    );
        }

        [Fact]
        public void CSharp_CA1000_ShouldNotGenerate()
        {
            VerifyCSharp(@"
using System;

public class GenericType1<T>
{
    private GenericType1()
    {
    }
 
    static GenericType1()
    {
    }

    protected static string TestProtected
    {
        get { return string.Empty; }
    }

    protected internal static string TestProtectedInternal
    {
        get { return string.Empty; }
    }

    internal static string TestInternal
    {
        get { return string.Empty; }
    }

    private static string TestPrivate
    {
        get { return string.Empty; }
    }

    protected static void OutputProtected(T data)
    {
        Console.Write(data);
    }

    protected internal static void OutputProtectedInternal(T data)
    {
        Console.Write(data);
    }

    internal static void OutputInternal(T data)
    {
        Console.Write(data);
    }

    private static void OutputPrivate(T data)
    {
        Console.Write(data);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(GenericType1<T> left, GenericType1<T> right)
    {
        return object.Equals(left, right);
    }

    public static bool operator !=(GenericType1<T> left, GenericType1<T> right)
    {
        return !object.Equals(left, right);
    }
}

public class OpenType<T>
{
}

public sealed class ClosedType : OpenType<String>
{

    public static void OutputProtected()
    {
    }

    public static string Test
    {
        get { return string.Empty; }
    }
}");
        }

        [Fact]
        public void Basic_CA1000_ShouldNotGenerate()
        {
            VerifyBasic(@"
Public Class GenericType1(Of T)
    Private Sub New()
    End Sub

    Shared Sub New()
    End Sub

    Protected Shared ReadOnly Property TestProtected() As String
        Get
            Return String.Empty
        End Get
    End Property

    Protected Friend Shared ReadOnly Property TestProtectedInternal() As String
        Get
            Return String.Empty
        End Get
    End Property

    Friend Shared ReadOnly Property TestInternal() As String
        Get
            Return String.Empty
        End Get
    End Property

    Private Shared ReadOnly Property TestPrivate() As String
        Get
            Return String.Empty
        End Get
    End Property

    Protected Shared Sub OutputProtected(data As T)
        Console.Write(data)
    End Sub

    Protected Friend Shared Sub OutputProtectedInternal(data As T)
        Console.Write(data)
    End Sub

    Friend Shared Sub OutputInternal(data As T)
        Console.Write(data)
    End Sub

    Private Shared Sub OutputPrivate(data As T)
        Console.Write(data)
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        Return MyBase.Equals(obj)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return MyBase.GetHashCode()
    End Function

    Public Shared Operator =(left As GenericType1(Of T), right As GenericType1(Of T)) As Boolean
        Return Object.Equals(left, right)
    End Operator

    Public Shared Operator <>(left As GenericType1(Of T), right As GenericType1(Of T)) As Boolean
        Return Not Object.Equals(left, right)
    End Operator
End Class

Public Class OpenType(Of T)
End Class

Public NotInheritable Class ClosedType
    Inherits OpenType(Of [String])

    Public Shared Sub OutputProtected()
    End Sub

    Public Shared ReadOnly Property Test() As String
        Get
            Return String.Empty
        End Get
    End Property
End Class");
        }
    }
}
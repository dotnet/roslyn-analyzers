// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class ReviewUnusedParametersTests : DiagnosticAnalyzerTestBase
    {
        #region Unit tests for no analyzer diagnostic

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void NoDiagnosticSimpleCasesTest()
        {
            VerifyCSharp(@"
using System;

public class NeatCode
{
    // Used parameter methods
    public void UsedParameterMethod1(string use)
    {
        Console.WriteLine(this);
        Console.WriteLine(use);
    }

    public void UsedParameterMethod2(string use)
    {
        UsedParameterMethod3(ref use);
    }

    public void UsedParameterMethod3(ref string use)
    {
        use = null;
    }
}
");

            VerifyBasic(@"
Imports System

Public Class NeatCode
    ' Used parameter methods
    Public Sub UsedParameterMethod1(use As String)
        Console.WriteLine(Me)
        Console.WriteLine(use)
    End Sub

    Public Sub UsedParameterMethod2(use As String)
        UsedParameterMethod3(use)
    End Sub

    Public Sub UsedParameterMethod3(ByRef use As String)
        use = Nothing
    End Sub
End Class
");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void NoDiagnosticDelegateTest()
        {
            VerifyCSharp(@"
using System;

public class NeatCode
{
    // Used parameter methods
    public void UsedParameterMethod1(Action a)
    {
        a();
    }

    public void UsedParameterMethod2(Action a1, Action a2)
    {
        try
        {
            a1();
        }
        catch(Exception)
        {
            a2();
        }
    }
}
");

            VerifyBasic(@"
Imports System

Public Class NeatCode
	' Used parameter methods
	Public Sub UsedParameterMethod1(a As Action)
		a()
	End Sub

	Public Sub UsedParameterMethod2(a1 As Action, a2 As Action)
		Try
			a1()
		Catch generatedExceptionName As Exception
			a2()
		End Try
	End Sub
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/8884")]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public void NoDiagnosticDelegateTest2_CSharp()
        {
            VerifyCSharp(@"
using System;

public class NeatCode
{
    // Used parameter methods
    public void UsedParameterMethod1(Action a)
    {
        Action a2 = new Action(() =>
        {
            a();
        });
    }
");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void NoDiagnosticDelegateTest2_VB()
        {
            VerifyBasic(@"
Imports System

Public Class NeatCode
	' Used parameter methods
	Public Sub UsedParameterMethod1(a As Action)
		Dim a2 As New Action(Sub() 
		                         a()
                             End Sub)
	End Sub
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/8884")]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public void NoDiagnosticUsingTest_CSharp()
        {
            VerifyCSharp(@"
using System;

class C
{
    void F(int x, IDisposable o)
    {
        using (o)
        {
            int y = x;
        }
    }
}
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/8884")]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public void NoDiagnosticUsingTest_VB()
        {
            VerifyBasic(@"
Imports System

Class C
	Private Sub F(x As Integer, o As IDisposable)
		Using o
			Dim y As Integer = x
		End Using
	End Sub
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/8884")]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public void NoDiagnosticLinqTest_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Linq;
using System.Reflection;

class C
{
    private object F(Assembly assembly)
    {
        var type = (from t in assembly.GetTypes()
                    select t.Attributes).FirstOrDefault();
        return type;
    }
}
");
        }


        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/8884")]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public void NoDiagnosticLinqTest_VB()
        {
            VerifyBasic(@"
Imports System
Imports System.Linq
Imports System.Reflection

Class C
    Private Function F(assembly As Assembly) As Object
        Dim type = (From t In assembly.DefinedTypes() Select t.Attributes).FirstOrDefault()
        Return type
    End Function
End Class
");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void NoDiagnosticSpecialCasesTest()
        {
            VerifyCSharp(@"
using System;

public abstract class Derived : Base, I
{
    // Override
    public override void VirtualMethod(int param)
    {
    }

    // Abstract
    public abstract void AbstractMethod(int param);

    // Implicit interface implementation
    public void Method1(int param)
    {
    }

    // Explicit interface implementation
    void I.Method2(int param)
    {
    }

    // Event handlers
    public void MyEventHandler(object o, EventArgs e)
    {
    }

    public void MyEventHandler2(object o, MyEventArgs e)
    {
    }

    private class MyEventArgs : EventArgs { }
}

public class Base
{
    // Virtual
    public virtual void VirtualMethod(int param)
    {
    }
}

public interface I
{
    void Method1(int param);
    void Method2(int param);
}
");

            VerifyBasic(@"
Imports System

Public MustInherit Class Derived
    Inherits Base
    Implements I
    ' Override
    Public Overrides Sub VirtualMethod(param As Integer)
    End Sub

    ' Abstract
    Public MustOverride Sub AbstractMethod(param As Integer)

    ' Explicit interface implementation - VB has no implicit interface implementation.
    Public Sub Method1(param As Integer) Implements I.Method1
    End Sub

    ' Explicit interface implementation
    Private Sub I_Method2(param As Integer) Implements I.Method2
    End Sub

    ' Event handlers
    Public Sub MyEventHandler(o As Object, e As EventArgs)
    End Sub

    Public Sub MyEventHandler2(o As Object, e As MyEventArgs)
    End Sub

    Private Class MyEventArgs
        Inherits EventArgs
    End Class
End Class

Public Class Base
    ' Virtual
    Public Overridable Sub VirtualMethod(param As Integer)
    End Sub
End Class

Public Interface I
    Sub Method1(param As Integer)
    Sub Method2(param As Integer)
End Interface
");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void NoDiagnosticForMethodsWithSpecialAttributesTest()
        {
            VerifyCSharp(@"
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

#define CONDITION_1

public class ConditionalMethodsClass
{
    [Conditional(""CONDITION_1"")]
    private static void ConditionalMethod(int a)
    {
        AnotherConditionalMethod(a);
    }

    [Conditional(""CONDITION_2"")]
    private static void AnotherConditionalMethod(int b)
    {
        Console.WriteLine(b);
    }
}

public class SerializableMethodsClass
{
    [OnSerializing]
    private void OnSerializingCallback(StreamingContext context)
    {
        Console.WriteLine(this);
    }

    [OnSerialized]
    private void OnSerializedCallback(StreamingContext context)
    {
        Console.WriteLine(this);
    }

    [OnDeserializing]
    private void OnDeserializingCallback(StreamingContext context)
    {
        Console.WriteLine(this);
    }

    [OnDeserialized]
    private void OnDeserializedCallback(StreamingContext context)
    {
        Console.WriteLine(this);
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Diagnostics
Imports System.Runtime.Serialization

#Define CONDITION_1

Public Class ConditionalMethodsClass
    <Conditional(""CONDITION_1"")> _
    Private Shared Sub ConditionalMethod(a As Integer)
        AnotherConditionalMethod(a)
    End Sub

    <Conditional(""CONDITION_2"")> _
    Private Shared Sub AnotherConditionalMethod(b As Integer)
        Console.WriteLine(b)
    End Sub
End Class

Public Class SerializableMethodsClass
    <OnSerializing> _
    Private Sub OnSerializingCallback(context As StreamingContext)
        Console.WriteLine(Me)
    End Sub

    <OnSerialized> _
    Private Sub OnSerializedCallback(context As StreamingContext)
        Console.WriteLine(Me)
    End Sub

    <OnDeserializing> _
    Private Sub OnDeserializingCallback(context As StreamingContext)
        Console.WriteLine(Me)
    End Sub

    <OnDeserialized> _
    Private Sub OnDeserializedCallback(context As StreamingContext)
        Console.WriteLine(Me)
    End Sub
End Class
");
        }

        #endregion

        #region Unit tests for analyzer diagnostic(s)

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public void DiagnosticForSimpleCasesTest()
        {
            VerifyCSharp(@"
using System;

class C
{
    public C(int param)
    {
    }

    public void UnusedParamMethod(int param)
    {
    }

    public static void UnusedParamStaticMethod(int param1)
    {
    }

    public void UnusedDefaultParamMethod(int defaultParam = 1)
    {
    }

    public void UnusedParamsArrayParamMethod(params int[] paramsArr)
    {
    }

    public void MultipleUnusedParamsMethod(int param1, int param2)
    {
    }

    private void UnusedRefParamMethod(ref int param1)
    {
    }

    public void UnusedErrorTypeParamMethod(UndefinedType param1)
    {
    }
}
",
      // Test0.cs(6,18): warning CA1801: Parameter param of method .ctor is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(6, 18, "param", ".ctor"),
      // Test0.cs(10,39): warning CA1801: Parameter param of method UnusedParamMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(10, 39, "param", "UnusedParamMethod"),
      // Test0.cs(14,52): warning CA1801: Parameter param1 of method UnusedParamStaticMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(14, 52, "param1", "UnusedParamStaticMethod"),
      // Test0.cs(18,46): warning CA1801: Parameter defaultParam of method UnusedDefaultParamMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(18, 46, "defaultParam", "UnusedDefaultParamMethod"),
      // Test0.cs(22,59): warning CA1801: Parameter paramsArr of method UnusedParamsArrayParamMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(22, 59, "paramsArr", "UnusedParamsArrayParamMethod"),
      // Test0.cs(26,48): warning CA1801: Parameter param1 of method MultipleUnusedParamsMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(26, 48, "param1", "MultipleUnusedParamsMethod"),
      // Test0.cs(26,60): warning CA1801: Parameter param2 of method MultipleUnusedParamsMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(26, 60, "param2", "MultipleUnusedParamsMethod"),
      // Test0.cs(30,47): warning CA1801: Parameter param1 of method UnusedRefParamMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(30, 47, "param1", "UnusedRefParamMethod"),
      // Test0.cs(34,58): warning CA1801: Parameter param1 of method UnusedErrorTypeParamMethod is never used. Remove the parameter or use it in the method body.
      GetCSharpUnusedParameterResultAt(34, 58, "param1", "UnusedErrorTypeParamMethod"));

            VerifyBasic(@"
Class C
    Public Sub New(param As Integer)
    End Sub

    Public Sub UnusedParamMethod(param As Integer)
    End Sub

    Public Shared Sub UnusedParamStaticMethod(param1 As Integer)
    End Sub

    Public Sub UnusedDefaultParamMethod(Optional defaultParam As Integer = 1)
    End Sub

    Public Sub UnusedParamsArrayParamMethod(ParamArray paramsArr As Integer())
    End Sub

    Public Sub MultipleUnusedParamsMethod(param1 As Integer, param2 As Integer)
    End Sub

    Private Sub UnusedRefParamMethod(ByRef param1 As Integer)
    End Sub

    Public Sub UnusedErrorTypeParamMethod(param1 As UndefinedType)
    End Sub
End Class
",
      // Test0.vb(3,20): warning CA1801: Parameter param of method .ctor is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(3, 20, "param", ".ctor"),
      // Test0.vb(6,34): warning CA1801: Parameter param of method UnusedParamMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(6, 34, "param", "UnusedParamMethod"),
      // Test0.vb(9,47): warning CA1801: Parameter param1 of method UnusedParamStaticMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(9, 47, "param1", "UnusedParamStaticMethod"),
      // Test0.vb(12,50): warning CA1801: Parameter defaultParam of method UnusedDefaultParamMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(12, 50, "defaultParam", "UnusedDefaultParamMethod"),
      // Test0.vb(15,56): warning CA1801: Parameter paramsArr of method UnusedParamsArrayParamMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(15, 56, "paramsArr", "UnusedParamsArrayParamMethod"),
      // Test0.vb(18,43): warning CA1801: Parameter param1 of method MultipleUnusedParamsMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(18, 43, "param1", "MultipleUnusedParamsMethod"),
      // Test0.vb(18,62): warning CA1801: Parameter param2 of method MultipleUnusedParamsMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(18, 62, "param2", "MultipleUnusedParamsMethod"),
      // Test0.vb(21,44): warning CA1801: Parameter param1 of method UnusedRefParamMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(21, 44, "param1", "UnusedRefParamMethod"),
      // Test0.vb(24,43): warning CA1801: Parameter param1 of method UnusedErrorTypeParamMethod is never used. Remove the parameter or use it in the method body.
      GetBasicUnusedParameterResultAt(24, 43, "param1", "UnusedErrorTypeParamMethod"));
        }

        #endregion

        #region Helpers

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewUnusedParametersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewUnusedParametersAnalyzer();
        }

        private static DiagnosticResult GetCSharpUnusedParameterResultAt(int line, int column, string parameterName, string methodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersMessage, parameterName, methodName);
            return GetCSharpResultAt(line, column, ReviewUnusedParametersAnalyzer.RuleId, message);
        }

        private static DiagnosticResult GetBasicUnusedParameterResultAt(int line, int column, string parameterName, string methodName)
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersMessage, parameterName, methodName);
            return GetBasicResultAt(line, column, ReviewUnusedParametersAnalyzer.RuleId, message);
        }

        #endregion
    }
}
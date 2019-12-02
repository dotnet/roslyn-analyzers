// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Maintainability.ReviewUnusedParametersAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpReviewUnusedParametersFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Maintainability.ReviewUnusedParametersAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability.BasicReviewUnusedParametersFixer>;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class ReviewUnusedParametersTests
    {
        #region Unit tests for no analyzer diagnostic

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public async Task NoDiagnosticSimpleCasesTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task NoDiagnosticDelegateTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Fact]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public async Task NoDiagnosticDelegateTest2_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
}");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public async Task NoDiagnosticDelegateTest2_VB()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Fact]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public async Task NoDiagnosticUsingTest_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        [Fact]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public async Task NoDiagnosticUsingTest_VB()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Fact]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public async Task NoDiagnosticLinqTest_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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


        [Fact]
        [WorkItem(8884, "https://github.com/dotnet/roslyn/issues/8884")]
        public async Task NoDiagnosticLinqTest_VB()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task NoDiagnosticSpecialCasesTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.InteropServices;

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

    public class MyEventArgs : EventArgs { }
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

public class ClassWithExtern
{
    [DllImport(""Dependency.dll"")]
    public static extern void DllImportMethod(int param);

    public static extern void ExternalMethod(int param);
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Runtime.InteropServices

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

    Public Class MyEventArgs
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

Public Class ClassWithExtern
    <DllImport(""Dependency.dll"")>
    Public Shared Sub DllImportMethod(param As Integer)
    End Sub

    Public Declare Function DeclareFunction Lib ""Dependency.dll"" (param As Integer) As Integer
End Class
");
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public async Task NoDiagnosticForMethodsWithSpecialAttributesTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
#define CONDITION_1

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

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

            await VerifyVB.VerifyAnalyzerAsync(@"
#Const CONDITION_1 = 5

Imports System
Imports System.Diagnostics
Imports System.Runtime.Serialization

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

        [Fact, WorkItem(1218, "https://github.com/dotnet/roslyn-analyzers/issues/1218")]
        public async Task NoDiagnosticForMethodsUsedAsDelegatesCSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C1
{
    private Action<object> _handler;

    public void Handler(object o1)
    {
    }

    public void SetupHandler()
    {
        _handler = Handler;
    }
}

public class C2
{
    public void Handler(object o1)
    {
    }

    public void TakesHandler(Action<object> handler)
    {
        handler(null);
    }

    public void SetupHandler()
    {
        TakesHandler(Handler);
    }
}

public class C3
{
    private Action<object> _handler;

    public C3()
    {
        _handler = Handler;
    }

    public void Handler(object o1)
    {
    }
}");
        }

        [Fact, WorkItem(1218, "https://github.com/dotnet/roslyn-analyzers/issues/1218")]
        public async Task NoDiagnosticForMethodsUsedAsDelegatesBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Public Class C1
    Private _handler As Action(Of Object)

    Public Sub Handler(o As Object)
    End Sub

    Public Sub SetupHandler()
        _handler = AddressOf Handler
    End Sub
End Class

Module M2
    Sub Handler(o As Object)
    End Sub

    Sub TakesHandler(handler As Action(Of Object))
        handler(Nothing)
    End Sub

    Sub SetupHandler()
        TakesHandler(AddressOf Handler)
    End Sub
End Module

Class C3
    Private _handler As Action(Of Object)

    Sub New()
        _handler = AddressOf Handler
    End Sub

    Sub Handler(o As Object)
    End Sub
End Class
");
        }

        [Fact, WorkItem(1218, "https://github.com/dotnet/roslyn-analyzers/issues/1218")]
        public async Task NoDiagnosticForObsoleteMethods()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C1
{
    [Obsolete]
    public void ObsoleteMethod(object o1)
    {
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C1
    <Obsolete>
    Public Sub ObsoleteMethod(o1 as Object)
    End Sub
End Class");
        }

        [Fact, WorkItem(1218, "https://github.com/dotnet/roslyn-analyzers/issues/1218")]
        public async Task NoDiagnosticMethodJustThrowsNotImplemented()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class MyAttribute: Attribute
{
    public int X;

    public MyAttribute(int x)
    {
        X = x;
    }
}
public class C1
{
    public int Prop1
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public void Method1(object o1)
    {
        throw new NotImplementedException();
    }

    public void Method2(object o1) => throw new NotImplementedException();

    [MyAttribute(0)]
    public void Method3(object o1)
    {
        throw new NotImplementedException();
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C1
    Property Prop1 As Integer
        Get
            Throw New NotImplementedException()
        End Get
        Set(ByVal value As Integer)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Sub Method1(o1 As Object)
        Throw New NotImplementedException()
    End Sub
End Class");
        }

        [Fact, WorkItem(1218, "https://github.com/dotnet/roslyn-analyzers/issues/1218")]
        public async Task NoDiagnosticMethodJustThrowsNotSupported()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C1
{
    public int Prop1
    {
        get
        {
            throw new NotSupportedException();
        }
        set
        {
            throw new NotSupportedException();
        }
    }

    public void Method1(object o1)
    {
        throw new NotSupportedException();
    }

    public void Method2(object o1) => throw new NotSupportedException();
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C1
    Property Prop1 As Integer
        Get
            Throw New NotSupportedException()
        End Get
        Set(ByVal value As Integer)
            Throw New NotSupportedException()
        End Set
    End Property

    Public Sub Method1(o1 As Object)
        Throw New NotSupportedException()
    End Sub
End Class");
        }

        [Fact]
        public async Task NoDiagnosticsForIndexer()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
class C
{
    public int this[int i]
    {
        get { return 0; }
        set { }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Public Property Item(i As Integer) As Integer
        Get
            Return 0
        End Get

        Set
        End Set
    End Property
End Class
");
        }

        [Fact]
        public async Task NoDiagnosticsForPropertySetter()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
class C
{
    public int Property
    {
        get { return 0; }
        set { }
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Public Property Property1 As Integer
        Get
            Return 0
        End Get

        Set
        End Set
    End Property
End Class
");
        }
        [Fact]
        public async Task NoDiagnosticsForFirstParameterOfExtensionMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
static class C
{
    static void ExtensionMethod(this int i) { }
    static int ExtensionMethod(this int i, int anotherParam) { return anotherParam; }
}
");
        }

        [Fact]
        public async Task NoDiagnosticsForSingleStatementMethodsWithDefaultParameters()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    public void Foo(string bar, string baz = null)
    {
        throw new NotImplementedException();
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Public Class C
    Public Sub Test(bar As String, Optional baz As String = Nothing)
        Throw New NotImplementedException()
    End Sub
End Class");
        }

        [Fact]
        [WorkItem(2589, "https://github.com/dotnet/roslyn-analyzers/issues/2589")]
        [WorkItem(2593, "https://github.com/dotnet/roslyn-analyzers/issues/2593")]
        public async Task NoDiagnosticDiscardParameterNames()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    public void M(int _, int _1, int _4)
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Public Class C
    ' _ is not an allowed identifier in VB.
    Public Sub M(_1 As Integer, _2 As Integer, _4 As Integer)
    End Sub
End Class
");
        }

        [Fact]
        [WorkItem(2466, "https://github.com/dotnet/roslyn-analyzers/issues/2466")]
        public async Task NoDiagnosticUsedLocalFunctionParameters()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    public void M()
    {
        LocalFunction(0);
        return;

        void LocalFunction(int x)
        {
            Console.WriteLine(x);
        }
    }
}
");
        }

        [Theory]
        [WorkItem(1375, "https://github.com/dotnet/roslyn-analyzers/issues/1375")]
        [InlineData("public", "dotnet_code_quality.api_surface = private")]
        [InlineData("private", "dotnet_code_quality.api_surface = internal, public")]
        [InlineData("public", "dotnet_code_quality.CA1801.api_surface = internal, private")]
        [InlineData("public", "dotnet_code_quality.CA1801.api_surface = Friend, Private")]
        [InlineData("public", "dotnet_code_quality.Usage.api_surface = internal, private")]
        [InlineData("public", @"dotnet_code_quality.api_surface = all
                                dotnet_code_quality.CA1801.api_surface = private")]
        public async Task EditorConfigConfiguration_ApiSurfaceOption(string accessibility, string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
public class C
{{
    {accessibility} void M(int unused)
    {{
    }}
}}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            }.RunAsync();

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"
Public Class C
    {accessibility} Sub M(unused As Integer)
    End Sub
End Class"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            }.RunAsync();
        }

        #endregion

        #region Unit tests for analyzer diagnostic(s)

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public async Task CSharp_DiagnosticForSimpleCasesTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

    public void UnusedErrorTypeParamMethod(UndefinedType param1) // error CS0246: The type or namespace name 'UndefinedType' could not be found.
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
          DiagnosticResult.CompilerError("CS0246").WithLocation(34, 44).WithMessage("The type or namespace name 'UndefinedType' could not be found (are you missing a using directive or an assembly reference?)"),
          // Test0.cs(34,58): warning CA1801: Parameter param1 of method UnusedErrorTypeParamMethod is never used. Remove the parameter or use it in the method body.
          GetCSharpUnusedParameterResultAt(34, 58, "param1", "UnusedErrorTypeParamMethod"));
        }

        [Fact]
        [WorkItem(459, "https://github.com/dotnet/roslyn-analyzers/issues/459")]
        public async Task Basic_DiagnosticForSimpleCasesTest()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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

    Public Sub UnusedErrorTypeParamMethod(param1 As UndefinedType) ' error BC30002: Type 'UndefinedType' is not defined.
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
      GetBasicUnusedParameterResultAt(24, 43, "param1", "UnusedErrorTypeParamMethod"),
      DiagnosticResult.CompilerError("BC30002").WithLocation(24, 53).WithMessage("Type 'UndefinedType' is not defined."));
        }

        [Fact]
        public async Task DiagnosticsForNonFirstParameterOfExtensionMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
static class C
{
    static void ExtensionMethod(this int i, int anotherParam) { }
}
",
    // Test0.cs(4,49): warning CA1801: Parameter anotherParam of method ExtensionMethod is never used. Remove the parameter or use it in the method body.
    GetCSharpUnusedParameterResultAt(4, 49, "anotherParam", "ExtensionMethod"));
        }

        [Fact]
        [WorkItem(2466, "https://github.com/dotnet/roslyn-analyzers/issues/2466")]
        public async Task DiagnosticForUnusedLocalFunctionParameters_01()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    public void M()
    {
        LocalFunction(0);
        return;

        void LocalFunction(int x)
        {
        }
    }
}",
            // Test0.cs(11,32): warning CA1801: Parameter x of method LocalFunction is never used. Remove the parameter or use it in the method body.
            GetCSharpUnusedParameterResultAt(11, 32, "x", "LocalFunction"));
        }

        [Fact]
        [WorkItem(2466, "https://github.com/dotnet/roslyn-analyzers/issues/2466")]
        public async Task DiagnosticForUnusedLocalFunctionParameters_02()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class C
{
    public void M()
    {
        // Flag unused parameter even if LocalFunction is unused.
        void LocalFunction(int x)
        {
        }
    }
}",
            // Test0.cs(9,32): warning CA1801: Parameter x of method LocalFunction is never used. Remove the parameter or use it in the method body.
            GetCSharpUnusedParameterResultAt(9, 32, "x", "LocalFunction"));
        }

        #endregion

        #region Helpers

        private static DiagnosticResult GetCSharpUnusedParameterResultAt(int line, int column, string parameterName, string methodName)
            => VerifyCS.Diagnostic(ReviewUnusedParametersAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(parameterName, methodName);

        private static DiagnosticResult GetBasicUnusedParameterResultAt(int line, int column, string parameterName, string methodName)
            => VerifyVB.Diagnostic(ReviewUnusedParametersAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(parameterName, methodName);

        #endregion
    }
}
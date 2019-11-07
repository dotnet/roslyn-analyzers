// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;

using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Maintainability.AvoidUninstantiatedInternalClassesAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpAvoidUninstantiatedInternalClassesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Maintainability.AvoidUninstantiatedInternalClassesAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability.BasicAvoidUninstantiatedInternalClassesFixer>;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class AvoidUninstantiatedInternalClassesTests
    {
        [Fact]
        public async Task CA1812_CSharp_Diagnostic_UninstantiatedInternalClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C { }
",
                GetCSharpResultAt(1, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_UninstantiatedInternalClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
End Class",
                GetBasicResultAt(1, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_UninstantiatedInternalStruct()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal struct CInternal { }");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_UninstantiatedInternalStruct()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Structure CInternal
End Structure");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_UninstantiatedPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"public class C { }");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_UninstantiatedPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Public Class C
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InstantiatedInternalClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C { }

public class D
{
    private readonly C _c = new C();
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InstantiatedInternalClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
End Class

Public Class D
     Private _c As New C
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"public class C
{
    internal class D { }
}",
                GetCSharpResultAt(3, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Public Class C
    Friend Class D
    End Class
End Class",
                GetBasicResultAt(2, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"public class C
{
    private readonly D _d = new D();

    internal class D { }
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Public Class C
    Private ReadOnly _d = New D

    Friend Class D
    End Class
End Class");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InternalModule()
        {
            // No static classes in VB.
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Module M
End Module");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InternalAbstractClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal abstract class A { }");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InternalAbstractClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend MustInherit Class A
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InternalDelegate()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
namespace N
{
    internal delegate void Del();
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InternalDelegate()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Namespace N
    Friend Delegate Sub Del()
End Namespace");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InternalEnum()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"namespace N
{
    internal enum E {}  // C# enums don't care if there are any members.
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InternalEnum()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Namespace N
    Friend Enum E
        None            ' VB enums require at least one member.
    End Enum
End Namespace");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_AttributeClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System;

internal class MyAttribute: Attribute {}
internal class MyOtherAttribute: MyAttribute {}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_AttributeClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System

Friend Class MyAttribute
    Inherits Attribute
End Class

Friend Class MyOtherAttribute
    Inherits MyAttribute
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            await new VerifyCS.Test
            {
                TestCode =
@"internal class C
{
    private static void Main() {}
}",
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            await new VerifyVB.Test
            {
                TestCode =
@"Friend Class C
    Public Shared Sub Main()
    End Sub
End Class",
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            await new VerifyCS.Test
            {
                TestCode =
@"internal class C
{
    private static int Main() { return 1; }
}",
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            await new VerifyVB.Test
            {
                TestCode =
@"Friend Class C
    Public Shared Function Main() As Integer
        Return 1
    End Function
End Class",
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningTask()
        {
            await new VerifyCS.Test
            {
                TestCode =
@" using System.Threading.Tasks;
internal static class C
{
    private static async Task Main() { await Task.Delay(1); }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp7_1,
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningTaskInt()
        {
            await new VerifyCS.Test
            {
                TestCode =
@" using System.Threading.Tasks;
internal static class C
{
    private static async Task<int> Main() { await Task.Delay(1); return 1; }
}",
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp7_1,
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_MainMethodIsNotStatic()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C
{
    private void Main() {}
}",
                GetCSharpResultAt(1, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_MainMethodIsNotStatic()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
    Private Sub Main()
    End Sub
End Class",
                GetBasicResultAt(1, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_MainMethodIsDifferentlyCased()
        {
            await new VerifyVB.Test
            {
                TestCode =
@"Friend Class C
    Private Shared Sub mAiN()
    End Sub
End Class",
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("BC30737").WithMessage("No accessible 'Main' method with an appropriate signature was found in 'TestProject'.")
                },
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                      var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                      return solution.WithProjectCompilationOptions(projectId, compilationOptions.WithOutputKind(OutputKind.ConsoleApplication));
                    }
                }
            }.RunAsync();
        }

        // The following tests are just to ensure that the messages are formatted properly
        // for types within namespaces.
        [Fact]
        public async Task CA1812_CSharp_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"namespace N
{
    internal class C { }
}",
                GetCSharpResultAt(3, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Namespace N
    Friend Class C
    End Class
End Namespace",
                GetBasicResultAt(2, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"namespace N
{
    public class C
    {
        internal class D { }
    }
}",
                GetCSharpResultAt(5, 24, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Namespace N
    Public Class C
        Friend Class D
        End Class
    End Class
End Namespace",
                GetBasicResultAt(3, 22, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_UninstantiatedInternalMef1ExportedClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System;
using System.ComponentModel.Composition;

namespace System.ComponentModel.Composition
{
    public class ExportAttribute: Attribute
    {
    }
}

[Export]
internal class C
{
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_UninstantiatedInternalMef1ExportedClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System
Imports System.ComponentModel.Composition

Namespace System.ComponentModel.Composition
    Public Class ExportAttribute
        Inherits Attribute
    End Class
End Namespace

<Export>
Friend Class C
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_UninstantiatedInternalMef2ExportedClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System;
using System.ComponentModel.Composition;

namespace System.ComponentModel.Composition
{
    public class ExportAttribute: Attribute
    {
    }
}

[Export]
internal class C
{
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_UninstantiatedInternalMef2ExportedClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System
Imports System.ComponentModel.Composition

Namespace System.ComponentModel.Composition
    Public Class ExportAttribute
        Inherits Attribute
    End Class
End Namespace

<Export>
Friend Class C
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_ImplementsIConfigurationSectionHandler()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System.Configuration;
using System.Xml;

internal class C : IConfigurationSectionHandler
{
    public object Create(object parent, object configContext, XmlNode section)
    {
        return null;
    }
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_ImplementsIConfigurationSectionHandler()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System.Configuration
Imports System.Xml

Friend Class C
    Implements IConfigurationSectionHandler
    Private Function IConfigurationSectionHandler_Create(parent As Object, configContext As Object, section As XmlNode) As Object Implements IConfigurationSectionHandler.Create
        Return Nothing
    End Function
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_DerivesFromConfigurationSection()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System.Configuration;

namespace System.Configuration
{
    public class ConfigurationSection
    {
    }
}

internal class C : ConfigurationSection
{
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_DerivesFromConfigurationSection()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System.Configuration

Namespace System.Configuration
    Public Class ConfigurationSection
    End Class
End Namespace

Friend Class C
    Inherits ConfigurationSection
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_DerivesFromSafeHandle()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System;
using System.Runtime.InteropServices;

internal class MySafeHandle : SafeHandle
{
    protected MySafeHandle(IntPtr invalidHandleValue, bool ownsHandle)
        : base(invalidHandleValue, ownsHandle)
    {
    }

    public override bool IsInvalid => true;

    protected override bool ReleaseHandle()
    {
        return true;
    }
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_DerivesFromSafeHandle()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System
Imports System.Runtime.InteropServices

Friend Class MySafeHandle
    Inherits SafeHandle

    Protected Sub New(invalidHandleValue As IntPtr, ownsHandle As Boolean)
        MyBase.New(invalidHandleValue, ownsHandle)
    End Sub

    Public Overrides ReadOnly Property IsInvalid As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides Function ReleaseHandle() As Boolean
        Return True
    End Function
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_DerivesFromTraceListener()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System.Diagnostics;

internal class MyTraceListener : TraceListener
{
    public override void Write(string message) { }
    public override void WriteLine(string message) { }
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_DerivesFromTraceListener()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System.Diagnostics

Friend Class MyTraceListener
    Inherits TraceListener

    Public Overrides Sub Write(message As String)
    End Sub

    Public Overrides Sub WriteLine(message As String)
    End Sub
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_InternalNestedTypeIsInstantiated()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C
{
    internal class C2
    {
    } 
}

public class D
{
    private readonly C.C2 _c2 = new C.C2();
}
");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_InternalNestedTypeIsInstantiated()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
    Friend Class C2
    End Class
End Class

Public Class D
    Private _c2 As new C.C2
End Class");
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_InternalNestedTypeIsNotInstantiated()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C
{
    internal class C2
    {
    } 
}",
                GetCSharpResultAt(
                    3, 20,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "C.C2"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_InternalNestedTypeIsNotInstantiated()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
    Friend Class C2
    End Class
End Class",
                GetBasicResultAt(
                    2, 18,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "C.C2"));
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_PrivateNestedTypeIsInstantiated()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal class C
{
    private readonly C2 _c2 = new C2();
    private class C2
    {
    } 
}",
                GetCSharpResultAt(
                    1, 16,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "C"));
        }

        [Fact]
        public async Task CA1812_Basic_Diagnostic_PrivateNestedTypeIsInstantiated()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Class C
    Private _c2 As New C2
    
    Private Class C2
    End Class
End Class",
                GetBasicResultAt(
                    1, 14,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "C"));
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_StaticHolderClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"internal static class C
{
    internal static void F() { }
}");
        }

        [Fact, WorkItem(1370, "https://github.com/dotnet/roslyn-analyzers/issues/1370")]
        public async Task CA1812_CSharp_NoDiagnostic_ImplicitlyInstantiatedFromSubTypeConstructor()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
internal class A
{
    public A()
    {
    }
}

internal class B : A
{
    public B()
    {
    }
}

internal class C<T>
{
}

internal class D : C<int>
{
    static void M()
    {
        var x = new B();
        var y = new D();
    }
}");
        }

        [Fact, WorkItem(1370, "https://github.com/dotnet/roslyn-analyzers/issues/1370")]
        public async Task CA1812_CSharp_NoDiagnostic_ExplicitlyInstantiatedFromSubTypeConstructor()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"
internal class A
{
    public A(int x)
    {
    }
}

internal class B : A
{
    public B(int x): base (x)
    {
    }
}

internal class C<T>
{
}

internal class D : C<int>
{
    public D(): base()
    {
    }

    static void M()
    {
        var x = new B(0);
        var y = new D();
    }
}");
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_StaticHolderClass()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Friend Module C
    Friend Sub F()
    End Sub
End Module");
        }

        [Fact]
        public async Task CA1812_CSharp_Diagnostic_EmptyInternalStaticClass()
        {
            // Note that this is not considered a "static holder class"
            // because it doesn't actually have any static members.
            await VerifyCS.VerifyAnalyzerAsync(
@"internal static class S { }",

                GetCSharpResultAt(
                    1, 23,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "S"));
        }

        [Fact]
        public async Task CA1812_CSharp_NoDiagnostic_UninstantiatedInternalClassInFriendlyAssembly()
        {
            await VerifyCS.VerifyAnalyzerAsync(
@"using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""TestProject"")]

internal class C { }"
                );
        }

        [Fact]
        public async Task CA1812_Basic_NoDiagnostic_UninstantiatedInternalClassInFriendlyAssembly()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"Imports System.Runtime.CompilerServices

<Assembly: InternalsVisibleToAttribute(""TestProject"")>

Friend Class C
End Class"
                );
        }

        [Fact, WorkItem(1370, "https://github.com/dotnet/roslyn-analyzers/issues/1370")]
        public async Task CA1812_Basic_NoDiagnostic_ImplicitlyInstantiatedFromSubTypeConstructor()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"
Friend Class A
    Public Sub New()
    End Sub
End Class

Friend Class B
    Inherits A
    Public Sub New()
    End Sub
End Class

Friend Class C(Of T)
End Class

Friend Class D
    Inherits C(Of Integer)
    Private Shared Sub M()
        Dim x = New B()
        Dim y = New D()
    End Sub
End Class");
        }

        [Fact, WorkItem(1370, "https://github.com/dotnet/roslyn-analyzers/issues/1370")]
        public async Task CA1812_Basic_NoDiagnostic_ExplicitlyInstantiatedFromSubTypeConstructor()
        {
            await VerifyVB.VerifyAnalyzerAsync(
@"
Friend Class A
    Public Sub New(ByVal x As Integer)
    End Sub
End Class

Friend Class B
    Inherits A

    Public Sub New(ByVal x As Integer)
        MyBase.New(x)
    End Sub
End Class

Friend Class C(Of T)
End Class

Friend Class D
    Inherits C(Of Integer)
    Public Sub New()
        MyBase.New()
    End Sub

    Private Shared Sub M()
        Dim x = New B(0)
        Dim y = New D()
    End Sub
End Class");
        }

        [Fact, WorkItem(1154, "https://github.com/dotnet/roslyn-analyzers/issues/1154")]
        public async Task CA1812_CSharp_GenericInternalClass_InstanciatedNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;

public static class X
{
    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare)
    {
        return source.OrderBy(new ComparisonComparer<T>(compare));
    }

    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        return source.OrderBy(t => t, comparer);
    }

    private class ComparisonComparer<T> : Comparer<T>
    {
        private readonly Comparison<T> _compare;

        public ComparisonComparer(Comparison<T> compare)
        {
            _compare = compare;
        }

        public override int Compare(T x, T y)
        {
            return _compare(x, y);
        }
    }
}
");
        }

        [Fact, WorkItem(1154, "https://github.com/dotnet/roslyn-analyzers/issues/1154")]
        public async Task CA1812_Basic_GenericInternalClass_InstanciatedNoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Module M
    <Extension()>
    Public Function OrderBy(Of T)(ByVal source As IEnumerable(Of T), compare As Comparison(Of T)) As IEnumerable(Of T)
        Return source.OrderBy(New ComparisonCompare(Of T)(compare))
    End Function

    <Extension()>
    Public Function OrderBy(Of T)(ByVal source As IEnumerable(Of T), comparer As IComparer(Of T)) As IEnumerable(Of T)
        Return source.OrderBy(Function(i) i, comparer)
    End Function

    Private Class ComparisonCompare(Of T)
        Inherits Comparer(Of T)

        Private _compare As Comparison(Of T)

        Public Sub New(compare As Comparison(Of T))
            _compare = compare
        End Sub

        Public Overrides Function Compare(x As T, y As T) As Integer
            Throw New NotImplementedException()
        End Function
    End Class
End Module
");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_CSharp_NoDiagnostic_GenericMethodWithNewConstraint()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

internal class InstantiatedType
{
}

internal static class Factory
{
    internal static T Create<T>()
        where T : new()
    {
        return new T();
    }
}

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Factory.Create<InstantiatedType>());
    }
}");
        }

        [Fact, WorkItem(1447, "https://github.com/dotnet/roslyn-analyzers/issues/1447")]
        public async Task CA1812_CSharp_NoDiagnostic_GenericMethodWithNewConstraintInvokedFromGenericMethod()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class InstantiatedClass
{
    public InstantiatedClass()
    {
    }
}

internal class InstantiatedClass2
{
    public InstantiatedClass2()
    {
    }
}

internal class InstantiatedClass3
{
    public InstantiatedClass3()
    {
    }
}

internal static class C
{
    private static T Create<T>()
        where T : new()
    {
        return new T();
    }

    public static void M<T>()
        where T : InstantiatedClass, new()
    {
        Create<T>();
    }

    public static void M2<T, T2>()
        where T : T2, new()
        where T2 : InstantiatedClass2
    {
        Create<T>();
    }

    public static void M3<T, T2, T3>()
        where T : T2, new()
        where T2 : T3
        where T3: InstantiatedClass3
    {
        Create<T>();
    }

    public static void M3()
    {
        M<InstantiatedClass>();
        M2<InstantiatedClass2, InstantiatedClass2>();
        M3<InstantiatedClass3, InstantiatedClass3, InstantiatedClass3>();
    }
}");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_Basic_NoDiagnostic_GenericMethodWithNewConstraint()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Module1
    Sub Main()
        Console.WriteLine(Create(Of InstantiatedType)())
    End Sub

    Friend Class InstantiatedType
    End Class

    Friend Function Create(Of T As New)() As T
        Return New T
    End Function
End Module");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_CSharp_NoDiagnostic_GenericTypeWithNewConstraint()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class InstantiatedType
{
}

internal class Factory<T> where T : new()
{
}

internal class Program
{
    public static void Main(string[] args)
    {
        var factory = new Factory<InstantiatedType>();
    }
}");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_Basic_NoDiagnostic_GenericTypeWithNewConstraint()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Module1
    Sub Main()
        Console.WriteLine(New Factory(Of InstantiatedType))
    End Sub

    Friend Class InstantiatedType
    End Class

    Friend Class Factory(Of T As New)
    End Class
End Module");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_CSharp_Diagnostic_NestedGenericTypeWithNoNewConstraint()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;

internal class InstantiatedType
{
}

internal class Factory<T> where T : new()
{
}

internal class Program
{
    public static void Main(string[] args)
    {
        var list = new List<Factory<InstantiatedType>>();
    }
}",
                GetCSharpResultAt(
                    4, 16,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "InstantiatedType"),
                GetCSharpResultAt(
                    8, 16,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "Factory<T>"));
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_Basic_Diagnostic_NestedGenericTypeWithNoNewConstraint()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Collections.Generic

Module Library
    Friend Class InstantiatedType
    End Class

    Friend Class Factory(Of T As New)
    End Class

    Sub Main()
        Dim a = New List(Of Factory(Of InstantiatedType))
    End Sub
End Module",
                GetBasicResultAt(
                    5, 18,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "Library.InstantiatedType"),
                GetBasicResultAt(
                    8, 18,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "Library.Factory(Of T)"));
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_CSharp_NoDiagnostic_NestedGenericTypeWithNewConstraint()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;

internal class InstantiatedType
{
}

internal class Factory1<T> where T : new()
{
}

internal class Factory2<T> where T : new()
{
}

internal class Program
{
    public static void Main(string[] args)
    {
        var factory = new Factory1<Factory2<InstantiatedType>>();
    }
}");
        }

        [Fact, WorkItem(1158, "https://github.com/dotnet/roslyn-analyzers/issues/1158")]
        public async Task CA1812_Basic_NoDiagnostic_NestedGenericTypeWithNewConstraint()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.Collections.Generic

Module Library
    Friend Class InstantiatedType
    End Class

    Friend Class Factory1(Of T As New)
    End Class

    Friend Class Factory2(Of T As New)
    End Class

    Sub Main()
        Dim a = New Factory1(Of Factory2(Of InstantiatedType))
    End Sub
End Module");
        }

        [Fact, WorkItem(1739, "https://github.com/dotnet/roslyn-analyzers/issues/1739")]
        public async Task CA1812_CSharp_NoDiagnostic_GenericTypeWithRecursiveConstraint()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public abstract class JobStateBase<TState>
    where TState : JobStateBase<TState>, new()
{
    public void SomeFunction ()
    {
        new JobStateChangeHandler<TState>();
    }
}

public class JobStateChangeHandler<TState>
    where TState : JobStateBase<TState>, new()
{
}
");
        }

        [Fact, WorkItem(2751, "https://github.com/dotnet/roslyn-analyzers/issues/2751")]
        public async Task CA1812_CSharp_NoDiagnostic_TypeDeclaredInCoClassAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[CoClass(typeof(CFoo))]
internal interface IFoo {}

internal class CFoo {}
");
        }

        [Fact, WorkItem(2751, "https://github.com/dotnet/roslyn-analyzers/issues/2751")]
        public async Task CA1812_CSharp_DontFailOnInvalidCoClassUsages()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[CoClass]
internal interface IFoo1 {}

[CoClass(CFoo)]
internal interface IFoo2 {}

[CoClass(typeof(CFoo), null)]
internal interface IFoo3 {}

[CoClass(typeof(IFoo3))] // This isn't a class-type
internal interface IFoo4 {}

internal class CFoo {}  // Test0.cs(16,16): warning CA1812: CFoo is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it static (Shared in Visual Basic).
",
                DiagnosticResult.CompilerError("CS7036").WithLocation(4, 2).WithMessage("There is no argument given that corresponds to the required formal parameter 'coClass' of 'CoClassAttribute.CoClassAttribute(Type)'"),
                DiagnosticResult.CompilerError("CS0119").WithLocation(7, 10).WithMessage("'CFoo' is a type, which is not valid in the given context"),
                DiagnosticResult.CompilerError("CS1729").WithLocation(10, 2).WithMessage("'CoClassAttribute' does not contain a constructor that takes 2 arguments"),
                GetCSharpResultAt(16, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "CFoo"));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, params string[] arguments)
            => new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule, params string[] arguments)
            => new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
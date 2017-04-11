// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUninstantiatedInternalClassesTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClass()
        {
            VerifyCSharp(
@"internal class C { }
",
                GetCSharpResultAt(1, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClass()
        {
            VerifyBasic(
@"Friend Class C
End Class",
                GetBasicResultAt(1, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedInternalStruct()
        {
            VerifyCSharp(
@"internal struct CInternal { }");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_UninstantiatedInternalStruct()
        {
            VerifyBasic(
@"Friend Structure CInternal
End Structure");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedPublicClass()
        {
            VerifyCSharp(
@"public class C { }");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_UninstantiatedPublicClass()
        {
            VerifyBasic(
@"Public Class C
End Class");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InstantiatedInternalClass()
        {
            VerifyCSharp(
@"internal class C { }

public class D
{
    private readonly C _c = new C();
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InstantiatedInternalClass()
        {
            VerifyBasic(
@"Friend Class C
End Class

Public Class D
     Private _c As New C
End Class");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(
@"public class C
{
    internal class D { }
}",
                GetCSharpResultAt(3, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(
@"Public Class C
    Friend Class D
    End Class
End Class",
                GetBasicResultAt(2, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(
@"public class C
{
    private readonly D _d = new D();

    internal class D { }
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(
@"Public Class C
    Private ReadOnly _d = New D

    Friend Class D
    End Class
End Class");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalModule()
        {
            // No static classes in VB.
            VerifyBasic(
@"Friend Module M
End Module");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InternalAbstractClass()
        {
            VerifyCSharp(
@"internal abstract class A { }");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalAbstractClass()
        {
            VerifyBasic(
@"Friend MustInherit Class A
End Class");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InternalDelegate()
        {
            VerifyCSharp(@"
namespace N
{
    internal delegate void Del();
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalDelegate()
        {
            VerifyBasic(@"
Namespace N
    Friend Delegate Sub Del()
End Namespace");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InternalEnum()
        {
            VerifyCSharp(
@"namespace N
{
    internal enum E {}  // C# enums don't care if there are any members.
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalEnum()
        {
            VerifyBasic(
@"Namespace N
    Friend Enum E
        None            ' VB enums require at least one member.
    End Enum
End Namespace");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_AttributeClass()
        {
            VerifyCSharp(
@"using System;

internal class MyAttribute: Attribute {}
internal class MyOtherAttribute: MyAttribute {}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_AttributeClass()
        {
            VerifyBasic(
@"Imports System

Friend Class MyAttribute
    Inherits Attribute
End Class

Friend Class MyOtherAttribute
    Inherits MyAttribute
End Class");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            VerifyCSharp(
@"internal class C
{
    private static void Main() {}
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            VerifyBasic(
@"Friend Class C
    Private Shared Sub Main()
    End Sub
End Class");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            VerifyCSharp(
@"internal class C
{
    private static int Main() { return 1; }
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            VerifyBasic(
@"Friend Class C
    Private Shared Function Main() As Integer
        Return 1
    End Sub
End Class");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_MainMethodIsNotStatic()
        {
            VerifyCSharp(
@"internal class C
{
    private void Main() {}
}",
                GetCSharpResultAt(1, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_MainMethodIsNotStatic()
        {
            VerifyBasic(
@"Friend Class C
    Private Sub Main()
    End Sub
End Class",
                GetBasicResultAt(1, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_MainMethodIsDifferentlyCased()
        {
            VerifyBasic(
@"Friend Class C
    Private Shared Sub mAiN()
    End Sub
End Class");
        }

        // The following tests are just to ensure that the messages are formatted properly
        // for types within namespaces.
        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            VerifyCSharp(
@"namespace N
{
    internal class C { }
}",
                GetCSharpResultAt(3, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            VerifyBasic(
@"Namespace N
    Friend Class C
    End Class
End Namespace",
                GetBasicResultAt(2, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyBasic(
@"Namespace N
    Public Class C
        Friend Class D
        End Class
    End Class
End Namespace",
                GetBasicResultAt(3, 22, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedInternalMef1ExportedClass()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_UninstantiatedInternalMef1ExportedClass()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedInternalMef2ExportedClass()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_UninstantiatedInternalMef2ExportedClass()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_ImplementsIConfigurationSectionHandler()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_ImplementsIConfigurationSectionHandler()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_DerivesFromConfigurationSection()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_DerivesFromConfigurationSection()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_DerivesFromSafeHandle()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_DerivesFromSafeHandle()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_DerivesFromTraceListener()
        {
            VerifyCSharp(
@"using System.Diagnostics;

internal class MyTraceListener : TraceListener
{
    public override void Write(string message) { }
    public override void WriteLine(string message) { }
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_DerivesFromTraceListener()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_InternalNestedTypeIsInstantiated()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_NoDiagnostic_InternalNestedTypeIsInstantiated()
        {
            VerifyBasic(
@"Friend Class C
    Friend Class C2
    End Class
End Class

Public Class D
    Private _c2 As new C.C2
End Class");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_InternalNestedTypeIsNotInstantiated()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_Diagnostic_InternalNestedTypeIsNotInstantiated()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_Diagnostic_PrivateNestedTypeIsInstantiated()
        {
            VerifyCSharp(
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
        public void CA1812_Basic_Diagnostic_PrivateNestedTypeIsInstantiated()
        {
            VerifyBasic(
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
        public void CA1812_CSharp_NoDiagnostic_StaticHolderClass()
        {
            VerifyCSharp(
@"internal static class C
{
    internal static void F() { }
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_StaticHolderClass()
        {
            VerifyBasic(
@"Friend Module C
    Friend Sub F()
    End Sub
End Module");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_EmptyInternalStaticClass()
        {
            // Note that this is not considered a "static holder class"
            // because it doesn't actually have any static members.
            VerifyCSharp(
@"internal static class S { }",

                GetCSharpResultAt(
                    1, 23,
                    AvoidUninstantiatedInternalClassesAnalyzer.Rule,
                    "S"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedInternalClassInFriendlyAssembly()
        {
            VerifyCSharp(
@"using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""TestProject"")]

internal class C { }"
                );
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_UninstantiatedInternalClassInFriendlyAssembly()
        {
            VerifyBasic(
@"Imports System.Runtime.CompilerServices

<Assembly: InternalsVisibleToAttribute(""TestProject"")>

Friend Class C
End Class"
                );
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUninstantiatedInternalClassesAnalyzer();
        }
    }
}
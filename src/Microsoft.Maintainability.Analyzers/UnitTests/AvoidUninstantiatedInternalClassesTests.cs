// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUninstantiatedInternalClassesTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClass()
        {
            VerifyCSharp(@"
internal class C { }
",
                GetCSharpResultAt(2, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClass()
        {
            VerifyBasic(@"
Friend Class C
End Class
",
                GetBasicResultAt(2, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedInternalStruct()
        {
            VerifyCSharp(@"
internal struct CInternal { }
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_UninstantiatedInternalStruct()
        {
            VerifyBasic(@"
Friend Structure CInternal
End Structure
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_UninstantiatedPublicClass()
        {
            VerifyCSharp(@"
public class C { }
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_UninstantiatedPublicClass()
        {
            VerifyBasic(@"
Public Class C
End Class
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InstantiatedInternalClass()
        {
            VerifyCSharp(@"
internal class C { }

public class D
{
    private readonly C _c = new C();
}
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InstantiatedInternalClass()
        {
            VerifyBasic(@"
Friend Class C
End Class

Public Class D
     Private _c As New C
End Class
");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(@"
public class C
{
    internal class D { }
}
",
                GetCSharpResultAt(4, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(@"
Public Class C
    Friend Class D
    End Class
End Class
",
                GetBasicResultAt(3, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            VerifyCSharp(@"
public class C
{
    private readonly D _d = new D();

    internal class D { }
}
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InstantiatedInternalClassNestedInPublicClass()
        {
            VerifyBasic(@"
Public Class C
    Private ReadOnly _d = New D

    Friend Class D
    End Class
End Class
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_ForInternalStaticClass()
        {
            VerifyCSharp(@"
internal static class S { }
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalModule()
        {
            // No static classes in VB.
            VerifyBasic(@"
Friend Module M
End Module
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InternalAbstractClass()
        {
            VerifyCSharp(@"
internal abstract class A { }
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalAbstractClass()
        {
            VerifyBasic(@"
Friend MustInherit Class A
End Class
");
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
End Namespace
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_InternalEnum()
        {
            VerifyCSharp(@"
namespace N
{
    internal enum E {}  // C# enums don't care if there are any members.
}");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_InternalEnum()
        {
            VerifyBasic(@"
Namespace N
    Friend Enum E
        None            ' VB enums require at least one member.
    End Enum
End Namespace
");
        }

        [Fact]
        public void CA1812_CSharp_NoDiagnostic_AttributeClass()
        {
            VerifyCSharp(@"
using System;

internal class MyAttribute: Attribute {}
internal class MyOtherAttribute: MyAttribute {}
");
        }

        [Fact]
        public void CA1812_Basic_NoDiagnostic_AttributeClass()
        {
            VerifyBasic(@"
Imports System

Friend Class MyAttribute
    Inherits Attribute
End Class

Friend Class MyOtherAttribute
    Inherits MyAttribute
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            VerifyCSharp(@"
internal class C
{
    private static void Main() {}
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningVoid()
        {
            VerifyBasic(@"
Friend Class C
    Private Shared Sub Main()
    End Sub
End Class
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_CSharp_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            VerifyCSharp(@"
internal class C
{
    private static int Main() { return 1; }
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_TypeContainingAssemblyEntryPointReturningInt()
        {
            VerifyBasic(@"
Friend Class C
    Private Shared Function Main() As Integer
        Return 1
    End Sub
End Class
");
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_MainMethodWithWrongReturnType()
        {
            VerifyCSharp(@"
internal class C
{
    private static string Main() { return ""; }
}",
                GetCSharpResultAt(2, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_MainMethodWithWrongReturnType()
        {
            VerifyBasic(@"
Friend Class C
    Private Shared Function Main() As String
        Return ""
    End Sub
End Class
",
                GetBasicResultAt(2, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_MainMethodIsNotStatic()
        {
            VerifyCSharp(@"
internal class C
{
    private void Main() {}
}
",
                GetCSharpResultAt(2, 16, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_MainMethodIsNotStatic()
        {
            VerifyBasic(@"
Friend Class C
    Private Sub Main()
    End Sub
End Class
",
                GetBasicResultAt(2, 14, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/881")]
        public void CA1812_Basic_NoDiagnostic_MainMethodIsDifferentlyCased()
        {
            VerifyBasic(@"
Friend Class C
    Private Shared Sub mAiN()
    End Sub
End Class
");
        }

        // The following tests are just to ensure that the messages are formatted properly
        // for types within namespaces.
        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            VerifyCSharp(@"
namespace N
{
    internal class C { }
}
",
                GetCSharpResultAt(4, 20, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassInNamespace()
        {
            VerifyBasic(@"
Namespace N
    Friend Class C
    End Class
End Namespace
",
                GetBasicResultAt(3, 18, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C"));
        }

        [Fact]
        public void CA1812_CSharp_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyCSharp(@"
namespace N
{
    public class C
    {
        internal class D { }
    }
}
",
                GetCSharpResultAt(6, 24, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
        }

        [Fact]
        public void CA1812_Basic_Diagnostic_UninstantiatedInternalClassNestedInPublicClassInNamespace()
        {
            VerifyBasic(@"
Namespace N
    Public Class C
        Friend Class D
        End Class
    End Class
End Namespace
",
                GetBasicResultAt(4, 22, AvoidUninstantiatedInternalClassesAnalyzer.Rule, "C.D"));
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
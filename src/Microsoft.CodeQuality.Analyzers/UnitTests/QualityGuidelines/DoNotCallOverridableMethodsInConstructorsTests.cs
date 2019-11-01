// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public partial class DoNotCallOverridableMethodsInConstructorsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallOverridableMethodsInConstructorsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallOverridableMethodsInConstructorsAnalyzer();
        }

        [Fact]
        public void CA2214VirtualMethodCSharp()
        {
            VerifyCSharp(@"
class C
{
    C()
    {
        Foo();
    }

    protected virtual void Foo() { }
}
",
            GetCA2214CSharpResultAt(6, 9));
        }

        [Fact]
        public void CA2214VirtualMethodCSharpWithScope()
        {
            VerifyCSharp(@"
class C
{
    C()
    {
        Foo();
    }

    [|protected virtual void Foo() { }|]
}
");
        }

        [Fact]
        public void CA2214VirtualMethodBasic()
        {
            VerifyBasic(@"
Class C
    Public Sub New()
        Foo()
    End Sub
    Overridable Sub Foo()
    End Sub
End Class
",
            GetCA2214BasicResultAt(4, 9));
        }

        [Fact]
        public void CA2214VirtualMethodBasicwithScope()
        {
            VerifyBasic(@"
Class C
    Public Sub New()
        Foo()
    End Sub
    [|Overridable Sub Foo()
    End Sub|]
End Class
");
        }

        [Fact]
        public void CA2214AbstractMethodCSharp()
        {
            VerifyCSharp(@"
abstract class C
{
    C()
    {
        Foo();
    }

    protected abstract void Foo();
}
",
            GetCA2214CSharpResultAt(6, 9));
        }

        [Fact]
        public void CA2214AbstractMethodBasic()
        {
            VerifyBasic(@"
MustInherit Class C
    Public Sub New()
        Foo()
    End Sub
    MustOverride Sub Foo()
End Class
",
            GetCA2214BasicResultAt(4, 9));
        }

        [Fact]
        public void CA2214MultipleInstancesCSharp()
        {
            VerifyCSharp(@"
abstract class C
{
    C()
    {
        Foo();
        Bar();
    }

    protected abstract void Foo();
    protected virtual void Bar() { }
}
",
            GetCA2214CSharpResultAt(6, 9),
            GetCA2214CSharpResultAt(7, 9));
        }

        [Fact]
        public void CA2214MultipleInstancesBasic()
        {
            VerifyBasic(@"
MustInherit Class C
    Public Sub New()
        Foo()
        Bar()
    End Sub
    MustOverride Sub Foo()
    Overridable Sub Bar()
    End Sub
End Class
",
           GetCA2214BasicResultAt(4, 9),
           GetCA2214BasicResultAt(5, 9));
        }

        [Fact]
        public void CA2214NotTopLevelCSharp()
        {
            VerifyCSharp(@"
abstract class C
{
    C()
    {
        if (true)
        {
            Foo();
        }

        if (false)
        {
            Foo(); // also check unreachable code
        }
    }

    protected abstract void Foo();
}
",
            GetCA2214CSharpResultAt(8, 13),
            GetCA2214CSharpResultAt(13, 13));
        }

        [Fact]
        public void CA2214NotTopLevelBasic()
        {
            VerifyBasic(@"
MustInherit Class C
    Public Sub New()
        If True Then
            Foo()
        End If

        If False Then
            Foo() ' also check unreachable code
        End If
    End Sub
    MustOverride Sub Foo()
End Class
",
            GetCA2214BasicResultAt(5, 13),
            GetCA2214BasicResultAt(9, 13));
        }

        [Fact]
        public void CA2214NoDiagnosticsOutsideConstructorCSharp()
        {
            VerifyCSharp(@"
abstract class C
{
    protected abstract void Foo();

    void Method()
    {
        Foo();
    }
}
");
        }

        [Fact]
        public void CA2214NoDiagnosticsOutsideConstructorBasic()
        {
            VerifyBasic(@"
MustInherit Class C
    MustOverride Sub Foo()

    Sub Method()
        Foo()
    End Sub
End Class
");
        }

        [Fact]
        public void CA2214SpecialInheritanceCSharp()
        {
            var source = @"
abstract class C : System.Web.UI.Control
{
    C()
    {
        // no diagnostics because we inherit from System.Web.UI.Control
        Foo();
        OnLoad(null);
    }

    protected abstract void Foo();
}

abstract class D : System.Windows.Forms.Control
{
    D()
    {
        // no diagnostics because we inherit from System.Windows.Forms.Control
        Foo();
        OnPaint(null);
    }

    protected abstract void Foo();
}

class ControlBase : System.Windows.Forms.Control
{
}

class E : ControlBase
{
    E()
    {
        OnGotFocus(null); // no diagnostics when we're not an immediate descendant of a special class
    }
}

abstract class F : System.ComponentModel.Component
{
    F()
    {
        // no diagnostics because we inherit from System.ComponentModel.Component
        Foo();
    }

    protected abstract void Foo();
}
";
            Document document = CreateDocument(source, LanguageNames.CSharp);
            Project project = document.Project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(System.Web.UI.Control).Assembly.Location));
            project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Control).Assembly.Location));
            DiagnosticAnalyzer analyzer = GetCSharpDiagnosticAnalyzer();
            GetSortedDiagnostics(analyzer, project.Documents.Single()).Verify(analyzer, GetDefaultPath(LanguageNames.CSharp));
        }

        [Fact]
        public void CA2214SpecialInheritanceBasic()
        {
            var source = @"
MustInherit Class C
    Inherits System.Web.UI.Control
    Public Sub New()
        ' no diagnostics because we inherit from System.Web.UI.Control
        Foo()
        OnLoad(Nothing)
    End Sub
    MustOverride Sub Foo()
End Class

MustInherit Class D
    Inherits System.Windows.Forms.Control
    Public Sub New()
        ' no diagnostics because we inherit from System.Windows.Forms.Control
        Foo()
        OnPaint(Nothing)
    End Sub
    MustOverride Sub Foo()
End Class

Class ControlBase
    Inherits System.Windows.Forms.Control
End Class

Class E
    Inherits ControlBase
    Public Sub New()
        OnGotFocus(Nothing) ' no diagnostics when we're not an immediate descendant of a special class
    End Sub
End Class

MustInherit Class F
    Inherits System.ComponentModel.Component
    Public Sub New()
        ' no diagnostics because we inherit from System.ComponentModel.Component
        Foo()
    End Sub
    MustOverride Sub Foo()
End Class
";
            Document document = CreateDocument(source, LanguageNames.VisualBasic);
            Project project = document.Project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(System.Web.UI.Control).Assembly.Location));
            project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Control).Assembly.Location));
            DiagnosticAnalyzer analyzer = GetBasicDiagnosticAnalyzer();
            GetSortedDiagnostics(analyzer, project.Documents.Single()).Verify(analyzer, GetDefaultPath(LanguageNames.VisualBasic));
        }

        [Fact]
        public void CA2214VirtualOnOtherClassesCSharp()
        {
            VerifyCSharp(@"
class D
{
    public virtual void Foo() {}
}

class C
{
    public C(object obj, D d)
    {
        if (obj.Equals(d))
        {
            d.Foo();
        }
    }
}
");
        }

        [Fact]
        public void CA2214VirtualOnOtherClassesBasic()
        {
            VerifyBasic(@"
Class D
    Public Overridable Sub Foo()
    End Sub
End Class

Class C
    Public Sub New(obj As Object, d As D)
        If obj.Equals(d) Then
            d.Foo()
        End If
    End Sub
End Class
");
        }

        [Fact, WorkItem(1652, "https://github.com/dotnet/roslyn-analyzers/issues/1652")]
        public void CA2214VirtualInvocationsInLambdaCSharp()
        {
            VerifyCSharp(@"
using System;

internal abstract class A
{
    private readonly Lazy<int> _lazyField;
    protected A()
    {
        _lazyField = new Lazy<int>(() => M());
    }

    protected abstract int M();
}
");
        }

        [Fact, WorkItem(1652, "https://github.com/dotnet/roslyn-analyzers/issues/1652")]
        public void CA2214VirtualInvocationsInLambdaBasic()
        {
            VerifyBasic(@"
Imports System

Friend MustInherit Class A
    Private ReadOnly _lazyField As Lazy(Of Integer)

    Protected Sub New()
        _lazyField = New Lazy(Of Integer)(Function() M())
    End Sub

    Protected MustOverride Function M() As Integer
End Class
");
        }

        private static DiagnosticResult GetCA2214CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotCallOverridableMethodsInConstructorsAnalyzer.RuleId, MicrosoftCodeQualityAnalyzersResources.DoNotCallOverridableMethodsInConstructors);
        }

        private static DiagnosticResult GetCA2214BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotCallOverridableMethodsInConstructorsAnalyzer.RuleId, MicrosoftCodeQualityAnalyzersResources.DoNotCallOverridableMethodsInConstructors);
        }
    }
}
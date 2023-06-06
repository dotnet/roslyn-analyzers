// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpProvideObsoleteAttributeMessageAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicProvideObsoleteAttributeMessageAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ProvideObsoleteAttributeMessageTests
    {
        [Fact]
        public async Task CSharpSimpleCasesAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[{|#0:Obsolete|}]
public class A
{
    [{|#1:Obsolete|}]
    public A() { }
    [{|#2:Obsolete("""")|}]
    public int field;
    [{|#3:Obsolete|}]
    public int Property { get; set; }
    [{|#4:Obsolete|}]
    public void Method() {}
    [{|#5:Obsolete|}]
    public event EventHandler<int> FieldEvent;
    [{|#6:Obsolete|}]
    public event EventHandler<int> PropertyEvent
    {
        add { }
        remove { }
    }
}
[{|#7:Obsolete|}]
public interface I {}
[{|#8:Obsolete|}]
public delegate void del(int x);
",
            VerifyCS.Diagnostic().WithLocation(0).WithArguments("A"),
            VerifyCS.Diagnostic().WithLocation(1).WithArguments("A"),
            VerifyCS.Diagnostic().WithLocation(2).WithArguments("field"),
            VerifyCS.Diagnostic().WithLocation(3).WithArguments("Property"),
            VerifyCS.Diagnostic().WithLocation(4).WithArguments("Method"),
            VerifyCS.Diagnostic().WithLocation(5).WithArguments("FieldEvent"),
            VerifyCS.Diagnostic().WithLocation(6).WithArguments("PropertyEvent"),
            VerifyCS.Diagnostic().WithLocation(7).WithArguments("I"),
            VerifyCS.Diagnostic().WithLocation(8).WithArguments("del"));
        }

        [Fact]
        public async Task BasicSimpleCasesAsync()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

<Obsolete>
Public Class A
    <Obsolete>
    Public Sub New()
    End Sub
    <Obsolete("""")>
    Public field As Integer
    <Obsolete>
    Public Property prop As Integer
    <Obsolete>
    Public Sub Method()
    End Sub
    <Obsolete>
    Public Event event1 As EventHandler(Of Integer)
End Class
<Obsolete>
Public Interface I
End Interface
<Obsolete>
Public Delegate Sub del(x As Integer)
",
            GetBasicResultAt(4, 2, "A"),
            GetBasicResultAt(6, 6, "New"),
            GetBasicResultAt(9, 6, "field"),
            GetBasicResultAt(11, 6, "prop"),
            GetBasicResultAt(13, 6, "Method"),
            GetBasicResultAt(16, 6, "event1"),
            GetBasicResultAt(19, 2, "I"),
            GetBasicResultAt(22, 2, "del"));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CSharpNoDiagnosticsForInternalAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Obsolete]
class A
{
    [Obsolete]
    A() { }
    [Obsolete("""")]
    int field;
    [Obsolete]
    int Property { get; set; }
    [Obsolete]
    void Method() {}
    [Obsolete]
    event EventHandler<int> event1;
}
[Obsolete]
interface I {}
[Obsolete]
delegate void del(int x);
");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task BasicNoDiagnosticsForInternalAsync()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

<Obsolete>
Class A
    <Obsolete>
    Sub New()
    End Sub
    <Obsolete("""")>
    Dim field As Integer
    <Obsolete>
    Property prop As Integer
    <Obsolete>
    Sub Method()
    End Sub
    <Obsolete>
    Event event1 As EventHandler(Of Integer)
End Class
<Obsolete>
Interface I
End Interface
<Obsolete>
Delegate Sub del(x As Integer)
");
        }

        [Fact]
        public async Task CSharpNoDiagnosticsAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Obsolete(""message"")]
class A
{
    [Obsolete(""valid"")]
    A() { }
    [Obsolete(""valid"")]
    int field;
    [Obsolete(""valid"", true)]
    int Property { get; set; }
    [Obsolete(""valid"", false)]
    void Method() {}
}
");
        }

        [Fact]
        public async Task BasicNoDiagnosticsAsync()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

<Obsolete(""valid"")>
Class A
    <Obsolete(""valid"")>
    Sub New()
    End Sub
    <Obsolete(""valid"", True)>
    Dim field As Integer
    <Obsolete(""valid"", False)>
    Property prop As Integer
    <Obsolete(""valid"", False)>
    Sub Method()
    End Sub
End Class
");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, string symbolName)
#pragma warning disable RS0030 // Do not use banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not use banned APIs
                .WithArguments(symbolName);

        private static DiagnosticResult GetBasicResultAt(int line, int column, string symbolName)
#pragma warning disable RS0030 // Do not use banned APIs
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not use banned APIs
                .WithArguments(symbolName);
    }
}
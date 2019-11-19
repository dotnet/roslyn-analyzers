// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AvoidInfiniteRecursion,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AvoidInfiniteRecursion,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public class AvoidInfiniteRecursionTests
    {
        [Fact]
        public async Task PropertySetterRecursion_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int Abc
    {
        set
        {
            this.Abc = value;
        }
    }
}",
                GetCSharpResultAt(8, 13, "Abc"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public WriteOnly Property Abc As Integer
        Set(ByVal value As Integer)
            Me.Abc = value
        End Set
    End Property
End Class
",
                GetBasicResultAt(5, 13, "Abc"));
        }

        [Fact]
        public async Task PropertySetterMultipleRecursion_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int Abc
    {
        set
        {
            this.Abc = value;
            if (value > 42)
            {
                Abc = value;
            }
        }
    }
}",
                GetCSharpResultAt(8, 13, "Abc"),
                GetCSharpResultAt(11, 17, "Abc"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public WriteOnly Property Abc As Integer
        Set(ByVal value As Integer)
            Me.Abc = value

            If value > 42 Then
                Abc = value
            End If
        End Set
    End Property
End Class
",
                GetBasicResultAt(5, 13, "Abc"),
                GetBasicResultAt(8, 17, "Abc"));
        }

        [Fact]
        public async Task PropertySetterNoRecursionWithField_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    private int abc;

    public int Abc
    {
        set
        {
            this.abc = value;
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Private abc As Integer

    Public WriteOnly Property Abc As Integer
        Set(ByVal value As Integer)
            Me.abc = value
        End Set
    End Property
End Class
");
        }

        [Fact]
        public async Task PropertySetterNoRecursionWithOtherProperty_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class A
{
    public int Foo { get; set; }

    public int Abc
    {
        set
        {
            this.Foo = value;
        }
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class A
    Public Property Foo As Integer

    Public WriteOnly Property Abc As Integer
        Set(ByVal value As Integer)
            Me.Foo = value
        End Set
    End Property
End Class
");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string symbolName)
                => VerifyCS.Diagnostic()
                    .WithLocation(line, column)
                    .WithArguments(symbolName);

        private DiagnosticResult GetBasicResultAt(int line, int column, string symbolName)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(symbolName);
    }
}

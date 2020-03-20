// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotUseMutableStructInReadonlyField,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotUseMutableStructInReadonlyField,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotUseMutableStructInReadonlyFieldTests
    {
        [Theory]
        [InlineData("System.Threading.SpinLock")]
        [InlineData("System.Runtime.InteropServices.GCHandle")]
        public async Task CA1070_KnownMutableStruct_Diagnostic(string mutableType)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
public class Class1
{{
    public readonly {mutableType} publicField;
    private readonly {mutableType} privateField;
}}

public struct Struct1
{{
    public readonly {mutableType} publicField;
    private readonly {mutableType} privateField;
}}",
                GetCSharpResultAt(4, 22 + mutableType.Length),
                GetCSharpResultAt(5, 23 + mutableType.Length),
                GetCSharpResultAt(10, 22 + mutableType.Length),
                GetCSharpResultAt(11, 23 + mutableType.Length));

            await VerifyVB.VerifyAnalyzerAsync($@"
Public Class Class1
    Public ReadOnly publicField As {mutableType}
    Private ReadOnly privateField As {mutableType}
End Class

Public Structure Struct1
    Public ReadOnly publicField As {mutableType}
    Private ReadOnly privateField As {mutableType}
End Structure",
                GetBasicResultAt(3, 21),
                GetBasicResultAt(4, 22),
                GetBasicResultAt(8, 21),
                GetBasicResultAt(9, 22));
        }

        [Fact]
        public async Task CA1070_OtherStruct_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Class1
{
    public readonly int publicInt;
    private readonly int privateInt;

    public readonly Shape publicStruct;
    private readonly Shape privateStruct;
}

public struct Struct1
{
    public readonly int publicInt;
    private readonly int privateInt;

    public readonly Shape publicStruct;
    private readonly Shape privateStruct;
}

public struct Shape {}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Class1
    Public ReadOnly publicInt As Integer
    Private ReadOnly privateInt As Integer

    Public ReadOnly publicStruct As Shape
    Private ReadOnly privateStruct As Shape
End Class

Public Structure Struct1
    Public ReadOnly publicInt As Integer
    Private ReadOnly privateInt As Integer

    Public ReadOnly publicStruct As Shape
    Private ReadOnly privateStruct As Shape
End Structure

Public Structure Shape
End Structure");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column);
    }
}

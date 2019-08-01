// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUsePropertyInsteadOfCountMethodWhenAvailableFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicUsePropertyInsteadOfCountMethodWhenAvailableFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public static partial class UsePropertyInsteadOfCountMethodWhenAvailableTests
    {
        [Theory]
        [InlineData("string[]", nameof(Array.Length))]
        [InlineData("System.Collections.Immutable.ImmutableArray<int>", nameof(ImmutableArray<int>.Length))]
        [InlineData("System.Collections.Generic.List<int>", nameof(List<int>.Count))]
        [InlineData("System.Collections.Generic.IList<int>", nameof(IList<int>.Count))]
        [InlineData("System.Collections.Generic.ICollection<int>", nameof(ICollection<int>.Count))]
        public static Task CSharp_Fixed(string type, string propertyName)
            => VerifyCS.VerifyCodeFixAsync(
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().Count();
}}
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(6, 30)
                    .WithArguments(propertyName),
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().{propertyName};
}}
");

        [Theory]
        [InlineData("string()", nameof(Array.Length))]
        [InlineData("System.Collections.Immutable.ImmutableArray(Of Integer)", nameof(ImmutableArray<int>.Length))]
        public static Task Basic_Fixed(string type, string propertyName)
            => VerifyVB.VerifyCodeFixAsync(
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().Count()
    End Function
End Module
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(8, 16)
                    .WithArguments(propertyName),
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().{propertyName}
    End Function
End Module
");

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<int>")]
        public static Task CSharp_NoDiagnostic(string type)
            => VerifyCS.VerifyAnalyzerAsync(
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().Count();
}}
");

        [Theory]
        [InlineData("System.Collections.Generic.List(Of Integer)")]
        [InlineData("System.Collections.Generic.IList(Of Integer)")]
        [InlineData("System.Collections.Generic.ICollection(Of Integer)")]
        [InlineData("System.Collections.Generic.IEnumerable(Of Integer)")]
        public static Task Basic_NoDiagnostic(string type)
            => VerifyVB.VerifyAnalyzerAsync(
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().Count()
    End Function
End Module
");
    }
}

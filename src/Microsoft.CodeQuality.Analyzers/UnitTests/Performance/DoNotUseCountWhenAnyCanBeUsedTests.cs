// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>;

namespace Microsoft.CodeQuality.Analyzers.Performance.UnitTests
{

    public static partial class DoNotUseCountWhenAnyCanBeUsedTests
    {
        [Fact]
        public static Task CSharp_NoDiagnostic_CountEqualsNonZero() => VerifyCS.VerifyAnalyzerAsync(
            @"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = Enumerable.Range(0, 0).Count().Equals(1);
    }
}");

        [Fact]
        public static Task Basic_NoDiagnostic_CountEqualsNonZero() => VerifyVB.VerifyAnalyzerAsync(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count().Equals(1)
    End Sub
End Class
");

        [Fact]
        public static Task CSharp_NoDiagnostic_NotCountEqualsZero() => VerifyCS.VerifyAnalyzerAsync(
                @"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = Enumerable.Range(0, 0).Sum().Equals(0);
    }
}");

        [Fact]
        public static Task Basic_NoDiagnostic_NotCountEqualsZero() => VerifyVB.VerifyAnalyzerAsync(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Sum().Equals(0)
    End Sub
End Class
");

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value) => VerifyCS.VerifyAnalyzerAsync(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = Enumerable.Range(0, 0).Sum() {CSharpOperatorText(@operator)} {value};
    }}
}}");

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value) => VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Sum() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
");

        [Fact]
        public static Task CSharp_NoDiagnostic_NotCoveredOperator() => VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = Enumerable.Range(0, 0).Count() + 0;
    }
}");

        [Fact]
        public static Task Basic_NoDiagnostic_NotCoveredOperator() => VerifyVB.VerifyAnalyzerAsync(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count() + 0
    End Sub
End Class
");

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value) => VerifyCS.VerifyAnalyzerAsync($@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = Enumerable.Range(0, 0).Count() {CSharpOperatorText(@operator)} {value};
    }}
}}");

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value) => VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
");

        [Fact]
        public static Task CSharp_NoDiagnostic_NonZeroEqualsCount() => VerifyCS.VerifyAnalyzerAsync(
                @"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = 1.Equals(Enumerable.Range(0, 0).Count());
    }
}");

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator) => VerifyCS.VerifyAnalyzerAsync(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} Enumerable.Range(0, 0).Sum();
    }}
}}");

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator) => VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Sum()
    End Sub
End Class
");

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator) => VerifyCS.VerifyAnalyzerAsync($@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} Enumerable.Range(0, 0).Count();
    }}
}}");

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator) => VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Count()
    End Sub
End Class
");

        [Fact]
        public static Task CSharp_NoDiagnostic_NotEnumerableCountEqualsConstant() => VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Linq;
namespace N
{
    class C
    {
        void M()
        {
            var b = Enumerable.Range(0, 0).Count() == 0;
        }
    }
    static class E
    {
        public static int Count<TSource>(this System.Collections.Generic.IEnumerable<TSource> source) => 0;
    }
}");

        [Fact]
        public static Task Basic_NoDiagnostic_NotEnumerableCountEqualsConstant() => VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Namespace N
    Class C
        Sub S()
            Dim b = Enumerable.Range(0, 0).Count() = 0
        End Sub
    End Class
    Module M
        <System.Runtime.CompilerServices.ExtensionAttribute()>
        Public Function Count(Of TSource)(ByVal source As System.Collections.Generic.IEnumerable(Of TSource)) As Integer
            Return 0
        End Function
    End Module
End Namespace
");

        [Fact]
        public static Task CSharp_NoDiagnostic_EqualsNotEnumerableCount() => VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Linq;
namespace N
{
    class C
    {
        void M()
        {
            var b = 0 == Enumerable.Range(0, 0).Count();
        }
    }
    static class E
    {
        public static int Count<TSource>(this System.Collections.Generic.IEnumerable<TSource> source) => 0;
    }
}");

        [Fact]
        public static Task Basic_NoDiagnostic_EqualsNotEnumerableCount()
        {
            return VerifyVB.VerifyAnalyzerAsync(
                $@"
Imports System
Imports System.Linq
Namespace N
    Class C
        Sub S()
            Dim b = 0 = Enumerable.Range(0, 0).Count()
        End Sub
    End Class
    Module M
        <System.Runtime.CompilerServices.ExtensionAttribute()>
        Public Function Count(Of TSource)(ByVal source As System.Collections.Generic.IEnumerable(Of TSource)) As Integer
            Return 0
        End Function
    End Module
End Namespace
");
        }
    }
}

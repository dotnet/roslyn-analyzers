// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotUseCountAsyncWhenAnyAsyncCanBeUsedAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{

    public static partial class DoNotUseCountAsyncWhenAnyAsyncCanBeUsedTests
    {
        [Fact]
        public static Task CSharp_NoDiagnostic_CountEqualsNonZero()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = (await {QueryableSymbol}.CountAsync()).Equals(1);
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_NoDiagnostic_CountEqualsNonZero()
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = (Await {QueryableSymbol}.CountAsync()).Equals(1)
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Fact]
        public static Task CSharp_NoDiagnostic_NotCountEqualsZero()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = (await {QueryableSymbol}.SumAsync()).Equals(0);
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_NoDiagnostic_NotCountEqualsZero()
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = (Await {QueryableSymbol}.SumAsync()).Equals(0)
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = await {QueryableSymbol}.SumAsync() {CSharpOperatorText(@operator)} {value};
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value)
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = Await {QueryableSymbol}.SumAsync() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Fact]
        public static Task CSharp_NoDiagnostic_NotCoveredOperator()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = await {QueryableSymbol}.CountAsync() + 0;
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_NoDiagnostic_NotCoveredOperator()
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = Await {QueryableSymbol}.CountAsync() + 0
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = await {QueryableSymbol}.CountAsync() {CSharpOperatorText(@operator)} {value};
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value)
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = Await {QueryableSymbol}.CountAsync() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Fact]
        public static Task CSharp_NoDiagnostic_NonZeroEqualsCount()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = 1.Equals({QueryableSymbol}.CountAsync());
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} {QueryableSymbol}.Sum();
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator)
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} {QueryableSymbol}.Sum()
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public static Task CSharp_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} await {QueryableSymbol}.CountAsync();
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public static Task Basic_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator)
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Await {QueryableSymbol}.CountAsync()
    End Sub
End Class
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();

        [Fact]
        public static Task CSharp_NoDiagnostic_NotEnumerableCountEqualsConstant()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
class C
{{
    async void M()
    {{
        var b = await {QueryableSymbol}.CountAsync() == 0;
    }}
}}
",
                        GetCSharpNotExtensions(),
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_NoDiagnostic_NotEnumerableCountEqualsConstant()
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Class C
    Async Sub S()
        Dim b = Await {QueryableSymbol}.CountAsync() = 0
    End Sub
End Class
",
                            GetBasicNotExtensions(),
                        },
                    },
            }.RunAsync();

        [Fact]
        public static Task CSharp_NoDiagnostic_EqualsNotEnumerableCount()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
using System.Linq;
using {ExtensionsNamespace};
namespace N
{{
    class C
    {{
        async void M()
        {{
            var b = 0 == {QueryableSymbol}.CountAsync();
        }}
    }}
    static class E
    {{
        public static int CountAsync<TSource>(this System.Linq.IQueryable<TSource> source) => 0;
    }}
}}
",
                        GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass),
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_NoDiagnostic_EqualsNotEnumerableCount()
            => new VerifyVB.Test
            {
                TestState =
                    {
                        Sources =
                        {
                            $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Namespace N
    Class C
        Async Sub S()
            Dim b = 0 = {QueryableSymbol}.CountAsync()
        End Sub
    End Class
    Module M
        <System.Runtime.CompilerServices.ExtensionAttribute()>
        Public Function CountAsync(Of TSource)(ByVal source As System.Linq.IQueryable(Of TSource)) As Integer
            Return 0
        End Function
    End Module
End Namespace
",
                            GetBasicExtensions(ExtensionsNamespace, ExtensionsClass),
                        },
                    },
            }.RunAsync();
    }
}

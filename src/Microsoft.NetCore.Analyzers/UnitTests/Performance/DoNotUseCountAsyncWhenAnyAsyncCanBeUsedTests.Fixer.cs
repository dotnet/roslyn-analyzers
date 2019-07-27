// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
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
        private const string QueryableSymbol = "Enumerable.Range(0, 0).AsQueryable()";

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionCountAsync(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionCountAsyncWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionCountAsync(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionCountAsyncWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionCountAsync(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionCountAsyncWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionCountAsync(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionCountAsyncWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationCountAsync()
        {
            return CSharpZeroEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationCountAsyncWithPredicate()
        {
            return CSharpZeroEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationCountAsync()
        {
            return BasicZeroEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationCountAsyncWithPredicate()
        {
            return BasicZeroEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task CSharpCountAsyncEqualsInvocation()
        {
            return CSharpCountEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task CSharpCountEqualsInvocationCountAsyncWithPredicate()
        {
            return CSharpCountEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task BasicCountAsyncEqualsInvocation()
        {
            return BasicCountEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task BasicCountAsyncEqualsInvocationCountAsyncWithPredicate()
        {
            return BasicCountEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        private static Task CSharpLeftBinaryExpresssionTestImpl(string symbol, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return CSharpBinaryExpresssionTestImpl(symbol, "{0} {1} {2}", @operator, value, negate, hasPredicate);
        }

        private static Task CSharpRightBinaryExpresssionTestImpl(string symbol, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return CSharpBinaryExpresssionTestImpl(symbol, "{2} {1} {0}", @operator, value, negate, hasPredicate);
        }

        private static Task CSharpBinaryExpresssionTestImpl(string symbol, string patternFormat, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            var predicate = CSharpPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"await {symbol}.CountAsync({predicate})", CSharpOperatorText(@operator), value);

            return CSharpTestImpl(symbol, pattern, negate, predicate);
        }

        private static Task CSharpZeroEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl(symbol, "0.Equals({0})", hasPredicate);
        }

        private static Task CSharpCountEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl(symbol, "({0}).Equals(0)", hasPredicate);
        }

        private static Task CSharpEqualsInvocationTestImpl(string symbol, string patternFormat, bool hasPredicate)
        {
            var predicate = CSharpPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"await {symbol}.CountAsync({predicate})");

            return CSharpTestImpl(symbol, pattern, true, predicate);
        }

        private static Task CSharpTestImpl(string symbol, string pattern, bool negate, string predicate)
        {
            var extensions = GetCSharpExtensions(ExtensionsNamespace, ExtensionsClass);
            var testSource = $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = {pattern};
    }}
}}
";
            var fixedSource = $@"using System;
using System.Linq;
using {ExtensionsNamespace};
class C
{{
    async void M()
    {{
        var b = {CSharpLogicalNotText(negate)}await {symbol}.AnyAsync({predicate});
    }}
}}
";

            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        testSource,
                        extensions,
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic(DoNotUseCountAsyncWhenAnyAsyncCanBeUsedAnalyzer.RuleId).WithSpan(8, 17, 8, 17 + pattern.Length),
                    }
                },
                FixedState =
                {
                    Sources =
                    {
                        fixedSource,
                        extensions,
                    }
                },
            };

            return test.RunAsync();
        }

        private static Task BasicLeftBinaryExpresssionTestImpl(string symbol, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return BasicBinaryExpresssionTestImpl(symbol, "{0} {1} {2}", @operator, value, negate, hasPredicate);
        }

        private static Task BasicRightBinaryExpresssionTestImpl(string symbol, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return BasicBinaryExpresssionTestImpl(symbol, "{2} {1} {0}", @operator, value, negate, hasPredicate);
        }

        private static Task BasicBinaryExpresssionTestImpl(string symbol, string patternFormat, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            var predicate = BasicPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Await {symbol}.CountAsync({predicate})", BasicOperatorText(@operator), value);

            return BasicTestImpl(symbol, pattern, negate, predicate);
        }

        private static Task BasicZeroEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl(symbol, "0.Equals({0})", hasPredicate);
        }

        private static Task BasicCountEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl(symbol, "({0}).Equals(0)", hasPredicate);
        }

        private static Task BasicEqualsInvocationTestImpl(string symbol, string patternFormat, bool hasPredicate)
        {
            var predicate = BasicPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Await {symbol}.CountAsync({predicate})");

            return BasicTestImpl(symbol, pattern, true, predicate);
        }

        private static Task BasicTestImpl(string symbol, string pattern, bool negate, string predicate)
        {
            var extensions = GetBasicExtensions(ExtensionsNamespace, ExtensionsClass);
            var testSource = $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub M()
        Dim b = {pattern}
    End Sub
End Class
";
            var fixedSource = $@"Imports  System
Imports  System.Linq
Imports {ExtensionsNamespace}
Class C
    Async Sub M()
        Dim b = {BasicLogicalNotText(negate)}Await {symbol}.AnyAsync({predicate})
    End Sub
End Class
";

            var test = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        testSource,
                        extensions,
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic(DoNotUseCountAsyncWhenAnyAsyncCanBeUsedAnalyzer.RuleId).WithSpan(6, 17, 6, 17 + pattern.Length),
                    }
                },
                FixedState =
                {
                    Sources =
                    {
                        fixedSource,
                        extensions,
                    }
                },
            };

            return test.RunAsync();
        }
    }
}

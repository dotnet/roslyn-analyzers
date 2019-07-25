// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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
        private const string EnumerableSymbol = "Enumerable.Range(0, 0)";
        private const string QueryableSymbol = "Enumerable.Range(0, 0).AsQueryable()";

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionEnumerableCount(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionEnumerableCountWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionEnumerableCount(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionEnumerableCountWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionEnumerableCount(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionEnumerableCountWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionEnumerableCount(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionEnumerableCountWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(EnumerableSymbol, @operator, value, negate, true);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationEnumerableCount()
        {
            return CSharpZeroEqualsInvocationTestImpl(EnumerableSymbol, false);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationEnumerableCountWithPredicate()
        {
            return CSharpZeroEqualsInvocationTestImpl(EnumerableSymbol, true);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationEnumerableCount()
        {
            return BasicZeroEqualsInvocationTestImpl(EnumerableSymbol, false);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationEnumerableCountWithPredicate()
        {
            return BasicZeroEqualsInvocationTestImpl(EnumerableSymbol, true);
        }

        [Fact]
        public static Task CSharpEnumerableCountEqualsInvocation()
        {
            return CSharpCountEqualsInvocationTestImpl(EnumerableSymbol, false);
        }

        [Fact]
        public static Task CSharpCountEqualsInvocationEnumerableCountWithPredicate()
        {
            return CSharpCountEqualsInvocationTestImpl(EnumerableSymbol, true);
        }

        [Fact]
        public static Task BasicEnumerableCountEqualsInvocation()
        {
            return BasicCountEqualsInvocationTestImpl(EnumerableSymbol, false);
        }

        [Fact]
        public static Task BasicCountEqualsInvocationEnumerableCountWithPredicate()
        {
            return BasicCountEqualsInvocationTestImpl(EnumerableSymbol, true);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionQueryableCount(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task CSharpLeftBinaryExpresssionQueryableCountWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionQueryableCount(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task CSharpRightBinaryExpresssionQueryableCountWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionQueryableCount(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public static Task BasicLeftBinaryExpresssionQueryableCountWithPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionQueryableCount(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public static Task BasicRightBinaryExpresssionQueryableCountWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(QueryableSymbol, @operator, value, negate, true);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationQueryableCount()
        {
            return CSharpZeroEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationQueryableCountWithPredicate()
        {
            return CSharpZeroEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationQueryableCount()
        {
            return BasicZeroEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationQueryableCountWithPredicate()
        {
            return BasicZeroEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task CSharpQueryableCountEqualsInvocation()
        {
            return CSharpCountEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task CSharpCountEqualsInvocationQueryableCountWithPredicate()
        {
            return CSharpCountEqualsInvocationTestImpl(QueryableSymbol, true);
        }

        [Fact]
        public static Task BasicQueryableCountEqualsInvocation()
        {
            return BasicCountEqualsInvocationTestImpl(QueryableSymbol, false);
        }

        [Fact]
        public static Task BasicCountEqualsInvocationQueryableCountWithPredicate()
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
            var pattern = string.Format(patternFormat, $"{symbol}.Count({predicate})", CSharpOperatorText(@operator), value);

            return CSharpTestImpl(symbol, pattern, negate, predicate);
        }

        private static Task CSharpZeroEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl(symbol, "0.Equals({0})", hasPredicate);
        }

        private static Task CSharpCountEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl(symbol, "{0}.Equals(0)", hasPredicate);
        }

        private static Task CSharpEqualsInvocationTestImpl(string symbol, string patternFormat, bool hasPredicate)
        {
            var predicate = CSharpPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"{symbol}.Count({predicate})");

            return CSharpTestImpl(symbol, pattern, true, predicate);
        }

        private static async Task CSharpTestImpl(string symbol, string pattern, bool negate, string predicate)
        {
            var source = $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {pattern};
    }}
}}
";
            var fixedSource = $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {CSharpLogicalNotText(negate)}{symbol}.Any({predicate});
    }}
}}
";

            await VerifyCS.VerifyCodeFixAsync(
                source,
                VerifyCS.Diagnostic(DoNotUseCountWhenAnyCanBeUsedAnalyzer.RuleId).WithSpan(8, 17, 8, 17 + pattern.Length),
                fixedSource);
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
            var pattern = string.Format(patternFormat, $"{symbol}.Count({predicate})", BasicOperatorText(@operator), value);

            return BasicTestImpl(symbol, pattern, negate, predicate);
        }

        private static Task BasicZeroEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl(symbol, "0.Equals({0})", hasPredicate);
        }

        private static Task BasicCountEqualsInvocationTestImpl(string symbol, bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl(symbol, "{0}.Equals(0)", hasPredicate);
        }

        private static Task BasicEqualsInvocationTestImpl(string symbol, string patternFormat, bool hasPredicate)
        {
            var predicate = BasicPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"{symbol}.Count({predicate})");

            return BasicTestImpl(symbol, pattern, true, predicate);
        }

        private static async Task BasicTestImpl(string symbol, string pattern, bool negate, string predicate)
        {
            var source = $@"
Imports  System
Imports  System.Linq
Class C
    Sub M()
        Dim b = {pattern}
    End Sub
End Class
";
            var fixedSource = $@"
Imports  System
Imports  System.Linq
Class C
    Sub M()
        Dim b = {BasicLogicalNotText(negate)}{symbol}.Any({predicate})
    End Sub
End Class
";

            await VerifyVB.VerifyCodeFixAsync(
                source,
                VerifyVB.Diagnostic(DoNotUseCountWhenAnyCanBeUsedAnalyzer.RuleId).WithSpan(6, 17, 6, 17 + pattern.Length),
                fixedSource);
        }
    }
}

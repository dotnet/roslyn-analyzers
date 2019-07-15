// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Xunit;
using static Microsoft.CodeQuality.Analyzers.Performance.UnitTests.DoNotUseCountWhenAnyCanBeUsedTestData;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>;

namespace Microsoft.CodeQuality.Analyzers.Performance.UnitTests
{
    public class DoNotUseCountWhenAnyCanBeUsedFixerTests
    {
        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.LeftCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task CSharpLeftBinaryExpresssion(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(@operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.LeftCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task CSharpLeftBinaryExpresssionPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return CSharpLeftBinaryExpresssionTestImpl(@operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.RightCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task CSharpRightBinaryExpresssion(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(@operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.RightCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task CSharpRightBinaryExpresssionWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return CSharpRightBinaryExpresssionTestImpl(@operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.LeftCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task BasicLeftBinaryExpresssion(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(@operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.LeftCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task BasicLeftBinaryExpresssionPredicate(BinaryOperatorKind @operator, int value, bool negate)
        {
            return BasicLeftBinaryExpresssionTestImpl(@operator, value, negate, true);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.RightCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task BasicRightBinaryExpresssion(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(@operator, value, negate, false);
        }

        [Theory]
        [MemberData(nameof(DoNotUseCountWhenAnyCanBeUsedTestData.RightCount_Diagnostic_TheoryData), MemberType = typeof(DoNotUseCountWhenAnyCanBeUsedTestData))]
        public static Task BasicRightBinaryExpresssionWithPredicate(int value, BinaryOperatorKind @operator, bool negate)
        {
            return BasicRightBinaryExpresssionTestImpl(@operator, value, negate, true);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationCount()
        {
            return CSharpZeroEqualsInvocationTestImpl(false);
        }

        [Fact]
        public static Task CSharpZeroEqualsInvocationCountWithPredicate()
        {
            return CSharpZeroEqualsInvocationTestImpl(true);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationCount()
        {
            return BasicZeroEqualsInvocationTestImpl(false);
        }

        [Fact]
        public static Task BasicZeroEqualsInvocationCountWithPredicate()
        {
            return BasicZeroEqualsInvocationTestImpl(true);
        }

        [Fact]
        public static Task CSharpCountEqualsInvocation()
        {
            return CSharpCountEqualsInvocationTestImpl(false);
        }

        [Fact]
        public static Task CSharpCountEqualsInvocationCountWithPredicate()
        {
            return CSharpCountEqualsInvocationTestImpl(true);
        }

        [Fact]
        public static Task BasicCountEqualsInvocation()
        {
            return BasicCountEqualsInvocationTestImpl(false);
        }

        [Fact]
        public static Task BasicCountEqualsInvocationCountWithPredicate()
        {
            return BasicCountEqualsInvocationTestImpl(true);
        }

        private static Task CSharpLeftBinaryExpresssionTestImpl(BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return CSharpBinaryExpresssionTestImpl("{0} {1} {2}", @operator, value, negate, hasPredicate);
        }

        private static Task CSharpRightBinaryExpresssionTestImpl(BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return CSharpBinaryExpresssionTestImpl("{2} {1} {0}", @operator, value, negate, hasPredicate);
        }

        private static Task CSharpBinaryExpresssionTestImpl(string patternFormat, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            var predicate = CSharpPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Enumerable.Range(0, 0).Count({predicate})", CSharpOperatorText(@operator), value);

            return CSharpTestImpl(pattern, negate, predicate);
        }

        private static Task CSharpZeroEqualsInvocationTestImpl(bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl("0.Equals({0})", hasPredicate);
        }

        private static Task CSharpCountEqualsInvocationTestImpl(bool hasPredicate)
        {
            return CSharpEqualsInvocationTestImpl("{0}.Equals(0)", hasPredicate);
        }

        private static Task CSharpEqualsInvocationTestImpl(string patternFormat, bool hasPredicate)
        {
            var predicate = CSharpPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Enumerable.Range(0, 0).Count({predicate})");

            return CSharpTestImpl(pattern, true, predicate);
        }

        private static async Task CSharpTestImpl(string pattern, bool negate, string predicate)
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
        var b = {CSharpLogicalNotText(negate)}Enumerable.Range(0, 0).Any({predicate});
    }}
}}
";

            await VerifyCS.VerifyCodeFixAsync(
                source,
                VerifyCS.Diagnostic(DoNotUseCountWhenAnyCanBeUsedAnalyzer.RuleId).WithSpan(8, 17, 8, 17 + pattern.Length),
                fixedSource);
        }

        private static Task BasicLeftBinaryExpresssionTestImpl(BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return BasicBinaryExpresssionTestImpl("{0} {1} {2}", @operator, value, negate, hasPredicate);
        }

        private static Task BasicRightBinaryExpresssionTestImpl(BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            return BasicBinaryExpresssionTestImpl("{2} {1} {0}", @operator, value, negate, hasPredicate);
        }

        private static Task BasicBinaryExpresssionTestImpl(string patternFormat, BinaryOperatorKind @operator, int value, bool negate, bool hasPredicate)
        {
            var predicate = BasicPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Enumerable.Range(0, 0).Count({predicate})", BasicOperatorText(@operator), value);

            return BasicTestImpl(pattern, negate, predicate);
        }

        private static Task BasicZeroEqualsInvocationTestImpl(bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl("0.Equals({0})", hasPredicate);
        }

        private static Task BasicCountEqualsInvocationTestImpl(bool hasPredicate)
        {
            return BasicEqualsInvocationTestImpl("{0}.Equals(0)", hasPredicate);
        }

        private static Task BasicEqualsInvocationTestImpl(string patternFormat, bool hasPredicate)
        {
            var predicate = BasicPredicateText(hasPredicate);
            var pattern = string.Format(patternFormat, $"Enumerable.Range(0, 0).Count({predicate})");

            return BasicTestImpl(pattern, true, predicate);
        }

        private static async Task BasicTestImpl(string pattern, bool negate, string predicate)
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
        Dim b = {BasicLogicalNotText(negate)}Enumerable.Range(0, 0).Any({predicate})
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

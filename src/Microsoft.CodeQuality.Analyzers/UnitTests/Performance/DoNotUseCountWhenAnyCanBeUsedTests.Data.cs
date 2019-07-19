// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Operations;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Performance.UnitTests
{
    public static class DoNotUseCountWhenAnyCanBeUsedTestData
    {
        public static TheoryData<BinaryOperatorKind, int> LeftCount_NoDiagnostic_TheoryData { get; } = new TheoryData<BinaryOperatorKind, int>
        {
            { BinaryOperatorKind.Equals            , 1 },
            { BinaryOperatorKind.NotEquals         , 1 },
            { BinaryOperatorKind.LessThanOrEqual   , 1 },
            { BinaryOperatorKind.GreaterThan       , 1 },
            { BinaryOperatorKind.LessThan          , 0 },
            { BinaryOperatorKind.GreaterThanOrEqual, 0 },
            { BinaryOperatorKind.Equals            , 2 },
            { BinaryOperatorKind.NotEquals         , 2 },
            { BinaryOperatorKind.LessThanOrEqual   , 2 },
            { BinaryOperatorKind.GreaterThan       , 2 },
            { BinaryOperatorKind.LessThan          , 2 },
            { BinaryOperatorKind.GreaterThanOrEqual, 2 },
        };

        public static TheoryData<int, BinaryOperatorKind> RightCount_NoDiagnostic_TheoryData { get; } = new TheoryData<int, BinaryOperatorKind>
        {
            { 1, BinaryOperatorKind.Equals             },
            { 1, BinaryOperatorKind.NotEquals          },
            { 1, BinaryOperatorKind.LessThan           },
            { 1, BinaryOperatorKind.GreaterThanOrEqual },
            { 0, BinaryOperatorKind.GreaterThan        },
            { 0, BinaryOperatorKind.LessThanOrEqual    },
            { 2, BinaryOperatorKind.Equals             },
            { 2, BinaryOperatorKind.NotEquals          },
            { 2, BinaryOperatorKind.LessThan           },
            { 2, BinaryOperatorKind.GreaterThanOrEqual },
            { 2, BinaryOperatorKind.GreaterThan        },
            { 2, BinaryOperatorKind.LessThanOrEqual    },
        };

        public static TheoryData<BinaryOperatorKind, int, bool> LeftCount_Fixer_TheoryData { get; } = new TheoryData<BinaryOperatorKind, int, bool>
        {
            { BinaryOperatorKind.Equals            , 0 , true }, // !Any
            { BinaryOperatorKind.NotEquals         , 0 , false }, // Any
            { BinaryOperatorKind.LessThanOrEqual   , 0 , true }, // !Any
            { BinaryOperatorKind.GreaterThan       , 0 , false }, // Any
            { BinaryOperatorKind.LessThan          , 1 , true }, // !Any
            { BinaryOperatorKind.GreaterThanOrEqual, 1 , false }, // Any
        };

        public static TheoryData<BinaryOperatorKind, int> LeftCount_Diagnostic_TheoryData { get; } = Build_LeftCount_Diagnostic_TheoryData();

        private static TheoryData<BinaryOperatorKind, int> Build_LeftCount_Diagnostic_TheoryData()
        {
            var theoryData = new TheoryData<BinaryOperatorKind, int>();
            foreach (var fixerData in LeftCount_Fixer_TheoryData)
            {
                theoryData.Add((BinaryOperatorKind)fixerData[0], (int)fixerData[1]);
            }
            return theoryData;
        }

        public static TheoryData<int, BinaryOperatorKind, bool> RightCount_Fixer_TheoryData { get; } = new TheoryData<int, BinaryOperatorKind, bool>
        {
            { 0, BinaryOperatorKind.Equals             , true }, // !Any
            { 0, BinaryOperatorKind.NotEquals          , false }, // Any
            { 0, BinaryOperatorKind.LessThan           , true }, // !Any
            { 0, BinaryOperatorKind.GreaterThanOrEqual , false }, // Any
            { 1, BinaryOperatorKind.GreaterThan        , true }, // !Any
            { 1, BinaryOperatorKind.LessThanOrEqual    , false }, // Any
        };

        public static TheoryData<int, BinaryOperatorKind> RightCount_Diagnostic_TheoryData { get; } = Build_RightCount_Diagnostic_TheoryData();

        private static TheoryData<int, BinaryOperatorKind> Build_RightCount_Diagnostic_TheoryData()
        {
            var theoryData = new TheoryData<int, BinaryOperatorKind>();
            foreach (var fixerData in RightCount_Fixer_TheoryData)
            {
                theoryData.Add((int)fixerData[0], (BinaryOperatorKind)fixerData[1]);
            }
            return theoryData;
        }

        public static string CSharpOperatorText(BinaryOperatorKind binaryOperatorKind)
        {
            switch (binaryOperatorKind)
            {
                case BinaryOperatorKind.Add: return "+";
                case BinaryOperatorKind.Equals: return "==";
                case BinaryOperatorKind.GreaterThan: return ">";
                case BinaryOperatorKind.GreaterThanOrEqual: return ">=";
                case BinaryOperatorKind.LessThan: return "<";
                case BinaryOperatorKind.LessThanOrEqual: return "<=";
                case BinaryOperatorKind.NotEquals: return "!=";
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperatorKind), binaryOperatorKind, $"Invalid value: {binaryOperatorKind}");
            }
        }

        public static string CSharpLogicalNotText(bool negate) => negate ? "!" : string.Empty;

        public static string CSharpPredicateText(bool hasPredicate) => hasPredicate ? "_ => true" : string.Empty;

        public static string BasicOperatorText(BinaryOperatorKind binaryOperatorKind)
        {
            switch (binaryOperatorKind)
            {
                case BinaryOperatorKind.Add: return "+";
                case BinaryOperatorKind.Equals: return "=";
                case BinaryOperatorKind.GreaterThan: return ">";
                case BinaryOperatorKind.GreaterThanOrEqual: return ">=";
                case BinaryOperatorKind.LessThan: return "<";
                case BinaryOperatorKind.LessThanOrEqual: return "<=";
                case BinaryOperatorKind.NotEquals: return "<>";
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperatorKind), binaryOperatorKind, $"Invalid value: {binaryOperatorKind}");
            }
        }

        public static string BasicLogicalNotText(bool negate) => negate ? "Not " : string.Empty;

        public static string BasicPredicateText(bool hasPredicate) => hasPredicate ? "Function(x) True" : string.Empty;
    }
}

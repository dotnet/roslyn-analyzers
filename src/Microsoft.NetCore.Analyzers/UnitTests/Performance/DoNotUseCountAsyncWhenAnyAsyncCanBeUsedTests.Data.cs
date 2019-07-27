// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Net;
using Microsoft.CodeAnalysis.Operations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public static partial class DoNotUseCountAsyncWhenAnyAsyncCanBeUsedTests
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
            { BinaryOperatorKind.Equals            , 0 , true }, // !AnyAsync
            { BinaryOperatorKind.NotEquals         , 0 , false }, // AnyAsync
            { BinaryOperatorKind.LessThanOrEqual   , 0 , true }, // !AnyAsync
            { BinaryOperatorKind.GreaterThan       , 0 , false }, // AnyAsync
            { BinaryOperatorKind.LessThan          , 1 , true }, // !AnyAsync
            { BinaryOperatorKind.GreaterThanOrEqual, 1 , false }, // AnyAsync
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
            { 0, BinaryOperatorKind.Equals             , true }, // !AnyAsync
            { 0, BinaryOperatorKind.NotEquals          , false }, // AnyAsync
            { 0, BinaryOperatorKind.LessThan           , true }, // !AnyAsync
            { 0, BinaryOperatorKind.GreaterThanOrEqual , false }, // AnyAsync
            { 1, BinaryOperatorKind.GreaterThan        , true }, // !AnyAsync
            { 1, BinaryOperatorKind.LessThanOrEqual    , false }, // AnyAsync
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

        private static string BasicLogicalNotText(bool negate) => negate ? "Not " : string.Empty;

        private static string BasicPredicateText(bool hasPredicate) => hasPredicate ? "Function(x) True" : string.Empty;

        private const string ExtensionsNamespace = @"Microsoft.EntityFrameworkCore";

        private const string ExtensionsClass = @"EntityFrameworkQueryableExtensions";

        private static string GetCSharpExtensions(string extensionsNamespace, string extensionsClass) => $@"namespace {extensionsNamespace}
{{
    public static class {extensionsClass}
    {{
        public static System.Threading.Tasks.Task<bool> AnyAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(false);
        public static System.Threading.Tasks.Task<bool> AnyAsync<TSource>(this System.Linq.IQueryable<TSource> q, System.Linq.Expressions.Expression<System.Func<TSource, bool>> predicate) => System.Threading.Tasks.Task.FromResult(false);
        public static System.Threading.Tasks.Task<int> CountAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(0);
        public static System.Threading.Tasks.Task<int> CountAsync<TSource>(this System.Linq.IQueryable<TSource> q, System.Linq.Expressions.Expression<System.Func<TSource, bool>> predicate) => System.Threading.Tasks.Task.FromResult(0);
        public static System.Threading.Tasks.Task<int> SumAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(0);
    }}
}}
";

        private static string GetCSharpNotExtensions() => @"namespace System.Linq
{
    public static class NotTheRightExtensions
    {
        public static System.Threading.Tasks.Task<bool> AnyAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(false);
        public static System.Threading.Tasks.Task<bool> AnyAsync<TSource>(this System.Linq.IQueryable<TSource> q, System.Linq.Expressions.Expression<System.Func<TSource, bool>> predicate) => System.Threading.Tasks.Task.FromResult(false);
        public static System.Threading.Tasks.Task<int> CountAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(0);
        public static System.Threading.Tasks.Task<int> CountAsync<TSource>(this System.Linq.IQueryable<TSource> q, System.Linq.Expressions.Expression<System.Func<TSource, bool>> predicate) => System.Threading.Tasks.Task.FromResult(0);
        public static System.Threading.Tasks.Task<int> SumAsync(this System.Linq.IQueryable q) => System.Threading.Tasks.Task.FromResult(0);
    }
}
";

        private static string GetBasicExtensions(string extensionsNamespace, string extensionsClass) => $@"Namespace Global.{extensionsNamespace}
    <System.Runtime.CompilerServices.Extension>
    Public Module {extensionsClass}
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Boolean)
            Return System.Threading.Tasks.Task.FromResult(False)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(Of TSource)(q As System.Linq.IQueryable(Of TSource), predicate As System.Linq.Expressions.Expression(Of System.Func(Of TSource, Boolean))) As System.Threading.Tasks.Task(Of Boolean)
            Return System.Threading.Tasks.Task.FromResult(False)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(Of TSource)(q As System.Linq.IQueryable(Of TSource), predicate As System.Linq.Expressions.Expression(Of System.Func(Of TSource, Boolean))) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function SumAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
    End Module
End Namespace
";

        private static string GetBasicNotExtensions() => @"Namespace Global.System.Linq
    <System.Runtime.CompilerServices.Extension>
    Public Module NotTheRightExtensions
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Boolean)
            Return System.Threading.Tasks.Task.FromResult(False)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(Of TSource)(q As System.Linq.IQueryable(Of TSource), predicate As System.Linq.Expressions.Expression(Of System.Func(Of TSource, Boolean))) As System.Threading.Tasks.Task(Of Boolean)
            Return System.Threading.Tasks.Task.FromResult(False)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(Of TSource)(q As System.Linq.IQueryable(Of TSource), predicate As System.Linq.Expressions.Expression(Of System.Func(Of TSource, Boolean))) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function SumAsync(q As System.Linq.IQueryable) As System.Threading.Tasks.Task(Of Integer)
            Return System.Threading.Tasks.Task.FromResult(0)
        End Function
    End Module
End Namespace
";
    }
}

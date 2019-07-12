// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Performance.UnitTests
{
    public class DoNotUseCountWhenAnyCanBeUsedTests : DiagnosticAnalyzerTestBase
    {
        #region Data for analyzer unit tests

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

        public static TheoryData<BinaryOperatorKind, int> LeftCount_Diagnostic_TheoryData { get; } = new TheoryData<BinaryOperatorKind, int>
        {
            { BinaryOperatorKind.Equals            , 0 }, // !Any
            { BinaryOperatorKind.NotEquals         , 0 }, // Any
            { BinaryOperatorKind.LessThanOrEqual   , 0 }, // !Any
            { BinaryOperatorKind.GreaterThan       , 0 }, // Any
            { BinaryOperatorKind.LessThan          , 1 }, // !Any
            { BinaryOperatorKind.GreaterThanOrEqual, 1 }, // Any
        };

        public static TheoryData<int, BinaryOperatorKind> RightCount_Diagnostic_TheoryData { get; } = new TheoryData<int, BinaryOperatorKind>
        {
            { 0, BinaryOperatorKind.Equals             }, // !Any
            { 0, BinaryOperatorKind.NotEquals          }, // Any
            { 0, BinaryOperatorKind.LessThan           }, // Any
            { 0, BinaryOperatorKind.GreaterThanOrEqual }, // !Any
            { 1, BinaryOperatorKind.GreaterThan        }, // !Any
            { 1, BinaryOperatorKind.LessThanOrEqual    }, // Any
        };

        #endregion

        #region Unit tests for no analyzer diagnostic

        [Fact]
        public void CSharp_NoDiagnostic_CountEqualsNonZero()
        {
            VerifyCSharp(
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
        }

        [Fact]
        public void Basic_NoDiagnostic_CountEqualsNonZero()
        {
            VerifyBasic(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count().Equals(1)
    End Sub
End Class
");
        }

        [Fact]
        public void CSharp_NoDiagnostic_NotCountEqualsZero()
        {
            VerifyCSharp(
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
        }

        [Fact]
        public void Basic_NoDiagnostic_NotCountEqualsZero()
        {
            VerifyBasic(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Sum().Equals(0)
    End Sub
End Class
");
        }

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void CSharp_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value)
        {
            VerifyCSharp(
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
        }

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void Basic_NoDiagnostic_LeftNotCount(BinaryOperatorKind @operator, int value)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Sum() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
");
        }

        [Fact]
        public void CSharp_NoDiagnostic_NotCoveredOperator()
        {
            VerifyCSharp(@"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = Enumerable.Range(0, 0).Count() + 0;
    }
}");
        }

        [Fact]
        public void Basic_NoDiagnostic_NotCoveredOperator()
        {
            VerifyBasic(
                @"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count() + 0
    End Sub
End Class
");
        }

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public void CSharp_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value)
        {
            VerifyCSharp($@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = Enumerable.Range(0, 0).Count() {CSharpOperatorText(@operator)} {value};
    }}
}}");
        }

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_TheoryData))]
        public void Basic_NoDiagnostic_LeftCount(BinaryOperatorKind @operator, int value)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
");
        }

        [Fact]
        public void CSharp_NoDiagnostic_NonZeroEqualsCount()
        {
            VerifyCSharp(
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
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void CSharp_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator)
        {
            VerifyCSharp(
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
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void Basic_NoDiagnostic_RightNotCount(int value, BinaryOperatorKind @operator)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Sum()
    End Sub
End Class
");
        }

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public void CSharp_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator)
        {
            VerifyCSharp($@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} Enumerable.Range(0, 0).Count();
    }}
}}");
        }

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_TheoryData))]
        public void Basic_NoDiagnostic_RightCount(int value, BinaryOperatorKind @operator)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Count()
    End Sub
End Class
");
        }

        [Fact]
        public void CSharp_NoDiagnostic_NotEnumerableCountEqualsConstant()
        {
            VerifyCSharp(@"
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
        }

        [Fact]
        public void Basic_NoDiagnostic_NotEnumerableCountEqualsConstant()
        {
            VerifyBasic(
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
        }

        [Fact]
        public void CSharp_NoDiagnostic_EqualsNotEnumerableCount()
        {
            VerifyCSharp(@"
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
        }

        [Fact]
        public void Basic_NoDiagnostic_EqualsNotEnumerableCount()
        {
            VerifyBasic(
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

        #endregion

        #region Unit tests for analyzer diagnostic(s)

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void CSharp_Diagnostic_LeftCount(BinaryOperatorKind @operator, int value)
        {
            VerifyCSharp(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = Enumerable.Range(0, 0).Count() {CSharpOperatorText(@operator)} {value};
    }}
}}",
                GetCSharpNameofResultAt(8, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void Basic_Diagnostic_LeftCount(BinaryOperatorKind @operator, int value)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count() {BasicOperatorText(@operator)} {value}
    End Sub
End Class
",
                GetBasicNameofResultAt(6, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void CSharp_Diagnostic_LeftCountWithPredicate(BinaryOperatorKind @operator, int value)
        {
            VerifyCSharp(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = Enumerable.Range(0, 0).Count(_ => true) {CSharpOperatorText(@operator)} {value};
    }}
}}",
                GetCSharpNameofResultAt(8, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public void Basic_Diagnostic_LeftCountWithPredicate(BinaryOperatorKind @operator, int value)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = Enumerable.Range(0, 0).Count(Function(x) True) {BasicOperatorText(@operator)} {value}
    End Sub
End Class
",
                GetBasicNameofResultAt(6, 17, "x"));
        }

        [Fact]
        public void CSharp_Diagnostic_ZeroEqualsCount()
        {
            VerifyCSharp(
                @"
using System;
using System.Linq;
class C
{
    void M()
    {
        var b = 0.Equals(Enumerable.Range(0, 0).Count());
    }
}",
                GetCSharpNameofResultAt(8, 17, "x"));
        }

        [Fact]
        public void Basic_Diagnostic_ZeroEqualsCount()
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = 0.Equals(Enumerable.Range(0, 0).Count())
    End Sub
End Class
",
                GetBasicNameofResultAt(6, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void CSharp_Diagnostic_RightCount(int value, BinaryOperatorKind @operator)
        {
            VerifyCSharp(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} Enumerable.Range(0, 0).Count();
    }}
}}",
                GetCSharpNameofResultAt(8, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void Basic_Diagnostic_RightCount(int value, BinaryOperatorKind @operator)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Count()
    End Sub
End Class
",
                GetBasicNameofResultAt(6, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void CSharp_Diagnostic_RightCountWithPredicate(int value, BinaryOperatorKind @operator)
        {
            VerifyCSharp(
                $@"
using System;
using System.Linq;
class C
{{
    void M()
    {{
        var b = {value} {CSharpOperatorText(@operator)} Enumerable.Range(0, 0).Count(_ => true);
    }}
}}",
                GetCSharpNameofResultAt(8, 17, "x"));
        }

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public void Basic_Diagnostic_RightCountWithPredicate(int value, BinaryOperatorKind @operator)
        {
            VerifyBasic(
                $@"
Imports System
Imports System.Linq
Class C
    Sub S()
        Dim b = {value} {BasicOperatorText(@operator)} Enumerable.Range(0, 0).Count(Function(x) True)
    End Sub
End Class
",
                GetBasicNameofResultAt(6, 17, "x"));
        }

        #endregion

        private static string CSharpOperatorText(BinaryOperatorKind binaryOperatorKind)
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

        private static string BasicOperatorText(BinaryOperatorKind binaryOperatorKind)
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

        private DiagnosticResult GetBasicNameofResultAt(int line, int column, string name)
        {
            var message = string.Format(MicrosoftPerformanceAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedMessage, name);
            return GetBasicResultAt(line, column, DoNotUseCountWhenAnyCanBeUsedAnalyzer.RuleId, message);
        }

        private DiagnosticResult GetCSharpNameofResultAt(int line, int column, string name)
        {
            var message = string.Format(MicrosoftPerformanceAnalyzersResources.DoNotUseCountWhenAnyCanBeUsedMessage, name);
            return GetCSharpResultAt(line, column, DoNotUseCountWhenAnyCanBeUsedAnalyzer.RuleId, message);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseCountWhenAnyCanBeUsedAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseCountWhenAnyCanBeUsedAnalyzer();
        }
    }
}

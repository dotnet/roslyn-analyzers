﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.CSharp.Analyzers.Performance;
using Microsoft.NetCore.VisualBasic.Analyzers.Performance;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public partial class DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task CountEqualsNonZero_NoDiagnostic(bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionEqualsInvocationCode(1, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task NonZeroEqualsCount_NoDiagnostic(bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetEqualsTargetExpressionInvocationCode(1, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Fact]
        public Task NotCountEqualsZero_NoDiagnostic()
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionEqualsInvocationCode(0, false, "Sum" + SourceProvider.MethodSuffix),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Fact]
        public Task ZeroEqualsNotCount_NoDiagnostic()
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetEqualsTargetExpressionInvocationCode(0, false, "Sum" + SourceProvider.MethodSuffix),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public Task LeftNotCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, false, "Sum" + SourceProvider.MethodSuffix),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public Task RightNotCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, false, "Sum" + SourceProvider.MethodSuffix),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task LeftCountNotComparison_NoDiagnostic(bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(BinaryOperatorKind.Add, int.MaxValue, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task RightCountNotComparison_NoDiagnostic(bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(int.MaxValue, BinaryOperatorKind.Add, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_Predicate_TheoryData))]
        public Task LeftCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_Predicate_TheoryData))]
        public Task RightCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, withPredicate, SourceProvider.MethodName),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public Task LeftNotTargetCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate, SourceProvider.MethodName),
                        SourceProvider.TestNamespace),
                extensionsSource:
                    SourceProvider.GetExtensionsCode(SourceProvider.TestNamespace, SourceProvider.TestExtensionsClass));

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public Task RightNotTargetCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate, SourceProvider.MethodName),
                        SourceProvider.TestNamespace),
                extensionsSource:
                    SourceProvider.GetExtensionsCode(SourceProvider.TestNamespace, SourceProvider.TestExtensionsClass));

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_Predicate_TheoryData))]
        public Task LeftTargetCountComparison_Fixed(BinaryOperatorKind @operator, int value, bool withPredicate, bool negate)
            => this.VerifyAsync(
                methodName: this.SourceProvider.MethodName,
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate, SourceProvider.MethodName)),
                        SourceProvider.ExtensionsNamespace),
                fixedSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetFixedExpressionCode(withPredicate, negate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(RightCount_Fixer_Predicate_TheoryData))]
        public Task RightTargetCountComparison_Fixed(int value, BinaryOperatorKind @operator, bool withPredicate, bool negate)
            => this.VerifyAsync(
                methodName: this.SourceProvider.MethodName,
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, withPredicate, SourceProvider.MethodName)),
                        SourceProvider.ExtensionsNamespace),
                fixedSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetFixedExpressionCode(withPredicate, negate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task CountEqualsZero_Fixed(bool withPredicate)
            => this.VerifyAsync(
                methodName: this.SourceProvider.MethodName,
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionEqualsInvocationCode(0, withPredicate, SourceProvider.MethodName)),
                        SourceProvider.ExtensionsNamespace),
                fixedSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetFixedExpressionCode(withPredicate, true),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public Task ZeroEqualsCount_Fixed(bool withPredicate)
            => this.VerifyAsync(
                methodName: this.SourceProvider.MethodName,
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetEqualsTargetExpressionInvocationCode(0, withPredicate, SourceProvider.MethodName)),
                        SourceProvider.ExtensionsNamespace),
                fixedSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetFixedExpressionCode(withPredicate, true),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);
    }

    public class CSharpDoNotUseCountWhenAnyCanBeUsedTestsEnumerable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountWhenAnyCanBeUsedTestsEnumerable(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "Count",
                      "global::System.Collections.Generic.IEnumerable<int>",
                      "System.Linq",
                      "Enumerable",
                      false),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.CSharpCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"using System;
using System.Linq;
class C
{{
    private const int IntZero = 0;
    private const uint UIntZero = 0u;
    private const long LongZero = 0L;
    private const ulong ULongZero = 0Lu;
    private const int IntOne = 1;
    private const uint UIntOne = 1u;
    private const long LongOne = 1L;
    private const ulong ULongOne = 1Lu;
    System.Collections.Generic.IEnumerable<string> GetData() => default;
    void M()
    {{
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(IntZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(UIntZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(LongZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(ULongZero)|}};

        _ = {{|{this.Verifier.DiagnosticId}:IntZero.Equals(GetData().Count())|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntZero.Equals(GetData().Count())|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(GetData().Count())|}};
        _ = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(GetData().Count())|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() == IntZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= IntOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:IntZero == GetData().Count()|}};
        _ = {{|{this.Verifier.DiagnosticId}:IntOne > GetData().Count()|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() == UIntZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= UIntOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntZero == GetData().Count()|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntOne > GetData().Count()|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() == LongZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= LongOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongZero == GetData().Count()|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongOne > GetData().Count()|}};
    }}
}}
",
                    @"using System;
using System.Linq;
class C
{
    private const int IntZero = 0;
    private const uint UIntZero = 0u;
    private const long LongZero = 0L;
    private const ulong ULongZero = 0Lu;
    private const int IntOne = 1;
    private const uint UIntOne = 1u;
    private const long LongOne = 1L;
    private const ulong ULongOne = 1Lu;
    System.Collections.Generic.IEnumerable<string> GetData() => default;
    void M()
    {
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
    }
}
");
    }

    public class CSharpDoNotUseLongCountWhenAnyCanBeUsedTestsEnumerable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseLongCountWhenAnyCanBeUsedTestsEnumerable(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "LongCount",
                      "global::System.Collections.Generic.IEnumerable<int>",
                      "System.Linq",
                      "Enumerable",
                      false),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.CSharpCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"using System;
using System.Linq;
class C
{{
    private const int IntZero = 0;
    private const uint UIntZero = 0u;
    private const long LongZero = 0L;
    private const ulong ULongZero = 0Lu;
    private const int IntOne = 1;
    private const uint UIntOne = 1u;
    private const long LongOne = 1L;
    private const ulong ULongOne = 1Lu;
    System.Collections.Generic.IEnumerable<string> GetData() => default;
    void M()
    {{
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(IntZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(UIntZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(LongZero)|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(ULongZero)|}};

        _ = {{|{this.Verifier.DiagnosticId}:IntZero.Equals(GetData().LongCount())|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntZero.Equals(GetData().LongCount())|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(GetData().LongCount())|}};
        _ = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(GetData().LongCount())|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() == IntZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= IntOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:IntZero == GetData().LongCount()|}};
        _ = {{|{this.Verifier.DiagnosticId}:IntOne > GetData().LongCount()|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() == UIntZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= UIntOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntZero == GetData().LongCount()|}};
        _ = {{|{this.Verifier.DiagnosticId}:UIntOne > GetData().LongCount()|}};

        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() == LongZero|}};
        _ = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= LongOne|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongZero == GetData().LongCount()|}};
        _ = {{|{this.Verifier.DiagnosticId}:LongOne > GetData().LongCount()|}};
    }}
}}
",
                    @"using System;
using System.Linq;
class C
{
    private const int IntZero = 0;
    private const uint UIntZero = 0u;
    private const long LongZero = 0L;
    private const ulong ULongZero = 0Lu;
    private const int IntOne = 1;
    private const uint UIntOne = 1u;
    private const long LongOne = 1L;
    private const ulong ULongOne = 1Lu;
    System.Collections.Generic.IEnumerable<string> GetData() => default;
    void M()
    {
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();

        _ = !GetData().Any();
        _ = GetData().Any();
        _ = !GetData().Any();
        _ = !GetData().Any();
    }
}
");
    }

    public class BasicDoNotUseCountWhenAnyCanBeUsedTestsEnumerable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountWhenAnyCanBeUsedTestsEnumerable(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Count",
                      "Global.System.Collections.Generic.IEnumerable(Of Integer)",
                      "System.Linq",
                      "Enumerable",
                      false),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.VisualBasicCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"Imports System
Imports System.Linq
Module C
    Private Const IntegerZero As Integer = 0
    Private Const UIntegerZero As UInteger = 0
    Private Const LongZero As Long = 0L
    Private Const ULongZero As ULong = 0L
    Private Const IntegerOne As Integer = 1
    Private Const UIntegerOne As UInteger = 1
    Private Const LongOne As Long = 1L
    Private Const ULongOne As ULong = 1L
    Function GetData() As System.Collections.Generic.IEnumerable(Of String)
        Return Nothing
    End Function
    Sub M()
        Dim b As Boolean

        b = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(IntegerZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(UIntegerZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(LongZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count().Equals(ULongZero)|}}

        b = {{|{this.Verifier.DiagnosticId}:IntegerZero.Equals(GetData().Count())|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerZero.Equals(GetData().Count())|}}
        b = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(GetData().Count())|}}
        b = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(GetData().Count())|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() = IntegerZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= IntegerOne|}}
        b = {{|{this.Verifier.DiagnosticId}:IntegerZero = GetData().Count()|}}
        b = {{|{this.Verifier.DiagnosticId}:IntegerOne > GetData().Count()|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() = UIntegerZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= UIntegerOne|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerZero = GetData().Count()|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerOne > GetData().Count()|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() = LongZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().Count() >= LongOne|}}
        b = {{|{this.Verifier.DiagnosticId}:LongZero = GetData().Count()|}}
        b = {{|{this.Verifier.DiagnosticId}:LongOne > GetData().Count()|}}
    End Sub
End Module
",
                    @"Imports System
Imports System.Linq
Module C
    Private Const IntegerZero As Integer = 0
    Private Const UIntegerZero As UInteger = 0
    Private Const LongZero As Long = 0L
    Private Const ULongZero As ULong = 0L
    Private Const IntegerOne As Integer = 1
    Private Const UIntegerOne As UInteger = 1
    Private Const LongOne As Long = 1L
    Private Const ULongOne As ULong = 1L
    Function GetData() As System.Collections.Generic.IEnumerable(Of String)
        Return Nothing
    End Function
    Sub M()
        Dim b As Boolean

        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
    End Sub
End Module
");
    }

    public class BasicDoNotUseLongCountWhenAnyCanBeUsedTestsEnumerable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseLongCountWhenAnyCanBeUsedTestsEnumerable(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "LongCount",
                      "Global.System.Collections.Generic.IEnumerable(Of Integer)",
                      "System.Linq",
                      "Enumerable",
                      false),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.VisualBasicCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"Imports System
Imports System.Linq
Module C
    Private Const IntegerZero As Integer = 0
    Private Const UIntegerZero As UInteger = 0
    Private Const LongZero As Long = 0L
    Private Const ULongZero As ULong = 0L
    Private Const IntegerOne As Integer = 1
    Private Const UIntegerOne As UInteger = 1
    Private Const LongOne As Long = 1L
    Private Const ULongOne As ULong = 1L
    Function GetData() As System.Collections.Generic.IEnumerable(Of String)
        Return Nothing
    End Function
    Sub M()
        Dim b As Boolean

        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(IntegerZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(UIntegerZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(LongZero)|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount().Equals(ULongZero)|}}

        b = {{|{this.Verifier.DiagnosticId}:IntegerZero.Equals(GetData().LongCount())|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerZero.Equals(GetData().LongCount())|}}
        b = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(GetData().LongCount())|}}
        b = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(GetData().LongCount())|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() = IntegerZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= IntegerOne|}}
        b = {{|{this.Verifier.DiagnosticId}:IntegerZero = GetData().LongCount()|}}
        b = {{|{this.Verifier.DiagnosticId}:IntegerOne > GetData().LongCount()|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() = UIntegerZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= UIntegerOne|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerZero = GetData().LongCount()|}}
        b = {{|{this.Verifier.DiagnosticId}:UIntegerOne > GetData().LongCount()|}}

        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() = LongZero|}}
        b = {{|{this.Verifier.DiagnosticId}:GetData().LongCount() >= LongOne|}}
        b = {{|{this.Verifier.DiagnosticId}:LongZero = GetData().LongCount()|}}
        b = {{|{this.Verifier.DiagnosticId}:LongOne > GetData().LongCount()|}}
    End Sub
End Module
",
                    @"Imports System
Imports System.Linq
Module C
    Private Const IntegerZero As Integer = 0
    Private Const UIntegerZero As UInteger = 0
    Private Const LongZero As Long = 0L
    Private Const ULongZero As ULong = 0L
    Private Const IntegerOne As Integer = 1
    Private Const UIntegerOne As UInteger = 1
    Private Const LongOne As Long = 1L
    Private Const ULongOne As ULong = 1L
    Function GetData() As System.Collections.Generic.IEnumerable(Of String)
        Return Nothing
    End Function
    Sub M()
        Dim b As Boolean

        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()

        b = Not GetData().Any()
        b = GetData().Any()
        b = Not GetData().Any()
        b = Not GetData().Any()
    End Sub
End Module
");
    }

    public class CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryable
            : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryable(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "Count",
                      "global::System.Linq.IQueryable<int>",
                      "System.Linq",
                      "Queryable",
                      false),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }
    }

    public class CSharpDoNotUseLongCountWhenAnyCanBeUsedTestsQueryable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseLongCountWhenAnyCanBeUsedTestsQueryable(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "LongCount",
                      "global::System.Linq.IQueryable<int>",
                      "System.Linq",
                      "Queryable",
                      false),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }
    }

    public class BasicDoNotUseCountWhenAnyCanBeUsedTestsQueryable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountWhenAnyCanBeUsedTestsQueryable(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Count",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "System.Linq",
                      "Queryable",
                      false),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }
    }

    public class BasicDoNotUseLongCountWhenAnyCanBeUsedTestsQueryable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseLongCountWhenAnyCanBeUsedTestsQueryable(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "LongCount",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "System.Linq",
                      "Queryable",
                      false),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId),
                  output)
        {
        }
    }

    public class CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "Count",
                      "global::System.Linq.IQueryable<int>",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.CSharpCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"using System;
using System.Linq;
namespace System.Data.Entity
{{
    public static class QueryableExtensions
    {{
        public static System.Threading.Tasks.Task<int> CountAsync<T>(this System.Linq.IQueryable<T> source) => default;
        public static System.Threading.Tasks.Task<bool> AnyAsync<T>(this System.Linq.IQueryable<T> source) => default;
    }}
    class C
    {{
        private const int IntZero = 0;
        private const uint UIntZero = 0u;
        private const long LongZero = 0L;
        private const ulong ULongZero = 0Lu;
        private const int IntOne = 1;
        private const uint UIntOne = 1u;
        private const long LongOne = 1L;
        private const ulong ULongOne = 1Lu;
        System.Linq.IQueryable<string> GetData() => default;
        async void M()
        {{
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().CountAsync()).Equals(IntZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().CountAsync()).Equals(UIntZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().CountAsync()).Equals(LongZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().CountAsync()).Equals(ULongZero)|}};

            _ = {{|{this.Verifier.DiagnosticId}:IntZero.Equals(await GetData().CountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntZero.Equals(await GetData().CountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(await GetData().CountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(await GetData().CountAsync())|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() == IntZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() >= IntOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:IntZero == await GetData().CountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:IntOne > await GetData().CountAsync()|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() == UIntZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() >= UIntOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntZero == await GetData().CountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntOne > await GetData().CountAsync()|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() == LongZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().CountAsync() >= LongOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongZero == await GetData().CountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongOne > await GetData().CountAsync()|}};
        }}
    }}
}}
",
                    @"using System;
using System.Linq;
namespace System.Data.Entity
{
    public static class QueryableExtensions
    {
        public static System.Threading.Tasks.Task<int> CountAsync<T>(this System.Linq.IQueryable<T> source) => default;
        public static System.Threading.Tasks.Task<bool> AnyAsync<T>(this System.Linq.IQueryable<T> source) => default;
    }
    class C
    {
        private const int IntZero = 0;
        private const uint UIntZero = 0u;
        private const long LongZero = 0L;
        private const ulong ULongZero = 0Lu;
        private const int IntOne = 1;
        private const uint UIntOne = 1u;
        private const long LongOne = 1L;
        private const ulong ULongOne = 1Lu;
        System.Linq.IQueryable<string> GetData() => default;
        async void M()
        {
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
        }
    }
}
");
    }

    public class CSharpDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "LongCount",
                      "global::System.Linq.IQueryable<int>",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.CSharpCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"using System;
using System.Linq;
namespace System.Data.Entity
{{
    public static class QueryableExtensions
    {{
        public static System.Threading.Tasks.Task<int> LongCountAsync<T>(this System.Linq.IQueryable<T> source) => default;
        public static System.Threading.Tasks.Task<bool> AnyAsync<T>(this System.Linq.IQueryable<T> source) => default;
    }}
    class C
    {{
        private const int IntZero = 0;
        private const uint UIntZero = 0u;
        private const long LongZero = 0L;
        private const ulong ULongZero = 0Lu;
        private const int IntOne = 1;
        private const uint UIntOne = 1u;
        private const long LongOne = 1L;
        private const ulong ULongOne = 1Lu;
        System.Linq.IQueryable<string> GetData() => default;
        async void M()
        {{
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().LongCountAsync()).Equals(IntZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().LongCountAsync()).Equals(UIntZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().LongCountAsync()).Equals(LongZero)|}};
            _ = {{|{this.Verifier.DiagnosticId}:(await GetData().LongCountAsync()).Equals(ULongZero)|}};

            _ = {{|{this.Verifier.DiagnosticId}:IntZero.Equals(await GetData().LongCountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntZero.Equals(await GetData().LongCountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongZero.Equals(await GetData().LongCountAsync())|}};
            _ = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals(await GetData().LongCountAsync())|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() == IntZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() >= IntOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:IntZero == await GetData().LongCountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:IntOne > await GetData().LongCountAsync()|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() == UIntZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() >= UIntOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntZero == await GetData().LongCountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:UIntOne > await GetData().LongCountAsync()|}};

            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() == LongZero|}};
            _ = {{|{this.Verifier.DiagnosticId}:await GetData().LongCountAsync() >= LongOne|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongZero == await GetData().LongCountAsync()|}};
            _ = {{|{this.Verifier.DiagnosticId}:LongOne > await GetData().LongCountAsync()|}};
        }}
    }}
}}
",
                    @"using System;
using System.Linq;
namespace System.Data.Entity
{
    public static class QueryableExtensions
    {
        public static System.Threading.Tasks.Task<int> LongCountAsync<T>(this System.Linq.IQueryable<T> source) => default;
        public static System.Threading.Tasks.Task<bool> AnyAsync<T>(this System.Linq.IQueryable<T> source) => default;
    }
    class C
    {
        private const int IntZero = 0;
        private const uint UIntZero = 0u;
        private const long LongZero = 0L;
        private const ulong ULongZero = 0Lu;
        private const int IntOne = 1;
        private const uint UIntOne = 1u;
        private const long LongOne = 1L;
        private const ulong ULongOne = 1Lu;
        System.Linq.IQueryable<string> GetData() => default;
        async void M()
        {
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();

            _ = !await GetData().AnyAsync();
            _ = await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
            _ = !await GetData().AnyAsync();
        }
    }
}
");
    }

    public class BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions
            : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Count",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.VisualBasicCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"Imports System
Imports System.Linq
Namespace System.Data.Entity
    <System.Runtime.CompilerServices.Extension>
    Public Module QueryableExtensions
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Integer)
            Return Nothing
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Boolean)
            Return Nothing
        End Function
    End Module
    Module C
        Private Const IntegerZero As Integer = 0
        Private Const UIntegerZero As UInteger = 0
        Private Const LongZero As Long = 0L
        Private Const ULongZero As ULong = 0L
        Private Const IntegerOne As Integer = 1
        Private Const UIntegerOne As UInteger = 1
        Private Const LongOne As Long = 1L
        Private Const ULongOne As ULong = 1L
        Function GetData() As System.Linq.IQueryable(Of String)
            Return Nothing
        End Function
        Async Sub M()
            Dim b As Boolean

            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().CountAsync()).Equals(IntegerZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().CountAsync()).Equals(UIntegerZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().CountAsync()).Equals(LongZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().CountAsync()).Equals(ULongZero)|}}

            b = {{|{this.Verifier.DiagnosticId}:IntegerZero.Equals( Await GetData().CountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerZero.Equals( Await GetData().CountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:LongZero.Equals( Await GetData().CountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals( Await GetData().CountAsync())|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() = IntegerZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() >= IntegerOne|}}
            b = {{|{this.Verifier.DiagnosticId}:IntegerZero = Await GetData().CountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:IntegerOne > Await GetData().CountAsync()|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() = UIntegerZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() >= UIntegerOne|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerZero = Await GetData().CountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerOne > Await GetData().CountAsync()|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() = LongZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().CountAsync() >= LongOne|}}
            b = {{|{this.Verifier.DiagnosticId}:LongZero = Await GetData().CountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:LongOne > Await GetData().CountAsync()|}}
        End Sub
    End Module
End Namespace
",
                    @"Imports System
Imports System.Linq
Namespace System.Data.Entity
    <System.Runtime.CompilerServices.Extension>
    Public Module QueryableExtensions
        <System.Runtime.CompilerServices.Extension>
        Public Function CountAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Integer)
            Return Nothing
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Boolean)
            Return Nothing
        End Function
    End Module
    Module C
        Private Const IntegerZero As Integer = 0
        Private Const UIntegerZero As UInteger = 0
        Private Const LongZero As Long = 0L
        Private Const ULongZero As ULong = 0L
        Private Const IntegerOne As Integer = 1
        Private Const UIntegerOne As UInteger = 1
        Private Const LongOne As Long = 1L
        Private Const ULongOne As ULong = 1L
        Function GetData() As System.Linq.IQueryable(Of String)
            Return Nothing
        End Function
        Async Sub M()
            Dim b As Boolean

            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
        End Sub
    End Module
End Namespace
");
    }

    public class BasicDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "LongCount",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }

        [Fact]
        public Task TestConstIdentifiers()
            => Test.Utilities.VisualBasicCodeFixVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>.VerifyCodeFixAsync(
                    $@"Imports System
Imports System.Linq
Namespace System.Data.Entity
    <System.Runtime.CompilerServices.Extension>
    Public Module QueryableExtensions
        <System.Runtime.CompilerServices.Extension>
        Public Function LongCountAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Integer)
            Return Nothing
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Boolean)
            Return Nothing
        End Function
    End Module
    Module C
        Private Const IntegerZero As Integer = 0
        Private Const UIntegerZero As UInteger = 0
        Private Const LongZero As Long = 0L
        Private Const ULongZero As ULong = 0L
        Private Const IntegerOne As Integer = 1
        Private Const UIntegerOne As UInteger = 1
        Private Const LongOne As Long = 1L
        Private Const ULongOne As ULong = 1L
        Function GetData() As System.Linq.IQueryable(Of String)
            Return Nothing
        End Function
        Async Sub M()
            Dim b As Boolean

            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().LongCountAsync()).Equals(IntegerZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().LongCountAsync()).Equals(UIntegerZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().LongCountAsync()).Equals(LongZero)|}}
            b = {{|{this.Verifier.DiagnosticId}:(Await GetData().LongCountAsync()).Equals(ULongZero)|}}

            b = {{|{this.Verifier.DiagnosticId}:IntegerZero.Equals( Await GetData().LongCountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerZero.Equals( Await GetData().LongCountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:LongZero.Equals( Await GetData().LongCountAsync())|}}
            b = {{|{this.Verifier.DiagnosticId}:ULongZero.Equals( Await GetData().LongCountAsync())|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() = IntegerZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() >= IntegerOne|}}
            b = {{|{this.Verifier.DiagnosticId}:IntegerZero = Await GetData().LongCountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:IntegerOne > Await GetData().LongCountAsync()|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() = UIntegerZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() >= UIntegerOne|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerZero = Await GetData().LongCountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:UIntegerOne > Await GetData().LongCountAsync()|}}

            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() = LongZero|}}
            b = {{|{this.Verifier.DiagnosticId}:Await GetData().LongCountAsync() >= LongOne|}}
            b = {{|{this.Verifier.DiagnosticId}:LongZero = Await GetData().LongCountAsync()|}}
            b = {{|{this.Verifier.DiagnosticId}:LongOne > Await GetData().LongCountAsync()|}}
        End Sub
    End Module
End Namespace
",
                    @"Imports System
Imports System.Linq
Namespace System.Data.Entity
    <System.Runtime.CompilerServices.Extension>
    Public Module QueryableExtensions
        <System.Runtime.CompilerServices.Extension>
        Public Function LongCountAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Integer)
            Return Nothing
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function AnyAsync(source As System.Linq.IQueryable(Of String)) As System.Threading.Tasks.Task(Of Boolean)
            Return Nothing
        End Function
    End Module
    Module C
        Private Const IntegerZero As Integer = 0
        Private Const UIntegerZero As UInteger = 0
        Private Const LongZero As Long = 0L
        Private Const ULongZero As ULong = 0L
        Private Const IntegerOne As Integer = 1
        Private Const UIntegerOne As UInteger = 1
        Private Const LongOne As Long = 1L
        Private Const ULongOne As ULong = 1L
        Function GetData() As System.Linq.IQueryable(Of String)
            Return Nothing
        End Function
        Async Sub M()
            Dim b As Boolean

            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()

            b = Not Await GetData().AnyAsync()
            b = Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
            b = Not Await GetData().AnyAsync()
        End Sub
    End Module
End Namespace
");
    }

    public class CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "Count",
                      "global::System.Linq.IQueryable<int>",
                      "Microsoft.EntityFrameworkCore",
                      "EntityFrameworkQueryableExtensions",
                      true),
                  new CSharpVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }
    }

    public class BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Count",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "Microsoft.EntityFrameworkCore",
                      "EntityFrameworkQueryableExtensions",
                      true),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }
    }

    public class BasicDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseLongCountAsyncWhenAnyAsyncCanBeUsedTestsEFCoreQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "LongCount",
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "Microsoft.EntityFrameworkCore",
                      "EntityFrameworkQueryableExtensions",
                      true),
                  new BasicVerifier<DoNotUseCountWhenAnyCanBeUsedAnalyzer, BasicDoNotUseCountWhenAnyCanBeUsedFixer>(DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId),
                  output)
        {
        }
    }
}

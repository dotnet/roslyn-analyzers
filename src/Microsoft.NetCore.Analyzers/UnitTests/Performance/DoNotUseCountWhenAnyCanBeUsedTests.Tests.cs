// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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
                        SourceProvider.GetTargetExpressionEqualsInvocationCode(1, withPredicate),
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
                        SourceProvider.GetEqualsTargetExpressionInvocationCode(1, withPredicate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Fact]
        public Task NotCountEqualsZero_NoDiagnostic()
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionEqualsInvocationCode(0, false, "Sum"),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Fact]
        public Task ZeroEqualsNotCount_NoDiagnostic()
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetEqualsTargetExpressionInvocationCode(0, false, "Sum"),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_Diagnostic_TheoryData))]
        public Task LeftNotCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, false, "Sum"),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(RightCount_Diagnostic_TheoryData))]
        public Task RightNotCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, false, "Sum"),
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
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(BinaryOperatorKind.Add, int.MaxValue, withPredicate),
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
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(int.MaxValue, BinaryOperatorKind.Add, withPredicate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_NoDiagnostic_Predicate_TheoryData))]
        public Task LeftCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(RightCount_NoDiagnostic_Predicate_TheoryData))]
        public Task RightCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, withPredicate),
                        SourceProvider.ExtensionsNamespace),
                extensionsSource:
                    SourceProvider.IsAsync ? SourceProvider.GetExtensionsCode(SourceProvider.ExtensionsNamespace, SourceProvider.ExtensionsClass) : null);

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_TheoryData))]
        public Task LeftNotTargetCountComparison_NoDiagnostic(BinaryOperatorKind @operator, int value, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate),
                        SourceProvider.TestNamespace),
                extensionsSource:
                    SourceProvider.GetExtensionsCode(SourceProvider.TestNamespace, SourceProvider.TestExtensionsClass));

        [Theory]
        [MemberData(nameof(RightCount_Fixer_TheoryData))]
        public Task RightNotTargetCountComparison_NoDiagnostic(int value, BinaryOperatorKind @operator, bool withPredicate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate),
                        SourceProvider.TestNamespace),
                extensionsSource:
                    SourceProvider.GetExtensionsCode(SourceProvider.TestNamespace, SourceProvider.TestExtensionsClass));

        [Theory]
        [MemberData(nameof(LeftCount_Fixer_Predicate_TheoryData))]
        public Task LeftTargetCountComparison_Fixed(BinaryOperatorKind @operator, int value, bool withPredicate, bool negate)
            => this.VerifyAsync(
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionBinaryExpressionCode(@operator, value, withPredicate)),
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
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionBinaryExpressionCode(value, @operator, withPredicate)),
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
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetTargetExpressionEqualsInvocationCode(0, withPredicate)),
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
                testSource:
                    SourceProvider.GetCodeWithExpression(
                        SourceProvider.WithDiagnostic(SourceProvider.GetEqualsTargetExpressionInvocationCode(0, withPredicate)),
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
                      "global::System.Collections.Generic.IEnumerable<int>",
                      "System.Linq",
                      "Enumerable",
                      false),
                  new CSharpVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class BasicDoNotUseCountWhenAnyCanBeUsedTestsEnumerable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountWhenAnyCanBeUsedTestsEnumerable(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Global.System.Collections.Generic.IEnumerable(Of Integer)",
                      typeof(System.Linq.Enumerable).Namespace,
                      typeof(System.Linq.Enumerable).Name,
                      false),
                  new BasicVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryable
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryable(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "global::System.Linq.IQueryable<int>",
                      typeof(System.Linq.Queryable).Namespace,
                      typeof(System.Linq.Queryable).Name,
                      false),
                  new CSharpVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(),
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
                      "Global.System.Linq.IQueryable(Of Integer)",
                      typeof(System.Linq.Queryable).Namespace,
                      typeof(System.Linq.Queryable).Name,
                      false),
                  new BasicVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountWhenAnyCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "global::System.Linq.IQueryable<int>",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new CSharpVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class BasicDoNotUseCountWhenAnyCanBeUsedTestsQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountWhenAnyCanBeUsedTestsQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "System.Data.Entity",
                      "QueryableExtensions",
                      true),
                  new BasicVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class CSharpDoNotUseCountWhenAnyCanBeUsedTestsEFCoreQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public CSharpDoNotUseCountWhenAnyCanBeUsedTestsEFCoreQueryableExtensions(ITestOutputHelper output)
            : base(
                  new CSharpTestsSourceCodeProvider(
                      "global::System.Linq.IQueryable<int>",
                      "Microsoft.EntityFrameworkCore",
                      "EntityFrameworkQueryableExtensions",
                      true),
                  new CSharpVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }

    public class BasicDoNotUseCountWhenAnyCanBeUsedTestsEFCoreQueryableExtensions
        : DoNotUseCountWhenAnyCanBeUsedTestsBase
    {
        public BasicDoNotUseCountWhenAnyCanBeUsedTestsEFCoreQueryableExtensions(ITestOutputHelper output)
            : base(
                  new BasicTestsSourceCodeProvider(
                      "Global.System.Linq.IQueryable(Of Integer)",
                      "Microsoft.EntityFrameworkCore",
                      "EntityFrameworkQueryableExtensions",
                      true),
                  new BasicVerifier<Microsoft.NetCore.Analyzers.Performance.DoNotUseCountWhenAnyCanBeUsedAnalyzer, Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotUseCountWhenAnyCanBeUsedFixer>(),
                  output)
        {
        }
    }
}

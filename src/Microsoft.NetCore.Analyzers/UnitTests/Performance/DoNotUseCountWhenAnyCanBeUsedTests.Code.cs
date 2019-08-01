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
        protected abstract class TestsSourceCodeProvider
        {
            protected TestsSourceCodeProvider(
                string targetType,
                string extensionsNamespace,
                string extensionsClass,
                bool isAsync,
                string asyncKeyword,
                string awaitKeyword,
                string commentPrefix)
            {
                TargetType = targetType;
                ExtensionsNamespace = extensionsNamespace;
                ExtensionsClass = extensionsClass;
                IsAsync = isAsync;
                AsyncKeyword = asyncKeyword;
                AwaitKeyword = awaitKeyword;
                CommentPrefix = commentPrefix;
            }

            public string AsyncKeyword { get; }
            public string AwaitKeyword { get; }
            public string CommentPrefix { get; }
            public string TargetType { get; }
            public string TestNamespace { get; } = "Test";
            public string TestExtensionsClass { get; } = "TestExtensions";
            public string ExtensionsNamespace { get; }
            public string ExtensionsClass { get; }
            public bool IsAsync { get; }

            public string GetTargetCode(string methodName)
                => $"{(IsAsync ? $"{AwaitKeyword} " : string.Empty)}GetData().{methodName}{(IsAsync ? "Async" : string.Empty)}";

            public abstract string GetCodeWithExpression(string expression, params string[] additionalNamspaces);

            internal string WithDiagnostic(string code)
                => $"{{|{(IsAsync ? DoNotUseCountWhenAnyCanBeUsedAnalyzer.AsyncRuleId : DoNotUseCountWhenAnyCanBeUsedAnalyzer.SyncRuleId)}:{code}|}}";

            internal string GetFixedExpressionCode(bool withPredicate, bool negate)
                => $@"{GetLogicalNotText(negate)}{GetTargetExpressionCode(withPredicate, "Any")}";

            internal string GetTargetExpressionBinaryExpressionCode(int value, BinaryOperatorKind @operator, bool withPredicate, string methodName = "Count")
                => $@"{value} {GetOperatorCode(@operator)} {GetTargetExpressionCode(withPredicate, methodName)}";

            internal string GetTargetExpressionBinaryExpressionCode(BinaryOperatorKind @operator, int value, bool withPredicate, string methodName = "Count")
                => $@"{GetTargetExpressionCode(withPredicate, methodName)} {GetOperatorCode(@operator)} {value}";

            public string GetTargetExpressionEqualsInvocationCode(int value, bool withPredicate, string methodName = "Count")
                => $@"{(IsAsync ? "(" : string.Empty)}{GetTargetExpressionCode(withPredicate, methodName)}{(IsAsync ? ")" : string.Empty)}.Equals({value})";

            internal string GetEqualsTargetExpressionInvocationCode(int value, bool withPredicate, string methodName = "Count")
                => $@"{value}.Equals({GetTargetExpressionCode(withPredicate, methodName)})";

            public string GetTargetExpressionCode(bool withPredicate, string methodName = "Count")
                => $@"{GetTargetCode(methodName)}({(withPredicate ? GetPredicateCode() : string.Empty)})";

            public abstract string GetSymbolInvocationCode(string methodName, params string[] arguments);
            public abstract string GetPredicateCode();
            public abstract string GetExtensionsCode(string namespaceName, string className);
            public abstract string GetOperatorCode(BinaryOperatorKind binaryOperatorKind);
            internal abstract object GetLogicalNotText(bool negate);
        }

        protected sealed class CSharpTestsSourceCodeProvider : TestsSourceCodeProvider
        {
            public CSharpTestsSourceCodeProvider(
                string targetType,
                string extensionsNamespace,
                string extensionsClass,
                bool isAsync)
                : base(
                    targetType,
                    extensionsNamespace,
                    extensionsClass,
                    isAsync,
                    "async",
                    "await",
                    "//")
            {
            }

            public override string GetCodeWithExpression(string expression, params string[] additionalNamspaces)
            {
                var builder = new StringBuilder()
                    .AppendLine("using System;");

                foreach (var aditionalNamespace in additionalNamspaces)
                {
                    builder
                        .Append("using ")
                        .Append(aditionalNamespace)
                        .Append(";")
                        .AppendLine();
                }

                builder
                    .Append(@"namespace ")
                    .Append(TestNamespace)
                    .Append(@"
{
    class C
    {
        ")
                    .Append(TargetType)
                    .Append(@" GetData() => default;
        ");

                if (IsAsync)
                {
                    builder
                        .Append(AsyncKeyword)
                        .Append(" ");
                };

                return builder
                    .Append(@"void M()
        {
            var b = ")
                    .Append(expression)
                    .AppendLine(@";
        }
    }
}")
                    .ToString();
            }

            public override string GetExtensionsCode(string namespaceName, string className)
            {
                string targetType;
                string targetTypeOfSource;
                string methodSuffix;
                string predicate;
                string boolReturnType;
                string intReturnType;
                string boolReturnValue;
                string intReturnValue;

                if (this.IsAsync)
                {
                    targetType = "global::System.Linq.IQueryable";
                    targetTypeOfSource = "global::System.Linq.IQueryable<TSource>";
                    methodSuffix = "Async";
                    predicate = "global::System.Linq.Expressions.Expression<global::System.Func<TSource, bool>>";
                    boolReturnType = "global::System.Threading.Tasks.Task<bool>";
                    intReturnType = "global::System.Threading.Tasks.Task<int>";
                    boolReturnValue = "global::System.Threading.Tasks.Task.FromResult<bool>(default)";
                    intReturnValue = "global::System.Threading.Tasks.Task.FromResult<int>(default)";
                }
                else
                {
                    targetType = "global::System.Collections.IEnumerable";
                    targetTypeOfSource = "global::System.Collections.Generic.IEnumerable<TSource>";
                    methodSuffix = string.Empty;
                    predicate = "global::System.Func<TSource, bool>";
                    boolReturnType = "bool";
                    intReturnType = "int";
                    boolReturnValue = "default";
                    intReturnValue = "default";
                }

                return $@"namespace {namespaceName}
{{
    public static class {className}
    {{
        public static {boolReturnType} Any{methodSuffix}(this {targetType} q) => {boolReturnValue};
        public static {boolReturnType} Any{methodSuffix}<TSource>(this {targetTypeOfSource} q, {predicate} predicate) => {boolReturnValue};
        public static {intReturnType} Count{methodSuffix}(this {targetType} q) => {intReturnValue};
        public static {intReturnType} Count{methodSuffix}<TSource>(this {targetTypeOfSource} q, {predicate} predicate) => {intReturnValue};
        public static {intReturnType} Sum{methodSuffix}(this {targetType} q) => {intReturnValue};
    }}
}}
";
            }

            public override string GetOperatorCode(BinaryOperatorKind binaryOperatorKind)
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

            public override string GetPredicateCode() => "_ => true";

            public override string GetSymbolInvocationCode(string methodName, params string[] arguments)
            {
                throw new NotImplementedException();
            }

            internal override object GetLogicalNotText(bool negate) => negate ? "!" : string.Empty;
        }

        protected sealed class BasicTestsSourceCodeProvider : TestsSourceCodeProvider
        {
            public BasicTestsSourceCodeProvider(
                string targetType,
                string extensionsNamespace,
                string extensionsClass,
                bool isAsync)
                : base(
                    targetType,
                    extensionsNamespace,
                    extensionsClass,
                    isAsync,
                    "Async",
                    "Await",
                    "'")
            {
            }

            public override string GetCodeWithExpression(string expression, params string[] additionalNamspaces)
            {
                var builder = new StringBuilder()
                    .AppendLine("Imports System");

                foreach (var aditionalNamespace in additionalNamspaces)
                {
                    builder
                        .Append("Imports ")
                        .Append(aditionalNamespace)
                        .AppendLine();
                }

                builder
                    .Append(@"Namespace Global.")
                    .Append(TestNamespace)
                    .Append(@"
    Class C
        Function GetData() As ")
                    .Append(TargetType)
                    .Append(@"
            Return Nothing
        End Function
        ");

                if (IsAsync)
                {
                    builder
                        .Append(AsyncKeyword)
                        .Append(" ");
                };

                return builder
                    .Append(@"Sub M()
            Dim b = ")
                    .Append(expression)
                    .AppendLine(@"
        End Sub
    End Class
End Namespace")
                    .ToString();
            }

            public override string GetExtensionsCode(string namespaceName, string className)
            {
                string targetType;
                string targetTypeOfSource;
                string methodSuffix;
                string predicate;
                string boolReturnType;
                string intReturnType;
                string boolReturnValue;
                string intReturnValue;

                if (this.IsAsync)
                {
                    targetType = "Global.System.Linq.IQueryable";
                    targetTypeOfSource = "Global.System.Linq.IQueryable(Of TSource)";
                    methodSuffix = "Async";
                    predicate = "Global.System.Linq.Expressions.Expression(Of Global.System.Func(Of TSource, Boolean))";
                    boolReturnType = "Global.System.Threading.Tasks.Task(Of Boolean)";
                    intReturnType = "Global.System.Threading.Tasks.Task(Of Integer)";
                    boolReturnValue = "Global.System.Threading.Tasks.Task.FromResult(Of Boolean)(Nothing)";
                    intReturnValue = "Global.System.Threading.Tasks.Task.FromResult(Of Integer)(Nothing)";
                }
                else
                {
                    targetType = "Global.System.Collections.IEnumerable";
                    targetTypeOfSource = "Global.System.Collections.Generic.IEnumerable(Of TSource)";
                    methodSuffix = string.Empty;
                    predicate = "Global.System.Func(Of TSource, Boolean)";
                    boolReturnType = "Boolean";
                    intReturnType = "Integer";
                    boolReturnValue = "Nothing";
                    intReturnValue = "Nothing";
                }

                return $@"Namespace Global.{namespaceName}
    <System.Runtime.CompilerServices.Extension>
    Public Module {className}
        <System.Runtime.CompilerServices.Extension>
        Public Function Any{methodSuffix}(q As {targetType}) As {boolReturnType}
            Return {boolReturnValue}
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function Any{methodSuffix}(Of TSource)(q As {targetTypeOfSource}, predicate As {predicate}) As {boolReturnType}
            Return {boolReturnValue}
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function Count{methodSuffix}(q As {targetType}) As {intReturnType}
            Return {intReturnValue}
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function Count{methodSuffix}(Of TSource)(q As {targetTypeOfSource}, predicate As {predicate}) As {intReturnType}
            Return {intReturnValue}
        End Function
        <System.Runtime.CompilerServices.Extension>
        Public Function Sum{methodSuffix}(q As {targetType}) As {intReturnType}
            Return {intReturnValue}
        End Function
    End Module
End Namespace
";
            }

            public override string GetOperatorCode(BinaryOperatorKind binaryOperatorKind)
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

            public override string GetPredicateCode() => "Function(x) True";

            public override string GetSymbolInvocationCode(string methodName, params string[] arguments)
            {
                throw new NotImplementedException();
            }

            internal override object GetLogicalNotText(bool negate) => negate ? "Not " : string.Empty;
        }

        protected abstract class VerifierBase
        {
            public abstract Task VerifyAsync(string[] testSources);
            internal abstract Task VerifyAsync(string[] testSources, string[] fixedSources);
        }

        protected sealed class CSharpVerifier<TAnalyzer, TCodeFix>
            : VerifierBase
            where TAnalyzer : DiagnosticAnalyzer, new()
            where TCodeFix : CodeFixProvider, new()
        {
            public override Task VerifyAsync(string[] testSources)
            {
                var test = new Test.Utilities.CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Test();

                foreach (var testSource in testSources)
                {
                    if (!string.IsNullOrEmpty(testSource))
                    {
                        test.TestState.Sources.Add(testSource);
                    }
                }

                return test.RunAsync();
            }

            internal override Task VerifyAsync(string[] testSources, string[] fixedSources)
            {
                var test = new Test.Utilities.CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Test();

                foreach (var testSource in testSources)
                {
                    if (!string.IsNullOrEmpty(testSource))
                    {
                        test.TestState.Sources.Add(testSource);
                    }
                }

                foreach (var fixedSource in fixedSources)
                {
                    if (!string.IsNullOrEmpty(fixedSource))
                    {
                        test.FixedState.Sources.Add(fixedSource);
                    }
                }

                return test.RunAsync();
            }
        }

        protected sealed class BasicVerifier<TAnalyzer, TCodeFix>
            : VerifierBase
            where TAnalyzer : DiagnosticAnalyzer, new()
            where TCodeFix : CodeFixProvider, new()
        {
            public override Task VerifyAsync(string[] testSources)
            {
                var test = new Test.Utilities.VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>.Test();

                foreach (var testSource in testSources)
                {
                    if (!string.IsNullOrEmpty(testSource))
                    {
                        test.TestState.Sources.Add(testSource);
                    }
                }

                return test.RunAsync();
            }

            internal override Task VerifyAsync(string[] testSources, string[] fixedSources)
            {
                var test = new Test.Utilities.VisualBasicCodeFixVerifier<TAnalyzer, TCodeFix>.Test();

                foreach (var testSource in testSources)
                {
                    if (!string.IsNullOrEmpty(testSource))
                    {
                        test.TestState.Sources.Add(testSource);
                    }
                }

                foreach (var fixedSource in fixedSources)
                {
                    if (!string.IsNullOrEmpty(fixedSource))
                    {
                        test.FixedState.Sources.Add(fixedSource);
                    }
                }

                return test.RunAsync();
            }
        }
    }
}

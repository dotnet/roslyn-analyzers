// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class StringBuilderAppendShouldNotTakeSubstringTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestFromProposingTicketFirstLine()
        {
            const string code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(2));
        return sb.ToString();
    }
}";
            var expectedDiagnostic =
                new DiagnosticResult(
                        StringBuilderAppendShouldNotTakeSubstring.RuleReplaceOneParameter)
                    .WithLocation("Test0.cs", 9, 9);

            VerifyCSharp(code, expectedDiagnostic);
        }

        [Fact]
        public void TestFromProposingTicketFirstLineBasic()
        {
            const string code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(2))
        Return sb.ToString()
    End Function
End Class
";

            var expectedDiagnostic =
                new DiagnosticResult(
                        StringBuilderAppendShouldNotTakeSubstring.RuleReplaceOneParameter)
                    .WithLocation("Test0.vb", 7, 9);

            VerifyBasic(code, expectedDiagnostic);
        }

        [Fact]
        public void TestFromProposingTicketSecondLine()
        {
            const string code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(0,6));
        return sb.ToString();
    }
}";
            var expectedDiagnostic = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceTwoParameter)
                .WithLocation("Test0.cs", 9, 9);

            VerifyCSharp(code, expectedDiagnostic);
        }

        [Fact]
        public void TestFromProposingTicketSecondLineBasic()
        {
            const string code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(0,6))
        Return sb.ToString()
    End Function
End Class
";
            var expectedDiagnostic = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceTwoParameter)
                .WithLocation("Test0.vb", 7, 9);

            VerifyBasic(code, expectedDiagnostic);
        }

        [Fact]
        public void FindsBothIssuesInExampleFromTicket()
        {
            const string code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(0, 6));
        sb.Append(text.Substring(2));
        return sb.ToString ();
    }
}";
            var expected1 = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceTwoParameter)
                .WithLocation("Test0.cs", 9, 9);
            var expected2 = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceOneParameter)
                .WithLocation("Test0.cs", 10, 9);
            VerifyCSharp(code, expected1, expected2);
        }

        [Fact]
        public void NoResultWhenReplacingTwoParameterVariantOnStringVariableWithChainOnStringParameter()
        {
            const string code = @"
using System.Text;
using System.Linq;

public class C
{
    private string Append5(string text)
    {
        var sb = new StringBuilder()
            .Append(text.Substring(4, 10).Reverse());
        return sb.ToString();
    }
}";
            VerifyCSharp(code);
        }

        [Fact]
        public void FindsNoDiagnosticWhenStringBuilderAppendToStringIsTextParameter()
        {
            const string code = @"
using System.Text;

public class C
{
    private void Append6(string s)
    {
        var sb = new StringBuilder();
        sb.Append(new StringBuilder().Append(s).ToString().Substring(2));
    }
}";
            // must not find anything as the input might have side effects
            VerifyCSharp(code);
        }

        [Theory]
        [InlineData(OperationKind.ArrayElementReference, "pArray[1]")]
        [InlineData(OperationKind.DefaultValue, "default(string)")]
        [InlineData(OperationKind.FieldReference, "someField")]
        [InlineData(OperationKind.InstanceReference, "this.someField")]
        [InlineData(OperationKind.InstanceReference, "c2.someField")]
        [InlineData(OperationKind.Literal, "\"literalString\"")]
        [InlineData(OperationKind.LocalReference, "pVariable")]
        [InlineData(OperationKind.NameOf, "nameof(Append)")]
        [InlineData(OperationKind.ParameterReference, "s")]
        public void FindsDiagnosticOnSafeOperationKindsAsTextParameter(OperationKind kind, string textParameter)
        {
            string code = $@"
using System.Text;

public class C
{{
    private string someField = ""foo-bar-bazz"";
    const string pConst =""TestTestTest"";

    private void Append(string s)
    {{
        string[] pArray = new [] {{ ""a"", ""b"" }};
        var pVariable = ""testVariableString"";
        var sb = new StringBuilder();
        var c2 = new C();
        sb.Append({textParameter}.Substring(2));
    }}
}}";
            var expected = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceOneParameter)
                .WithLocation("Test0.cs", 15, 9);
            VerifyCSharp(code, expected);
        }

        [Theory]
        [InlineData(OperationKind.ArrayElementReference, "pArray[1]")]
        [InlineData(OperationKind.DefaultValue, "default(int)")]
        [InlineData(OperationKind.FieldReference, "someField")]
        [InlineData(OperationKind.InstanceReference, "this.someField")]
        [InlineData(OperationKind.InstanceReference, "c2.someField")]
        [InlineData(OperationKind.Literal, "42")]
        [InlineData(OperationKind.LocalReference, "variable")]
        [InlineData(OperationKind.ParameterReference, "start")]
        [InlineData(OperationKind.SizeOf, "sizeof(char)")] // TODO sizeof should work, returns bytes of an unmanaged type
        [InlineData(OperationKind.Conversion, "(int)3.14")] // Conversion, but conversion of a constant is known to be constant by the compiler => works
        public void FindsDiagnosticOnSafeOperationKindsAsStartIndexParameterForOneParameterSubstring(OperationKind kind, string startIndexParameter)
        {
            string code = $@"
using System.Text;

public class C
{{
    private int someField = 2;

    private void Append(int start, StringBuilder sb)
    {{
        var c2 = new C();
        int variable = 23;
        int[] pArray = new [] {{ 1, 2, 3 }};
        sb.Append(""testTest"".Substring({startIndexParameter}));
    }}
}}";
            var expected = new DiagnosticResult(
                    StringBuilderAppendShouldNotTakeSubstring.RuleReplaceOneParameter)
                .WithLocation("Test0.cs", 13, 9);
            VerifyCSharp(code, expected);
        }
        
        [Theory]
        [InlineData(OperationKind.Conversion, "twico", false)]
        [InlineData(OperationKind.Conversion, "(int)someLong", false)]
        [InlineData(OperationKind.Invocation, "someMethod()", false)]
        [InlineData(OperationKind.PropertyReference, "SomeProperty", false)]
        [InlineData(OperationKind.Decrement, "--variable", false)]
        [InlineData(OperationKind.BinaryOperator, "someField + 3", false)]
        [InlineData(OperationKind.Conditional, "b ? 1 : 2", false)]
        [InlineData(OperationKind.Coalesce, "nullableField ?? 23", false)]
        [InlineData(OperationKind.ObjectCreation, "new TypeWithImplicitConversionOperator()", false)]
        [InlineData(OperationKind.Await, "(await asyncMethod())", false)]
        [InlineData(OperationKind.SimpleAssignment, "variable = 42", false)]
        [InlineData(OperationKind.Parenthesized, "(variable = 23)", false)]
        [InlineData(OperationKind.InterpolatedString, "$\"some{variable}\"", true)]
        [InlineData(OperationKind.TypeOf, "typeof(C)", true)]
        [InlineData(OperationKind.NameOf, "nameof(start)", true)]
        [InlineData(OperationKind.IsPattern, "(pArray is String)", true)]
        [InlineData(OperationKind.Increment, "variable++", false)]
        [InlineData(OperationKind.Throw, "throw new NotImplementedException()", true)]
        [InlineData(OperationKind.Decrement, "variable--", false)]
        public void FindsNoDiagnosticOnUnsafeOperationKindsAsStartIndexParameterForOneParameterSubstring(OperationKind kind, string startIndexParameter, bool allowCompilationErrors)
        {
            string code = $@"
using System.Text;
using System.Threading.Tasks;

public class C
{{
    private int someField = 2;
    private int? nullableField = 3;
    private long someLong = 1000000000;
    private int someMethod() => 23;
    public int SomeProperty {{get; set; }}

    public class TypeWithImplicitConversionOperator
    {{
        public static implicit operator int(TypeWithImplicitConversionOperator input)
        {{
            return 22;
        }}
    }}

    private async Task<int> asyncMethod()
    {{
        await Task.Delay(100);
        return someField;
    }}

    private async Task Append(int start, StringBuilder sb, bool b)
    {{
        var c2 = new C();
        int variable = 23;
        int[] pArray = new [] {{ 1, 2, 3 }};
        var twico = new TypeWithImplicitConversionOperator();
        sb.Append(""testTest"".Substring({startIndexParameter}));
    }}
}}";
            VerifyCSharp(code, allowCompilationErrors ? TestValidationMode.AllowCompileErrors : TestValidationMode.AllowCompileWarnings);
        }

        [Theory]
        //[InlineData(OperationKind.None, "")]
        //[InlineData(OperationKind.Invalid, "")]
        //[InlineData(OperationKind.Block, "")]
        //[InlineData(OperationKind.VariableDeclarationGroup, "")]
        //[InlineData(OperationKind.Switch, "")]
        //[InlineData(OperationKind.Loop, "")]
        //[InlineData(OperationKind.Labeled, "")]
        //[InlineData(OperationKind.Branch, "")]
        //[InlineData(OperationKind.Empty, "")]
        //[InlineData(OperationKind.Return, "")]
        //[InlineData(OperationKind.YieldBreak, "")]
        //[InlineData(OperationKind.Lock, "")]
        //[InlineData(OperationKind.Try, "")]
        //[InlineData(OperationKind.Using, "")]
        //[InlineData(OperationKind.YieldReturn, "")]
        //[InlineData(OperationKind.ExpressionStatement, "")]
        //[InlineData(OperationKind.LocalFunction, "")]
        //[InlineData(OperationKind.Stop, "")]
        //[InlineData(OperationKind.End, "")]
        //[InlineData(OperationKind.RaiseEvent, "")]
        //[InlineData(OperationKind.Conversion, "")]
        [InlineData(OperationKind.Invocation, "someMethod()", false)]
        //[InlineData(OperationKind.MethodReference, "")]
        [InlineData(OperationKind.PropertyReference, "SomeProperty", false)]
        //[InlineData(OperationKind.EventReference, "")]
        //[InlineData(OperationKind.UnaryOperator, "")]
        [InlineData(OperationKind.BinaryOperator, "someField + \"add\"", false)]
        [InlineData(OperationKind.Conditional, "(true ? someField : \"somethingElse\")", false)]
        [InlineData(OperationKind.Coalesce, "(someField ?? \"else\")", false)]
        //[InlineData(OperationKind.AnonymousFunction, "")]
        [InlineData(OperationKind.ObjectCreation, "new string('y', 100)", false)]
        //[InlineData(OperationKind.TypeParameterObjectCreation, "")]
        //[InlineData(OperationKind.ArrayCreation, "")]
        //[InlineData(OperationKind.IsType, "")]
        [InlineData(OperationKind.Await, "(await asyncMethod())", false)]
        [InlineData(OperationKind.SimpleAssignment, "s = \"foo\"", false)]
        //[InlineData(OperationKind.CompoundAssignment, "")]
        [InlineData(OperationKind.Parenthesized, "(s = \"foo\")", false)]
        //[InlineData(OperationKind.EventAssignment, "")]
        //[InlineData(OperationKind.ConditionalAccess, "")]
        //[InlineData(OperationKind.ConditionalAccessInstance, "")]
        [InlineData(OperationKind.InterpolatedString, "$\"some{i}\"", false)]
        //[InlineData(OperationKind.AnonymousObjectCreation, "")]
        //[InlineData(OperationKind.ObjectOrCollectionInitializer, "")]
        //[InlineData(OperationKind.MemberInitializer, "")]
        //[InlineData(OperationKind.CollectionElementInitializer, "")]
        //[InlineData(OperationKind.Tuple, "")]
        //[InlineData(OperationKind.DynamicObjectCreation, "")]
        //[InlineData(OperationKind.DynamicMemberReference, "")]
        //[InlineData(OperationKind.DynamicInvocation, "")]
        //[InlineData(OperationKind.DynamicIndexerAccess, "")]
        //[InlineData(OperationKind.TranslatedQuery, "")]
        //[InlineData(OperationKind.DelegateCreation, "")]
        [InlineData(OperationKind.TypeOf, "typeof(C)", true)]
        [InlineData(OperationKind.SizeOf, "sizeOf(C)", true)]
        [InlineData(OperationKind.AddressOf, "addressOf()", true)]
        [InlineData(OperationKind.IsPattern, "(pArray is String)", true)]
        [InlineData(OperationKind.Increment, "i++", true)]
        [InlineData(OperationKind.Throw, "throw new NotImplementedException()", true)]
        [InlineData(OperationKind.Decrement, "i--", true)]
        //[InlineData(OperationKind.DeconstructionAssignment, "")]
        //[InlineData(OperationKind.DeclarationExpression, "")]
        //[InlineData(OperationKind.OmittedArgument, "")]
        //[InlineData(OperationKind.FieldInitializer, "")]
        //[InlineData(OperationKind.VariableInitializer, "")]
        //[InlineData(OperationKind.PropertyInitializer, "")]
        //[InlineData(OperationKind.ParameterInitializer, "")]
        //[InlineData(OperationKind.ArrayInitializer, "")]
        //[InlineData(OperationKind.VariableDeclarator, "")]
        //[InlineData(OperationKind.VariableDeclaration, "")]
        //[InlineData(OperationKind.Argument, "")]
        //[InlineData(OperationKind.CatchClause, "")]
        //[InlineData(OperationKind.SwitchCase, "")]
        //[InlineData(OperationKind.CaseClause, "")]
        //[InlineData(OperationKind.InterpolatedStringText, "")]
        //[InlineData(OperationKind.Interpolation, "")]
        //[InlineData(OperationKind.ConstantPattern, "")]
        //[InlineData(OperationKind.DeclarationPattern, "")]
        public void FindsNoDiagnosticOnDangerousOperationKindsAsTextParameter(OperationKind kind, string textParameter, bool allowCompilationErrors)
        {
            string code = $@"
using System.Text;
using System.Threading.Tasks;

public class C
{{
    private string someField = ""foo-bar-bazz"";

    public string SomeProperty {{get; set; }}

    private string someMethod() => someField;

    private async Task<string> asyncMethod()
    {{
        await Task.Delay(100);
        return someField;
    }}

    private async Task Append(string s)
    {{
        const string pConst =""TestTestTest"";
        int i = 1;
        string[] pArray = new [] {{ ""a"", ""b"" }};
        var pVariable = ""testVariableString"";
        var sb = new StringBuilder();
        sb.Append({textParameter}.Substring(2));
    }}
}}";

            VerifyCSharp(code, allowCompilationErrors ? TestValidationMode.AllowCompileErrors : TestValidationMode.AllowCompileWarnings);
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }
    }
}
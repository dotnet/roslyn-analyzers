// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

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
                    StringBuilderAppendShouldNotTakeSubstring.RuleIdTwoParameterId,
                    DiagnosticHelpers.DefaultDiagnosticSeverity)
                .WithLocation("Test0.cs", 9, 9);

            VerifyCSharp(code, expectedDiagnostic);
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
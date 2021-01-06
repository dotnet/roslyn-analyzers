﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.CSharp.Analyzers.CSharpImmutableObjectMethodAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests
{
    public class UseReturnValueFromImmutableObjectMethodTests
    {
        [Fact]
        public async Task CSharpVerifyDiagnostics()
        {
            var source = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

class TestSimple
{
    void M()
    {
        Document document = default(Document);
        document.WithText(default(SourceText));

        Project project = default(Project);
        project.AddDocument(""Sample.cs"", default(SourceText));

        Solution solution = default(Solution);
        solution.AddProject(""Sample"", ""Sample"", ""CSharp"");

        Compilation compilation = default(Compilation);
        compilation.RemoveAllSyntaxTrees();
    }
}
";
            DiagnosticResult documentExpected = GetCSharpExpectedDiagnostic(10, 9, "Document", "WithText");
            DiagnosticResult projectExpected = GetCSharpExpectedDiagnostic(13, 9, "Project", "AddDocument");
            DiagnosticResult solutionExpected = GetCSharpExpectedDiagnostic(16, 9, "Solution", "AddProject");
            DiagnosticResult compilationExpected = GetCSharpExpectedDiagnostic(19, 9, "Compilation", "RemoveAllSyntaxTrees");

            await VerifyCS.VerifyAnalyzerAsync(source, documentExpected, projectExpected, solutionExpected, compilationExpected);
        }

        [Fact]
        public async Task CSharp_VerifyDiagnosticOnExtensionMethod()
        {
            var source = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

class TestExtensionMethodTrivia
{
    void M()
    {
        SyntaxNode node = default(SyntaxNode);
        node.WithLeadingTrivia<SyntaxNode>();
    }
}";
            DiagnosticResult expected = GetCSharpExpectedDiagnostic(10, 9, "SyntaxNode", "WithLeadingTrivia");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task CSharp_VerifyNoDiagnostic()
        {
            var source = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ConsoleApplication1
{
    class TestNoDiagnostic
    {
        public Document M()
        {
            Document document = default(Document);
            var newDocument = document.WithText(default(SourceText));
            document = document.WithText(default(SourceText));

            OtherMethod(document.WithText(default(SourceText)));
            return document.WithText(default(SourceText));
        }

        public void OtherMethod(Document document)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        private static DiagnosticResult GetCSharpExpectedDiagnostic(int line, int column, string objectName, string methodName) =>
#pragma warning disable RS0030 // Do not used banned APIs
            VerifyCS.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(objectName, methodName);
    }
}

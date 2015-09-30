using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Analyzers.MetaAnalyzers;
using Xunit;
using Microsoft.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers;

namespace Microsoft.CodeAnalysis.UnitTests.MetaAnalyzers
{
    public class UseReturnValueFromImmutableObjectMethodTests : CodeFixTestBase
    {

        [Fact]
        public void CSharpVerifyDiagnostics()
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
    }
}
";
            var documentExpected = GetCSharpExpectedDiagnostic(10, 9);
            var projectExpected = GetCSharpExpectedDiagnostic(13, 9);
            var solutionExpected = GetCSharpExpectedDiagnostic(16, 9);

            VerifyCSharp(source, documentExpected, projectExpected, solutionExpected);
        }

        [Fact]
        public void CSharp_VerifyDiagnosticOnExtensionMethod()
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
            var expected = GetCSharpExpectedDiagnostic(10, 9);
            VerifyCSharp(source, expected);
        }


        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return null;
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpImmutableObjectMethodAnalyzer();
        }

        private static DiagnosticResult GetCSharpExpectedDiagnostic(int line, int column)
        {
            return GetExpectedDiagnostic(LanguageNames.CSharp, line, column);
        }

        private static DiagnosticResult GetExpectedDiagnostic(string language, int line, int column)
        {
            var fileName = language == LanguageNames.CSharp ? "Test0.cs" : "Test0.vb";
            return new DiagnosticResult
            {
                Id = DiagnosticIds.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocation,
                Message = CodeAnalysisDiagnosticsResources.DoNotIgnoreReturnValueOnImmutableObjectMethodInvocationMessage,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation(fileName, line, column)
                }
            };
        }
    }
}

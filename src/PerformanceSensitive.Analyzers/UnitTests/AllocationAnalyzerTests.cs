using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClrHeapAllocationAnalyzer.Test
{
    public abstract class AllocationAnalyzerTests
    {
        protected static readonly List<MetadataReference> references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IList<>).Assembly.Location)
            };

        protected IList<SyntaxNode> GetExpectedDescendants(IEnumerable<SyntaxNode> nodes, ImmutableArray<SyntaxKind> expected)
        {
            var descendants = new List<SyntaxNode>();
            foreach (var node in nodes)
            {
                if (expected.Any(e => e == node.Kind()))
                {
                    descendants.Add(node);
                    continue;
                }

                foreach (var child in node.ChildNodes())
                {
                    if (expected.Any(e => e == child.Kind()))
                    {
                        descendants.Add(child);
                        continue;
                    }

                    if (child.ChildNodes().Count() > 0)
                        descendants.AddRange(GetExpectedDescendants(child.ChildNodes(), expected));
                }
            }
            return descendants;
        }

        protected Info ProcessCode(DiagnosticAnalyzer analyzer, string sampleProgram,
            ImmutableArray<SyntaxKind> expected, bool allowBuildErrors = false, string filePath = "")
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Script);
            var tree = CSharpSyntaxTree.ParseText(sampleProgram, options, filePath);
            var compilation = CSharpCompilation.Create("Test", new[] { tree }, references);

            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error) > 0)
            {
                var msg = "There were Errors in the sample code\n";
                if (allowBuildErrors == false)
                    Assert.Fail(msg + string.Join("\n", diagnostics));
                else
                    Console.WriteLine(msg + string.Join("\n", diagnostics));
            }

            var semanticModel = compilation.GetSemanticModel(tree);
            var matches = GetExpectedDescendants(tree.GetRoot().ChildNodes(), expected);

            // Run the code tree through the analyzer and record the allocations it reports
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
            var allocations = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().Distinct(DiagnosticEqualityComparer.Instance).ToList();

            return new Info
            {
                Options = options,
                Tree = tree,
                Compilation = compilation,
                Diagnostics = diagnostics,
                SemanticModel = semanticModel,
                Matches = matches,
                Allocations = allocations,
            };
        }

        protected class Info
        {
            public CSharpParseOptions Options { get; set; }
            public SyntaxTree Tree { get; set; }
            public CSharpCompilation Compilation { get; set; }
            public ImmutableArray<Diagnostic> Diagnostics { get; set; }
            public SemanticModel SemanticModel { get; set; }
            public IList<SyntaxNode> Matches { get; set; }
            public List<Diagnostic> Allocations { get; set; }
        }
    }
}

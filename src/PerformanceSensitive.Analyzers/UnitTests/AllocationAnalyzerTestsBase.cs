// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public abstract class AllocationAnalyzerTestsBase : DiagnosticAnalyzerTestBase
    {
        protected static readonly List<MetadataReference> references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IList<>).Assembly.Location)
            };

        protected ImmutableArray<SyntaxNode> GetExpectedDescendants(IEnumerable<SyntaxNode> nodes, ImmutableArray<SyntaxKind> expected)
        {
            var descendants = ImmutableArray.CreateBuilder<SyntaxNode>();
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
            return descendants.ToImmutable();
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
                    Assert.True(false, msg + string.Join("\n", diagnostics));
                else
                    Console.WriteLine(msg + string.Join("\n", diagnostics));
            }

            var semanticModel = compilation.GetSemanticModel(tree);
            var matches = GetExpectedDescendants(tree.GetRoot().ChildNodes(), expected);

            // Run the code tree through the analyzer and record the allocations it reports
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
            var allocations = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
                .Distinct(DiagnosticEqualityComparer.Instance)
                .ToImmutableArray();

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
            public ImmutableArray<SyntaxNode> Matches { get; set; }
            public ImmutableArray<Diagnostic> Allocations { get; set; }
        }

        public const string PerformanceSensitiveAttributeSource = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Roslyn.Utilities
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    internal sealed class PerformanceSensitiveAttribute : Attribute
    {
        public PerformanceSensitiveAttribute(string uri)
        {
            Uri = uri;
        }

        public string Uri
        {
            get;
        }

        public string Constraint
        {
            get;
            set;
        }

        public bool AllowCaptures
        {
            get;
            set;
        }

        public bool AllowGenericEnumeration
        {
            get;
            set;
        }

        public bool AllowLocks
        {
            get;
            set;
        }

        public bool OftenCompletesSynchronously
        {
            get;
            set;
        }

        public bool IsParallelEntry
        {
            get;
            set;
        }
    }
}";

        protected void VerifyCSharp(string source, bool withAttribute, params DiagnosticResult[] expected)
        {
            if (withAttribute)
            {
                VerifyCSharp(new[] { source, PerformanceSensitiveAttributeSource }, expected);
            }
            else
            {
                VerifyCSharp(source, expected);
            }
        }

        protected void VerifyCSharp(FileAndSource fileAndSource, bool withAttribute, params DiagnosticResult[] expected)
        {
            if (withAttribute)
            {
                VerifyCSharp(new[]
                {
                    fileAndSource,
                    new FileAndSource() {  Source = PerformanceSensitiveAttributeSource, FilePath = @"c:\temp\PerformanceSensitiveAttribute.cs" }
                }, expected);
            }
            else
            {
                VerifyCSharp(new[] { fileAndSource }, expected);
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.SourceGeneratorAttributeAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.SourceGeneratorAttributeAnalyzerFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class MissingGeneratorAttributeRuleTests
    {
        private const string SourceGenerator = @"
using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GeneratorAttribute : Attribute
    {
    }

    public interface ISourceGenerator
    {
        void Initialize(GeneratorInitializationContext context);
        void Execute(GeneratorExecutionContext context);
    }

    public struct GeneratorInitializationContext
    {
    }

    public readonly struct GeneratorExecutionContext
    {
    }
}";

        [Fact]
        public async Task Test1()
        {
            var code = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

public class {|RS1035:CustomGenerator|} : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}

    public void Execute(GeneratorExecutionContext context)
    {
    }
}";

            var fixedCode = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[Generator]
public class {|RS1035:CustomGenerator|} : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) {}

    public void Execute(GeneratorExecutionContext context)
    {
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code , SourceGenerator },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference}
                },
                FixedState =
                {
                    Sources = { fixedCode, SourceGenerator },
                    AdditionalReferences = { AdditionalMetadataReferences.CodeAnalysisReference},
                },
            }.RunAsync();
        }
    }
}

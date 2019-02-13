// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Test.Utilities;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class IgnoreTests : AllocationAnalyzerTestsBase
    {
        [Fact(Skip = "Compiler generated code shouldn't have PerformanceSensitiveAttribute, " +
            "but this might be needed if we want to use editorconfig to set scope to all code.")]
        public void AnalyzeProgram_TakesIgnoredAttributesIntoAccount()
        {
            const string sampleProgram =
                @"using System;
                
                [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
                public void CreateString1() {
                    string str = new string('a', 5);
                }

                [System.CodeDom.Compiler.GeneratedCodeAttribute(""MyCompiler"", ""1.0.0.3"")]
                public void CreateString2() {
                    string str = new string('a', 5);
                }

                [System.ObsoleteAttribute]
                public void CreateString3() {
                    string str = new string('a', 5);
                }";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ObjectInitializerExpression));
            Assert.Single(info.Allocations);
        }

        [Fact]
        public void AnalyzeProgram_TakesIgnoredFilesIntoAccount()
        {
            var source = @"
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        string str = new string('a', 5);
    }
}";
            VerifyCSharp(new FileAndSource() { Source = source, FilePath = "Test0.g.cs" }, withAttribute: true);
            VerifyCSharp(new FileAndSource() { Source = source, FilePath = "Test0.G.cS" }, withAttribute: true);

            VerifyCSharp(new FileAndSource() { Source = source, FilePath = "Test0.cs" }, withAttribute: true,
                                                // test.cs(10,22): info HAA0502: Explicit new reference type allocation
                                                GetCSharpResultAt(10, 22, ExplicitAllocationAnalyzer.NewObjectRule));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExplicitAllocationAnalyzer();
        }
    }
}
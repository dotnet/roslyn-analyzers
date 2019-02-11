using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClrHeapAllocationAnalyzer.Test
{
    [TestClass]
    public class IgnoreTests : AllocationAnalyzerTests
    {
        [TestMethod]
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
            Assert.AreEqual(1, info.Allocations.Count);
        }

        [TestMethod]
        public void AnalyzeProgram_TakesIgnoredFilesIntoAccount()
        {
            const string sampleProgram =
                @"using System;
                public void CreateString() {
                    string str = new string('a', 5);
                }";

            var analyser = new ExplicitAllocationAnalyzer();
            void Check(int expectedCount, string path)
            {
                var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ObjectInitializerExpression), filePath: path);
                Assert.AreEqual(expectedCount, info.Allocations.Count);
            }

            Check(0, "test.g.cs");
            Check(0, "test.G.cS");
            Check(1, "test.cs");
            Check(1, "test.cpp");
        }
    }
}
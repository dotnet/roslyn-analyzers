// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class IdentifiersShouldNotContainTypeNamesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new IdentifiersShouldNotContainTypeNames();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new IdentifiersShouldNotContainTypeNames();
        }

        [Fact]
        public void CSharp_CA1720_NoDiagnostic()
        {
            VerifyCSharp(@"
class IntA
{
}
");
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic1()
        {
            VerifyCSharp(@"
class Int
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 7, identifierName: "Int"));
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic2()
        {
            VerifyCSharp(@"
struct Int32
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 8, identifierName: "Int32"));
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic3()
        {
            VerifyCSharp(@"
enum Int64
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 6, identifierName: "Int64"));
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic4()
        {
            VerifyCSharp(@"
class Foo
{
   void Int ()
   {
   }
}
",
    GetCA1720CSharpResultAt(line: 4, column: 9, identifierName: "Int"));
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic5()
        {
            VerifyCSharp(@"
class Bar
{
   void BarMethod (int Int)
   {
   }
}
",
    GetCA1720CSharpResultAt(line: 4, column: 24, identifierName: "Int"));
        }

        [Fact]
        public void CSharp_CA1720_SomeDiagnostic6()
        {
            VerifyCSharp(@"
class FooBar
{
   int Int;
}
",
    GetCA1720CSharpResultAt(line: 4, column: 8, identifierName: "Int"));
        }

        [Fact]
        public void Basic_CA1720_NoDiagnostic()
        {
            VerifyBasic(@"
");
        }

        #region Helpers

        private static DiagnosticResult GetCA1720CSharpResultAt(int line, int column, string identifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage, identifierName);
            return GetCSharpResultAt(line, column, IdentifiersShouldNotContainTypeNames.RuleId, message);
        }

        private static DiagnosticResult GetCA1720BasicResultAt(int line, int column, string identifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            string message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage, identifierName);
            return GetBasicResultAt(line, column, IdentifiersShouldNotContainTypeNames.RuleId, message);
        }
        #endregion
    }
}
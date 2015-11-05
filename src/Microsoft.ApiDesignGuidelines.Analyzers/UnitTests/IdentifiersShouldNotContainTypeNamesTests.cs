// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
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
struct Int
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 1, identifierName: "Int"));
        }


        [Fact]
        public void Basic_CA1720_NoDiagnostic()
        {
            VerifyBasic(@"
");
        }

        [Fact]
        public void Basic_CA1720_SomeDiagnostic()
        {
            VerifyBasic(@"
",
    GetCA1720BasicResultAt(line: 0, column: 0, identifierName: ""),
    GetCA1720BasicResultAt(line: 1, column: 1, identifierName: ""));
        }




        #region Helpers

        private static DiagnosticResult GetCA1720CSharpResultAt(int line, int column, string identifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            var message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage, identifierName);
            return GetCSharpResultAt(line, column, IdentifiersShouldNotContainTypeNames.RuleId, message);
        }

        private static DiagnosticResult GetCA1720BasicResultAt(int line, int column, string identifierName)
        {
            // Add a public read-only property accessor for positional argument '{0}' of attribute '{1}'.
            var message = string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage, identifierName);
            return GetBasicResultAt(line, column, IdentifiersShouldNotContainTypeNames.RuleId, message);
        }
        #endregion
    }
}
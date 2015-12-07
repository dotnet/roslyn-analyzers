// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class AvoidEmptyInterfacesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidEmptyInterfacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidEmptyInterfacesAnalyzer();
        }

        [Fact]
        public void TestCSharpEmptyInterface1()
        {
            VerifyCSharp(@"
interface I
{
}", CreateCSharpResult(2, 11));
        }

        [Fact]
        public void TestBasicEmptyInterface1()
        {
            VerifyBasic(@"
Interface I
End Interface", CreateBasicResult(2, 11));
        }

        [Fact]
        public void TestCSharpNonEmptyInterface1()
        {
            VerifyCSharp(@"
interface I
{
    void DoStuff();
}");
        }

        [Fact]
        public void TestBasicNonEmptyInterface1()
        {
            VerifyBasic(@"
Interface I
    Function GetStuff() as Integer
End Interface");
        }


        private static DiagnosticResult CreateCSharpResult(int line, int col)
        {
            return GetCSharpResultAt(line, col, AvoidEmptyInterfacesAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidEmptyInterfacesMessage);
        }

        private static DiagnosticResult CreateBasicResult(int line, int col)
        {
            return GetBasicResultAt(line, col, AvoidEmptyInterfacesAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidEmptyInterfacesMessage);
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class UseIntegralOrStringArgumentForIndexersTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicUseIntegralOrStringArgumentForIndexersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpUseIntegralOrStringArgumentForIndexersAnalyzer();
        }
 
        [Fact]
        public void UseIntegralOrStringArgumentForIndexersChar()
        {
            VerifyCSharp(@"
    public class Months
    {
        string[] month = new char[] {'J', 'F', 'M'};
        public string this[char index]
        {
            get
            {
                return month[index];
            }
        }
    }", CreateCSharpResult(5, 23));
        }

        [Fact]
        public void UseIntegralOrStringArgumentForIndexersInt()
        {
            VerifyCSharp(@"
    public class Months
    {
        string[] month = new char[] {'J', 'F', 'M'};
        public string this[int index]
        {
            get
            {
                return month[index];
            }
        }
    }");
        }

        private static DiagnosticResult CreateCSharpResult(int line, int col)
        {
            return GetCSharpResultAt(line, col, UseIntegralOrStringArgumentForIndexersAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersMessage);
        }

        private static DiagnosticResult CreateBasicResult(int line, int col)
        {
            return GetBasicResultAt(line, col, UseIntegralOrStringArgumentForIndexersAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersMessage);
        }
    }
}
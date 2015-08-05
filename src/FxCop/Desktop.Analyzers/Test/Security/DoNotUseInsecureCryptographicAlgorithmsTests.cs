// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public class DoNotUseInsecureCryptographicAlgorithmsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotUseInsecureCryptographicAlgorithmsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotUseInsecureCryptographicAlgorithmsAnalyzer();
        }


        private const string CA5350RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseMD5RuleId;
        private const string CA5351RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseDESRuleId;
        private const string CA5352RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRC2RuleId;
        private const string CA5353RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseTripleDESRuleId;
        private const string CA5355RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRIPEMD160RuleId;
        private const string CA5356RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseDSARuleId;
        private const string CA5357RuleName = DoNotUseInsecureCryptographicAlgorithmsAnalyzer.DoNotUseRijndaelRuleId;

        private readonly string CA5350Message = DesktopAnalyzersResources.DoNotUseMD5;
        private readonly string CA5351Message = DesktopAnalyzersResources.DoNotUseDES;
        private readonly string CA5352Message = DesktopAnalyzersResources.DoNotUseRC2;
        private readonly string CA5353Message = DesktopAnalyzersResources.DoNotUseTripleDES;
        private readonly string CA5355Message = DesktopAnalyzersResources.DoNotUseRIPEMD160;
        private readonly string CA5356Message = DesktopAnalyzersResources.DoNotUseDSA;
        private readonly string CA5357Message = DesktopAnalyzersResources.DoNotUseRijndael;

    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class PropertiesShouldNotReturnArraysTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPropertiesShouldNotReturnArraysAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPropertiesShouldNotReturnArraysAnalyzer();
        }

        [Fact]
        public void TestCSharpPropertiesShouldNotReturnArraysArray()
        {
            VerifyCSharp(@"
    public class Book
    {
        private string[] _Pages;

        public Book(string[] pages)
        {
            _Pages = pages;
        }

        public string[] Pages
        {
            get { return _Pages; }
        }
    }
 ", CreateCSharpResult(11, 25));
        }

        [Fact]
        public void TestCSharpPropertiesShouldNotReturnArraysOverride()
        {
            VerifyCSharp(@"
    public class Book
    {
        public Book(string[] pages)
        {
            _Pages = pages;
        }

        public override string[] Pages
        {
            get { return _Pages; }
        }
    }
");
        }

        private static DiagnosticResult CreateCSharpResult(int line, int col)
        {
            return GetCSharpResultAt(line, col, PropertiesShouldNotReturnArraysAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotReturnArraysMessage);
        }

        private static DiagnosticResult CreateBasicResult(int line, int col)
        {
            return GetBasicResultAt(line, col, PropertiesShouldNotReturnArraysAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotReturnArraysMessage);
        }
    }

}
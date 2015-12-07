// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class ExceptionsShouldBePublicTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicExceptionsShouldBePublicAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpExceptionsShouldBePublicAnalyzer();
        }

        [Fact]
        public void TestNonPublicException()
        {
            VerifyCSharp(@"
using System;
class InternalException : Exception
{
}", 
            GetCA1064CSharpResultAt(3, 7));
        }

        [Fact]
        public void TestNonPublicException2()
        {
            VerifyCSharp(@"
using System;
private class PrivateException : SystemException
{
}",
            GetCA1064CSharpResultAt(3, 15));
        }

        [Fact]
        public void TestPublicException()
        {
            VerifyCSharp(@"
using System;
public class BasicException : Exception
{
}");
        }

        [Fact]
        public void TestNonExceptionType()
        {
            VerifyCSharp(@"
using System.IO;
public class NonException : StringWriter
{
}");
        }

        private DiagnosticResult GetCA1064CSharpResultAt(int line, int column) =>
            GetCSharpResultAt(line, column, ExceptionsShouldBePublicAnalyzer.RuleId, 
                MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicMessage);                
    }
}
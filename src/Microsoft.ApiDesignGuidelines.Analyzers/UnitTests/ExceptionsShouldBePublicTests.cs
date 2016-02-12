// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class ExceptionsShouldBePublicTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ExceptionsShouldBePublicAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExceptionsShouldBePublicAnalyzer();
        }

        [Fact]
        public void TestCSharpNonPublicException()
        {
            VerifyCSharp(@"
using System;
class InternalException : Exception
{
}",
            GetCA1064CSharpResultAt(3, 7));
        }

        [Fact]
        public void TestCSharpNonPublicException2()
        {
            VerifyCSharp(@"
using System;
private class PrivateException : SystemException
{
}",
            GetCA1064CSharpResultAt(3, 15));
        }

        [Fact]
        public void TestCSharpPublicException()
        {
            VerifyCSharp(@"
using System;
public class BasicException : Exception
{
}");
        }

        [Fact]
        public void TestCSharpNonExceptionType()
        {
            VerifyCSharp(@"
using System.IO;
public class NonException : StringWriter
{
}");
        }

        [Fact]
        public void TestVBasicNonPublicException()
        {
            VerifyBasic(@"
Imports System
Class InternalException
   Inherits Exception
End Class",
            GetCA1064VBasicResultAt(3, 7));
        }

        [Fact]
        public void TestVBasicNonPublicException2()
        {
            VerifyBasic(@"
Imports System
Private Class PrivateException
   Inherits SystemException
End Class",
            GetCA1064VBasicResultAt(3, 15));
        }

        [Fact]
        public void TestVBasicPublicException()
        {
            VerifyBasic(@"
Imports System
Public Class BasicException
   Inherits Exception
End Class");
        }

        [Fact]
        public void TestVBasicNonExceptionType()
        {
            VerifyBasic(@"
Imports System
Public Class NonException
   Inherits StringWriter
End Class");
        }

        private DiagnosticResult GetCA1064CSharpResultAt(int line, int column) =>
            GetCSharpResultAt(line, column, ExceptionsShouldBePublicAnalyzer.RuleId,
                MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicMessage);

        private DiagnosticResult GetCA1064VBasicResultAt(int line, int column) =>
            GetBasicResultAt(line, column, ExceptionsShouldBePublicAnalyzer.RuleId,
                MicrosoftApiDesignGuidelinesAnalyzersResources.ExceptionsShouldBePublicMessage);
    }
}
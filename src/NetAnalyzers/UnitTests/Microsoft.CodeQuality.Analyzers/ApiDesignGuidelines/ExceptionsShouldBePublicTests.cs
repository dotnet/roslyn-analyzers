// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ExceptionsShouldBePublicAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ExceptionsShouldBePublicFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ExceptionsShouldBePublicAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ExceptionsShouldBePublicFixer>;
namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ExceptionsShouldBePublicTests
    {
        [Fact]
        public async Task TestCSharpNonPublicException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
class InternalException : Exception
{
}",
            GetCA1064CSharpResultAt(3, 7));
        }

        [Fact]
        public async Task TestCSharpNonPublicException2()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
internal class Outer
{
    private class PrivateException : SystemException
    {
    }
}",
            GetCA1064CSharpResultAt(5, 19));
        }

        [Fact]
        public async Task TestCSharpPublicException()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
public class BasicException : Exception
{
}");
        }

        [Fact]
        public async Task TestCSharpNonExceptionType()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
public class NonException : StringWriter
{
}");
        }

        [Fact]
        public async Task TestVBasicNonPublicException()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Class InternalException
   Inherits Exception
End Class",
            GetCA1064VBasicResultAt(3, 7));
        }

        [Fact]
        public async Task TestVBasicNonPublicException2()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Public Class Outer
    Private Class PrivateException
        Inherits SystemException
    End Class
End Class",
            GetCA1064VBasicResultAt(4, 19));
        }

        [Fact]
        public async Task TestVBasicPublicException()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Public Class BasicException
   Inherits Exception
End Class");
        }

        [Fact]
        public async Task TestVBasicNonExceptionType()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System.IO
Imports System.Text
Public Class NonException
   Inherits StringWriter
End Class");
        }

        private static DiagnosticResult GetCA1064CSharpResultAt(int line, int column)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);
#pragma warning restore RS0030 // Do not used banned APIs

        private static DiagnosticResult GetCA1064VBasicResultAt(int line, int column)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic()
                .WithLocation(line, column);
#pragma warning restore RS0030 // Do not used banned APIs
    }
}
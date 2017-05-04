// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class UriParametersShouldNotBeStringsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UriParametersShouldNotBeStringsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UriParametersShouldNotBeStringsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new UriParametersShouldNotBeStringsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UriParametersShouldNotBeStringsFixer();
        }

        [Fact]
        public void CA1054WarningWithUrl()
        {
            var code = @"
using System;

public class A
{
    public static void Method(string url) { }
}
";

            var fix = @"
using System;

public class A
{
    public static void Method(string url) { }

    public static void Method(Uri url)
    {
        throw new NotImplementedException();
    }
}
";

            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void CA1054MultipleWarningWithUrl()
        {
            var code = @"
using System;

public class A
{
    public static void Method(string url, string url2) { }
}
";
            var fixSingle = @"
using System;

public class A
{
    public static void Method(string url, string url2) { }

    public static void Method(Uri url, string url2)
    {
        throw new NotImplementedException();
    }
}
";

            VerifyCSharpFix(code, fixSingle, onlyFixFirstFixableDiagnostic: true);

            var fixAllSequentially = @"
using System;

public class A
{
    public static void Method(string url, string url2) { }

    public static void Method(Uri url, string url2)
    {
        throw new NotImplementedException();
    }

    public static void Method(string url, Uri url2)
    {
        throw new NotImplementedException();
    }

    public static void Method(Uri url, Uri url2)
    {
        throw new NotImplementedException();
    }
}
";

            VerifyCSharpFix(code, fixAllSequentially, onlyFixFirstFixableDiagnostic: false);
        }

        [Fact]
        public void CA1054MultipleWarningWithUrlWithOverload()
        {
            // Following original FxCop implementation. but this seems strange.
            var code = @"
using System;

public class A
{
    public static void Method(string url, string url2) { }
    public static void Method(Uri url, Uri url2) { }
}
";
            var fix = @"
using System;

public class A
{
    public static void Method(string url, string url2) { }
    public static void Method(Uri url, Uri url2) { }

    public static void Method(Uri url, string url2)
    {
        throw new NotImplementedException();
    }

    public static void Method(string url, Uri url2)
    {
        throw new NotImplementedException();
    }
}
";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void CA1054WarningVB()
        {
            // C# and VB shares same implementation. so just one vb test
            var code = @"
Imports System

Public Class A
    Public Sub Method(firstUri As String)
    End Sub
End Class
";
            var fix = @"
Imports System

Public Class A
    Public Sub Method(firstUri As String)
    End Sub

    Public Sub Method(firstUri As Uri)
        Throw New NotImplementedException()
    End Sub
End Class
";

            VerifyBasicFix(code, fix);
        }
    }
}
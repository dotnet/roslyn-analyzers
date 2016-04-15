// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class UriParametersShouldNotBeStringsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UriParametersShouldNotBeStringsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UriParametersShouldNotBeStringsAnalyzer();
        }

        [Fact]
        public void CA1054NoWarningWithUrlNotStringType()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(int url) { }
    }
");
        }

        [Fact]
        public void CA1054WarningWithUrl()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(string url) { }
    }
", GetCA1054CSharpResultAt(6, 42, "url", "A.Method(string)"));
        }

        [Fact]
        public void CA1054NoWarningWithUrlWithOverload()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(string url) { }
        public static void Method(Uri url) { }
    }
");
        }

        [Fact]
        public void CA1054MultipleWarningWithUrl()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(string url, string url2) { }
    }
", GetCA1054CSharpResultAt(6, 42, "url", "A.Method(string, string)")
 , GetCA1054CSharpResultAt(6, 54, "url2", "A.Method(string, string)"));
        }

        [Fact]
        public void CA1054NoMultipleWarningWithUrlWithOverload()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(string url, string url2) { }
        public static void Method(string url, Uri url2) { }
        public static void Method(Uri url, string url2) { }
        public static void Method(Uri url, Uri url2) { }
    }
");
        }

        [Fact]
        public void CA1054MultipleWarningWithUrlWithOverload()
        {
            // Following original FxCop implementation. but this seems strange.
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static void Method(string url, string url2) { }
        public static void Method(Uri url, Uri url2) { }
    }
", GetCA1054CSharpResultAt(6, 42, "url", "A.Method(string, string)")
 , GetCA1054CSharpResultAt(6, 54, "url2", "A.Method(string, string)"));

        }

        [Fact]
        public void CA1054NoWarningNotPublic()
        {
            VerifyCSharp(@"
    using System;

    internal class A : IComparable
    {
        public static void Method(string url) { }
    }
");
        }

        [Fact]
        public void CA1054NoWarningDerivedFromAttribute()
        {
            VerifyCSharp(@"
    using System;

    internal class A : Attribute
    {
        public bool void Method(string url) { }
    }
");
        }

        [Fact]
        public void CA1054WarningVB()
        {
            // C# and VB shares same implementation. so just one vb test
            VerifyBasic(@"
    Imports System
    
    Public Module A
        Public Sub Method(firstUri As String)
        End Sub
    End Module
", GetCA1054BasicResultAt(5, 27, "firstUri", "A.Method(String)"));
        }

        private static DiagnosticResult GetCA1054CSharpResultAt(int line, int column, params string[] args)
        {
            return GetCSharpResultAt(line, column, UriParametersShouldNotBeStringsAnalyzer.Rule, args);
        }

        private static DiagnosticResult GetCA1054BasicResultAt(int line, int column, params string[] args)
        {
            return GetBasicResultAt(line, column, UriParametersShouldNotBeStringsAnalyzer.Rule, args);
        }
    }
}
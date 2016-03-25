// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class PassSystemUriObjectsInsteadOfStringsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPassSystemUriObjectsInsteadOfStringsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPassSystemUriObjectsInsteadOfStringsAnalyzer();
        }

        [Fact]
        public void CA2234NoWarningWithUrl()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(1);
        }

        public static Method(int url) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithUri()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(1);
        }

        public static Method(int uri) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithUrn()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(1);
        }

        public static Method(int urn) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithUriButNoString()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(1);
        }

        public static Method(int urn) { }
        public static Method(Uri uri) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithStringButNoUri()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string uri) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithStringButNoUrl()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string url) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningWithStringButNoUrn()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string urn) { }
    }
");
        }

        [Fact]
        public void CA2234WarningWithUri()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string uri) { }
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri)", "A.Method(string)"));
        }

        [Fact]
        public void CA2234WarningWithUrl()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string url) { }
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri)", "A.Method(string)"));
        }

        [Fact]
        public void CA2234WarningWithUrn()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string urn) { }
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri)", "A.Method(string)"));
        }

        [Fact]
        public void CA2234WarningWithCompoundUri()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string myUri) { }
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri)", "A.Method(string)"));
        }

        [Fact]
        public void CA2234NoWarningWithSubstring()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string myuri) { }
        public static Method(Uri uri) { }
    }
");
        }

        [Fact]
        public void CA2234WarningWithMultipleParameter1()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", ""test"", ""test"");
        }

        public static Method(string param1, string param2, string lastUrl) { }
        public static Method(string param1, string param2, Uri lastUrl) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(string, string, Uri)", "A.Method(string, string, string)"));
        }

        [Fact]
        public void CA2234WarningWithMultipleParameter2()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", 0, ""test"");
        }

        public static Method(string firstUri, int i, string lastUrl) { }
        public static Method(Uri uri, int i, string lastUrl) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri, int, string)", "A.Method(string, int, string)"));
        }

        [Fact]
        public void CA2234NoWarningForSelf()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", null);
        }

        public static Method(string firstUri, Uri lastUri) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningForSelf2()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", null);
        }

        public static Method(string firstUri, Uri lastUri) { }
        public static Method(int other, Uri lastUri) { }
    }
");
        }

        [Fact]
        public void CA2234WarningWithMultipleUri()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", null);
        }

        public static Method(string firstUri, Uri lastUrl) { }
        public static Method(Uri uri, Uri lastUrl) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri, Uri)", "A.Method(string, Uri)"));
        }

        [Fact]
        public void CA2234WarningWithMultipleOverload()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", ""test2"");
        }

        public static Method(string firstUri, string lastUrl) { }
        public static Method(Uri uri, string lastUrl) { }
        public static Method(string uri, Uri lastUrl) { }
        public static Method(Uri uri, Uri lastUrl) { }
    }
", GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(Uri, string)", "A.Method(string, string)")
 , GetCA2234CSharpResultAt(8, 13, "A.Method()", "A.Method(string, Uri)", "A.Method(string, string)"));
        }

        [Fact]
        public void CA2234NoWarningSignatureMismatchingNumberOfParameter()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", null);
        }

        public static Method(string firstUri, string lastUrl) { }
        public static Method(Uri uri) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningSignatureMismatchingParameterType()
        {
            VerifyCSharp(@"
    using System;

    public class A : IComparable
    {
        public static Method()
        {
            Method(""test"", null);
        }

        public static Method(string firstUri, string lastUrl) { }
        public static Method(Uri uri, int i) { }
    }
");
        }

        [Fact]
        public void CA2234NoWarningNotPublic()
        {
            VerifyCSharp(@"
    using System;

    internal class A : IComparable
    {
        public static Method()
        {
            Method(""test"");
        }

        public static Method(string uri) { }
        public static Method(Uri uri) { }
    }
");
        }

        [Fact]
        public void CA2234WarningVB()
        {
            // since VB and C# shares almost all code except to get method overload group expression
            // we only need to test that part
            VerifyBasic(@"
    Imports System
    
    Public Module A
        Public Sub Method()
            Method(""test"", 0, ""test"")
        End Sub
    
        Public Sub Method(firstUri As String, i As Integer, lastUrl As String)
        End Sub
    
        Public Sub Method(Uri As Uri, i As Integer, lastUrl As String)
        End Sub
    End Module
", GetCA2234BasicResultAt(6, 13, "A.Method()", "A.Method(Uri, Integer, String)", "A.Method(String, Integer, String)"));
        }

        private static DiagnosticResult GetCA2234CSharpResultAt(int line, int column, params string[] args)
        {
            return GetCSharpResultAt(line, column, PassSystemUriObjectsInsteadOfStringsAnalyzer.Rule, args);
        }

        private static DiagnosticResult GetCA2234BasicResultAt(int line, int column, params string[] args)
        {
            return GetBasicResultAt(line, column, PassSystemUriObjectsInsteadOfStringsAnalyzer.Rule, args);
        }
    }
}
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
", GetCA2234CSharpResultAt(8, 13));
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
", GetCA2234CSharpResultAt(8, 13));
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
", GetCA2234CSharpResultAt(8, 13));
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
", GetCA2234CSharpResultAt(8, 13));
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
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13));
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

        public static Method(string firstUri, int 0, string lastUrl) { }
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13));
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
        public static Method(int other) { }
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
        public static Method(Uri uri) { }
    }
", GetCA2234CSharpResultAt(8, 13));
        }

        [Fact]
        public void CA2234WarningVB()
        {
            // since VB and C# shares almost all code except to get method overload group expression
            // we only need to test that part
            VerifyBasic(@"
    Imports System
    
    Module Module1
        Public Sub Method()
            Method(""test"", 0, ""test"")
        End Sub
    
        Public Sub Method(firstUri As String, 0 As Integer, lastUrl As String)
        End Sub
    
        Public Sub Method(Uri As Uri)
        End Sub
    End Module
", GetCA2234BasicResultAt(6, 13));
        }

        internal static readonly string CA2234Name = "CA2234";
        internal static readonly string CA2234Message = MicrosoftApiDesignGuidelinesAnalyzersResources.PassSystemUriObjectsInsteadOfStringsMessage;

        private static DiagnosticResult GetCA2234CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA2234Name, CA2234Message);
        }

        private static DiagnosticResult GetCA2234BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA2234Name, CA2234Message);
        }
    }
}
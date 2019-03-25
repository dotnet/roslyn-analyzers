// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForXssVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForXssVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForXssVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForXssVulnerabilities();
        }

        [Fact]
        public void Simple_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Response.Write(""<HTML>"" + input + ""</HTML>"");
    }
}",
                GetCSharpResultAt(10, 9, 9, 24, "void HttpResponse.Write(string s)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void Simple_VB_Diagnostic()
        {
            VerifyBasic(@"
");
        }

        [Fact]
        public void Simple_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Response.Write(""<HTML><TITLE>test</TITLE><BODY>Hello world!</BODY></HTML>"");
    }
}");
        }

        [Fact]
        public void Int32_Parse_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        string integer = Int32.Parse(input).ToString();
        Response.Write(""<HTML>"" + integer + ""</HTML>"");
    }
}");
        }

        [Fact]
        public void HttpServerUtility_HtmlEncode_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        string encoded = Server.HtmlEncode(input);
        Response.Write(""<HTML>"" + encoded + ""</HTML>"");
    }
}");
        }
    }
}

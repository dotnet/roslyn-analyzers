// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForOpenRedirectVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForOpenRedirectVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForOpenRedirectVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForOpenRedirectVulnerabilities();
        }

        [Fact]
        public void HttpResponse_Redirect_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.Response.Redirect(input);
    }
}",
                GetCSharpResultAt(10, 9, 9, 24, "void HttpResponse.Redirect(string url)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void HttpResponse_Redirect_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        if (String.IsNullOrWhiteSpace(input))
        {
            this.Response.Redirect(""https://example.org/login.html"");
        }
    }
}");
        }

        [Fact]
        public void HttpResponse_RedirectToRoutePermanent_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.Response.RedirectToRoutePermanent(input);
    }
}",
                GetCSharpResultAt(10, 9, 9, 24, "void HttpResponse.RedirectToRoutePermanent(string routeName)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void HttpResponseBase_RedirectLocation_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        new HttpResponseWrapper(this.Response).RedirectLocation = input;
    }
}",
                GetCSharpResultAt(10, 9, 9, 24, "string HttpResponseWrapper.RedirectLocation", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }
    }
}
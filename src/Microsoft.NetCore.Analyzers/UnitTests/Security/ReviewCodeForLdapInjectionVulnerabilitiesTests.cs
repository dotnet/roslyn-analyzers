// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForLdapInjectionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForLdapInjectionVulnerabilities.Rule;

        protected override IEnumerable<string> AdditionalCSharpSources => new string[] { AntiXssApis.CSharp };

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForLdapInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForLdapInjectionVulnerabilities();
        }

        [Fact]
        public void DirectoryEntry_Path_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.DirectoryServices;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        new DirectoryEntry(input);
    }
}",
                GetCSharpResultAt(12, 9, 11, 24, "DirectoryEntry.DirectoryEntry(string path)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void DirectoryEntry_Username_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.DirectoryServices;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        new DirectoryEntry(""path"", input, ""password"");
    }
}");
        }

        [Fact]
        public void DirectorySearcher_Filter_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.DirectoryServices;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        DirectorySearcher ds = new DirectorySearcher();
        ds.Filter = ""(lastName="" + input + "")"";
    }
}",
                GetCSharpResultAt(13, 9, 11, 24, "string DirectorySearcher.Filter", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void DirectoryEntry_Path_Sanitized_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.DirectoryServices;
using System.Web;
using System.Web.UI;
using Microsoft.Security.Application;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        input = Encoder.LdapDistinguishedNameEncode(input);
        new DirectoryEntry(input);
    }
}");
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.NetCore.Analyzers.Security;
    using Test.Utilities;
    using Xunit;

    public class ReviewCodeForDllInjectionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForDllInjectionVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForDllInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForDllInjectionVulnerabilities();
        }

        [Fact]
        public void HttpRequest_Form_LocalString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
    using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Assembly.LoadFrom(input);
    }
}
            ",
                GetCSharpResultAt(20, 21, 15, 28, "string SqlCommand.CommandText", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

    }
}

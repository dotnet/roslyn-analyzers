// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForCommandExecutionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForCommandExecutionVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForCommandExecutionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForCommandExecutionVulnerabilities();
        }

        [Fact]
        public void Process_Start_fileName_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Process p = Process.Start(input);
    }
}",
                GetCSharpResultAt(12, 21, 11, 24, "Process Process.Start(string fileName)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void Process_Start_arguments_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        Process p = Process.Start(""copy"", input + "" \\\\somewhere\\public"");
    }
}",
                GetCSharpResultAt(12, 21, 11, 24, "Process Process.Start(string fileName, string arguments)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void ProcessStartInfo_Constructor_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        ProcessStartInfo i = new ProcessStartInfo(input);
    }
}",
                GetCSharpResultAt(12, 30, 11, 24, "ProcessStartInfo.ProcessStartInfo(string fileName)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void ProcessStartInfo_Arguments_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        ProcessStartInfo i = new ProcessStartInfo(""copy"") 
        {
            Arguments = input + "" \\\\somewhere\\public"",
        };
    }
}",
                GetCSharpResultAt(14, 13, 11, 24, "string ProcessStartInfo.Arguments", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }
    }
}

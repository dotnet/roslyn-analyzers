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
        public void Assembly_LoadFrom_Diagnostic()
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
                GetCSharpResultAt(15, 9, 14, 24, "Assembly Assembly.LoadFrom(string assemblyFile)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void Assembly_Load_Bytes_Diagnostic()
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
        byte[] bytes = Convert.FromBase64String(input);
        Assembly.Load(bytes);
    }
}
            ",
                GetCSharpResultAt(16, 9, 14, 24, "Assembly Assembly.Load(byte[] rawAssembly)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void Assembly_LoadFrom_NoDiagnostic()
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
        Assembly.LoadFrom(""foo.dll"");
    }
}
            ");
        }

        [Fact]
        public void AppDomain_ExecuteAssembly_Diagnostic()
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
        AppDomain.CurrentDomain.ExecuteAssembly(input);
    }
}
            ",
                GetCSharpResultAt(15, 9, 14, 24, "int AppDomain.ExecuteAssembly(string assemblyFile)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        
    }
}

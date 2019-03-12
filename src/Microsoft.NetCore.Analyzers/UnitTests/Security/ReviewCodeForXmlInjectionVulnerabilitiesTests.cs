// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForXmlInjectionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForXmlInjectionVulnerabilities.Rule;

        protected override IEnumerable<string> AdditionalCSharpSources => new string[] { AntiXssApis.CSharp };

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForXmlInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForXmlInjectionVulnerabilities();
        }

        [Fact]
        public void XmlAttribute_InnerXml_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        XmlDocument d = new XmlDocument();
        XmlAttribute a = d.CreateAttribute(""attr"");
        a.InnerXml = input;
    }
}",
                GetCSharpResultAt(13, 9, 10, 24, "string XmlAttribute.InnerXml", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void XmlTextWriter_WriteRaw_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        XmlTextWriter t = new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
        t.WriteRaw(input);
    }
}",
                GetCSharpResultAt(14, 9, 12, 24, "void XmlTextWriter.WriteRaw(string data)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void XmlTextWriter_WriteRaw_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        XmlTextWriter t = new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
        t.WriteRaw(""<root/>"");
    }
}");
        }


        [Fact]
        public void XmlNotation_InnerXml_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        XmlDocument d = new XmlDocument();
        XmlNotation n = (XmlNotation) d.CreateNode(XmlNodeType.Notation, String.Empty, String.Empty);
        n.InnerXml = input;
    }
}",
                GetCSharpResultAt(13, 9, 10, 24, "string XmlNotation.InnerXml", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void XmlNotation_InnerXml_Sanitized_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml;
using Microsoft.Security.Application;

public partial class WebForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        XmlDocument d = new XmlDocument();
        XmlNotation n = (XmlNotation) d.CreateNode(XmlNodeType.Notation, String.Empty, String.Empty);
        n.InnerXml = AntiXss.XmlEncode(input);
    }
}");
        }
    }
}
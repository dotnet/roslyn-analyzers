// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities.MinimalImplementations;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForXPathInjectionVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForXPathInjectionVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForXPathInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForXPathInjectionVulnerabilities();
        }

        [Fact]
        public void XPathNavigator_Select_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml.XPath;

public partial class WebForm : System.Web.UI.Page
{
    public XPathNavigator XPathNavigator { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.XPathNavigator.Select(input);
    }
}",
                GetCSharpResultAt(12, 9, 11, 24, "XPathNodeIterator XPathNavigator.Select(string xpath)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void XPathNavigator_Select_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml.XPath;

public partial class WebForm : System.Web.UI.Page
{
    public XPathNavigator XPathNavigator { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.XPathNavigator.Select(""//nodes"");
    }
}");
        }

        [Fact]
        public void XmlNode_SelectSingleNode_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    public XmlNode XmlNode { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.XmlNode.SelectSingleNode(input);
    }
}",
                GetCSharpResultAt(12, 9, 11, 24, "XmlNode XmlNode.SelectSingleNode(string xpath)", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void TemplateControl_XPath_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Web.UI;
using System.Xml;

public partial class WebForm : System.Web.UI.Page
{
    public MyTemplateControl MyTemplateControl { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.MyTemplateControl.UntrustedInputGoesHere(input);
    }
}

public class MyTemplateControl : TemplateControl
{
    public object UntrustedInputGoesHere(string untrustedInput)
    {
        return this.XPath(untrustedInput, (IXmlNamespaceResolver) null);
    }
}
",
                GetCSharpResultAt(21, 16, 12, 24, "object TemplateControl.XPath(string xPathExpression, IXmlNamespaceResolver resolver)", "object MyTemplateControl.UntrustedInputGoesHere(string untrustedInput)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }

        [Fact]
        public void XmlDataSource_XPath_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;
using System.Web.UI.WebControls;

public partial class WebForm : System.Web.UI.Page
{
    public XmlDataSource XmlDataSource { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        string input = Request.Form[""in""];
        this.XmlDataSource.XPath = input;
    }
}",
                GetCSharpResultAt(12, 9, 11, 24, "string XmlDataSource.XPath", "void WebForm.Page_Load(object sender, EventArgs e)", "NameValueCollection HttpRequest.Form", "void WebForm.Page_Load(object sender, EventArgs e)"));
        }
    }
}
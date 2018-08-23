// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    using Microsoft.NetCore.CSharp.Analyzers.Security;
    using Microsoft.NetCore.VisualBasic.Analyzers.Security;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Test.Utilities;
    using Xunit;
    using Microsoft.NetCore.Analyzers.Security;

    public class ReviewCodeForSqlInjectionVulnerabilitiesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForSqlInjectionVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForSqlInjectionVulnerabilities();
        }

        private const string SystemWebNamespacesCSharp = @"
namespace System.Collections.Specialized
{
    public class NameValueCollection
    {
        public string this[string name]
        {
            get { return ""input""; }
        }
    }
}

namespace System.Web
{
    public class HttpRequest
    {
        public System.Collections.Specialized.NameValueCollection Form { get; }
    }
}

namespace System.Web.UI
{
    public class Page
    {
        public System.Web.HttpRequest Request { get; }      
    }
}";

        [Fact]
        public void BadInput()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string input = Request.Form[""in""];
            if (Request.Form != null && !String.IsNullOrWhiteSpace(input))
            {
                SqlCommand sqlCommand = new SqlCommand()
                {
                    CommandText = input,
                    CommandType = CommandType.Text,
                };
            }
        }
     }
}
            ");
        }
    }
}
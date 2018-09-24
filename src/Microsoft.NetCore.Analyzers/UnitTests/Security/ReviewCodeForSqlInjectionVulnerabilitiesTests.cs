// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.UnitTests.Security
{
    using Microsoft.NetCore.CSharp.Analyzers.Security;
    using Microsoft.NetCore.VisualBasic.Analyzers.Security;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Test.Utilities;
    using Xunit;
    using Microsoft.NetCore.Analyzers.Security;
    using System.Linq;
    using System;

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

        protected DiagnosticResult GetCSharpResultAt(int sinkLine, int sinkColumn, int sourceLine, int sourceColumn, string sink, string sinkContainingMethod, string source, string sourceContainingMethod)
        {
            this.PrintActualDiagnosticsOnFailure = true;
            return GetCSharpResultAt(
                new[] {
                    Tuple.Create(SystemWebNamespacesCSharpLineCount + sinkLine, sinkColumn),
                    Tuple.Create(SystemWebNamespacesCSharpLineCount + sourceLine, sourceColumn)
                },
                ReviewCodeForSqlInjectionVulnerabilities.Rule,
                sink,
                sinkContainingMethod,
                source,
                sourceContainingMethod);
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

        public string Get(string name)
        {
            return ""input"";
        }
    }
}

namespace System.Web
{
    public class HttpRequest
    {
        public System.Collections.Specialized.NameValueCollection Form { get; }
        public System.Collections.Specialized.NameValueCollection QueryString { get; }
        public string[] UserLanguages { get; }
        public string this[string name]
        {
            get { return ""input""; }
        }
    }
}

namespace System.Web.UI
{
    public class Page
    {
        public System.Web.HttpRequest Request { get; }      
    }
}";

        private static readonly int SystemWebNamespacesCSharpLineCount = SystemWebNamespacesCSharp.Where(ch => ch == '\n').Count();

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalString_Diagnostic()
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
            ",
                GetCSharpResultAt(21, 21, 16, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStringMoreBlocks_Diagnostic()
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
            string input;
            if (Request.Form != null)
            {
                input = Request.Form[""in""];
            }
            else
            {
                input = ""SELECT 1"";
            }

            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(28, 17, 19, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact] //(Skip = "This doesn't work :-(")]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_And_QueryString_LocalStringMoreBlocks_Diagnostic()
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
            string input;
            if (Request.Form != null)
            {
                input = Request.Form[""in""];
            }
            else
            {
                input = Request.QueryString[""in""];
            }

            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(28, 17, 19, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"),
                GetCSharpResultAt(28, 17, 23, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.QueryString", "Page_Load"));
        }



        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Direct_Diagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = Request.Form[""in""],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(18, 17, 18, 31, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Substring_Diagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = Request.Form[""in""].Substring(1),
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(18, 17, 18, 31, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void Sanitized_HttpRequest_Form_Direct_NoDiagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = ""SELECT * FROM users WHERE id < "" + int.Parse(Request.Form[""in""]).ToString(),
                CommandType = CommandType.Text,
            };
        }
     }
}
            ");
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Item_Diagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = Request[""in""],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(18, 17, 18, 31, "string SqlCommand.CommandText", "Page_Load", "string HttpRequest.this[string name]", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Item_Enters_SqlParameters_NoDiagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = ""SELECT * FROM users WHERE username = @username"",
                CommandType = CommandType.Text,
            };

            sqlCommand.Parameters.Add(""@username"", SqlDbType.NVarChar, 16).Value = Request[""in""];

            sqlCommand.ExecuteReader();
        }
     }
}
            ");
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Item_Sql_Constructor_Diagnostic()
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
            SqlCommand sqlCommand = new SqlCommand(Request[""in""]);
        }
     }
}
            ",
                GetCSharpResultAt(16, 37, 16, 52, "SqlCommand.SqlCommand(string cmdText)", "Page_Load", "string HttpRequest.this[string name]", "Page_Load"));
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Method_Diagnostic()
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
            string input = Request.Form.Get(""in"");
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(19, 17, 16, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalNameValueCollectionString_Diagnostic()
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
            System.Collections.Specialized.NameValueCollection nvc = Request.Form;
            string input = nvc[""in""];
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(20, 17, 16, 70, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact(Skip = "Doesn't work, array isn't tainted")]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_List_Diagnostic()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> allTheInputs = new List<string>(new string[] { Request.Form[""in""] });
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = allTheInputs[0],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(29, 17, 24, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Array_Diagnostic()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string[] allTheInputs = new string[] { Request.Form[""in""] };
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = allTheInputs[0],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(21, 17, 18, 52, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }



        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStructNameValueCollectionString_Diagnostic()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        public struct MyStruct
        {
            public NameValueCollection nvc;
            public string s;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MyStruct myStruct = new MyStruct();
            myStruct.nvc = this.Request.Form;
            myStruct.s = myStruct.nvc[""in""];
            string input = myStruct.s;
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(29, 17, 24, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStructConstructorNameValueCollectionString_Diagnostic()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        public struct MyStruct
        {
            public MyStruct(NameValueCollection v)
            {
                this.nvc = v;
                this.s = null;
            }

            public NameValueCollection nvc;
            public string s;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MyStruct myStruct = new MyStruct();
            myStruct.nvc = this.Request.Form;
            myStruct.s = myStruct.nvc[""in""];
            string input = myStruct.s;
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(35, 17, 30, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_UserLanguages_Direct_Diagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = Request.UserLanguages[0],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(18, 17, 18, 31, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_UserLanguages_LocalStringArray_Diagnostic()
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
            string[] languages = Request.UserLanguages;
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = languages[0],
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(19, 17, 16, 34, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_UserLanguages_LocalStringModified_Diagnostic()
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
            string language = ""SELECT * FROM languages WHERE language = '"" + Request.UserLanguages[0] + ""'"";
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = language,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ",
                GetCSharpResultAt(19, 17, 16, 78, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void OkayInputLocalStructNameValueCollectionString_NoDiagnostic()
        {
            VerifyCSharp(
                SystemWebNamespacesCSharp + @"

namespace VulnerableWebApp
{
    using System;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public partial class WebForm : System.Web.UI.Page
    {
        public struct MyStruct
        {
            public NameValueCollection nvc;
            public string s;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MyStruct myStruct = new MyStruct();
            myStruct.nvc = this.Request.Form;
            myStruct.s = myStruct.nvc[""in""];
            string input = myStruct.s;
            myStruct.s = ""SELECT 1"";
            input = myStruct.s;
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = input,
                CommandType = CommandType.Text,
            };
        }
     }
}
            ");
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void OkayInputConst_NoDiagnostic()
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
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = ""SELECT * FROM users WHERE username = 'foo'"",
                CommandType = CommandType.Text,
            };
        }
     }
}
            ");
        }
    }
}
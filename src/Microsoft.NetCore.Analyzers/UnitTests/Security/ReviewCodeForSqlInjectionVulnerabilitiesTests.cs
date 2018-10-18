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
                    Tuple.Create(sinkLine, sinkColumn),
                    Tuple.Create(sourceLine, sourceColumn)
                },
                ReviewCodeForSqlInjectionVulnerabilities.Rule,
                sink,
                sinkContainingMethod,
                source,
                sourceContainingMethod);
        }

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            // Okay, so there aren't any dependencies, but if there were!
            this.VerifyCSharp(
                new[] { source }.ToFileAndSource(),
                expected);
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(20, 21, 15, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        //[Fact(Skip = "Need something to handle output parameters for delegates")]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_DelegateInvocation_OutParam_LocalString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public delegate void StringOutputDelegate(string input, out string output);

        public static StringOutputDelegate StringOutput;

        protected void Page_Load(object sender, EventArgs e)
        {
            StringOutput(Request.Form[""in""], out string input);
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
                GetCSharpResultAt(24, 21, 19, 26, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_InterfaceInvocation_OutParam_LocalString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public interface IBlah { void StringOutput(string input, out string output); }

        public static IBlah Blah;

        protected void Page_Load(object sender, EventArgs e)
        {
            Blah.StringOutput(Request.Form[""in""], out string input);
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
                GetCSharpResultAt(24, 21, 19, 31, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStringMoreBlocks_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(27, 17, 18, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_And_QueryString_LocalStringMoreBlocks_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(27, 17, 18, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"),
                GetCSharpResultAt(27, 17, 22, 25, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.QueryString", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Direct_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(17, 17, 17, 31, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Substring_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(17, 17, 17, 31, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void Sanitized_HttpRequest_Form_Direct_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
        public void Sanitized_HttpRequest_Form_TryParse_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            if (Int16.TryParse(Request.Form[""in""], out short i))
            {
                SqlCommand sqlCommand = new SqlCommand()
                {
                    CommandText = ""SELECT * FROM users WHERE id < "" + i.ToString(),
                    CommandType = CommandType.Text,
                };
            }
        }
     }
}
            ");
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Item_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(17, 17, 17, 31, "string SqlCommand.CommandText", "Page_Load", "string HttpRequest.this[string key]", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Item_Enters_SqlParameters_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(15, 37, 15, 52, "SqlCommand.SqlCommand(string cmdText)", "Page_Load", "string HttpRequest.this[string key]", "Page_Load"));
        }


        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Method_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(18, 17, 15, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalNameValueCollectionString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(19, 17, 15, 70, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact(Skip = "Doesn't work, array isn't tainted, ObjectCreation visited before ArrayInitializer")]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_List_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(28, 17, 23, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact(Skip = "Doesn't work, array isn't tainted")]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_Array_List_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            string[] array = new string[] { Request.Form[""in""] };
            List<string> allTheInputs = new List<string>();
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
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(20, 17, 17, 52, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }



        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStructNameValueCollectionString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(28, 17, 23, 28, "string SqlCommand.CommandText", "Page_Load", "NameValueCollection HttpRequest.Form", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_Form_LocalStructConstructorNameValueCollectionString_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"

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
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(17, 17, 17, 31, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_UserLanguages_LocalStringArray_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(18, 17, 15, 34, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void HttpRequest_UserLanguages_LocalStringModified_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
                GetCSharpResultAt(18, 17, 15, 78, "string SqlCommand.CommandText", "Page_Load", "string[] HttpRequest.UserLanguages", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void OkayInputLocalStructNameValueCollectionString_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
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
            VerifyCSharpWithDependencies(@"
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

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void SimpleInterprocedural()
        {
            VerifyCSharpWithDependencies(@"
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
            string taintedInput = this.Request[""input""];
            MyDatabaseLayer layer = new MyDatabaseLayer();
            layer.MakeSqlInjection(taintedInput);
        }
    }

    public class MyDatabaseLayer
    {
        public void MakeSqlInjection(string sqlInjection)
        {
            SqlCommand sqlCommand = new SqlCommand()
            {
                CommandText = ""SELECT * FROM users WHERE username = '"" + sqlInjection + ""'"",
                CommandType = CommandType.Text,
            };
        }
    }
}",
                GetCSharpResultAt(27, 17, 15, 35, "string SqlCommand.CommandText", "Page_Load", "string HttpRequest.this[string key]", "Page_Load"));
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
        public void SimpleLocalFunction()
        {
            VerifyCSharpWithDependencies(@"
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
            string taintedInput = this.Request[""input""];

            SqlCommand injectSql(string sqlInjection)
            {
                return new SqlCommand()
                {
                    CommandText = ""SELECT * FROM users WHERE username = '"" + sqlInjection + ""'"",
                    CommandType = CommandType.Text,
                };
            };

            injectSql(taintedInput);
        }
    }
}",
                GetCSharpResultAt(21, 21, 15, 35, "string SqlCommand.CommandText", "Page_Load", "string HttpRequest.this[string key]", "Page_Load"));
        }

    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class ReviewCodeForInformationDisclosureVulnerabilitiesTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => ReviewCodeForInformationDisclosureVulnerabilities.Rule;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReviewCodeForInformationDisclosureVulnerabilities();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ReviewCodeForInformationDisclosureVulnerabilities();
        }

        [Fact]
        public void ExceptionToString_ConsoleOutWriteLine()
        {
            this.VerifyCSharp(@"
using System;

public class Class
{
    public void Blah()
    {
        try
        {
            object o = null;
            o.ToString();
        }
        catch (Exception e)
        {
            Console.Out.WriteLine(e.ToString());
        }
    }
}
");
        }

        [Fact]
        public void NullReferenceExceptionToString_HttpResponseWrite()
        {
            this.VerifyCSharp(@"
using System;
using System.Web;

public class Class
{
    public void Blah(HttpResponse response)
    {
        try
        {
            object o = null;
            o.ToString();
        }
        catch (NullReferenceException nre)
        {
            response.Write(nre.ToString());
        }
    }
}
",
                GetCSharpResultAt(16, 13, 16, 28, "void HttpResponse.Write(string s)", "void Class.Blah(HttpResponse response)", "string Exception.ToString()", "void Class.Blah(HttpResponse response)"));
        }

        [Fact]
        public void NullReferenceExceptionMessage_HtmlSelectInnerHtml()
        {
            this.VerifyCSharp(@"
using System;
using System.Web.UI.HtmlControls;

public class Class
{
    public HtmlSelect Select;
    public void Blah()
    {
        try
        {
            object o = null;
            o.ToString();
        }
        catch (NullReferenceException nre)
        {
            Select.InnerHtml = nre.Message;
        }
    }
}
",
                GetCSharpResultAt(17, 13, 17, 32, "string HtmlSelect.InnerHtml", "void Class.Blah()", "string Exception.Message", "void Class.Blah()"));
        }

        [Fact]
        public void NullReferenceExceptionStackTrace_BulletedListText()
        {
            this.VerifyCSharp(@"
using System;
using System.Web.UI.WebControls;

public class Class
{
    public BulletedList BulletedList;
    public void Blah()
    {
        try
        {
            object o = null;
            o.ToString();
        }
        catch (NullReferenceException nre)
        {
            this.BulletedList.Text = nre.StackTrace;
        }
    }
}
",
                GetCSharpResultAt(17, 13, 17, 38, "string BulletedList.Text", "void Class.Blah()", "string Exception.StackTrace", "void Class.Blah()"));
        }

        [Fact]
        public void TryUsingTryUsingTry()
        {
            this.VerifyCSharp(@"
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

public class Class
{
    public BulletedList BulletedList;

    public async Task DoSomethingNotReallyAsync(Stream stream)
    {
        try
        {
            using (stream)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        ValidateStreamIsNotMemoryStream(stream);
                        sw.Write(""Hello world!"");
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            
        }
    }

    private static void ValidateStreamIsNotMemoryStream(Stream stream)
    {
        if (stream is MemoryStream)
        {
            throw new ArgumentException(nameof(stream));
        }
    }
}
");
        }
    }
}
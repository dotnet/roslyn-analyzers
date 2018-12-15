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
        public void NullReferenceException_HttpResponseWrite()
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
    }
}
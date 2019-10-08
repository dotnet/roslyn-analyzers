// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseXmlReaderForXPathDocumentTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestStreamParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(Stream stream)
    {
        var obj = new XPathDocument(stream);
    }
}",
            GetCSharpResultAt(10, 19, UseXmlReaderForXPathDocument.RealRule, "XPathDocument", "XPathDocument"));
        }

        [Fact]
        public void TestStringParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(string uri)
    {
        var obj = new XPathDocument(uri);
    }
}",
            GetCSharpResultAt(9, 19, UseXmlReaderForXPathDocument.RealRule, "XPathDocument", "XPathDocument"));
        }

        [Fact]
        public void TestStringAndXmlSpaceParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(string uri, XmlSpace space)
    {
        var obj = new XPathDocument(uri, space);
    }
}",
            GetCSharpResultAt(10, 19, UseXmlReaderForXPathDocument.RealRule, "XPathDocument", "XPathDocument"));
        }

        [Fact]
        public void TestTextReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(TextReader reader)
    {
        var obj = new XPathDocument(reader);
    }
}",
            GetCSharpResultAt(10, 19, UseXmlReaderForXPathDocument.RealRule, "XPathDocument", "XPathDocument"));
        }

        [Fact]
        public void TestXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(XmlReader reader)
    {
        var obj = new XPathDocument(reader);
    }
}");
        }

        [Fact]
        public void TestXmlReaderAndXmlSpaceParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.XPath;

class TestClass
{
    public void TestMethod(XmlReader reader, XmlSpace space)
    {
        var obj = new XPathDocument(reader, space);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseXmlReaderForXPathDocument();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseXmlReaderForXPathDocument();
        }
    }
}

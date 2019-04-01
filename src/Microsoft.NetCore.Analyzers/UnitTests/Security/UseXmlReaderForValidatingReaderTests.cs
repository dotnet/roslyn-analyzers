// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseXmlReaderForValidatingReaderTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestStreamAndXmlNodeTypeAndXmlParseContextParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;

class TestClass
{
    public void TestMethod(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
    {
        var obj = new XmlValidatingReader(xmlFragment, fragType, context);
    }
}",
            GetCSharpResultAt(10, 19, UseXmlReaderForValidatingReader.RealRule, "XmlValidatingReader", "XmlValidatingReader"));
        }

        [Fact]
        public void TestStringAndXmlNodeTypeAndXmlParseContextParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    public void TestMethod(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
    {
        var obj = new XmlValidatingReader(xmlFragment, fragType, context);
    }
}",
            GetCSharpResultAt(9, 19, UseXmlReaderForValidatingReader.RealRule, "XmlValidatingReader", "XmlValidatingReader"));
        }

        [Fact]
        public void TestXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    public void TestMethod(XmlReader xmlReader)
    {
        var obj = new XmlValidatingReader(xmlReader);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseXmlReaderForValidatingReader();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseXmlReaderForValidatingReader();
        }
    }
}

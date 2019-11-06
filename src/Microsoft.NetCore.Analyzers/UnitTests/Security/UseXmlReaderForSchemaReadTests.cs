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
    public class UseXmlReaderForSchemaReadTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestReadWithStreamAndValidationEventHandlerParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod(Stream stream, ValidationEventHandler validationEventHandler)
    {
        XmlSchema.Read(stream, validationEventHandler);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForSchemaRead.RealRule, "XmlSchema", "Read"));
        }

        [Fact]
        public void TestTextReaderAndValidationEventHandlerParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod(TextReader reader, ValidationEventHandler validationEventHandler)
    {
        XmlSchema.Read(reader, validationEventHandler);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForSchemaRead.RealRule, "XmlSchema", "Read"));
        }

        [Fact]
        public void TestXmlReaderAndValidationEventHandlerParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod(XmlReader reader, ValidationEventHandler validationEventHandler)
    {
        XmlSchema.Read(reader, validationEventHandler);
    }
}");
        }

        [Fact]
        public void XmlSchemaReadDocSample1_Solution()
        {
            VerifyCSharp(@"
using System.IO;
using System.Xml;
using System.Xml.Schema;

class TestClass
{
    public XmlSchema Test
    {
        get
        {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlReader reader = XmlReader.Create(tr, new XmlReaderSettings() { XmlResolver = null });
            XmlSchema schema = XmlSchema.Read(reader , null);
            return schema;
        }
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseXmlReaderForSchemaRead();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseXmlReaderForSchemaRead();
        }
    }
}

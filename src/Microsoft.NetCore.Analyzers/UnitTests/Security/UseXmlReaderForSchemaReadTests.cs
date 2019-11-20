﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.UseXmlReaderForSchemaRead,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseXmlReaderForSchemaReadTests
    {
        [Fact]
        public async Task TestReadWithStreamAndValidationEventHandlerParametersDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(10, 9, "XmlSchema", "Read"));
        }

        [Fact]
        public async Task TestTextReaderAndValidationEventHandlerParametersDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(10, 9, "XmlSchema", "Read"));
        }

        [Fact]
        public async Task TestXmlReaderAndValidationEventHandlerParametersNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task XmlSchemaReadDocSample1_Solution()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}

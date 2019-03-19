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
    public class DoNotAddSchemaByURLTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestAddWithStringStringParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod()
    {
        XmlSchemaCollection xsc = new XmlSchemaCollection();
        xsc.Add(""urn: bookstore - schema"", ""books.xsd"");
    }
}",
            GetCSharpResultAt(10, 9, DoNotAddSchemaByURL.Rule));
        }

        [Fact]
        public void TestAddWithNullStringParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod()
    {
        XmlSchemaCollection xsc = new XmlSchemaCollection();
        xsc.Add(null, ""urn: bookstore - schema"");
    }
}",
            GetCSharpResultAt(10, 9, DoNotAddSchemaByURL.Rule));
        }

        [Fact]
        public void TestAddWithXmlSchemaCollectionParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod()
    {
        XmlSchemaCollection xsc = new XmlSchemaCollection();
        xsc.Add(xsc);
    }
}");
        }

        [Fact]
        public void TestAddWithXmlSchemaParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Schema;

class TestClass
{
    public void TestMethod()
    {
        XmlSchemaCollection xsc = new XmlSchemaCollection();
        xsc.Add(new XmlSchema());
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotAddSchemaByURL();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotAddSchemaByURL();
        }
    }
}

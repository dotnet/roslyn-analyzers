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

            VerifyBasic(@"
Imports System
Imports System.Xml.Schema

class TestClass
    public Sub TestMethod
        Dim xsc As New XmlSchemaCollection
        xsc.Add(""urn: bookstore - schema"", ""books.xsd"")
    End Sub
End Class",
            GetBasicResultAt(8, 9, DoNotAddSchemaByURL.Rule));
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
        xsc.Add(null, ""books.xsd"");
    }
}",
            GetCSharpResultAt(10, 9, DoNotAddSchemaByURL.Rule));

            VerifyBasic(@"
Imports System
Imports System.Xml.Schema

class TestClass
    public Sub TestMethod
        Dim xsc As New XmlSchemaCollection
        xsc.Add(Nothing, ""books.xsd"")
    End Sub
End Class",
            GetBasicResultAt(8, 9, DoNotAddSchemaByURL.Rule));
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

        [Fact]
        public void TestNormalAddMethodNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Schema;

class TestClass
{
    public static void Add (string ns, string uri)
    {
    }

    public void TestMethod()
    {
        TestClass.Add(""urn: bookstore - schema"", ""books.xsd"");
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

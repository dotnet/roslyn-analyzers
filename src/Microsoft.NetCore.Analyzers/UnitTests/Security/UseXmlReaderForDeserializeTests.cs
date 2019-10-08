// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseXmlReaderForDeserializeTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestDeserializeWithStreamParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(Stream stream)
    {
        new XmlSerializer(typeof(TestClass)).Deserialize(stream);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDeserialize.RealRule, "XmlSerializer", "Deserialize"));
        }

        [Fact]
        public void TestDeserializeWithTextReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(TextReader textReader)
    {
        new XmlSerializer(typeof(TestClass)).Deserialize(textReader);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDeserialize.RealRule, "XmlSerializer", "Deserialize"));
        }

        [Fact]
        public void TestBaseClassInvokesDeserializeWithXmlSerializationReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Serialization;

class TestClass : XmlSerializer
{
    protected override object Deserialize(XmlSerializationReader xmlSerializationReader)
    {
        return base.Deserialize(xmlSerializationReader);
    }
}",
            GetCSharpResultAt(10, 16, UseXmlReaderForDeserialize.RealRule, "XmlSerializer", "Deserialize"));

            VerifyBasic(@"
Imports System
Imports System.IO
Imports System.Xml.Serialization

Class TestClass
    Inherits XmlSerializer
    Protected Overrides Function Deserialize(xmlSerializationReader As XmlSerializationReader) As Object
        Deserialize = MyBase.Deserialize(xmlSerializationReader)
    End Function
End Class",
            GetBasicResultAt(9, 23, UseXmlReaderForDeserialize.RealRule, "XmlSerializer", "Deserialize"));
        }

        [Fact]
        public void TesDerivedClassInvokesDeserializeWithXmlSerializationReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Serialization;

class TestClass : XmlSerializer
{
    protected override object Deserialize(XmlSerializationReader xmlSerializationReader)
    {
        return new TestClass();
    }

    public void TestMethod(XmlSerializationReader xmlSerializationReader)
    {
        Deserialize(xmlSerializationReader);
    }
}",
            GetCSharpResultAt(15, 9, UseXmlReaderForDeserialize.RealRule, "TestClass", "Deserialize"));

            VerifyBasic(@"
Imports System
Imports System.IO
Imports System.Xml.Serialization

Class TestClass
    Inherits XmlSerializer
    Protected Overrides Function Deserialize(xmlSerializationReader As XmlSerializationReader) As Object
        Deserialize = new TestClass()
    End Function

    Public Sub TestMethod(xmlSerializationReader As XmlSerializationReader)
        Deserialize(xmlSerializationReader)
    End Sub
End Class",
            GetBasicResultAt(13, 9, UseXmlReaderForDeserialize.RealRule, "TestClass", "Deserialize"));
        }

        [Fact]
        public void TestWithTwoLevelsOfInheritanceAndOverridesDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml.Serialization;

class TestClass : XmlSerializer
{
    protected override object Deserialize(XmlSerializationReader xmlSerializationReader)
    {
        return new TestClass();
    }
}

class SubTestClass : TestClass
{
    protected override object Deserialize(XmlSerializationReader xmlSerializationReader)
    {
        return new TestClass();
    }

    public void TestMethod(XmlSerializationReader xmlSerializationReader)
    {
        Deserialize(xmlSerializationReader);
    }
}",
            GetCSharpResultAt(23, 9, UseXmlReaderForDeserialize.RealRule, "SubTestClass", "Deserialize"));
        }

        [Fact]
        public void TestDeserializeWithXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(XmlReader xmlReader)
    {
        new XmlSerializer(typeof(TestClass)).Deserialize(xmlReader);
    }
}");
        }

        [Fact]
        public void TestDeserializeWithXmlReaderAndStringParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(XmlReader xmlReader, string str)
    {
        var xmlSerializer = new XmlSerializer(typeof(TestClass));
        new XmlSerializer(typeof(TestClass)).Deserialize(xmlReader, str);
    }
}");
        }

        [Fact]
        public void TestDeserializeWithXmlReaderAndXmlDeserializationEventsParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(XmlReader xmlReader, XmlDeserializationEvents xmlDeserializationEvents)
    {
        new XmlSerializer(typeof(TestClass)).Deserialize(xmlReader, xmlDeserializationEvents);
    }
}");
        }

        [Fact]
        public void TestDeserializeWithXmlReaderAndStringAndXmlDeserializationEventsParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    public void TestMethod(XmlReader xmlReader, string str, XmlDeserializationEvents xmlDeserializationEvents)
    {
        new XmlSerializer(typeof(TestClass)).Deserialize(xmlReader, str, xmlDeserializationEvents);
    }
}");
        }

        [Fact]
        public void TestDerivedFromANormalClassNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    protected virtual object Deserialize (XmlSerializationReader xmlSerializationReader)
    {
        return new TestClass();
    }
}

class SubTestClass : TestClass
{
    protected override object Deserialize(XmlSerializationReader xmlSerializationReader)
    {
        return new SubTestClass();
    }

    public void TestMethod(XmlSerializationReader xmlSerializationReader)
    {
        Deserialize(xmlSerializationReader);
    }
}");
        }

        [Fact]
        public void TestNormalClassReadXmlWithXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

class TestClass
{
    public object Deserialize (XmlSerializationReader xmlSerializationReader)
    {
        return new TestClass();
    }

    public void TestMethod(XmlSerializationReader xmlSerializationReader)
    {
        new TestClass().Deserialize(xmlSerializationReader);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseXmlReaderForDeserialize();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseXmlReaderForDeserialize();
        }
    }
}

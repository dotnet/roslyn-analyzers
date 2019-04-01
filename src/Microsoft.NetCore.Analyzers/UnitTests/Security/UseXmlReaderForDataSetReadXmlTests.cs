// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseXmlReaderForDataSetReadXmlTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestReadXmlWithStreamParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new FileStream(""xmlFilename"", FileMode.Open));
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));

            VerifyBasic(@"
Imports System
Imports System.Data
Imports System.IO

Class TestClass
    Public Sub TestMethod()
        Dim dataSet As new DataSet
        dataSet.ReadXml(new FileStream(""xmlFilename"", FileMode.Open))
    End Sub
End Class",
            GetBasicResultAt(9, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlWithStreamAndXmlReadModeParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Data;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new FileStream(""xmlFilename"", FileMode.Open), XmlReadMode.Auto);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlWithStringParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(""Filename"");
    }
}",
            GetCSharpResultAt(9, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlWithStringXmlReadModeParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(""Filename"", XmlReadMode.Auto);
    }
}",
            GetCSharpResultAt(9, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlWithTextReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new StreamReader(""TestFile.txt""));
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlWithTextReaderAndXmlReadModeParametersDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new StreamReader(""TestFile.txt""), XmlReadMode.Auto);
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXml"));
        }

        [Fact]
        public void TestReadXmlSchemaWithStreamParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXmlSchema(new FileStream(""xmlFilename"", FileMode.Open));
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXmlSchema"));
        }

        [Fact]
        public void TestReadXmlSchemaWithStringParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXmlSchema(""Filename"");
    }
}",
            GetCSharpResultAt(9, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXmlSchema"));
        }

        [Fact]
        public void TestReadXmlSchemaWithTextReaderParameterDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXmlSchema(new StreamReader(""TestFile.txt""));
    }
}",
            GetCSharpResultAt(10, 9, UseXmlReaderForDataSetReadXml.RealRule, "DataSet", "ReadXmlSchema"));
        }

        [Fact]
        public void TestReadXmlWithXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
    }
}");
        }

        [Fact]
        public void TestReadXmlWithXmlReaderAndXmlReadModeParametersNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXml(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)), XmlReadMode.Auto);
    }
}");
        }

        [Fact]
        public void TestReadXmlSchemaWithXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass
{
    public void TestMethod()
    {
        new DataSet().ReadXmlSchema(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
    }
}");
        }

        [Fact]
        public void TestReadXmlSerializableWithXmlReaderParameterNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass : DataSet
{
    protected override void ReadXmlSerializable(XmlReader xmlReader)
    {
    }

    public void TestMethod()
    {
        ReadXmlSerializable(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
    }
}");

            VerifyBasic(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml

Class TestClass
    Inherits DataSet
    Protected Overrides Sub ReadXmlSerializable(xmlReader As XmlReader)
    End Sub
        
    Public Sub TestMethod()
        ReadXmlSerializable(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)))
    End Sub
End Class");
        }

        [Fact]
        public void TestDerivedFromANormalClassNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass
{
    protected virtual void ReadXmlSerializable(XmlReader xmlReader)
    {
    }
}

class SubTestClass : TestClass
{
    protected override void ReadXmlSerializable(XmlReader xmlReader)
    {
    }

    public void TestMethod()
    {
        ReadXmlSerializable(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
    }
}");
        }

        [Fact]
        public void TestTwoLevelsOfInheritanceAndOverridesNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml;

class TestClass : DataSet
{
    protected override void ReadXmlSerializable(XmlReader xmlReader)
    {
    }
}

class SubTestClass : TestClass
{
    protected override void ReadXmlSerializable(XmlReader xmlReader)
    {
    }

    public void TestMethod()
    {
        ReadXmlSerializable(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
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

class TestClass
{
    public void ReadXml (XmlReader reader)
    {
    }

    public void TestMethod()
    {
        var testClass = new TestClass();
        testClass.ReadXml(new XmlTextReader(new FileStream(""xmlFilename"", FileMode.Open)));
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseXmlReaderForDataSetReadXml();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseXmlReaderForDataSetReadXml();
        }
    }
}

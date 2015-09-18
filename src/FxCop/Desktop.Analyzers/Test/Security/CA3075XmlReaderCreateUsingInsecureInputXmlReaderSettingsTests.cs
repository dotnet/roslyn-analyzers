// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string CA3075XmlReaderCreateInsecureInputMessage = DesktopAnalyzersResources.XmlReaderCreateInsecureInputDiagnosis;

        private DiagnosticResult GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, CA3075XmlReaderCreateInsecureInputMessage);
        }

        private DiagnosticResult GetCA3075XmlReaderCreateInsecureInputBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, CA3075XmlReaderCreateInsecureInputMessage);
        }

        [Fact]
        public void XmlReaderSettingsDefaultAsFieldShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public XmlReaderSettings settings = new XmlReaderSettings();

        public void TestMethod(string path)
        {
            var reader = XmlReader.Create(path, settings);  // we treat the field the same as parameter
        }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(12, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public settings As New XmlReaderSettings()

        Public Sub TestMethod(path As String)
            Dim reader = XmlReader.Create(path, settings)
            ' we treat the field the same as parameter
        End Sub
    End Class
End Namespace",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(9, 26));
        }

        [Fact]
        public void XmlReaderSettingsAsFieldSetDtdProcessingToParseWithNoCreateShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse }; 

        public void TestMethod(){}
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public settings As New XmlReaderSettings() With { _
            .DtdProcessing = DtdProcessing.Parse _
        }

        Public Sub TestMethod()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsAsFieldDefaultAndDtdProcessingToIgnoreShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public XmlReaderSettings settings = new XmlReaderSettings(); 

        public void TestMethod(string path)
        {
            this.settings.DtdProcessing = DtdProcessing.Prohibit;
            var reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public settings As New XmlReaderSettings()

        Public Sub TestMethod(path As String)
            Me.settings.DtdProcessing = DtdProcessing.Prohibit
            Dim reader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsAsInputSetDtdProcessingToParseShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path, XmlReaderSettings settings)
        {
            var reader = XmlReader.Create(path, settings);
        }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(10, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String, settings As XmlReaderSettings)
            Dim reader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(7, 26)
             );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

public class TestClass
{
    XmlReaderSettings settings;
    public XmlReader Test
    {
        get
        {
            var xml = """";
            XmlReader reader = XmlReader.Create(xml, settings);
            return reader;
        }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(12, 32)
            );

            VerifyBasic(@"
Imports System.Xml

Public Class TestClass
    Private settings As XmlReaderSettings
    Public ReadOnly Property Test() As XmlReader
        Get
            Dim xml = """"
            Dim reader As XmlReader = XmlReader.Create(xml, settings)
            Return reader
        End Get
    End Property
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(9, 39)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInTryShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass6a
{
    XmlReaderSettings settings;
    private void TestMethod()
    {
        try
        {
            var xml = """";
            var reader = XmlReader.Create(xml, settings);
        }
        catch (Exception) { throw; }
        finally { }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(13, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass6a
    Private settings As XmlReaderSettings
    Private Sub TestMethod()
        Try
            Dim xml = """"
            Dim reader = XmlReader.Create(xml, settings)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(9, 26)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInCatchShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass6a
{
    XmlReaderSettings settings;
    private void TestMethod()
    {
        try {        }
        catch (Exception) { 
            var xml = """";
            var reader = XmlReader.Create(xml, settings);
        }
        finally { }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(13, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass6a
    Private settings As XmlReaderSettings
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim xml = """"
            Dim reader = XmlReader.Create(xml, settings)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(10, 26)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInFinallyShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass6a
{
    XmlReaderSettings settings;
    private void TestMethod()
    {
        try {  }
        catch (Exception) { throw; }
        finally { 
            var xml = """";
            var reader = XmlReader.Create(xml, settings);
        }
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(14, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass6a
    Private settings As XmlReaderSettings
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim xml = """"
            Dim reader = XmlReader.Create(xml, settings)
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(12, 26)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    XmlReaderSettings settings;
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            var reader = XmlReader.Create(xml, settings);
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(12, 26)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private settings As XmlReaderSettings
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim reader = XmlReader.Create(xml, settings)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(10, 22)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    
    delegate void Del();

    Del d = delegate () {
        var xml = """";
        XmlReaderSettings settings = null;
        var reader = XmlReader.Create(xml, settings);
    };
}
",
                GetCA3075XmlReaderCreateInsecureInputCSharpResultAt(12, 22)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass

    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim xml = """"
    Dim settings As XmlReaderSettings = Nothing
    Dim reader = XmlReader.Create(xml, settings)

End Sub
End Class",
                GetCA3075XmlReaderCreateInsecureInputBasicResultAt(11, 18)
            );
        }

        [Fact]
        public void XmlReaderSettingsAsInputSetDtdProcessingToProhibitShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path, XmlReaderSettings settings)
        {
            settings.DtdProcessing = DtdProcessing.Prohibit;
            var reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String, settings As XmlReaderSettings)
            settings.DtdProcessing = DtdProcessing.Prohibit
            Dim reader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsAsInputSetPropertiesToSecureValuesShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path, XmlReaderSettings settings)
        {
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.MaxCharactersFromEntities = (long)1e7;
            settings.XmlResolver = null;
            var reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String, settings As XmlReaderSettings)
            settings.DtdProcessing = DtdProcessing.Parse
            settings.MaxCharactersFromEntities = CLng(10000000.0)
            settings.XmlResolver = Nothing
            Dim reader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void RealCodeSnippitFromCustomerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {         
        public static string TestMethod(string inputRule)
        {
            string outputRule;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();         // CA3075 for not setting secure Xml resolver
                StringReader stringReader = new StringReader(inputRule);
                XmlTextReader textReader = new XmlTextReader(stringReader)
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Auto,
                    IgnoreComments = true,
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };
                XmlReader reader = XmlReader.Create(textReader, settings);
                xmlDoc.Load(reader);
                XmlAttribute enabledAttribute = xmlDoc.CreateAttribute(""enabled"");
                XmlAttributeCollection ruleAttrColl = xmlDoc.DocumentElement.Attributes;
                XmlAttribute nameAttribute = (XmlAttribute)ruleAttrColl.GetNamedItem(""name"");
                ruleAttrColl.Remove(ruleAttrColl[""enabled""]);
                ruleAttrColl.InsertAfter(enabledAttribute, nameAttribute);
                outputRule = xmlDoc.OuterXml;
            }
            catch (XmlException e)
            {
                throw new Exception(""Compliance policy parsing error"", e);
            }
            return outputRule;
        }
    }
}
",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(15, 29)
            );

            VerifyBasic(@"
Imports System.IO
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public Shared Function TestMethod(inputRule As String) As String
            Dim outputRule As String
            Try
                Dim xmlDoc As New XmlDocument()
                ' CA3075 for not setting secure Xml resolver
                Dim stringReader As New StringReader(inputRule)
                Dim textReader As New XmlTextReader(stringReader) With { _
                    .DtdProcessing = DtdProcessing.Ignore, _
                    .XmlResolver = Nothing _
                }
                Dim settings As New XmlReaderSettings() With { _
                    .ConformanceLevel = ConformanceLevel.Auto, _
                    .IgnoreComments = True, _
                    .DtdProcessing = DtdProcessing.Ignore, _
                    .XmlResolver = Nothing _
                }
                Dim reader As XmlReader = XmlReader.Create(textReader, settings)
                xmlDoc.Load(reader)
                Dim enabledAttribute As XmlAttribute = xmlDoc.CreateAttribute(""enabled"")
                Dim ruleAttrColl As XmlAttributeCollection = xmlDoc.DocumentElement.Attributes
                Dim nameAttribute As XmlAttribute = DirectCast(ruleAttrColl.GetNamedItem(""name""), XmlAttribute)
                ruleAttrColl.Remove(ruleAttrColl(""enabled""))
                ruleAttrColl.InsertAfter(enabledAttribute, nameAttribute)
                outputRule = xmlDoc.OuterXml
            Catch e As XmlException
                Throw New Exception(""Compliance policy parsing error"", e)
            End Try
            Return outputRule
        End Function
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(10, 21)
            );
        }
    }
}

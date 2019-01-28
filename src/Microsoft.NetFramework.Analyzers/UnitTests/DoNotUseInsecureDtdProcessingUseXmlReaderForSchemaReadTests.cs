// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private DiagnosticResult GetCA3075SchemaReadCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, string.Format(_CA3075LoadXmlMessage, "Read"));
        }

        private DiagnosticResult GetCA3075SchemaReadBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, string.Format(_CA3075LoadXmlMessage, "Read"));
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Xml.Schema;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            TextReader tr = new StreamReader(path);
            XmlSchema schema = XmlSchema.Read(tr, null);
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(12, 32)
            );

            VerifyBasic(@"
Imports System.IO
Imports System.Xml.Schema

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim tr As TextReader = New StreamReader(path)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        End Sub
    End Class
End Namespace",
                GetCA3075SchemaReadBasicResultAt(9, 39)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Xml.Schema;

class TestClass
{
    public XmlSchema Test
    {
        get
        {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
            return schema;
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(13, 32)
            );

            VerifyBasic(@"
Imports System.IO
Imports System.Xml.Schema

Class TestClass
    Public ReadOnly Property Test() As XmlSchema
        Get
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
            Return schema
        End Get
    End Property
End Class",
                GetCA3075SchemaReadBasicResultAt(10, 39)
            );
        }

        [Fact]
        public void UseUseXmlSchemaReadWithoutXmlTextReaderInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;
using System.IO;
using System.Xml.Schema;

class TestClass
{
    XmlSchema privateDoc;
    public XmlSchema GetDoc
    {
        set
        {
            if (value == null)
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
                privateDoc = schema;
            }
            else
                privateDoc = value;
        }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(17, 36)
            );

            VerifyBasic(@"
Imports System.Data
Imports System.IO
Imports System.Xml.Schema

Class TestClass
    Private privateDoc As XmlSchema
    Public WriteOnly Property GetDoc() As XmlSchema
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim tr As TextReader = New StreamReader(src)
                Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
                privateDoc = schema
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075SchemaReadBasicResultAt(13, 43)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Data;
using System.IO;
using System.Xml.Schema;
class TestClass
{
    private void TestMethod()
    {
        try
        {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075SchemaReadCSharpResultAt(14, 32)
            );

            VerifyBasic(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 39)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
    using System;
    using System.Data;
    using System.IO;
    using System.Xml.Schema;
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception)
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            }
            finally { }
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(15, 36)
            );

            VerifyBasic(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(12, 39)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
    using System;
    using System.Data;
    using System.IO;
    using System.Xml.Schema;
    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { throw; }
            finally
            {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            }
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(16, 36)
            );

            VerifyBasic(@"
Imports System
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim src = """"
            Dim tr As TextReader = New StreamReader(src)
            Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)
        End Try
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(14, 39)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Xml.Schema;
    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => {
                var src = """";
                TextReader tr = new StreamReader(src);
                XmlSchema schema = XmlSchema.Read(tr, null);
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                GetCA3075SchemaReadCSharpResultAt(13, 36)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Async Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim tr As TextReader = New StreamReader(src)
        Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)

End Function)
    End Function

    Private Async Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 35)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithoutXmlTextReaderInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;
using System.IO;
using System.Xml.Schema;
    class TestClass
    {
        delegate void Del();

        Del d = delegate () {
            var src = """";
            TextReader tr = new StreamReader(src);
            XmlSchema schema = XmlSchema.Read(tr, null);
        };
    }",
                GetCA3075SchemaReadCSharpResultAt(12, 32)
            );

            VerifyBasic(@"
Imports System.Data
Imports System.IO
Imports System.Xml.Schema
Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim tr As TextReader = New StreamReader(src)
    Dim schema As XmlSchema = XmlSchema.Read(tr, Nothing)

End Sub
End Class",
                GetCA3075SchemaReadBasicResultAt(11, 31)
            );
        }

        [Fact]
        public void UseXmlSchemaReadWithXmlTextReaderShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Schema;

namespace TestNamespace
{
    public class UseXmlReaderForSchemaRead
    {
        public void TestMethod19(XmlTextReader reader)
        {
            XmlSchema schema = XmlSchema.Read(reader, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Schema

Namespace TestNamespace
    Public Class UseXmlReaderForSchemaRead
        Public Sub TestMethod19(reader As XmlTextReader)
            Dim schema As XmlSchema = XmlSchema.Read(reader, Nothing)
        End Sub
    End Class
End Namespace");
        }
    }
}

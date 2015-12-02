// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private DiagnosticResult CA3075ReadXmlSchemaGetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXmlSchema"));
        }

        private DiagnosticResult CA3075ReadXmlSchemaGetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXmlSchema"));
        }

        [Fact]
        public void UseDataSetReadXmlSchemaShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataSetReadXmlSchema
    {
        public void TestMethod(string path)
        {
            DataSet ds = new DataSet();
            ds.ReadXmlSchema(path);
        }
    }
}
",
                CA3075ReadXmlSchemaGetCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataSetReadXmlSchema
        Public Sub TestMethod(path As String)
            Dim ds As New DataSet()
            ds.ReadXmlSchema(path)
        End Sub
    End Class
End Namespace",
                CA3075ReadXmlSchemaGetBasicResultAt(8, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    public DataSet Test
    {
        get {
            var src = """";
            DataSet ds = new DataSet();
            ds.ReadXmlSchema(src);
            return ds;
        }
    }
}",
                CA3075ReadXmlSchemaGetCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Public ReadOnly Property Test() As DataSet
        Get
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXmlSchema(src)
            Return ds
        End Get
    End Property
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
DataSet privateDoc;
public DataSet GetDoc
        {
            set
            {
                if (value == null)
                {
                    var src = """";
                    DataSet ds = new DataSet();
                    ds.ReadXmlSchema(src);
                    privateDoc = ds;
                }
                else
                    privateDoc = value;
            }
        }
}",
                CA3075ReadXmlSchemaGetCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private privateDoc As DataSet
    Public WriteOnly Property GetDoc() As DataSet
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim ds As New DataSet()
                ds.ReadXmlSchema(src)
                privateDoc = ds
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
  using System;
  using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(src);
            }
            catch (Exception) { throw; }
            finally { }
        }
    }",
                CA3075ReadXmlSchemaGetCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXmlSchema(src)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception)
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(src);
            }
            finally { }
        }
    }",
                CA3075ReadXmlSchemaGetCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXmlSchema(src)
        Finally
        End Try
    End Sub
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
   using System;
   using System.Data;

    class TestClass
    {
        private void TestMethod()
        {
            try { }
            catch (Exception) { throw; }
            finally
            {
                var src = """";
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(src);
            }
        }
    }",
                CA3075ReadXmlSchemaGetCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXmlSchema(src)
        End Try
    End Sub
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
 using System.Threading.Tasks;
using System.Data;

    class TestClass
    {
        private async Task TestMethod()
        {
            await Task.Run(() => {
                var src = """";
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(src);
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                CA3075ReadXmlSchemaGetCSharpResultAt(12, 17)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim ds As New DataSet()
        ds.ReadXmlSchema(src)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(10, 9)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var src = """";
        DataSet ds = new DataSet();
        ds.ReadXmlSchema(src);
    };
}",
                CA3075ReadXmlSchemaGetCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim ds As New DataSet()
    ds.ReadXmlSchema(src)

End Sub
End Class",
                CA3075ReadXmlSchemaGetBasicResultAt(10, 5)
            );
        }

        [Fact]
        public void UseDataSetReadXmlSchemaWithXmlReaderShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataSetReadXmlSchema
    {
        public void TestMethod(XmlReader reader)
        {
            DataSet ds = new DataSet();
            ds.ReadXmlSchema(reader);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataSetReadXmlSchema
        Public Sub TestMethod(reader As XmlReader)
            Dim ds As New DataSet()
            ds.ReadXmlSchema(reader)
        End Sub
    End Class
End Namespace");
        }
    }
}
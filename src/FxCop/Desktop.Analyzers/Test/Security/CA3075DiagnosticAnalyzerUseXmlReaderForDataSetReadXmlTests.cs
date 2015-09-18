// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private DiagnosticResult GetCA3075DataSetReadXmlCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXml"));
        }

        private DiagnosticResult GetCA3075DataSetReadXmlBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXml"));
        }

        [Fact]
        public void UseDataSetReadXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataSetReadXml
    {
        public void TestMethod1214(string path)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(path);
        }
    }
}
",
                GetCA3075DataSetReadXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataSetReadXml
        Public Sub TestMethod1214(path As String)
            Dim ds As New DataSet()
            ds.ReadXml(path)
        End Sub
    End Class
End Namespace",
                GetCA3075DataSetReadXmlBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInGetShouldGenerateDiagnostic()
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
            ds.ReadXml(src);
            return ds;
        }
    }
}",
                GetCA3075DataSetReadXmlCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Public ReadOnly Property Test() As DataSet
        Get
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXml(src)
            Return ds
        End Get
    End Property
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInSetShouldGenerateDiagnostic()
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
                    ds.ReadXml(src);
                    privateDoc = ds;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075DataSetReadXmlCSharpResultAt(15, 21)
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
                ds.ReadXml(src)
                privateDoc = ds
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInTryBlockShouldGenerateDiagnostic()
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
                ds.ReadXml(src);
            }
            catch (Exception) { throw; }
            finally { }
        }
    }",
                GetCA3075DataSetReadXmlCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXml(src)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInCatchBlockShouldGenerateDiagnostic()
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
                ds.ReadXml(src);
            }
            finally { }
        }
    }",
                GetCA3075DataSetReadXmlCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim ds As New DataSet()
            ds.ReadXml(src)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInFinallyBlockShouldGenerateDiagnostic()
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
                ds.ReadXml(src);
            }
        }
    }",
                GetCA3075DataSetReadXmlCSharpResultAt(15, 17)
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
            ds.ReadXml(src)
        End Try
    End Sub
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInAsyncAwaitShouldGenerateDiagnostic()
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
                ds.ReadXml(src);
            });
        }

        private async void TestMethod2()
        {
            await TestMethod();
        }
    }",
                GetCA3075DataSetReadXmlCSharpResultAt(12, 17)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim ds As New DataSet()
        ds.ReadXml(src)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(10, 9)
            );
        }

        [Fact]
        public void UseDataSetReadXmlInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var src = """";
        DataSet ds = new DataSet();
        ds.ReadXml(src);
    };
}",
                GetCA3075DataSetReadXmlCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim ds As New DataSet()
    ds.ReadXml(src)

End Sub
End Class",
                GetCA3075DataSetReadXmlBasicResultAt(10, 5)
            );
        }

        [Fact]
        public void UseDataSetReadXmlWithXmlReaderShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataSetReadXml
    {
        public void TestMethod1214Ok(XmlReader reader)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(reader);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataSetReadXml
        Public Sub TestMethod1214Ok(reader As XmlReader)
            Dim ds As New DataSet()
            ds.ReadXml(reader)
        End Sub
    End Class
End Namespace");
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private DiagnosticResult GetCA3075DataTableReadXmlCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXml"));
        }

        private DiagnosticResult GetCA3075DataTableReadXmlBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, string.Format(CA3075LoadXmlMessage, "ReadXml"));
        }

        [Fact]
        public void UseDataTableReadXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.IO;
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataTableReadXml
    {
        public void TestMethod(Stream stream)
        {
            DataTable table = new DataTable();
            table.ReadXml(stream);
        }
    }
}
",
                GetCA3075DataTableReadXmlCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.IO
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataTableReadXml
        Public Sub TestMethod(stream As Stream)
            Dim table As New DataTable()
            table.ReadXml(stream)
        End Sub
    End Class
End Namespace",
                GetCA3075DataTableReadXmlBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    public DataTable Test
    {
        get {
            var src = """";
            DataTable dt = new DataTable();
            dt.ReadXml(src);
            return dt;
        }
    }
}",
                GetCA3075DataTableReadXmlCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Public ReadOnly Property Test() As DataTable
        Get
            Dim src = """"
            Dim dt As New DataTable()
            dt.ReadXml(src)
            Return dt
        End Get
    End Property
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
DataTable privateDoc;
public DataTable GetDoc
        {
            set
            {
                if (value == null)
                {
                    var src = """";
                    DataTable dt = new DataTable();
                    dt.ReadXml(src);
                    privateDoc = dt;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075DataTableReadXmlCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private privateDoc As DataTable
    Public WriteOnly Property GetDoc() As DataTable
        Set
            If value Is Nothing Then
                Dim src = """"
                Dim dt As New DataTable()
                dt.ReadXml(src)
                privateDoc = dt
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInTryBlockShouldGenerateDiagnostic()
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
                DataTable dt = new DataTable();
                dt.ReadXml(src);
            }
            catch (Exception) { throw; }
            finally { }
        }
    }",
                GetCA3075DataTableReadXmlCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
            Dim src = """"
            Dim dt As New DataTable()
            dt.ReadXml(src)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInCatchBlockShouldGenerateDiagnostic()
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
                DataTable dt = new DataTable();
                dt.ReadXml(src);
            }
            finally { }
        }
    }",
                GetCA3075DataTableReadXmlCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim src = """"
            Dim dt As New DataTable()
            dt.ReadXml(src)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInFinallyBlockShouldGenerateDiagnostic()
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
            DataTable dt = new DataTable();
            dt.ReadXml(src);
        }
    }
}",
                GetCA3075DataTableReadXmlCSharpResultAt(15, 13)
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
            Dim dt As New DataTable()
            dt.ReadXml(src)
        End Try
    End Sub
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInAsyncAwaitShouldGenerateDiagnostic()
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
            DataTable dt = new DataTable();
            dt.ReadXml(src);
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075DataTableReadXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Data

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim src = """"
        Dim dt As New DataTable()
        dt.ReadXml(src)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(10, 9)
            );
        }

        [Fact]
        public void UseDataTableReadXmlInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Data;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var src = """";
        DataTable dt = new DataTable();
        dt.ReadXml(src);
    };
}",
                GetCA3075DataTableReadXmlCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Data

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim src = """"
    Dim dt As New DataTable()
    dt.ReadXml(src)

End Sub
End Class",
                GetCA3075DataTableReadXmlBasicResultAt(10, 5)
            );
        }

        [Fact]
        public void UseDataTableReadXmlWithXmlReaderShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class UseXmlReaderForDataTableReadXml
    {
        public void TestMethod(XmlReader reader)
        {
            DataTable table = new DataTable();
            table.ReadXml(reader);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class UseXmlReaderForDataTableReadXml
        Public Sub TestMethod(reader As XmlReader)
            Dim table As New DataTable()
            table.ReadXml(reader)
        End Sub
    End Class
End Namespace");
        }
    }
}
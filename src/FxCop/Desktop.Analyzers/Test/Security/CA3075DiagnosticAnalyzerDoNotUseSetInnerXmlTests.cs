// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private readonly string CA3075InnerXmlMessage = DesktopAnalyzersResources.DoNotUseSetInnerXmlDiagnosis;

        [Fact]
        public void UseXmlDocumentSetInnerXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class DoNotUseSetInnerXml
    {
        public void TestMethod(string xml)
        {
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class DoNotUseSetInnerXml
        Public Sub TestMethod(xml As String)
            Dim doc As New XmlDocument() With { _
                 .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        End Sub
    End Class
End Namespace",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    public XmlDocument Test
    {
        get {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
            return doc;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Public ReadOnly Property Test() As XmlDocument
        Get
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
            Return doc
        End Get
    End Property
End Class",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
XmlDocument privateDoc;
public XmlDocument GetDoc
        {
            set
            {
                if (value == null)
                {
                    var xml = """";
                    XmlDocument doc = new XmlDocument() { XmlResolver = null };
                    doc.InnerXml = xml;
                    privateDoc = doc;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075InnerXmlCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private privateDoc As XmlDocument
    Public WriteOnly Property GetDoc() As XmlDocument
        Set
            If value Is Nothing Then
                Dim xml = """"
                Dim doc As New XmlDocument() With { _
                     .XmlResolver = Nothing _
                }
                doc.InnerXml = xml
                privateDoc = doc
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075InnerXmlBasicResultAt(13, 17)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                 .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception)
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
        finally { }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                 .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        Finally
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception) { throw; }
        finally
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(15, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                 .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(14, 13)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075InnerXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim doc As New XmlDocument() With { _
            .XmlResolver = Nothing _
        }
        doc.InnerXml = xml

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 9)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var xml = """";
        XmlDocument doc = new XmlDocument() { XmlResolver = null };
        doc.InnerXml = xml;
    };
}",
                GetCA3075InnerXmlCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim xml = """"
    Dim doc As New XmlDocument() With { _
        .XmlResolver = Nothing _
    }
    doc.InnerXml = xml

End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 5)
            );
        }

        [Fact]
        public void UseXmlDocumentSetInnerXmlInlineShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class DoNotUseSetInnerXml
    {
        public void TestMethod(string xml)
        {
            XmlDocument doc = new XmlDocument()
            {
                XmlResolver = null,
                InnerXml = xml
            };
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class DoNotUseSetInnerXml
        Public Sub TestMethod(xml As String)
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing, _
                .InnerXml = xml _
            }
        End Sub
    End Class
End Namespace",
                GetCA3075InnerXmlBasicResultAt(10, 17)
            );
        }

        [Fact]
        public void UseXmlDataDocumentInnerXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class DoNotUseSetInnerXml
    {
        public void TestMethod(string xml)
        {
            XmlDataDocument doc = new XmlDataDocument(){ XmlResolver = null };
            doc.InnerXml = xml;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class DoNotUseSetInnerXml
        Public Sub TestMethod(xml As String)
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        End Sub
    End Class
End Namespace",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    public XmlDataDocument Test
    {
        get {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.InnerXml = xml;
            return doc;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Public ReadOnly Property Test() As XmlDataDocument
        Get
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
            Return doc
        End Get
    End Property
End Class",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
XmlDataDocument privateDoc;
public XmlDataDocument GetDoc
        {
            set
            {
                if (value == null)
                {
                    var xml = """";
                    XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
                    doc.InnerXml = xml;
                    privateDoc = doc;
                }
                else
                    privateDoc = value;
            }
        }
}",
                GetCA3075InnerXmlCSharpResultAt(15, 21)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private privateDoc As XmlDataDocument
    Public WriteOnly Property GetDoc() As XmlDataDocument
        Set
            If value Is Nothing Then
                Dim xml = """"
                Dim doc As New XmlDataDocument() With { _
                    .XmlResolver = Nothing _
                }
                doc.InnerXml = xml
                privateDoc = doc
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class
",
                GetCA3075InnerXmlBasicResultAt(13, 17)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                 .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception)
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
        finally { }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        Finally
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception) { throw; }
        finally
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(15, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.InnerXml = xml
        End Try
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(14, 13)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.InnerXml = xml;
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075InnerXmlCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim doc As New XmlDataDocument() With { _
            .XmlResolver = Nothing _
        }
        doc.InnerXml = xml

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 9)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    delegate void Del();

    Del d = delegate () {
        var xml = """";
        XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
        doc.InnerXml = xml;
    };
}",
                GetCA3075InnerXmlCSharpResultAt(11, 9)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
    Dim xml = """"
    Dim doc As New XmlDataDocument() With { _
        .XmlResolver = Nothing _
    }
    doc.InnerXml = xml

End Sub
End Class",
                GetCA3075InnerXmlBasicResultAt(12, 5)
            );
        }

        [Fact]
        public void UseXmlDataDocumentSetInnerXmlInlineShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Data;

namespace FxCopUnsafeXml
{
    public class DoNotUseSetInnerXml
    {
        public void TestMethod(string xml)
        {
            XmlDataDocument doc = new XmlDataDocument()
            {
                XmlResolver = null,
                InnerXml = xml
            };
        }
    }
}",
                GetCA3075InnerXmlCSharpResultAt(14, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Data

Namespace FxCopUnsafeXml
    Public Class DoNotUseSetInnerXml
        Public Sub TestMethod(xml As String)
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing, _
                .InnerXml = xml _
            }
        End Sub
    End Class
End Namespace",
                GetCA3075InnerXmlBasicResultAt(10, 17)
            );
        }

        private DiagnosticResult GetCA3075InnerXmlCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, CA3075InnerXmlMessage);
        }

        private DiagnosticResult GetCA3075InnerXmlBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, CA3075InnerXmlMessage);
        }
    }
}

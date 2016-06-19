// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Test.Utilities;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_CA3075XmlReaderCreateWrongOverloadMessage = DesktopAnalyzersResources.XmlReaderCreateWrongOverloadMessage;

        private DiagnosticResult GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, s_CA3075XmlReaderCreateWrongOverloadMessage);
        }

        private DiagnosticResult GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, s_CA3075XmlReaderCreateWrongOverloadMessage);
        }

        [Fact]
        public void UseXmlReaderCreateWrongOverloadShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            var reader = XmlTextReader.Create(""doc.xml"");
        }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(10, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim reader = XmlTextReader.Create(""doc.xml"")
        End Sub
    End Class
End Namespace",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(7, 26)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    
    public XmlReader Test
    {
        get {
            XmlReader reader = XmlTextReader.Create(""doc.xml"");
            return reader;
        }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(10, 32)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass

    Public ReadOnly Property Test() As XmlReader
        Get
            Dim reader As XmlReader = XmlTextReader.Create(""doc.xml"")
            Return reader
        End Get
    End Property
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(8, 39)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass1
{
    XmlReader reader;
    public XmlReader Test
    {
        set
        {
            if (value == null)
                reader = XmlTextReader.Create(""doc.xml"");
            else
                reader = value;
        }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(12, 26)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass1
    Private reader As XmlReader
    Public WriteOnly Property Test() As XmlReader
        Set
            If value Is Nothing Then
                reader = XmlTextReader.Create(""doc.xml"")
            Else
                reader = value
            End If
        End Set
    End Property
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(9, 26)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInTryShouldGenerateDiagnostic()
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
            var reader = XmlTextReader.Create(""doc.xml"");
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(11, 26)
            );

            VerifyBasic(@"
Imports System
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
            Dim reader = XmlTextReader.Create(""doc.xml"")
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(8, 26)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInCatchShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try {        }
        catch (Exception) { 
            var reader = XmlTextReader.Create(""doc.xml"");
        }
        finally { }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(11, 26)
            );

            VerifyBasic(@"
Imports System
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim reader = XmlTextReader.Create(""doc.xml"")
        Finally
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(9, 26)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInFinallyShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try {        }
        catch (Exception) { throw; }
        finally {
            var reader = XmlTextReader.Create(""doc.xml"");
        }
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(12, 26)
            );

            VerifyBasic(@"
Imports System
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim reader = XmlTextReader.Create(""doc.xml"")
        End Try
    End Sub
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(11, 26)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => { var reader = XmlTextReader.Create(""doc.xml""); });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(9, 45)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Async Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim reader = XmlTextReader.Create(""doc.xml"")

End Function)
    End Function

    Private Async Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(8, 22)
            );
        }

        [Fact]
        public void UseXmlReaderCreateInsecureOverloadInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    delegate void Del();

    Del d = delegate () { var reader = XmlTextReader.Create(""doc.xml""); };
}",
                GetCA3075XmlReaderCreateWrongOverloadCSharpResultAt(8, 40)
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Delegate Sub Del()

    Private d As Del = Sub() 
                            Dim reader = XmlTextReader.Create(""doc.xml"")
                       End Sub
End Class",
                GetCA3075XmlReaderCreateWrongOverloadBasicResultAt(8, 42)
            );
        }
    }
}

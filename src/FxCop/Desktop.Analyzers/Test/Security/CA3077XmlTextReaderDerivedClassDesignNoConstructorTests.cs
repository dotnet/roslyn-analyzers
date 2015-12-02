// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3077DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void NonXmlTextReaderDerivedTypeWithNoConstructorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            throw new NotImplementedException();
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlResolver
        Public Overrides Function GetEntity(absoluteUri As Uri, role As String, ofObjectToReturn As Type) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace");

        }

        [Fact]
        public void NonXmlTextReaderDerivedTypeWithConstructorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlResolver
    {
        public TestClass() {}

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            throw new NotImplementedException();
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlResolver
        Public Sub New()
        End Sub

        Public Overrides Function GetEntity(absoluteUri As Uri, role As String, ofObjectToReturn As Type) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace");

        }

        [Fact]
        public void TextReaderDerivedTypeWithNoConstructorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader {}
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
    End Class
End Namespace"
            );
        }

        [Fact]
        public void TextReaderDerivedTypeWithMethodAndNoConstructorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {
        public void TestMethod() {};
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub TestMethod()
        End Sub
    End Class
End Namespace"
            );
        }
    }
}


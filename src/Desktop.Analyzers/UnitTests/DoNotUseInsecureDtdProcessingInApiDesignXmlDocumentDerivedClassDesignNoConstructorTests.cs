// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingInApiDesignAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string CA3077NoConstructorMessage = DesktopAnalyzersResources.XmlDocumentDerivedClassNoConstructorMessage;

        private DiagnosticResult GetCA3077NoConstructorCSharpResultAt(int line, int column, string name)
        {
            return GetCSharpResultAt(line, column, CA3077RuleId, string.Format(CA3077NoConstructorMessage, name));
        }

        private DiagnosticResult GetCA3077NoConstructorBasicResultAt(int line, int column, string name)
        {
            return GetBasicResultAt(line, column, CA3077RuleId, string.Format(CA3077NoConstructorMessage, name));
        }

        [Fact]
        public void NonXmlDocumentDerivedTypeWithNoConstructorShouldNotGenerateDiagnostic()
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
        public void NonXmlDocumentDerivedTypeWithConstructorShouldNotGenerateDiagnostic()
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
        public void XmlDocumentDerivedTypeWithNoConstructorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument {}
}",
                GetCA3077NoConstructorCSharpResultAt(7, 11, "TestClass")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
    End Class
End Namespace",
                GetCA3077NoConstructorBasicResultAt(5, 11, "TestClass")
            );
        }
    }
}

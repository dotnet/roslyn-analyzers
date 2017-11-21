// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Test.Utilities;
using Xunit;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_CA3075XmlDocumentWithNoSecureResolverMessage = MicrosoftNetFrameworkAnalyzersResources.XmlDocumentWithNoSecureResolverMessage;

        private DiagnosticResult GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, s_CA3075XmlDocumentWithNoSecureResolverMessage);
        }

        private DiagnosticResult GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, s_CA3075XmlDocumentWithNoSecureResolverMessage);
        }


        [Fact]
        public void XmlDocumentSetResolverToNullShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
            doc.XmlResolver = Nothing
        End Sub
    End Class
End Namespace
"
            );
        }

        [Fact]
        public void XmlDocumentSetResolverToNullInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument()
            {
                XmlResolver = null
            };
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentAsFieldSetResolverToNullInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument()
        {
            XmlResolver = null
        };
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument() With { _
            .XmlResolver = Nothing _
        }
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentAsFieldSetInsecureResolverInInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument() { XmlResolver = new XmlUrlResolver() };
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(8, 54)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument() With { _
            .XmlResolver = New XmlUrlResolver() _
        }
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(7, 13)
            );
        }

        [Fact]
        public void XmlDocumentAsFieldNoResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument();
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(8, 34)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument()
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(6, 37)
            );
        }

        [Fact]
        public void XmlDocumentUseSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XmlSecureResolver resolver)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = resolver;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(resolver As XmlSecureResolver)
            Dim doc As New XmlDocument()
            doc.XmlResolver = resolver
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentSetSecureResolverInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XmlSecureResolver resolver)
        {
            XmlDocument doc = new XmlDocument()
            {
                XmlResolver = resolver
            };
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(resolver As XmlSecureResolver)
            Dim doc As New XmlDocument() With { _
                .XmlResolver = resolver _
            }
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentUseSecureResolverWithPermissionsShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            PermissionSet myPermissions = new PermissionSet(PermissionState.None);
            WebPermission permission = new WebPermission(PermissionState.None);
            permission.AddPermission(NetworkAccess.Connect, ""http://www.contoso.com/"");
            permission.AddPermission(NetworkAccess.Connect, ""http://litwareinc.com/data/"");
            myPermissions.SetPermission(permission);
            XmlSecureResolver resolver = new XmlSecureResolver(new XmlUrlResolver(), myPermissions);

            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = resolver;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Net
Imports System.Security
Imports System.Security.Permissions
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim myPermissions As New PermissionSet(PermissionState.None)
            Dim permission As New WebPermission(PermissionState.None)
            permission.AddPermission(NetworkAccess.Connect, ""http://www.contoso.com/"")
            permission.AddPermission(NetworkAccess.Connect, ""http://litwareinc.com/data/"")
            myPermissions.SetPermission(permission)
            Dim resolver As New XmlSecureResolver(New XmlUrlResolver(), myPermissions)

            Dim doc As New XmlDocument()
            doc.XmlResolver = resolver
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentSetResolverToNullInTryClauseShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.XmlResolver = null;
            }
            catch { throw; }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
            Try
                doc.XmlResolver = Nothing
            Catch
                Throw
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentNoResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument();
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(10, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(7, 24)
            );
        }

        [Fact]
        public void XmlDocumentUseNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver();     // warn
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
            doc.XmlResolver = New XmlUrlResolver()
            ' warn
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(8, 13)
            );
        }

        [Fact]
        public void XmlDocumentUseNonSecureResolverInTryClauseShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        { 
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.XmlResolver = new XmlUrlResolver();    // warn
            }
            catch { throw; }
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(13, 17)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
            Try
                    ' warn
                doc.XmlResolver = New XmlUrlResolver()
            Catch
                Throw
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(10, 17)
            );
        }

        [Fact]
        public void XmlDocumentReassignmentSetResolverToNullInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument();
            doc = new XmlDocument()
            {
                XmlResolver = null
            };
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument()
            doc = New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlDocumentReassignmentDefaultShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XmlDocument doc = new XmlDocument()
            {
                XmlResolver = null
            };
            doc = new XmlDocument();    // warn
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(14, 19)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            doc = New XmlDocument()
            ' warn
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(10, 19)
            );
        }

        [Fact]
        public void XmlDocumentSetResolversInDifferentBlock()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            {
                XmlDocument doc = new XmlDocument();
            }
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
            }
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(11, 35)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            If True Then
                Dim doc As New XmlDocument()
            End If
            If True Then
                Dim doc As New XmlDocument()
                doc.XmlResolver = Nothing
            End If
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(8, 28)
            );
        }

        [Fact]
        public void XmlDocumentAsFieldSetResolverToInsecureResolverInOnlyMethodShouldGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument();

        public void Method1()
        {
            this.doc.XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(8, 34),
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(12, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument()
        ' warn
        Public Sub Method1()
            Me.doc.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(6, 37),
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(9, 13)
            );
        }

        [Fact]
        public void XmlDocumentAsFieldSetResolverToInsecureResolverInSomeMethodShouldGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument();     // warn

        public void Method1()
        {
            this.doc.XmlResolver = null;
        }

        public void Method2()
        {
            this.doc.XmlResolver = new XmlUrlResolver();    // warn
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(8, 34),
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(17, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument()
        ' warn
        Public Sub Method1()
            Me.doc.XmlResolver = Nothing
        End Sub

        Public Sub Method2()
            Me.doc.XmlResolver = New XmlUrlResolver()
            ' warn
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(6, 37),
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void XmlDocumentAsFieldSetResolverToNullInSomeMethodShouldGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlDocument doc = new XmlDocument();

        public void Method1()
        {
            this.doc.XmlResolver = null;
        }

        public void Method2(XmlReader reader)
        {
            this.doc.Load(reader);
        }
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(8, 34)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public doc As XmlDocument = New XmlDocument()

        Public Sub Method1()
            Me.doc.XmlResolver = Nothing
        End Sub

        Public Sub Method2(reader As XmlReader)
            Me.doc.Load(reader)
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(6, 37)
            );
        }

        [Fact]
        public void XmlDocumentCreatedAsTempNotSetResolverShouldGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {

        public void Method1()
        {
            Method2(new XmlDocument());
        }

        public void Method2(XmlDocument doc){}
    }
}",
                GetCA3075XmlDocumentWithNoSecureResolverCSharpResultAt(11, 21)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass

        Public Sub Method1()
            Method2(New XmlDocument())
        End Sub

        Public Sub Method2(doc As XmlDocument)
        End Sub
    End Class
End Namespace",
                GetCA3075XmlDocumentWithNoSecureResolverBasicResultAt(8, 21)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeNotSetResolverShouldNotGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass1 : XmlDocument
    {
        public TestClass1()
        {
            XmlResolver = null;
        }
    }     

    class TestClass2
    {
        void TestMethod()
        {
            var c = new TestClass1();
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass1
        Inherits XmlDocument
        Public Sub New()
            XmlResolver = Nothing
        End Sub
    End Class

    Class TestClass2
        Private Sub TestMethod()
            Dim c = New TestClass1()
        End Sub
    End Class
End Namespace
"
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeWithNoSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class DerivedType : XmlDocument {}   

    class TestClass
    {
        void TestMethod()
        {
            var c = new DerivedType();
        }
    }
    
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class DerivedType
        Inherits XmlDocument
    End Class

    Class TestClass
        Private Sub TestMethod()
            Dim c = New DerivedType()
        End Sub
    End Class

End Namespace"
            );
        }
    }
}

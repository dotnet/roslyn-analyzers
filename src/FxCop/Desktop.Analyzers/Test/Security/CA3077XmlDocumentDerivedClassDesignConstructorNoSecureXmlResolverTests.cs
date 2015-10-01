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
        private const string CA3077RuleId = CA3077DiagnosticAnalyzer.RuleId;

        private readonly string CA3077ConstructorMessage = DesktopAnalyzersResources.XmlDocumentDerivedClassConstructorNoSecureXmlResolverDiagnosis;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicCA3077DiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpCA3077DiagnosticAnalyzer();
        }

        private DiagnosticResult GetCA3077ConstructorCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3077RuleId, CA3077ConstructorMessage);
        }

        private DiagnosticResult GetCA3077ConstructorBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3077RuleId, CA3077ConstructorMessage);
        }

        [Fact]
        public void XmlDocumentDerivedTypeWithEmptyConstructorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {
        public TestClass () {}
    }
}",
                GetCA3077ConstructorCSharpResultAt(9, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(7, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetResolverToNullInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        public TestClass()
        {
            this.XmlResolver = null;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlDocumentDerivedTypeSetInsecureResolverInOnlyCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    { 
        public TestClass(XmlResolver resolver)
        {
            this.XmlResolver = resolver;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(9, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New(resolver As XmlResolver)
            Me.XmlResolver = resolver
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(7, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetInsecureResolverInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        public TestClass()
        {
            this.XmlResolver = null;
        }

        public TestClass(XmlResolver resolver)
        {
            this.XmlResolver = resolver;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(14, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub New(resolver As XmlResolver)
            Me.XmlResolver = resolver
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(11, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetSecureResolverForVariableInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    { 
        public TestClass(XmlDocument doc)
        {
            doc.XmlResolver = null;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(9, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New(doc As XmlDocument)
            doc.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(7, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetSecureResolverWithOutThisInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    { 
        public TestClass(XmlDocument doc)
        {
            doc.XmlResolver = null;
            XmlResolver = null;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New(doc As XmlDocument)
            doc.XmlResolver = Nothing
            XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlDocumentDerivedTypeSetSecureResolverToAXmlDocumentFieldInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    { 
        private XmlDocument doc = new XmlDocument();
        public TestClass(XmlDocument doc)
        {
            this.doc.XmlResolver = null;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(10, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private doc As New XmlDocument()
        Public Sub New(doc As XmlDocument)
            Me.doc.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(8, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetSecureResolverAtLeastOnceInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    { 
        public TestClass(bool flag)
        {
            if (flag)
            {
                XmlResolver = null;
            }
            else
            {
                XmlResolver = new XmlUrlResolver();
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New(flag As Boolean)
            If flag Then
                XmlResolver = Nothing
            Else
                XmlResolver = New XmlUrlResolver()
            End If
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToHidingFieldInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver;
        public TestClass()
        {
            this.XmlResolver = null;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(10, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private XmlResolver As XmlResolver
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(8, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToBaseXmlResolverInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver;
        public TestClass()
        {
            base.XmlResolver = null;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private XmlResolver As XmlResolver
        Public Sub New()
            MyBase.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToBaseXmlResolverInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        private XmlResolver XmlResolver;
        public TestClass()
        {
            base.XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(10, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private XmlResolver As XmlResolver
        Public Sub New()
            MyBase.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(8, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToHidingPropertyInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            this.XmlResolver = null;
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(11, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private Property XmlResolver() As XmlResolver
            Get
                Return m_XmlResolver
            End Get
            Set
                m_XmlResolver = Value
            End Set
        End Property
        Private m_XmlResolver As XmlResolver

        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(17, 20)
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToBaseWithHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            base.XmlResolver = null;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private Property XmlResolver() As XmlResolver
            Get
                Return m_XmlResolver
            End Get
            Set
                m_XmlResolver = Value
            End Set
        End Property
        Private m_XmlResolver As XmlResolver

        Public Sub New()
            MyBase.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToBaseWithHidingPropertyInCtorShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            base.XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077ConstructorCSharpResultAt(11, 16)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Private Property XmlResolver() As XmlResolver
            Get
                Return m_XmlResolver
            End Get
            Set
                m_XmlResolver = Value
            End Set
        End Property
        Private m_XmlResolver As XmlResolver

        Public Sub New()
            MyBase.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077ConstructorBasicResultAt(17, 20)
            );
        }
    }
}

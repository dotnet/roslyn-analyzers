// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Test.Utilities;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDtdProcessingInApiDesignAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string s_CA3077InsecureMethodMessage = DesktopAnalyzersResources.XmlDocumentDerivedClassSetInsecureXmlResolverInMethodMessage;

        private DiagnosticResult GetCA3077InsecureMethodCSharpResultAt(int line, int column, string name)
        {
            return GetCSharpResultAt(line, column, CA3077RuleId, string.Format(s_CA3077InsecureMethodMessage, name));
        }

        private DiagnosticResult GetCA3077InsecureMethodBasicResultAt(int line, int column, string name)
        {
            return GetBasicResultAt(line, column, CA3077RuleId, string.Format(s_CA3077InsecureMethodMessage, name));
        }


        [Fact]
        public void XmlDocumentDerivedTypeNoCtorSetUrlResolverToXmlResolverMethodShouldGenerateDiagnostic()
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
            XmlResolver = null;
        }

        public void method()
        {
            XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077InsecureMethodCSharpResultAt(16, 13, "method")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            XmlResolver = Nothing
        End Sub
        Public Sub method()
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077InsecureMethodBasicResultAt(11, 13, "method")
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            XmlResolver = new XmlUrlResolver();    
        }
    }
}",
                GetCA3077InsecureMethodCSharpResultAt(16, 13, "method")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub method()
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077InsecureMethodBasicResultAt(12, 13, "method")
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToXmlResolverMethodShouldNotGenerateDiagnostic()
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

        public void method()
        {
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
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub method()
            XmlResolver = Nothing
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToThisXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            this.XmlResolver = new XmlUrlResolver();    
        }
    }
}",
                GetCA3077InsecureMethodCSharpResultAt(16, 13, "method")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub method()
            Me.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077InsecureMethodBasicResultAt(12, 13, "method")
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToThisXmlResolverMethodShouldNotGenerateDiagnostic()
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

        public void method()
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

        Public Sub method()
            Me.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToBaseXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            base.XmlResolver = new XmlUrlResolver();    
        }
    }
}",
                GetCA3077InsecureMethodCSharpResultAt(16, 13, "method")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub method()
            MyBase.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077InsecureMethodBasicResultAt(12, 13, "method")
            );
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetNullToBaseXmlResolverMethodShouldNotGenerateDiagnostic()
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

        public void method()
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
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub

        Public Sub method()
            MyBase.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToVariableMethodShouldNotGenerateDiagnostic()
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

        public void method(XmlDocument doc)
        {
            doc.XmlResolver = new XmlUrlResolver();    
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

        Public Sub method(doc As XmlDocument)
            doc.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlDocumentDerivedTypeSetUrlResolverToHidingXmlResolverFieldInMethodShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlDocument 
    {    
        XmlResolver XmlResolver;    //hide XmlDocument.XmlResolver roperty 

        public TestClass()
        {
            base.XmlResolver = null;
        }

        public void method()
        {
            this.XmlResolver = new XmlUrlResolver();    //ok   
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
        'hide XmlDocument.XmlResolver roperty 
        Public Sub New()
            MyBase.XmlResolver = Nothing
        End Sub

        Public Sub method()
            Me.XmlResolver = New XmlUrlResolver()
            'ok   
        End Sub
    End Class
End Namespace");
        }
    }
}

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
        private static readonly string CA3077InsecureMethodMessage = DesktopAnalyzersResources.XmlDocumentDerivedClassSetInsecureXmlResolverInMethodDiagnosis;

        private DiagnosticResult GetCA3077InsecureMethodCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3077RuleId, CA3077InsecureMethodMessage);
        }

        private DiagnosticResult GetCA3077InsecureMethodBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3077RuleId, CA3077InsecureMethodMessage);
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
            DtdProcessing = DtdProcessing.Prohibit;
            XmlResolver = null;
        }

        public void method()
        {
            XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077InsecureMethodCSharpResultAt(17, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlDocument
        Public Sub New()
            DtdProcessing = DtdProcessing.Prohibit
            XmlResolver = Nothing
        End Sub
        Public Sub method()
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077InsecureMethodBasicResultAt(12, 13)
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
                GetCA3077InsecureMethodCSharpResultAt(16, 13)
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
                GetCA3077InsecureMethodBasicResultAt(12, 13)
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
                GetCA3077InsecureMethodCSharpResultAt(16, 13)
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
                GetCA3077InsecureMethodBasicResultAt(12, 13)
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
                GetCA3077InsecureMethodCSharpResultAt(16, 13)
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
                GetCA3077InsecureMethodBasicResultAt(12, 13)
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

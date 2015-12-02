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
        public void TextReaderDerivedTypeWithEmptyConstructorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {
        public TestClass () {}
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New()
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeNullResolverAndProhibitInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeUrlResolverAndProhibitInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass()
        {
            this.XmlResolver = new XmlUrlResolver();
            this.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New()
            Me.XmlResolver = New XmlUrlResolver()
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSecureResolverAndParseInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass(XmlSecureResolver resolver)
        {
            this.XmlResolver = resolver;
            this.DtdProcessing = DtdProcessing.Parse;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New(resolver As XmlSecureResolver)
            Me.XmlResolver = resolver
            Me.DtdProcessing = DtdProcessing.Parse
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeNullResolverInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
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
        Inherits XmlTextReader
        Public Sub New()
            Me.XmlResolver = Nothing
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeIgnoreInOnlyCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass(XmlSecureResolver resolver)
        {
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New(resolver As XmlSecureResolver)
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetInsecureResolverInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }

        public TestClass(XmlResolver resolver)
        {
            this.XmlResolver = resolver;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
	Class TestClass
		Inherits XmlTextReader
		Public Sub New()
			Me.XmlResolver = Nothing
			Me.DtdProcessing = DtdProcessing.Ignore
		End Sub

		Public Sub New(resolver As XmlResolver)
			Me.XmlResolver = resolver
			Me.DtdProcessing = DtdProcessing.Ignore
		End Sub
	End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSecureSettingsForVariableInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    { 
        public TestClass(XmlTextReader reader)
        {
            reader.XmlResolver = null;
            reader.DtdProcessing = DtdProcessing.Ignore
        }
    }
}"
            );

            VerifyBasic(@"CONVERSION ERROR: Code could not be converted. Details:

-- line 13 col 9: this symbol not expected in EmbeddedStatement

Please check for any errors in the original code and try again."
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSecureSettingsWithOutThisInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    { 
        public TestClass(XmlTextReader reader)
        {
            reader.XmlResolver = null;
            XmlResolver = null;
            DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New(reader As XmlTextReader)
            reader.XmlResolver = Nothing
            XmlResolver = Nothing
            DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsToAXmlTextReaderFieldInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    { 
        private XmlTextReader reader = new XmlTextReader(""path"");
        public TestClass()
        {
            this.reader.XmlResolver = null;  
            this.reader.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private reader As New XmlTextReader("""")
        Public Sub New()
            Me.reader.XmlResolver = Nothing
            Me.reader.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsAtLeastOnceInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    { 
        public TestClass(bool flag)
        {
            if (flag)
            {
                XmlResolver = null;
                DtdProcessing = DtdProcessing.Ignore;
            }
            else
            {
                XmlResolver = new XmlUrlResolver();
                DtdProcessing = DtdProcessing.Parse;
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New(flag As Boolean)
            If flag Then
                XmlResolver = Nothing
                DtdProcessing = DtdProcessing.Ignore
            Else
                XmlResolver = New XmlUrlResolver()
                DtdProcessing = DtdProcessing.Parse
            End If
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsAtLeastOnceInCtorShouldNotGenerateDiagnosticFalseNeg()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    { 
        public TestClass(bool flag)
        {
            if (flag)
            {
                XmlResolver = null;
                DtdProcessing = DtdProcessing.Parse;
            }
            else
            {
                XmlResolver = new XmlUrlResolver();
                DtdProcessing = DtdProcessing.Ignore;
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New(flag As Boolean)
            If flag Then
                XmlResolver = Nothing
                DtdProcessing = DtdProcessing.Parse
            Else
                XmlResolver = New XmlUrlResolver()
                DtdProcessing = DtdProcessing.Ignore
            End If
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetIgnoreToHidingFieldInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing;
        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private DtdProcessing As DtdProcessing
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetNullToHidingFieldInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        XmlResolver XmlResolver;
        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private XmlResolver As XmlResolver
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetNullToBaseXmlResolverInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        XmlResolver XmlResolver;
        public TestClass()
        {
            base.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private XmlResolver As XmlResolver
        Public Sub New()
            MyBase.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetProhibitToBaseInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing;
        public TestClass()
        {
            this.XmlResolver = null;
            base.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private DtdProcessing As DtdProcessing
        Public Sub New()
            Me.XmlResolver = Nothing
            MyBase.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsToBaseWithHidingFieldsInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing;
        XmlResolver XmlResolver;
        public TestClass()
        {
            base.XmlResolver = null;
            base.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private DtdProcessing As DtdProcessing
        Private XmlResolver As XmlResolver
        Public Sub New()
            MyBase.XmlResolver = Nothing
            MyBase.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsToBaseInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        public TestClass()
        {
            base.XmlResolver = null;
            base.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New()
            MyBase.XmlResolver = Nothing
            MyBase.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetUrlResolverToBaseXmlResolverInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {                 
        DtdProcessing DtdProcessing;
        XmlResolver XmlResolver;
        public TestClass()
        {
            base.XmlResolver = new XmlUrlResolver();
            base.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private DtdProcessing As DtdProcessing
        Private XmlResolver As XmlResolver
        Public Sub New()
            MyBase.XmlResolver = New XmlUrlResolver()
            MyBase.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetNullToHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
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
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetProhibitToHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing { set; get; }

        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private Property DtdProcessing() As DtdProcessing
            Get
                Return m_DtdProcessing
            End Get
            Set
                m_DtdProcessing = Value
            End Set
        End Property
        Private m_DtdProcessing As DtdProcessing

        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetSecureSettingsToHidingPropertiesInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing { set; get; }   
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            this.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private Property DtdProcessing() As DtdProcessing
            Get
                Return m_DtdProcessing
            End Get
            Set
                m_DtdProcessing = Value
            End Set
        End Property
        Private m_DtdProcessing As DtdProcessing
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
            Me.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetNullToBaseWithHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        XmlResolver XmlResolver { set; get; }

        public TestClass()
        {
            base.XmlResolver = null;
            this.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
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
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetIgnoreToBaseWithHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        DtdProcessing DtdProcessing { set; get; }

        public TestClass()
        {
            this.XmlResolver = null;
            base.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private Property DtdProcessing() As DtdProcessing
            Get
                Return m_DtdProcessing
            End Get
            Set
                m_DtdProcessing = Value
            End Set
        End Property
        Private m_DtdProcessing As DtdProcessing

        Public Sub New()
            Me.XmlResolver = Nothing
            MyBase.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetParseToBaseWithHidingPropertyInCtorShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {    
        XmlResolver XmlResolver { set; get; }  
        DtdProcessing DtdProcessing { set; get; }

        public TestClass()
        {
            base.XmlResolver = null;
            base.DtdProcessing = DtdProcessing.Parse;
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Private Property XmlResolver() As XmlResolver
            Get
                Return m_XmlResolver
            End Get
            Set
                m_XmlResolver = Value
            End Set
        End Property
        Private m_XmlResolver As XmlResolver
        Private Property DtdProcessing() As DtdProcessing
            Get
                Return m_DtdProcessing
            End Get
            Set
                m_DtdProcessing = Value
            End Set
        End Property
        Private m_DtdProcessing As DtdProcessing

        Public Sub New()
            MyBase.XmlResolver = Nothing
            MyBase.DtdProcessing = DtdProcessing.Parse
        End Sub
    End Class
End Namespace"
            );
        }
    }
}

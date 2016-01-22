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
        private static readonly string CA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodMessage = DesktopAnalyzersResources.XmlTextReaderDerivedClassSetInsecureSettingsInMethodDiagnosis;

        private DiagnosticResult GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3077RuleId, CA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodMessage);
        }

        private DiagnosticResult GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3077RuleId, CA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodMessage);
        }

        [Fact]
        public void XmlTextReaderDerivedTypeNoCtorSetUrlResolverToXmlResolverMethodShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class TestClass : XmlTextReader 
    {
        public void method()
        {
            XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(11, 13)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub method()
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(8, 13)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetUrlResolverToXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(17, 13)
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

        Public Sub method()
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetDtdProcessingToParseMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            DtdProcessing = DtdProcessing.Parse;
        }
    }
}",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(17, 13)
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

        Public Sub method()
            DtdProcessing = DtdProcessing.Parse
        End Sub
    End Class
End Namespace",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetUrlResolverToThisXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            this.XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(17, 13)
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

        Public Sub method()
            Me.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetUrlResolverToBaseXmlResolverMethodShouldGenerateDiagnostic()
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

        public void method()
        {
            base.XmlResolver = new XmlUrlResolver();
        }
    }
}",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(17, 13)
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

        Public Sub method()
            MyBase.XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetXmlResolverToNullMethodShouldNotGenerateDiagnostic()
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
        Inherits XmlTextReader
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub

        Public Sub method()
            XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetDtdProcessingToProhibitMethodShouldNotGenerateDiagnostic()
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

        public void method()
        {
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
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub

        Public Sub method()
            DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetDtdProcessingToTypoMethodShouldNotGenerateDiagnostic()
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

        public void method()
        {
            DtdProcessing = DtdProcessing.prohibit;
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

        Public Sub method()
            DtdProcessing = DtdProcessing.prohibit
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeParseAndNullResolverMethodShouldNotGenerateDiagnostic()
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

        public void method()
        {
            DtdProcessing = DtdProcessing.Parse;
            XmlResolver = null;
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

        Public Sub method()
            DtdProcessing = DtdProcessing.Parse
            XmlResolver = Nothing
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeIgnoreAndUrlResolverMethodShouldNotGenerateDiagnostic()
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

        public void method()
        {
            DtdProcessing = DtdProcessing.Ignore;
            XmlResolver = new XmlUrlResolver();
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

        Public Sub method()
            DtdProcessing = DtdProcessing.Ignore
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeParseAndUrlResolverMethodShouldGenerateDiagnostic()
        {
            var diagWith2Locations = GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodCSharpResultAt(17, 13);

            diagWith2Locations.Locations = new DiagnosticResultLocation[] 
                {
                    diagWith2Locations.Locations[0],
                    new DiagnosticResultLocation(diagWith2Locations.Locations[0].Path, 18, 13)
                };

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

        public void method()
        {
            DtdProcessing = DtdProcessing.Parse;
            XmlResolver = new XmlUrlResolver();
        }
    }
}",
                diagWith2Locations
            );

            diagWith2Locations = GetCA3077XmlTextReaderDerivedClassSetInsecureSettingsInMethodBasicResultAt(13, 13);

            diagWith2Locations.Locations = new DiagnosticResultLocation[]
                {
                    diagWith2Locations.Locations[0],
                    new DiagnosticResultLocation(diagWith2Locations.Locations[0].Path, 14, 13)
                };

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Inherits XmlTextReader
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub

        Public Sub method()
            DtdProcessing = DtdProcessing.Parse
            XmlResolver = New XmlUrlResolver()
        End Sub
    End Class
End Namespace",
                diagWith2Locations
            );
        }


        [Fact]
        public void XmlTextReaderDerivedTypeSecureResolverInOnePathMethodShouldNotGenerateDiagnostic()
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

        public void method(bool flag)
        {
            DtdProcessing = DtdProcessing.Parse;
            if (flag)
            {
                XmlResolver = null;
            }
            else
            {  
                XmlResolver = new XmlUrlResolver();   // intended false negative, due to the lack of flow analysis
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
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub

        Public Sub method(flag As Boolean)
            DtdProcessing = DtdProcessing.Parse
            If flag Then
                XmlResolver = Nothing
            Else
                    ' intended false negative, due to the lack of flow analysis
                XmlResolver = New XmlUrlResolver()
            End If
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void XmlTextReaderDerivedTypeSetInsecureSettingsInSeperatePathsMethodShouldNotGenerateDiagnostic()
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

        public void method(bool flag)
        {
            if (flag)
            {
                // secure
                DtdProcessing = DtdProcessing.Ignore;
                XmlResolver = null;
            }
            else
            {  
                // insecure
                DtdProcessing = DtdProcessing.Parse;
                XmlResolver = new XmlUrlResolver();   // intended false negative, due to the lack of flow analysis
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
        Public Sub New()
            Me.XmlResolver = Nothing
            Me.DtdProcessing = DtdProcessing.Prohibit
        End Sub

        Public Sub method(flag As Boolean)
            If flag Then
                ' secure
                DtdProcessing = DtdProcessing.Ignore
                XmlResolver = Nothing
            Else
                ' insecure
                DtdProcessing = DtdProcessing.Parse
                    ' intended false negative, due to the lack of flow analysis
                XmlResolver = New XmlUrlResolver()
            End If
        End Sub
    End Class
End Namespace");

        }
    }
}


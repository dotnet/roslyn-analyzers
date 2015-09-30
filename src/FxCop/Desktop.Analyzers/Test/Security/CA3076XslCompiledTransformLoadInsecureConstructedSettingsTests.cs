// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3076DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private const string CA3076RuleId = CA3076DiagnosticAnalyzer<SyntaxKind>.RuleId;

        private readonly string CA3076LoadInsecureInputMessage = DesktopAnalyzersResources.XslCompiledTransformLoadInsecureInputDiagnosis;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicCA3076DiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpCA3076DiagnosticAnalyzer();
        }

        private DiagnosticResult GetCA3076LoadCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3076RuleId, CA3076LoadInsecureInputMessage);
        }

        private DiagnosticResult GetCA3076LoadBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3076RuleId, CA3076LoadInsecureInputMessage);
        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload1ShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(IXPathNavigable stylesheet)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(stylesheet);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Xml.XPath

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheet As IXPathNavigable)
            Dim xslCompiledTransform As New XslCompiledTransform()
            xslCompiledTransform.Load(stylesheet)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload1InTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml.Xsl;
using System.Xml.XPath;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(IXPathNavigable stylesheet)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheet);
            }
            catch { throw; }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl
Imports System.Xml.XPath

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheet As IXPathNavigable)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheet)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload1InCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml.Xsl;
using System.Xml.XPath;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(IXPathNavigable stylesheet)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheet);
            }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl
Imports System.Xml.XPath

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheet As IXPathNavigable)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheet)
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload1InFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml.Xsl;
using System.Xml.XPath;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(IXPathNavigable stylesheet)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheet);
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl
Imports System.Xml.XPath

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheet As IXPathNavigable)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheet)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload2ShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(String stylesheetUri)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(stylesheetUri);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheetUri As [String])
            Dim xslCompiledTransform As New XslCompiledTransform()
            xslCompiledTransform.Load(stylesheetUri)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload2InTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(String stylesheetUri)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheetUri);
            }
            catch { throw; }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheetUri As [String])
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheetUri)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload2InCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(String stylesheetUri)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheetUri);            
            }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheetUri As [String])
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheetUri)
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload2InFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(String stylesheetUri)
        {
            try {   }
            catch { throw; }
            finally { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(stylesheetUri);    
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(stylesheetUri As [String])
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(stylesheetUri)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload3ShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(Type compiledStylesheet)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(compiledStylesheet);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(compiledStylesheet As Type)
            Dim xslCompiledTransform As New XslCompiledTransform()
            xslCompiledTransform.Load(compiledStylesheet)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload3InTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(Type compiledStylesheet)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(compiledStylesheet);
            }
            catch { throw; }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(compiledStylesheet As Type)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(compiledStylesheet)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload3InCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(Type compiledStylesheet)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(compiledStylesheet);            
            }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(compiledStylesheet As Type)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(compiledStylesheet)
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSecureOverload3InFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(Type compiledStylesheet)
        {
            try {   }
            catch { throw; }
            finally { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(compiledStylesheet);    
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(compiledStylesheet As Type)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                xslCompiledTransform.Load(compiledStylesheet)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadTrustedXsltAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings = System.Xml.Xsl.XsltSettings.TrustedXslt;
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = System.Xml.Xsl.XsltSettings.TrustedXslt
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadTrustedXsltAndNullResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings = System.Xml.Xsl.XsltSettings.TrustedXslt;
            xslCompiledTransform.Load(""testStylesheet"", settings, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = System.Xml.Xsl.XsltSettings.TrustedXslt
            xslCompiledTransform.Load("""", settings, Nothing)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadTrustedSourceAndSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        { 
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings = System.Xml.Xsl.XsltSettings.TrustedXslt;
            var resolver = new XmlSecureResolver(new XmlUrlResolver(), """");
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
	Class TestClass
		Private Shared Sub TestMethod()
			Dim xslCompiledTransform As New XslCompiledTransform()
			Dim settings = System.Xml.Xsl.XsltSettings.TrustedXslt
			Dim resolver = New XmlSecureResolver(New XmlUrlResolver(), """")
			xslCompiledTransform.Load(""testStylesheet"", settings, resolver)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void UseXslCompiledTransformLoadDefaultAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings();
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings()
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadDefaultAndSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings();
            xslCompiledTransform.Load(""testStylesheet"", settings, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings()
            xslCompiledTransform.Load("""", settings, Nothing)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadDefaultPropertyAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  XsltSettings.Default;
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = XsltSettings.[Default]
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadEnableScriptAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings() { EnableScript = true };
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings() With { _
                .EnableScript = True _
            }
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadSetEnableScriptToTrueAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings();
            settings.EnableScript = true;
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings()
            settings.EnableScript = True
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadEnableDocumentFunctionAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings() { EnableDocumentFunction = true };
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings() With { _
                .EnableDocumentFunction = True _
            }
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(13, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadSetEnableDocumentFunctionToTrueAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings();
            settings.EnableDocumentFunction = true;
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings()
            settings.EnableDocumentFunction = True
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadSetEnableDocumentFunctionToTrueAndSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings() { EnableDocumentFunction = true };
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings() With { _
                .EnableDocumentFunction = True _
            }
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, Nothing)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadSetEnableScriptPropertyToTrueAndSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings();
            settings.EnableScript = true;
            xslCompiledTransform.Load(""testStylesheet"", settings, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings()
            settings.EnableScript = True
            xslCompiledTransform.Load("""", settings, Nothing)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadConstructSettingsWithTrueParamAndNonSecureResolverShouldGenerateDiagnostic1()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings(true, false);
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings(True, False)
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadConstructSettingsWithTrueParamAndNonSecureResolverShouldGenerateDiagnostic2()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings(false, true);
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(14, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings(False, True)
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadConstructSettingsWithFalseParamsAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var settings =  new XsltSettings(false, false);
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim settings = New XsltSettings(False, False)
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadNullSettingsAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", null, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", Nothing, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadDefaultAsArgumentAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", XsltSettings.Default, resolver);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", XsltSettings.[Default], resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadTrustedXsltAsArgumentAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", XsltSettings.TrustedXslt, resolver);
        }
    }
}",
                GetCA3076LoadInsecureConstructedCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod()
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", XsltSettings.TrustedXslt, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadInsecureConstructedBasicResultAt(10, 13)
            );
        }
    }
}

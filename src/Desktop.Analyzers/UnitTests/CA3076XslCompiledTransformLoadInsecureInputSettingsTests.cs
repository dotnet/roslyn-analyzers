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
        private readonly string CA3076LoadInsecureConstructedMessage = DesktopAnalyzersResources.XslCompiledTransformLoadInsecureConstructedDiagnosis;

        private DiagnosticResult GetCA3076LoadInsecureConstructedCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3076RuleId, CA3076LoadInsecureConstructedMessage);
        }

        private DiagnosticResult GetCA3076LoadInsecureConstructedBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3076RuleId, CA3076LoadInsecureConstructedMessage);
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            var resolver = new XmlUrlResolver();
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
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
        Private Shared Sub TestMethod(settings As XsltSettings)
            Dim xslCompiledTransform As New XslCompiledTransform()
            Dim resolver = New XmlUrlResolver()
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadInsecureConstructedBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsAndNonSecureResolverInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                var resolver = new XmlUrlResolver();
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}",
                GetCA3076LoadInsecureConstructedCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                Dim resolver = New XmlUrlResolver()
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadInsecureConstructedBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsAndNonSecureResolverInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                var resolver = new XmlUrlResolver();
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}",
                GetCA3076LoadInsecureConstructedCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                Dim resolver = New XmlUrlResolver()
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadInsecureConstructedBasicResultAt(12, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsAndNonSecureResolverInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                var resolver = new XmlUrlResolver();
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}",
                GetCA3076LoadInsecureConstructedCSharpResultAt(16, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                Dim resolver = New XmlUrlResolver()
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadInsecureConstructedBasicResultAt(14, 17)
            );
        }
        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsAndNullResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
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
        Private Shared Sub TestMethod(settings As XsltSettings)
            Dim xslCompiledTransform As New XslCompiledTransform()
            xslCompiledTransform.Load("""", settings, Nothing)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsReconstructDefaultAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings = XsltSettings.Default;
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
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings = XsltSettings.[Default]
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsReconstructTrustedXsltAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings = XsltSettings.TrustedXslt;
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings = XsltSettings.TrustedXslt
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsReconstructTrustedXsltAndNonSecureResolverInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try
            {              
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings = XsltSettings.TrustedXslt;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings = XsltSettings.TrustedXslt
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsReconstructTrustedXsltAndNonSecureResolverInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings = XsltSettings.TrustedXslt;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings = XsltSettings.TrustedXslt
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsReconstructTrustedXsltAndNonSecureResolverInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings = XsltSettings.TrustedXslt;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(16, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings = XsltSettings.TrustedXslt
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(14, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToFalseAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings.EnableScript = false;
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableScript = False
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToFalseAndNonSecureResolverInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try
            {              
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToFalseAndNonSecureResolverInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToFalseAndNonSecureResolverInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(16, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(14, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlSecureResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings.EnableScript = true;
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
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlSecureResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableScript = True
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndSecureResolverInTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlSecureResolver resolver)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlSecureResolver)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndSecureResolverInCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlSecureResolver resolver)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlSecureResolver)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndSecureResolverInFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlSecureResolver resolver)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlSecureResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndSecureResolverAsyncAwaitShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlSecureResolver resolver)
        {
            try {   }
            catch { throw; }
            finally {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlSecureResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndNonSecureResolverShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings.EnableScript = true;
            xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(13, 13)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableScript = True
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(10, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndNonSecureResolverInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(11, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndNonSecureResolverInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(15, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndNonSecureResolverInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { throw; }
            finally {                 
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}",
                GetCA3076LoadCSharpResultAt(16, 17)
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableScript = True
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(14, 17)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetOneToTrueAndNonSecureResolverAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private async Task TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            await Task.Run(() =>
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableScript = true;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            });
        }
        private async void TestMethod2()
        {
            await TestMethod(null, null);
        }
    }
}",
                GetCA3076LoadCSharpResultAt(16, 17)
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Function TestMethod(settings As XsltSettings, resolver As XmlResolver) As Task
            Await Task.Run(Function() 
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableScript = True
            xslCompiledTransform.Load("""", settings, resolver)

End Function)
        End Function
        Private Sub TestMethod2()
            Await TestMethod(Nothing, Nothing)
        End Sub
    End Class
End Namespace",
                GetCA3076LoadBasicResultAt(12, 13)
            );
        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetBothToFalseAndNonSecureResolverShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            settings.EnableDocumentFunction = false;
            settings.EnableScript = false;
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
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableDocumentFunction = False
            settings.EnableScript = False
            xslCompiledTransform.Load("""", settings, resolver)
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetBothToFalseAndNonSecureResolverInTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableDocumentFunction = false;
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            catch { throw; }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableDocumentFunction = False
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetBothToFalseAndNonSecureResolverInCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch 
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableDocumentFunction = false;
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
            finally { }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableDocumentFunction = False
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            Finally
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetBothToFalseAndNonSecureResolverInFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            try {   }
            catch { throw; }
            finally 
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableDocumentFunction = false;
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            }
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(settings As XsltSettings, resolver As XmlResolver)
            Try
            Catch
                Throw
            Finally
                Dim xslCompiledTransform As New XslCompiledTransform()
                settings.EnableDocumentFunction = False
                settings.EnableScript = False
                xslCompiledTransform.Load("""", settings, resolver)
            End Try
        End Sub
    End Class
End Namespace");

        }

        [Fact]
        public void UseXslCompiledTransformLoadInputSettingsSetBothToFalseAndNonSecureResolverAsyncAwaitShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace TestNamespace
{
    class TestClass
    {
        private async Task TestMethod(XsltSettings settings, XmlResolver resolver)
        {
            await Task.Run(() =>
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                settings.EnableDocumentFunction = false;
                settings.EnableScript = false;
                xslCompiledTransform.Load(""testStylesheet"", settings, resolver);
            });
        }
        private async void TestMethod2()
        {
            await TestMethod(null, null);
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml
Imports System.Xml.Xsl

Namespace TestNamespace
    Class TestClass
        Private Function TestMethod(settings As XsltSettings, resolver As XmlResolver) As Task
            Await Task.Run(Function() 
            Dim xslCompiledTransform As New XslCompiledTransform()
            settings.EnableDocumentFunction = False
            settings.EnableScript = False
            xslCompiledTransform.Load("""", settings, resolver)

End Function)
        End Function
        Private Sub TestMethod2()
            Await TestMethod(Nothing, Nothing)
        End Sub
    End Class
End Namespace");

        }
    }
}

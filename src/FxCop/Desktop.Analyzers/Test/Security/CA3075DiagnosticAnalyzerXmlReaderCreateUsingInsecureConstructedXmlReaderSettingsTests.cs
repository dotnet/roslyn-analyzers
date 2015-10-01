// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string CA3075XmlReaderCreateUsingInsecureConstructedXmlReaderSettingsMessage = DesktopAnalyzersResources.XmlReaderCreateInsecureConstructedDiagnosis;

        private DiagnosticResult GetCA3075XmlReaderCreateUsingInsecureConstructedXmlReaderSettingsCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, CA3075XmlReaderCreateUsingInsecureConstructedXmlReaderSettingsMessage);
        }

        private DiagnosticResult GetCA3075XmlReaderCreateUsingInsecureConstructedXmlReaderSettingsBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, CA3075XmlReaderCreateUsingInsecureConstructedXmlReaderSettingsMessage);
        }

        [Fact]
        public void DefaultXmlReaderSettingsShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );
            
            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings()
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }
        
        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInInitializerTargetFx452ShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Reflection;               
using System.Xml;   

[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.2"", FrameworkDisplayName = "".NET Framework 4.5.2"")]

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Reflection
Imports System.Xml

<Assembly: System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework, Version = v4.5.2"", FrameworkDisplayName := "".NET Framework 4.5.2"")>

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With {
                .DtdProcessing = DtdProcessing.Parse _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsOnlySetMaxCharRoZeroInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings(){ MaxCharactersFromEntities = 0 };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With { _
                .MaxCharactersFromEntities = 0 _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetSecureResolverInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path, XmlSecureResolver resolver)
        {
            XmlReaderSettings settings = new XmlReaderSettings(){ XmlResolver = resolver };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String, resolver As XmlSecureResolver)
            Dim settings As New XmlReaderSettings() With { _
                .XmlResolver = resolver _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseAndMaxCharToNonZeroInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
                                        {
                                            DtdProcessing = DtdProcessing.Parse,
                                            MaxCharactersFromEntities = (long)1e7
                                        };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse, _
                .MaxCharactersFromEntities = CLng(10000000.0) _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseAndSecureResolverInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path, XmlSecureResolver resolver)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
                                        {
                                            DtdProcessing = DtdProcessing.Parse,
                                            XmlResolver = resolver
                                        };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String, resolver As XmlSecureResolver)
            Dim settings As New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse, _
                .XmlResolver = resolver _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseWithOtherValuesSecureInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
                                        {
                                            DtdProcessing = DtdProcessing.Parse,
                                            MaxCharactersFromEntities = (long)1e7,
                                            XmlResolver = null
                                        };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse, _
                .MaxCharactersFromEntities = CLng(10000000.0), _
                .XmlResolver = Nothing _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings()
            settings.DtdProcessing = DtdProcessing.Parse
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System;
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(path, settings);
            }
            catch (Exception) { throw; }
            finally { }
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
                Dim settings As New XmlReaderSettings()
                settings.DtdProcessing = DtdProcessing.Parse
                Dim reader As XmlReader = XmlReader.Create(path, settings)
            Catch generatedExceptionName As Exception
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System;
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try { }
            catch (Exception) { 
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(path, settings);
            }
            finally { }
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch generatedExceptionName As Exception
                Dim settings As New XmlReaderSettings()
                settings.DtdProcessing = DtdProcessing.Parse
                Dim reader As XmlReader = XmlReader.Create(path, settings)
            Finally
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;
using System;
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch (Exception) { throw; }
            finally { 
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(path, settings);
            }
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml
Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch generatedExceptionName As Exception
                Throw
            Finally
                Dim settings As New XmlReaderSettings()
                settings.DtdProcessing = DtdProcessing.Parse
                Dim reader As XmlReader = XmlReader.Create(path, settings)
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInUnusedOneShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse };   
            settings = new XmlReaderSettings();
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse _
            }
            settings = New XmlReaderSettings()
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void XmlReaderSettingsSetDtdProcessingToParseInUsedOneShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings = new XmlReaderSettings(){ DtdProcessing = DtdProcessing.Parse };
            XmlReader reader = XmlReader.Create(path, settings);
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim settings As New XmlReaderSettings()
            settings = New XmlReaderSettings() With { _
                .DtdProcessing = DtdProcessing.Parse _
            }
            Dim reader As XmlReader = XmlReader.Create(path, settings)
        End Sub
    End Class
End Namespace"
            );
        }
    }
}

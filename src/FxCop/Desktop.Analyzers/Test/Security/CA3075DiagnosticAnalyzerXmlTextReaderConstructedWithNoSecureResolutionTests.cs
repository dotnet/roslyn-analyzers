// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class CA3075DiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private static readonly string CA3075XmlTextReaderConstructedWithNoSecureResolutionMessage = DesktopAnalyzersResources.XmlTextReaderConstructedWithNoSecureResolutionDiagnosis;

        private DiagnosticResult GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, CA3075XmlTextReaderConstructedWithNoSecureResolutionMessage);
        }

        private DiagnosticResult GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, CA3075XmlTextReaderConstructedWithNoSecureResolutionMessage);
        }

        [Fact]
        public void ConstructXmlTextReaderShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(10, 27)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim reader As New XmlTextReader(path)
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(7, 17)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {
                XmlTextReader reader = new XmlTextReader(path);
            }
            catch { throw ; }
            finally {}
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(11, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
                Dim reader As New XmlTextReader(path)
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(8, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { 
                XmlTextReader reader = new XmlTextReader(path);
            }
            finally {}
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(12, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch
                Dim reader As New XmlTextReader(path)
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(9, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { throw ; }
            finally {
                XmlTextReader reader = new XmlTextReader(path);
            }
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(13, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch
                Throw
            Finally
                Dim reader As New XmlTextReader(path)
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(11, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetResolverToSecureValueShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
            reader.XmlResolver = null;
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(10, 27)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim reader As New XmlTextReader(path)
            reader.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(7, 17)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetResolverToSecureValueInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {
                XmlTextReader reader = new XmlTextReader(path);
                reader.XmlResolver = null;
            }
            catch { throw ; }
            finally {}
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(11, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
                Dim reader As New XmlTextReader(path)
                reader.XmlResolver = Nothing
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(8, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetResolverToSecureValueInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { 
                XmlTextReader reader = new XmlTextReader(path);
                reader.XmlResolver = null;
            }
            finally {}
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(12, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch
                Dim reader As New XmlTextReader(path)
                reader.XmlResolver = Nothing
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(9, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetResolverToSecureValueInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { throw ; }
            finally {
                XmlTextReader reader = new XmlTextReader(path);
                reader.XmlResolver = null;
            }
        }
    }
}
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(13, 31)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch
                Throw
            Finally
                Dim reader As New XmlTextReader(path)
                reader.XmlResolver = Nothing
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(11, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetDtdProcessingToSecureValueShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
            reader.DtdProcessing = DtdProcessing.Prohibit;
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
            Dim reader As New XmlTextReader(path)
            reader.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetDtdProcessingToSecureValueInTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
            }
            catch { throw ; }
            finally {}
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
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetDtdProcessingToSecureValueInCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { 
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
            }
            finally {}
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
            Catch
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
            Finally
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetDtdProcessingToSecureValueInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { throw ; }
            finally {
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
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
            Catch
                Throw
            Finally
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
            End Try
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetResolverAndDtdProcessingToSecureValuesShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.XmlResolver = null;
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
            Dim reader As New XmlTextReader(path)
            reader.DtdProcessing = DtdProcessing.Prohibit
            reader.XmlResolver = Nothing
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void ConstructXmlTextReaderSetSetResolverAndDtdProcessingToSecureValueInTryBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
                reader.XmlResolver = null;
            }
            catch { throw ; }
            finally {}
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
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
                reader.XmlResolver = Nothing
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void ConstructXmlTextReaderSetSetResolverAndDtdProcessingToSecureValueInCatchBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { 
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
                reader.XmlResolver = null;
            }
            finally {}
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
            Catch
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
                reader.XmlResolver = Nothing
            Finally
            End Try
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void ConstructXmlTextReaderSetSetResolverAndDtdProcessingToSecureValueInFinallyBlockShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            try {   }
            catch { throw ; }
            finally {
                XmlTextReader reader = new XmlTextReader(path);
                reader.DtdProcessing = DtdProcessing.Prohibit;
                reader.XmlResolver = null;
            }
        }
    }
}
");
            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Try
            Catch
                Throw
            Finally
                Dim reader As New XmlTextReader(path)
                reader.DtdProcessing = DtdProcessing.Prohibit
                reader.XmlResolver = Nothing
            End Try
        End Sub
    End Class
End Namespace
"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderSetResolverAndDtdProcessingToSecureValuesInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(string path)
        {
            XmlTextReader doc = new XmlTextReader(path)
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
        }
    }
}");
            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(path As String)
            Dim doc As New XmlTextReader(path) With { _
                .DtdProcessing = DtdProcessing.Prohibit, _
                .XmlResolver = Nothing _
            }
        End Sub
    End Class
End Namespace
");
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetResolverToSecureValueInInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(string path)
        {
            XmlTextReader doc = new XmlTextReader(path)
            {
                XmlResolver = null
            };
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(10, 27)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(path As String)
            Dim doc As New XmlTextReader(path) With { _
                .XmlResolver = Nothing _
            }
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(7, 17)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetDtdProcessingToSecureValueInInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        private static void TestMethod(string path)
        {
            XmlTextReader doc = new XmlTextReader(path)
            {
                DtdProcessing = DtdProcessing.Prohibit
            };
        }
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Private Shared Sub TestMethod(path As String)
            Dim doc As New XmlTextReader(path) With { _
                .DtdProcessing = DtdProcessing.Prohibit _
            }
        End Sub
    End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldSetBothToSecureValuesInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"")
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"") With { _
            .DtdProcessing = DtdProcessing.Prohibit, _
            .XmlResolver = Nothing _
        }
        End Class
End Namespace");
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetResolverToSecureValuesInInitializerShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"")
        {
            XmlResolver = null
        };
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"

Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"") With { _
           .XmlResolver = Nothing _
        }
        End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(7, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetDtdProcessingToSecureValuesInInitializerShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"")
        {
            DtdProcessing = DtdProcessing.Prohibit
        };
    }
}"
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"") With { _
            .DtdProcessing = DtdProcessing.Prohibit _
        }
        End Class
End Namespace"
            );
        }

        [Fact]
        public void ConstructDefaultXmlTextReaderAsFieldSetBothToSecureValuesInMethodShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public TestMethod()
        {
            reader.XmlResolver = null;
            reader.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub New()
            reader.XmlResolver = Nothing
            reader.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace
",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetResolverToSecureValueInMethodShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public TestMethod()
        {
            reader.XmlResolver = null;
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub New()
            reader.XmlResolver = Nothing
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetResolverToSecureValueInMethodInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public void TestMethod()
        {
            try
            {
                reader.XmlResolver = null;
            }
            catch { throw; }
            finally { }
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub TestMethod()
            Try
                reader.XmlResolver = Nothing
            Catch
                Throw
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetResolverToSecureValueInMethodInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public void TestMethod()
        {
            try {  }
            catch { reader.XmlResolver = null; }
            finally { }
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub TestMethod()
            Try
            Catch
                reader.XmlResolver = Nothing
            Finally
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetResolverToSecureValueInMethodInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public void TestMethod()
        {
            try {   }
            catch { throw; }
            finally { reader.XmlResolver = null; }
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub TestMethod()
            Try
            Catch
                Throw
            Finally
                reader.XmlResolver = Nothing
            End Try
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderAsFieldOnlySetDtdProcessingToSecureValueInMethodShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {
        public XmlTextReader reader = new XmlTextReader(""file.xml"");

        public TestMethod()
        {
            reader.DtdProcessing = DtdProcessing.Ignore;
        }
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(8, 30)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass
        Public reader As XmlTextReader = New XmlTextReader(""file.xml"")

        Public Sub New()
            reader.DtdProcessing = DtdProcessing.Ignore
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(6, 16)
            );
        }

        [Fact]
        public void XmlTextReaderDerivedTypeWithNoSecureSettingsShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

namespace TestNamespace
{
    class DerivedType : XmlTextReader {}   

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
        Inherits XmlTextReader
    End Class

    Class TestClass
        Private Sub TestMethod()
            Dim c = New DerivedType()
        End Sub
    End Class

End Namespace");
        }

        [Fact]
        public void XmlTextReaderCreatedAsTempNoSettingsShouldGenerateDiagnostics()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    class TestClass
    {

        public void Method1(string path)
        {
            Method2(new XmlTextReader(path));
        }

        public void Method2(XmlTextReader reader){}
    }
}",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionCSharpResultAt(11, 21)
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Class TestClass

        Public Sub Method1(path As String)
            Method2(New XmlTextReader(path))
        End Sub

        Public Sub Method2(reader As XmlTextReader)
        End Sub
    End Class
End Namespace",
                GetCA3075XmlTextReaderConstructedWithNoSecureResolutionBasicResultAt(8, 21)
            );
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetDtdProcessingProhibitTargetFx46ShouldNotGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Reflection;               
using System.Xml;   

[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.6"", FrameworkDisplayName = "".NET Framework 4.6"")]

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod(string path)
        {
            XmlTextReader reader = new XmlTextReader(path);
            reader.DtdProcessing = DtdProcessing.Prohibit;
        }
    }
}
"
            );

            VerifyBasic(@"
Imports System.Reflection
Imports System.Xml

<Assembly: System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework, Version = v4.6"", FrameworkDisplayName := "".NET Framework 4.6"")>

Namespace TestNamespace
    Public Class TestClass
        Public Sub TestMethod(path As String)
            Dim reader As New XmlTextReader(path)
            reader.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void ConstructXmlTextReaderOnlySetDtdProcessingProhibitTargetFx452ShouldNotGenerateDiagnostic()
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
            XmlTextReader reader = new XmlTextReader(path);
            reader.DtdProcessing = DtdProcessing.Prohibit;
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
            Dim reader As New XmlTextReader(path)
            reader.DtdProcessing = DtdProcessing.Prohibit
        End Sub
    End Class
End Namespace"
            );
        }
    }
}

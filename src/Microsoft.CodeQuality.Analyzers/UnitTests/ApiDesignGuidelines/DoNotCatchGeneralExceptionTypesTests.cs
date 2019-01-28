// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using Test.Utilities;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class DoNotCatchGeneralExceptionTypesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCatchGeneralExceptionTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCatchGeneralExceptionTypesAnalyzer();
        }

        [Fact]
        public void CSharp_Diagnostic_GeneralCatch()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (IOException e)
                        {
                        }
                        catch
                        {
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(18, 25));
        }

        [Fact]
        public void Basic_Diagnostic_GeneralCatch()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As IOException
                        Catch
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(10, 25));
        }

        [Fact]
        public void CSharp_Diagnostic_GeneralCatchInGetAccessor()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public int TestProperty
                    {
                        get
                        {
                            try
                            {
                                FileStream fileStream = new FileStream(""name"", FileMode.Create);
                            }
                            catch (IOException e)
                            {
                            }
                            catch
                            {
                            }
                            return 0;
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(20, 29));
        }

        [Fact]
        public void Basic_Diagnostic_GeneralCatchInGetAccessor()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public ReadOnly Property X() As Integer
                        Get
                            Try
                                Dim fileStream As New FileStream(""name"", FileMode.Create)
                            Catch e As IOException
                            Catch
                            End Try
                            Return 0
                        End Get
                    End Property
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(11, 29));
        }

        [Fact]
        public void CSharp_NoDiagnostic_GeneralCatchRethrow()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (IOException e)
                        {
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }");
        }

        [Fact]
        public void Basic_NoDiagnostic_GeneralCatchRethrow()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As IOException
                        Catch
                            Throw
                        End Try
                    End Sub
                End Class
            End Namespace
            ");
        }

        [Fact]
        public void CSharp_Diagnostic_GeneralCatchThrowNew()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (IOException e)
                        {
                        }
                        catch
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(18, 25));
        }

        [Fact]
        public void Basic_Diagnostic_GeneralCatchThrowNew()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As IOException
                        Catch
                            Throw New System.NotImplementedException()
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(10, 25));
        }

        [Fact]
        public void CSharp_Diagnostic_GeneralCatchWithRethrowFromSpecificCatch()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (IOException e)
                        {
                            throw;
                        }
                        catch
                        {
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(19, 25));
        }

        [Fact]
        public void Basic_Diagnostic_GeneralCatchWithRethrowFromSpecificCatch()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As IOException
                            Throw
                        Catch
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(11, 25));
        }

        [Fact]
        public void CSharp_Diagnostic_GenericException()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(15, 25));
        }

        [Fact]
        public void Basic_Diagnostic_GenericException()
        {
            VerifyBasic(@"
            Imports System
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As Exception
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(10, 25));
        }

        [Fact]
        public void CSharp_Diagnostic_SystemException()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (SystemException e)
                        {
                        }
                    }
                }
            }",
            GetCA1031CSharpResultAt(15, 25));
        }

        [Fact]
        public void Basic_Diagnostic_SystemException()
        {
            VerifyBasic(@"
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As System.Exception
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(9, 25));
        }

        [Fact]
        public void CSharp_Diagnostic_GeneralCatchInLambdaExpression()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;

            namespace TestNamespace
            {
                class TestClass
                {
                    public static void TestMethod()
                    {
                        Action action = () =>
                        {
                            try
                            {
                                FileStream fileStream = new FileStream(""name"", FileMode.Create);
                            }
                            catch
                            {
                            }
                        };
                    }
                }
            }",
            GetCA1031CSharpResultAt(17, 29));
        }

        [Fact]
        public void Basic_Diagnostic_GeneralCatchInLambdaExpression()
        {
            VerifyBasic(@"
            Imports System
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Sub TestMethod()
                        Dim action As Action = Function() 
                            Try
                                Dim fileStream As New FileStream(""name"", FileMode.Create)
                            Catch
                            End Try
                        End Function
                    End Sub
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(11, 29));

            VerifyBasic(@"
            Imports System
            Imports System.IO

            Namespace TestNamespace
                Class TestClass
                    Public Shared Function TestMethod() As Double
                        Dim action As Action = Function() 
                            Try
                                Dim fileStream As New FileStream(""name"", FileMode.Create)
                            Catch
                            End Try
                            Return 0
                        End Function
                    End Function
                End Class
            End Namespace
            ",
            GetCA1031BasicResultAt(11, 29));
        }

        private static DiagnosticResult GetCA1031CSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotCatchGeneralExceptionTypesAnalyzer.Rule, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotCatchGeneralExceptionTypesMessage);
        }

        private static DiagnosticResult GetCA1031BasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotCatchGeneralExceptionTypesAnalyzer.Rule, MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotCatchGeneralExceptionTypesMessage);
        }
    }
}
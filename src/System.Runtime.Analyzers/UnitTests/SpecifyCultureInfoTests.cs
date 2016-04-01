// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class SpecifyCultureInfoTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SpecifyCultureInfoAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SpecifyCultureInfoAnalyzer();
        }

        [Fact]
        public void CA1304_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

    public class CultureInfoTestClass0
    {
        public string SpecifyCultureInfo01()
        {
            return ""foo"".ToLower();
        }
    }

    public class CultureInfoTestClass1
    {
        public string LowercaseAString(string name)
        {
            return name.ToLower();
        }

        public string InsideALambda(string insideLambda)
        {
            Func<string> ddd = () =>
            {
                return insideLambda.ToLower();
            };

            return null;
        }

        public string PropertyWithALambda
        {
            get
            {
                Func<string> ddd = () =>
                {
                    return ""InsideGetter"".ToLower();
                };

                return null;
            }
        }
    }

    public class CultureInfoTestClass2
    {
        public static void Method()
        {
            MethodOverloadHasInheritedCultureInfo(""Foo""); // No Diag - Inherited CultureInfo
            MethodOverloadHasCultureInfoAsFirstArgument(""Foo"");
            MethodOverloadHasCultureInfoAsLastArgument(""Foo"");
            MethodOverloadHasMoreThanCultureInfo(""Foo""); // No Diag
            MethodOverloadHasJustCultureInfo();
            MethodOverloadCount3();
            MethodOverloadWithJustCulltureInfoAsExtraParameter("""", """"); // No Diag
            MethodOverloadWithJustCulltureInfoAsExtraParameter(2, 3);
        }

        public static void MethodOverloadHasInheritedCultureInfo(string format)
        {
            MethodOverloadHasInheritedCultureInfo(new DerivedCultureInfo(""""), format);
        }

        public static void MethodOverloadHasInheritedCultureInfo(DerivedCultureInfo provider, string format)
        {
            Console.WriteLine(string.Format(provider, format));
        }

        public static void MethodOverloadHasCultureInfoAsFirstArgument(string format)
        {
            MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo.CurrentCulture, format);
        }

        public static void MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo provider, string format)
        {
            Console.WriteLine(string.Format(provider, format));
        }
        public static void MethodOverloadHasCultureInfoAsLastArgument(string format)
        {
            MethodOverloadHasCultureInfoAsLastArgument(format, CultureInfo.CurrentCulture);
        }

        public static void MethodOverloadHasCultureInfoAsLastArgument(string format, CultureInfo provider)
        {
            Console.WriteLine(string.Format(provider, format));
        }

        public static void MethodOverloadHasCultureInfoAsLastArgument(CultureInfo provider, string format)
        {
            Console.WriteLine(string.Format(provider, format));
        }

        public static void MethodOverloadHasMoreThanCultureInfo(string format)
        {
            MethodOverloadHasMoreThanCultureInfo(format, null, CultureInfo.CurrentCulture);
        }

        public static void MethodOverloadHasMoreThanCultureInfo(string format, string what, CultureInfo provider)
        {
            Console.WriteLine(string.Format(provider, format));
        }

        public static void MethodOverloadHasJustCultureInfo()
        {
            MethodOverloadHasJustCultureInfo(CultureInfo.CurrentCulture);
        }

        public static void MethodOverloadHasJustCultureInfo(CultureInfo provider)
        {
            Console.WriteLine(string.Format(provider, """"));
        }

        public static void MethodOverloadWithJustCulltureInfoAsExtraParameter(string a, string b)
        {
            MethodOverloadWithJustCulltureInfoAsExtraParameter(a, CultureInfo.CurrentCulture, b);
        }

        public static void MethodOverloadWithJustCulltureInfoAsExtraParameter(string a, CultureInfo provider, string b)
        {
            Console.WriteLine(string.Format(provider, """"));
        }

        public static void MethodOverloadWithJustCulltureInfoAsExtraParameter(int a, int b)
        {
            MethodOverloadWithJustCulltureInfoAsExtraParameter(a, b, CultureInfo.CurrentCulture);
        }

        public static void MethodOverloadWithJustCulltureInfoAsExtraParameter(int a, int b, CultureInfo provider)
        {
            Console.WriteLine(string.Format(provider, """"));
        }

        public static void MethodOverloadCount3()
        {
            MethodOverloadCount3(CultureInfo.CurrentCulture);
        }

        public static void MethodOverloadCount3(CultureInfo provider)
        {
            Console.WriteLine(string.Format(provider, """"));
        }
        public static void MethodOverloadCount3(string b)
        {
        }
    }

    public class DerivedCultureInfo : CultureInfo
    {
        public DerivedCultureInfo(string name) :
            base(name)
        {

        }
    }",
            GetCSharpResultAt(9, 20, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass0.SpecifyCultureInfo01()", "string.ToLower(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(17, 20, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.LowercaseAString(string)", "string.ToLower(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(24, 24, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.InsideALambda(string)", "string.ToLower(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(36, 28, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.PropertyWithALambda.get", "string.ToLower(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(49, 13, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(string)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(System.Globalization.CultureInfo, string)"),
            GetCSharpResultAt(50, 13, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(string)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(string, System.Globalization.CultureInfo)"),
            GetCSharpResultAt(52, 13, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(53, 13, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadCount3()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadCount3(System.Globalization.CultureInfo)"),
            GetCSharpResultAt(55, 13, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadWithJustCulltureInfoAsExtraParameter(int, int)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadWithJustCulltureInfoAsExtraParameter(int, int, System.Globalization.CultureInfo)"));
        }

        [Fact]
        public void CA1304_VisualBasic()
        {
            this.PrintActualDiagnosticsOnFailure = true;
            VerifyBasic(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass0
    Public Function SpecifyCultureInfo01() As String
        Return ""foo"".ToLower()
    End Function
End Class

Public Class CultureInfoTestClass1
    Public Function LowercaseAString(name As String) As String
        Return name.ToLower()
    End Function

    Public Function InsideALambda(insideLambda As String) As String
        Dim ddd As Func(Of String) = Function()
                                        Return insideLambda.ToLower()
                                     End Function

        Return Nothing
    End Function

    Public ReadOnly Property PropertyWithALambda() As String
        Get
            Dim ddd As Func(Of String) = Function()
                                            Return ""InsideGetter"".ToLower()
                                         End Function
            Return Nothing
        End Get
    End Property
End Class

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        ' No Diag - Inherited CultureInfo
        MethodOverloadHasInheritedCultureInfo(""Foo"")
        MethodOverloadHasCultureInfoAsFirstArgument(""Foo"")
        MethodOverloadHasCultureInfoAsLastArgument(""Foo"")
        ' No Diag
        MethodOverloadHasMoreThanCultureInfo(""Foo"")
        MethodOverloadHasJustCultureInfo()
        MethodOverloadCount3()
        ' No Diag
        MethodOverloadWithJustCulltureInfoAsExtraParameter("""", """")
        MethodOverloadWithJustCulltureInfoAsExtraParameter(2, 3)
    End Sub

    Public Shared Sub MethodOverloadHasInheritedCultureInfo(format As String)
        MethodOverloadHasInheritedCultureInfo(New DerivedCultureInfo(""""), format)
    End Sub

    Public Shared Sub MethodOverloadHasInheritedCultureInfo(provider As DerivedCultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsFirstArgument(format As String)
        MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo.CurrentCulture, format)
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsFirstArgument(provider As CultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(format As String)
        MethodOverloadHasCultureInfoAsLastArgument(format, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(format As String, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(provider As CultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasMoreThanCultureInfo(format As String)
        MethodOverloadHasMoreThanCultureInfo(format, Nothing, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasMoreThanCultureInfo(format As String, what As String, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasJustCultureInfo()
        MethodOverloadHasJustCultureInfo(CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasJustCultureInfo(provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub

    Public Shared Sub MethodOverloadWithJustCulltureInfoAsExtraParameter(a As String, b As String)
        MethodOverloadWithJustCulltureInfoAsExtraParameter(a, CultureInfo.CurrentCulture, b)
    End Sub

    Public Shared Sub MethodOverloadWithJustCulltureInfoAsExtraParameter(a As String, provider As CultureInfo, b As String)
        Console.WriteLine(String.Format(provider, """"))
    End Sub

    Public Shared Sub MethodOverloadWithJustCulltureInfoAsExtraParameter(a As Integer, b As Integer)
        MethodOverloadWithJustCulltureInfoAsExtraParameter(a, b, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadWithJustCulltureInfoAsExtraParameter(a As Integer, b As Integer, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub

    Public Shared Sub MethodOverloadCount3()
        MethodOverloadCount3(CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadCount3(provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub

    Public Shared Sub MethodOverloadCount3(b As String)
    End Sub
End Class

Public Class DerivedCultureInfo
    Inherits CultureInfo
    Public Sub New(name As String)
        MyBase.New(name)
    End Sub
End Class",
            GetBasicResultAt(7, 16, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass0.SpecifyCultureInfo01()", "String.ToLower(System.Globalization.CultureInfo)"),
            GetBasicResultAt(13, 16, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.LowercaseAString(String)", "String.ToLower(System.Globalization.CultureInfo)"),
            GetBasicResultAt(18, 48, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.InsideALambda(String)", "String.ToLower(System.Globalization.CultureInfo)"),
            GetBasicResultAt(27, 52, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.PropertyWithALambda()", "String.ToLower(System.Globalization.CultureInfo)"),
            GetBasicResultAt(38, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(String)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(System.Globalization.CultureInfo, String)"),
            GetBasicResultAt(39, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(String)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(String, System.Globalization.CultureInfo)"),
            GetBasicResultAt(42, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo(System.Globalization.CultureInfo)"),
            GetBasicResultAt(43, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadCount3()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadCount3(System.Globalization.CultureInfo)"),
            GetBasicResultAt(46, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadWithJustCulltureInfoAsExtraParameter(Integer, Integer)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadWithJustCulltureInfoAsExtraParameter(Integer, Integer, System.Globalization.CultureInfo)"));
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class SpecifyIFormatProviderTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SpecifyIFormatProviderAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SpecifyIFormatProviderAnalyzer();
        }

        [Fact]
        public void CA1305_StringReturningStringFormatOverloads_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class IFormatProviderStringTest
{
    public static string SpecifyIFormatProvider1()
    {
        return string.Format(""Foo {0}"", ""bar"");
    }

    public static string SpecifyIFormatProvider2()
    {
        return string.Format(""Foo {0} {1}"", ""bar"", ""foo"");
    }

    public static string SpecifyIFormatProvider3()
    {
        return string.Format(""Foo {0} {1} {2}"", ""bar"", ""foo"", ""bar"");
    }

    public static string SpecifyIFormatProvider4()
    {
        return string.Format(""Foo {0} {1} {2} {3}"", ""bar"", ""foo"", ""bar"", """");
    }
}",
GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 16, "string.Format(string, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider1()",
                                                            "string.Format(IFormatProvider, string, params object[])"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(15, 16, "string.Format(string, object, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider2()",
                                                            "string.Format(IFormatProvider, string, params object[])"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(20, 16, "string.Format(string, object, object, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider3()",
                                                            "string.Format(IFormatProvider, string, params object[])"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(25, 16, "string.Format(string, params object[])",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider4()",
                                                            "string.Format(IFormatProvider, string, params object[])"));
        }

        [Fact]
        public void CA1305_StringReturningUserMethodOverloads_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class IFormatProviderStringTest
{
    public static void SpecifyIFormatProvider()
    {
        IFormatProviderOverloads.LeadingIFormatProviderReturningString(""Bar"");
        IFormatProviderOverloads.TrailingIFormatProviderReturningString(""Bar"");
        IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(""Bar"");
    }
}

internal static class IFormatProviderOverloads
{
    public static string LeadingIFormatProviderReturningString(string format)
    {
        return LeadingIFormatProviderReturningString(CultureInfo.CurrentCulture, format);
    }

    public static string LeadingIFormatProviderReturningString(IFormatProvider provider, string format)
    {
        return string.Format(provider, format);
    }

    public static string TrailingIFormatProviderReturningString(string format)
    {
        return TrailingIFormatProviderReturningString(format, CultureInfo.CurrentCulture);
    }

    public static string TrailingIFormatProviderReturningString(string format, IFormatProvider provider)
    {
        return string.Format(provider, format);
    }

    public static string TrailingIFormatProviderReturningString(IFormatProvider provider, string format)
    {
        return string.Format(provider, format);
    }

    public static string UserDefinedParamsMatchMethodOverload(string format, params object[] objects)
    {
        return null;
    }

    public static string UserDefinedParamsMatchMethodOverload(IFormatProvider provider, string format, params object[] objs)
    {
        return null;
    }
}",
GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.LeadingIFormatProviderReturningString(string)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                           "IFormatProviderOverloads.LeadingIFormatProviderReturningString(IFormatProvider, string)"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.TrailingIFormatProviderReturningString(string)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                           "IFormatProviderOverloads.TrailingIFormatProviderReturningString(string, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(string, params object[])",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                           "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(IFormatProvider, string, params object[])"));
        }

        [Fact]
        public void CA1305_StringReturningNoDiagnostics_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class IFormatProviderStringTest
{
    public static void SpecifyIFormatProvider6()
    {
        IFormatProviderOverloads.IFormatProviderAsDerivedTypeOverload(""Bar"");
    }

    public static void SpecifyIFormatProvider7()
    {
        IFormatProviderOverloads.UserDefinedParamsMismatchMethodOverload(""Bar"");
    }
}

internal static class IFormatProviderOverloads
{
    public static string IFormatProviderAsDerivedTypeOverload(string format)
    {
        return null;
    }

    public static string IFormatProviderAsDerivedTypeOverload(DerivedClass provider, string format)
    {
        return null;
    }

    public static string UserDefinedParamsMismatchMethodOverload(string format)
    {
        return null;
    }

    public static string UserDefinedParamsMismatchMethodOverload(IFormatProvider provider, string format, params object[] objs)
    {
        return null;
    }
}

public class DerivedClass : IFormatProvider
{
    public object GetFormat(Type formatType)
    {
        throw new NotImplementedException();
    }
}");
        }

        [Fact]
        public void CA1305_NonStringReturningStringFormatOverloads_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;

public static class IFormatProviderStringTest
{
    public static void TestMethod()
    {
        int x = Convert.ToInt32(""1"");
        long y = Convert.ToInt64(""1"");
        IFormatProviderOverloads.LeadingIFormatProvider(""1"");
        IFormatProviderOverloads.TrailingIFormatProvider(""1"");
    }
}

internal static class IFormatProviderOverloads
{
    public static void LeadingIFormatProvider(string format)
    {
        LeadingIFormatProvider(CultureInfo.CurrentCulture, format);
    }

    public static void LeadingIFormatProvider(IFormatProvider provider, string format)
    {
        Console.WriteLine(string.Format(provider, format));
    }

    public static void TrailingIFormatProvider(string format)
    {
        TrailingIFormatProvider(format, CultureInfo.CurrentCulture);
    }

    public static void TrailingIFormatProvider(string format, IFormatProvider provider)
    {
        Console.WriteLine(string.Format(provider, format));
    }
}",
GetIFormatProviderAlternateRuleCSharpResultAt(9, 17, "Convert.ToInt32(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "Convert.ToInt32(string, IFormatProvider)"),
GetIFormatProviderAlternateRuleCSharpResultAt(10, 18, "Convert.ToInt64(string)",
                                                      "IFormatProviderStringTest.TestMethod()",
                                                      "Convert.ToInt64(string, IFormatProvider)"),
GetIFormatProviderAlternateRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.LeadingIFormatProvider(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.LeadingIFormatProvider(IFormatProvider, string)"),
GetIFormatProviderAlternateRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.TrailingIFormatProvider(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.TrailingIFormatProvider(string, IFormatProvider)"));
        }

        [Fact]
        public void CA1305_NonStringReturningStringFormatOverloads_TargetMethodNoGenerics_CSharp()
        {
            VerifyCSharp(@"
using System;

public static class IFormatProviderStringTest
{
    public static void TestMethod()
    {
        IFormatProviderOverloads.TargetMethodIsNonGeneric(""1"");
        IFormatProviderOverloads.TargetMethodIsGeneric<int>(""1""); // No Diagnostics because the target method can be generic
    }
}

internal static class IFormatProviderOverloads
{
    public static void TargetMethodIsNonGeneric(string format)
    {
    }

    public static void TargetMethodIsNonGeneric<T>(string format, IFormatProvider provider)
    {
    }

    public static void TargetMethodIsGeneric<T>(string format)
    {
    }

    public static void TargetMethodIsGeneric(string format, IFormatProvider provider)
    {
    }
}",
GetIFormatProviderAlternateRuleCSharpResultAt(8, 9, "IFormatProviderOverloads.TargetMethodIsNonGeneric(string)",
                                                    "IFormatProviderStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.TargetMethodIsNonGeneric<T>(string, IFormatProvider)"));
        }

        [Fact]
        public void CA1305_StringReturningUICultureIFormatProvider_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class UICultureAsIFormatProviderReturningStringTest
{
    public static void TestMethod()
    {
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", CultureInfo.CurrentUICulture);
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", CultureInfo.InstalledUICulture);
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", Thread.CurrentThread.CurrentUICulture);
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", Thread.CurrentThread.CurrentUICulture, CultureInfo.InstalledUICulture);
    }
}

internal static class IFormatProviderOverloads
{
    public static string IFormatProviderReturningString(string format, IFormatProvider provider)
    {
        return null;
    }

    public static string IFormatProviderReturningString(string format, IFormatProvider provider, IFormatProvider provider2)
    {
        return null;
    }
}",
GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(10, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "CultureInfo.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(11, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "CultureInfo.InstalledUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(12, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "Thread.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "Thread.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "CultureInfo.InstalledUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, IFormatProvider, IFormatProvider)"));
        }

        [Fact]
        public void CA1305_NonStringReturningUICultureIFormatProvider_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class UICultureAsIFormatProviderReturningNonStringTest
{
    public static void TestMethod()
    {
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", CultureInfo.CurrentUICulture);
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", CultureInfo.InstalledUICulture);
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", Thread.CurrentThread.CurrentUICulture);
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", Thread.CurrentThread.CurrentUICulture, CultureInfo.InstalledUICulture);
    }
}

internal static class IFormatProviderOverloads
{
    public static void IFormatProviderReturningNonString(string format, IFormatProvider provider)
    {
    }

    public static void IFormatProviderReturningNonString(string format, IFormatProvider provider, IFormatProvider provider2)
    {
    }
}",
GetIFormatProviderAlternateRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleCSharpResultAt(10, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "CultureInfo.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)"),
GetIFormatProviderAlternateRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleCSharpResultAt(11, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "CultureInfo.InstalledUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)"),
GetIFormatProviderAlternateRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleCSharpResultAt(12, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "Thread.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider)"),
GetIFormatProviderUICultureRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "Thread.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "CultureInfo.InstalledUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, IFormatProvider, IFormatProvider)"));
        }


        [Fact]
        public void CA1305_AcceptNullForIFormatProvider_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class UICultureAsIFormatProviderReturningStringTest
{
    public static void TestMethod()
    {
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", null);
    }
}

internal static class IFormatProviderOverloads
{
    public static string IFormatProviderReturningString(string format, IFormatProvider provider)
    {
        return null;
    }
}");
        }

        [Fact]
        public void CA1305_DoesNotRecommendObsoleteOverload_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class TestClass
{
    public static void TestMethod()
    {
        IFormatProviderOverloads.TrailingObsoleteIFormatProvider(""1"");
    }
}

internal static class IFormatProviderOverloads
{
    public static string TrailingObsoleteIFormatProvider(string format)
    {
        return null;
    }

    [Obsolete]
    public static string TrailingObsoleteIFormatProvider(string format, IFormatProvider provider)
    {
        return null;
    }
}");
        }

        [Fact]
        public void CA1305_RuleException_NoDiagnostics_CSharp()
        {
            VerifyCSharp(@"
using System;
using System.Globalization;
using System.Threading;

public static class IFormatProviderStringTest
{
    public static void TrailingThreadCurrentUICulture()
    {
        var s = new System.Resources.ResourceManager(null);
        Console.WriteLine(s.GetObject("""", Thread.CurrentThread.CurrentUICulture));
        Console.WriteLine(s.GetStream("""", Thread.CurrentThread.CurrentUICulture));
        Console.WriteLine(s.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, false));

        var activator = Activator.CreateInstance(null, System.Reflection.BindingFlags.CreateInstance, null, null, Thread.CurrentThread.CurrentUICulture);
        Console.WriteLine(activator);
    }
}");
        }

        [Fact]
        public void CA1305_StringReturningStringFormatOverloads_VisualBasic()
        {
            VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class IFormatProviderStringTest
    Private Sub New()
    End Sub

    Public Shared Function SpecifyIFormatProvider1() As String
        Return String.Format(""Foo {0}"", ""bar"")
    End Function

    Public Shared Function SpecifyIFormatProvider2() As String
        Return String.Format(""Foo {0} {1}"", ""bar"", ""foo"")
    End Function

    Public Shared Function SpecifyIFormatProvider3() As String
        Return String.Format(""Foo {0} {1} {2}"", ""bar"", ""foo"", ""bar"")
    End Function

    Public Shared Function SpecifyIFormatProvider4() As String
        Return String.Format(""Foo {0} {1} {2} {3}"", ""bar"", ""foo"", ""bar"", """")
    End Function
End Class",
GetIFormatProviderAlternateStringRuleBasicResultAt(11, 16, "String.Format(String, Object)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider1()",
                                                           "String.Format(IFormatProvider, String, ParamArray Object())"),
GetIFormatProviderAlternateStringRuleBasicResultAt(15, 16, "String.Format(String, Object, Object)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider2()",
                                                           "String.Format(IFormatProvider, String, ParamArray Object())"),
GetIFormatProviderAlternateStringRuleBasicResultAt(19, 16, "String.Format(String, Object, Object, Object)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider3()",
                                                           "String.Format(IFormatProvider, String, ParamArray Object())"),
GetIFormatProviderAlternateStringRuleBasicResultAt(23, 16, "String.Format(String, ParamArray Object())",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider4()",
                                                           "String.Format(IFormatProvider, String, ParamArray Object())"));
        }

       [Fact]
       public void CA1305_StringReturningUserMethodOverloads_VisualBasic()
       {
           VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class IFormatProviderStringTest
    Private Sub New()
    End Sub
    Public Shared Sub SpecifyIFormatProvider()
        IFormatProviderOverloads.LeadingIFormatProviderReturningString(""Bar"")
        IFormatProviderOverloads.TrailingIFormatProviderReturningString(""Bar"")
        IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(""Bar"")
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub
    Public Shared Function LeadingIFormatProviderReturningString(format As String) As String
        Return LeadingIFormatProviderReturningString(CultureInfo.CurrentCulture, format)
    End Function

    Public Shared Function LeadingIFormatProviderReturningString(provider As IFormatProvider, format As String) As String
        Return String.Format(provider, format)
    End Function

    Public Shared Function TrailingIFormatProviderReturningString(format As String) As String
        Return TrailingIFormatProviderReturningString(format, CultureInfo.CurrentCulture)
    End Function

    Public Shared Function TrailingIFormatProviderReturningString(format As String, provider As IFormatProvider) As String
        Return String.Format(provider, format)
    End Function

    Public Shared Function TrailingIFormatProviderReturningString(provider As IFormatProvider, format As String) As String
        Return String.Format(provider, format)
    End Function

    Public Shared Function UserDefinedParamsMatchMethodOverload(format As String, ParamArray objects As Object()) As String
        Return Nothing
    End Function

    Public Shared Function UserDefinedParamsMatchMethodOverload(provider As IFormatProvider, format As String, ParamArray objs As Object()) As String
        Return Nothing
    End Function
End Class",
GetIFormatProviderAlternateStringRuleBasicResultAt(10, 9, "IFormatProviderOverloads.LeadingIFormatProviderReturningString(String)",
                                                          "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                          "IFormatProviderOverloads.LeadingIFormatProviderReturningString(IFormatProvider, String)"),
GetIFormatProviderAlternateStringRuleBasicResultAt(11, 9, "IFormatProviderOverloads.TrailingIFormatProviderReturningString(String)",
                                                          "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                          "IFormatProviderOverloads.TrailingIFormatProviderReturningString(String, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleBasicResultAt(12, 9, "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(String, ParamArray Object())",
                                                          "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                          "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(IFormatProvider, String, ParamArray Object())"));
       }

       [Fact]
       public void CA1305_StringReturningNoDiagnostics_VisualBasic()
       {
            VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class IFormatProviderStringTest
    Private Sub New()
    End Sub
    Public Shared Sub SpecifyIFormatProvider6()
        IFormatProviderOverloads.IFormatProviderAsDerivedTypeOverload(""Bar"")
    End Sub

    Public Shared Sub SpecifyIFormatProvider7()
        IFormatProviderOverloads.UserDefinedParamsMismatchMethodOverload(""Bar"")
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub

    Public Shared Function IFormatProviderAsDerivedTypeOverload(format As String) As String
        Return Nothing
    End Function

    Public Shared Function IFormatProviderAsDerivedTypeOverload(provider As DerivedClass, format As String) As String
        Return Nothing
    End Function

    Public Shared Function UserDefinedParamsMismatchMethodOverload(format As String) As String
        Return Nothing
    End Function

    Public Shared Function UserDefinedParamsMismatchMethodOverload(provider As IFormatProvider, format As String, ParamArray objs As Object()) As String
        Return Nothing
    End Function
End Class

Public Class DerivedClass
    Implements IFormatProvider

    Public Function GetFormat(formatType As Type) As Object Implements IFormatProvider.GetFormat
        Throw New NotImplementedException()
    End Function
End Class");
       }

       [Fact]
       public void CA1305_NonStringReturningStringFormatOverloads_VisualBasic()
       {
           VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class IFormatProviderStringTest
    Private Sub New()
    End Sub
    Public Shared Sub TestMethod()
        Dim x As Integer = Convert.ToInt32(""1"")
        Dim y As Long = Convert.ToInt64(""1"")
        IFormatProviderOverloads.LeadingIFormatProvider(""1"")
        IFormatProviderOverloads.TrailingIFormatProvider(""1"")
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub
    Public Shared Sub LeadingIFormatProvider(format As String)
        LeadingIFormatProvider(CultureInfo.CurrentCulture, format)
    End Sub

    Public Shared Sub LeadingIFormatProvider(provider As IFormatProvider, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub TrailingIFormatProvider(format As String)
        TrailingIFormatProvider(format, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub TrailingIFormatProvider(format As String, provider As IFormatProvider)
        Console.WriteLine(String.Format(provider, format))
    End Sub
End Class",
GetIFormatProviderAlternateRuleBasicResultAt(10, 28, "Convert.ToInt32(String)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "Convert.ToInt32(String, IFormatProvider)"),
GetIFormatProviderAlternateRuleBasicResultAt(11, 25, "Convert.ToInt64(String)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "Convert.ToInt64(String, IFormatProvider)"),
GetIFormatProviderAlternateRuleBasicResultAt(12, 9, "IFormatProviderOverloads.LeadingIFormatProvider(String)",
                                                    "IFormatProviderStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.LeadingIFormatProvider(IFormatProvider, String)"),
GetIFormatProviderAlternateRuleBasicResultAt(13, 9, "IFormatProviderOverloads.TrailingIFormatProvider(String)",
                                                    "IFormatProviderStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.TrailingIFormatProvider(String, IFormatProvider)"));
       }

        [Fact]
       public void CA1305_StringReturningUICultureIFormatProvider_VisualBasic()
       {
           VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class UICultureAsIFormatProviderReturningStringTest
    Private Sub New()
    End Sub
    Public Shared Sub TestMethod()
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", CultureInfo.CurrentUICulture)
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", CultureInfo.InstalledUICulture)
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", Thread.CurrentThread.CurrentUICulture)
        IFormatProviderOverloads.IFormatProviderReturningString(""1"", Thread.CurrentThread.CurrentUICulture, CultureInfo.InstalledUICulture)
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub
    Public Shared Function IFormatProviderReturningString(format As String, provider As IFormatProvider) As String
        Return Nothing
    End Function

    Public Shared Function IFormatProviderReturningString(format As String, provider As IFormatProvider, provider2 As IFormatProvider) As String
        Return Nothing
    End Function
End Class",
GetIFormatProviderAlternateStringRuleBasicResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)",
                                                          "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleBasicResultAt(10, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "CultureInfo.CurrentUICulture",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleBasicResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)",
                                                          "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleBasicResultAt(11, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "CultureInfo.InstalledUICulture",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)"),
GetIFormatProviderAlternateStringRuleBasicResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)",
                                                          "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleBasicResultAt(12, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "Thread.CurrentUICulture",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleBasicResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "Thread.CurrentUICulture",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureStringRuleBasicResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                          "CultureInfo.InstalledUICulture",
                                                          "IFormatProviderOverloads.IFormatProviderReturningString(String, IFormatProvider, IFormatProvider)"));
       }

       [Fact]
       public void CA1305_NonStringReturningUICultureIFormatProvider_VisualBasic()
       {
           VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class UICultureAsIFormatProviderReturningNonStringTest
    Private Sub New()
    End Sub
    Public Shared Sub TestMethod()
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", CultureInfo.CurrentUICulture)
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", CultureInfo.InstalledUICulture)
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", Thread.CurrentThread.CurrentUICulture)
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", Thread.CurrentThread.CurrentUICulture, CultureInfo.InstalledUICulture)
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub
    Public Shared Sub IFormatProviderReturningNonString(format As String, provider As IFormatProvider)
    End Sub

    Public Shared Sub IFormatProviderReturningNonString(format As String, provider As IFormatProvider, provider2 As IFormatProvider)
    End Sub
End Class",
GetIFormatProviderAlternateRuleBasicResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)",
                                                    "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleBasicResultAt(10, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "CultureInfo.CurrentUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)"),
GetIFormatProviderAlternateRuleBasicResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)",
                                                    "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleBasicResultAt(11, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "CultureInfo.InstalledUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)"),
GetIFormatProviderAlternateRuleBasicResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)",
                                                    "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleBasicResultAt(12, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "Thread.CurrentUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)"),
GetIFormatProviderUICultureRuleBasicResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "Thread.CurrentUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider, IFormatProvider)"),
GetIFormatProviderUICultureRuleBasicResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "CultureInfo.InstalledUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider, IFormatProvider)"));
       }

        [Fact]
        public void CA1305_NonStringReturningComputerInfoInstalledUICultureIFormatProvider_VisualBasic()
        {
            VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading
Imports Microsoft.VisualBasic.Devices

Public NotInheritable Class UICultureAsIFormatProviderReturningNonStringTest
    Private Sub New()
    End Sub
    Public Shared Sub TestMethod()
        Dim computerInfo As New Microsoft.VisualBasic.Devices.ComputerInfo()
        IFormatProviderOverloads.IFormatProviderReturningNonString(""1"", computerInfo.InstalledUICulture)
    End Sub
End Class

Friend NotInheritable Class IFormatProviderOverloads
    Private Sub New()
    End Sub
    Public Shared Sub IFormatProviderReturningNonString(format As String, provider As IFormatProvider)
    End Sub
End Class",
GetIFormatProviderUICultureRuleBasicResultAt(12, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                    "ComputerInfo.InstalledUICulture",
                                                    "IFormatProviderOverloads.IFormatProviderReturningNonString(String, IFormatProvider)"));
        }

        [Fact]
       public void CA1305_RuleException_NoDiagnostics_VisualBasic()
       {
           VerifyBasic(@"
Imports System
Imports System.Globalization
Imports System.Threading

Public NotInheritable Class IFormatProviderStringTest
    Private Sub New()
    End Sub
    Public Shared Sub TrailingThreadCurrentUICulture()
        Dim s = New System.Resources.ResourceManager(Nothing)
        Console.WriteLine(s.GetObject("""", Thread.CurrentThread.CurrentUICulture))
        Console.WriteLine(s.GetStream("""", Thread.CurrentThread.CurrentUICulture))
        Console.WriteLine(s.GetResourceSet(Thread.CurrentThread.CurrentUICulture, False, False))

        Dim activator__1 = Activator.CreateInstance(Nothing, System.Reflection.BindingFlags.CreateInstance, Nothing, Nothing, Thread.CurrentThread.CurrentUICulture)
        Console.WriteLine(activator__1)
    End Sub
End Class");
       }

        private DiagnosticResult GetIFormatProviderAlternateStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateStringRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureStringRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateStringRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetBasicResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateStringRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetBasicResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureStringRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetBasicResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureStringRule, arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return GetBasicResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureRule, arg1, arg2, arg3);
        }
    }
}
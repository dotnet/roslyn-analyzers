// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SpecifyIFormatProviderAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpSpecifyIFormatProviderFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SpecifyIFormatProviderAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicSpecifyIFormatProviderFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class SpecifyIFormatProviderTests
    {
        [Fact]
        public async Task CA1305_StringReturningStringFormatOverloads_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningUserMethodOverloads_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningNoDiagnostics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

    public static void SpecifyIFormatProvider8()
    {
        IFormatProviderOverloads.MethodOverloadWithMismatchRefKind(""Bar"");
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

    public static string MethodOverloadWithMismatchRefKind(string format)
    {
        return null;
    }

    public static string MethodOverloadWithMismatchRefKind(IFormatProvider provider, ref string format)
    {
        return null;
    }

    public static string MethodOverloadWithMismatchRefKind(out IFormatProvider provider, string format)
    {
        provider = null;
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
        public async Task CA1305_NonStringReturningStringFormatOverloads_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_NonStringReturningStringFormatOverloads_TargetMethodNoGenerics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningUICultureIFormatProvider_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_NonStringReturningUICultureIFormatProvider_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_AcceptNullForIFormatProvider_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_DoesNotRecommendObsoleteOverload_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_RuleException_NoDiagnostics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningStringFormatOverloads_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningUserMethodOverloads_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningNoDiagnostics_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_NonStringReturningStringFormatOverloads_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_StringReturningUICultureIFormatProvider_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_NonStringReturningUICultureIFormatProvider_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_NonStringReturningComputerInfoInstalledUICultureIFormatProvider_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task CA1305_RuleException_NoDiagnostics_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Fact]
        [WorkItem(2394, "https://github.com/dotnet/roslyn-analyzers/issues/2394")]
        public async Task CA1305_BoolToString_NoDiagnostics()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Foo
{
    public string Bar(bool b1, System.Boolean b2)
    {
        return b1.ToString() + b2.ToString();
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Foo
    Public Function Bar(ByVal b As Boolean) As String
        Return b.ToString()
    End Function
End Class
");
        }

        [Fact]
        [WorkItem(2394, "https://github.com/dotnet/roslyn-analyzers/issues/2394")]
        public async Task CA1305_CharToString_NoDiagnostics()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Foo
{
    public string Bar(char c1, System.Char c2)
    {
        return c1.ToString() + c2.ToString();
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Foo
    Public Function Bar(ByVal c As Char) As String
        Return c.ToString()
    End Function
End Class
");
        }

        [Fact]
        [WorkItem(2394, "https://github.com/dotnet/roslyn-analyzers/issues/2394")]
        public async Task CA1305_StringToString_NoDiagnostics()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Foo
{
    public string Bar(string s1, System.String s2)
    {
        return s1.ToString() + s2.ToString();
    }
}");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Foo
    Public Function Bar(ByVal s As String) As String
        Return s.ToString()
    End Function
End Class
");
        }

        [Fact]
        public async Task CA1305_Parse_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Foo
{
    public Foo()
    {
        sbyte.Parse("""");
        byte.Parse("""");
        short.Parse("""");
        ushort.Parse("""");
        int.Parse("""");
        uint.Parse("""");
        long.Parse("""");
        ulong.Parse("""");

        char.Parse(""""); // no issue because no overload with format provider

        float.Parse("""");
        double.Parse("""");
        decimal.Parse("""");
    }
}",
            GetIFormatProviderAlternateStringRuleCSharpResultAt(6, 9, "sbyte.Parse(string)", "Foo.Foo()", "sbyte.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(7, 9, "byte.Parse(string)", "Foo.Foo()", "byte.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(8, 9, "short.Parse(string)", "Foo.Foo()", "short.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(9, 9, "ushort.Parse(string)", "Foo.Foo()", "ushort.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 9, "int.Parse(string)", "Foo.Foo()", "int.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "uint.Parse(string)", "Foo.Foo()", "uint.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "long.Parse(string)", "Foo.Foo()", "long.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(13, 9, "ulong.Parse(string)", "Foo.Foo()", "ulong.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(17, 9, "float.Parse(string)", "Foo.Foo()", "float.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(18, 9, "double.Parse(string)", "Foo.Foo()", "double.Parse(string, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleCSharpResultAt(19, 9, "decimal.Parse(string)", "Foo.Foo()", "decimal.Parse(string, IFormatProvider)"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Foo
    Public Sub New()
        SByte.Parse("""")
        Byte.Parse("""")
        Short.Parse("""")
        UShort.Parse("""")
        Integer.Parse("""")
        UInteger.Parse("""")
        Long.Parse("""")
        ULong.Parse("""")

        Char.Parse("""")

        Single.Parse("""")
        Double.Parse("""")
        Decimal.Parse("""")
    End Sub
End Class",
            GetIFormatProviderAlternateStringRuleBasicResultAt(4, 9, "SByte.Parse(String)", "Foo.New()", "SByte.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(5, 9, "Byte.Parse(String)", "Foo.New()", "Byte.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(6, 9, "Short.Parse(String)", "Foo.New()", "Short.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(7, 9, "UShort.Parse(String)", "Foo.New()", "UShort.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(8, 9, "Integer.Parse(String)", "Foo.New()", "Integer.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(9, 9, "UInteger.Parse(String)", "Foo.New()", "UInteger.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(10, 9, "Long.Parse(String)", "Foo.New()", "Long.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(11, 9, "ULong.Parse(String)", "Foo.New()", "ULong.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(15, 9, "Single.Parse(String)", "Foo.New()", "Single.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(16, 9, "Double.Parse(String)", "Foo.New()", "Double.Parse(String, IFormatProvider)"),
            GetIFormatProviderAlternateStringRuleBasicResultAt(17, 9, "Decimal.Parse(String)", "Foo.New()", "Decimal.Parse(String, IFormatProvider)"));
        }

        [Theory]
        // No data
        [InlineData("")]
        // Invalid option
        [InlineData("dotnet_code_quality.CA1305.exclude_tryparse_methods = FOO")]
        // Valid options
        [InlineData("dotnet_code_quality.CA1305.exclude_tryparse_methods = true")]
        [InlineData("dotnet_code_quality.CA1305.exclude_tryparse_methods = false")]
        public async Task CA1305_TryParse(string editorConfigText)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using System;

public class Foo
{
    public Foo()
    {
        sbyte.TryParse("""", out _);
        byte.TryParse("""", out _);
        short.TryParse("""", out _);
        ushort.TryParse("""", out _);
        int.TryParse("""", out _);
        uint.TryParse("""", out _);
        long.TryParse("""", out _);
        ulong.TryParse("""", out _);

        char.TryParse("""", out _); // no issue because no overload with format provider

        float.TryParse("""", out _);
        double.TryParse("""", out _);
        decimal.TryParse("""", out _);

        DateTime.TryParse("""", out _);
        TimeSpan.TryParse("""", out _);

        TryParse("""", out _);
    }

    public void TryParse(string s, out Foo f)
    {
        f = null;
    }

    public void TryParse(string s, IFormatProvider format, out Foo f)
    {
        f = null;
    }
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) },
                }
            };

            if (editorConfigText.EndsWith("false", System.StringComparison.OrdinalIgnoreCase))
            {
                csharpTest.ExpectedDiagnostics.AddRange(new[]
                {
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(8, 9, "sbyte.TryParse(string, out sbyte)", "Foo.Foo()", "sbyte.TryParse(string, NumberStyles, IFormatProvider, out sbyte)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(9, 9, "byte.TryParse(string, out byte)", "Foo.Foo()", "byte.TryParse(string, NumberStyles, IFormatProvider, out byte)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 9, "short.TryParse(string, out short)", "Foo.Foo()", "short.TryParse(string, NumberStyles, IFormatProvider, out short)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "ushort.TryParse(string, out ushort)", "Foo.Foo()", "ushort.TryParse(string, NumberStyles, IFormatProvider, out ushort)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "int.TryParse(string, out int)", "Foo.Foo()", "int.TryParse(string, NumberStyles, IFormatProvider, out int)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(13, 9, "uint.TryParse(string, out uint)", "Foo.Foo()", "uint.TryParse(string, NumberStyles, IFormatProvider, out uint)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(14, 9, "long.TryParse(string, out long)", "Foo.Foo()", "long.TryParse(string, NumberStyles, IFormatProvider, out long)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(15, 9, "ulong.TryParse(string, out ulong)", "Foo.Foo()", "ulong.TryParse(string, NumberStyles, IFormatProvider, out ulong)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(19, 9, "float.TryParse(string, out float)", "Foo.Foo()", "float.TryParse(string, NumberStyles, IFormatProvider, out float)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(20, 9, "double.TryParse(string, out double)", "Foo.Foo()", "double.TryParse(string, NumberStyles, IFormatProvider, out double)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(21, 9, "decimal.TryParse(string, out decimal)", "Foo.Foo()", "decimal.TryParse(string, NumberStyles, IFormatProvider, out decimal)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(23, 9, "DateTime.TryParse(string, out DateTime)", "Foo.Foo()", "DateTime.TryParse(string, IFormatProvider, DateTimeStyles, out DateTime)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(24, 9, "TimeSpan.TryParse(string, out TimeSpan)", "Foo.Foo()", "TimeSpan.TryParse(string, IFormatProvider, out TimeSpan)"),
                    GetIFormatProviderAlternateStringRuleCSharpResultAt(26, 9, "Foo.TryParse(string, out Foo)", "Foo.Foo()", "Foo.TryParse(string, IFormatProvider, out Foo)"),
                });
            }

            await csharpTest.RunAsync();

            var vbTest = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Imports System

Public Class Foo
    Public Sub New()
        Dim v1 As SByte
        SByte.TryParse("""", v1)
        Dim v2 As Byte
        Byte.TryParse("""", v2)
        Dim v3 As Short
        Short.TryParse("""", v3)
        Dim v4 As UShort
        UShort.TryParse("""", v4)
        Dim v5 As Integer
        Integer.TryParse("""", v5)
        Dim v6 As UInteger
        UInteger.TryParse("""", v6)
        Dim v7 As Long
        Long.TryParse("""", v7)
        Dim v8 As ULong
        ULong.TryParse("""", v8)

        Dim v9 As Char
        Char.TryParse("""", v9)

        Dim v10 As Single
        Single.TryParse("""", v10)
        Dim v11 As Double
        Double.TryParse("""", v11)
        Dim v12 As Decimal
        Decimal.TryParse("""", v12)

        Dim v13 As DateTime
        DateTime.TryParse("""", v13)
        Dim v14 As TimeSpan
        TimeSpan.TryParse("""", v14)

        Dim v15 As Foo
        TryParse("""", v15)
    End Sub

    Public Sub TryParse(ByVal s As String, ByRef f As Foo)
        f = Nothing
    End Sub

    Public Sub TryParse(ByVal s As String, ByVal format As IFormatProvider, ByRef f As Foo)
        f = Nothing
    End Sub
End Class"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) },
                }
            };

            if (editorConfigText.EndsWith("false", System.StringComparison.OrdinalIgnoreCase))
            {
                vbTest.ExpectedDiagnostics.AddRange(new[]
                {
                    GetIFormatProviderAlternateStringRuleBasicResultAt(7, 9, "SByte.TryParse(String, ByRef SByte)", "Foo.New()", "SByte.TryParse(String, NumberStyles, IFormatProvider, ByRef SByte)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(9, 9, "Byte.TryParse(String, ByRef Byte)", "Foo.New()", "Byte.TryParse(String, NumberStyles, IFormatProvider, ByRef Byte)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(11, 9, "Short.TryParse(String, ByRef Short)", "Foo.New()", "Short.TryParse(String, NumberStyles, IFormatProvider, ByRef Short)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(13, 9, "UShort.TryParse(String, ByRef UShort)", "Foo.New()", "UShort.TryParse(String, NumberStyles, IFormatProvider, ByRef UShort)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(15, 9, "Integer.TryParse(String, ByRef Integer)", "Foo.New()", "Integer.TryParse(String, NumberStyles, IFormatProvider, ByRef Integer)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(17, 9, "UInteger.TryParse(String, ByRef UInteger)", "Foo.New()", "UInteger.TryParse(String, NumberStyles, IFormatProvider, ByRef UInteger)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(19, 9, "Long.TryParse(String, ByRef Long)", "Foo.New()", "Long.TryParse(String, NumberStyles, IFormatProvider, ByRef Long)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(21, 9, "ULong.TryParse(String, ByRef ULong)", "Foo.New()", "ULong.TryParse(String, NumberStyles, IFormatProvider, ByRef ULong)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(27, 9, "Single.TryParse(String, ByRef Single)", "Foo.New()", "Single.TryParse(String, NumberStyles, IFormatProvider, ByRef Single)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(29, 9, "Double.TryParse(String, ByRef Double)", "Foo.New()", "Double.TryParse(String, NumberStyles, IFormatProvider, ByRef Double)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(31, 9, "Decimal.TryParse(String, ByRef Decimal)", "Foo.New()", "Decimal.TryParse(String, NumberStyles, IFormatProvider, ByRef Decimal)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(34, 9, "Date.TryParse(String, ByRef Date)", "Foo.New()", "Date.TryParse(String, IFormatProvider, DateTimeStyles, ByRef Date)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(36, 9, "TimeSpan.TryParse(String, ByRef TimeSpan)", "Foo.New()", "TimeSpan.TryParse(String, IFormatProvider, ByRef TimeSpan)"),
                    GetIFormatProviderAlternateStringRuleBasicResultAt(39, 9, "Foo.TryParse(String, ByRef Foo)", "Foo.New()", "Foo.TryParse(String, IFormatProvider, ByRef Foo)"),
                });
            }

            await vbTest.RunAsync();
        }

        private DiagnosticResult GetIFormatProviderAlternateStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateStringRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.UICultureStringRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.UICultureRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateStringRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateStringRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderAlternateRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureStringRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.UICultureStringRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }

        private DiagnosticResult GetIFormatProviderUICultureRuleBasicResultAt(int line, int column, string arg1, string arg2, string arg3)
        {
            return new DiagnosticResult(SpecifyIFormatProviderAnalyzer.UICultureRule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
        }
    }
}
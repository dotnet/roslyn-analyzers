// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class SpecifyIFormatProviderTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicSpecifyIFormatProviderAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpSpecifyIFormatProviderAnalyzer();
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
        return string.Format(""Foo {0} {1} {2} {3}"", ""bar"", ""foo"", ""bar"", "");
    }
}",
GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 16, "string.Format(string, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider1()",
                                                            "string.Format(System.IFormatProvider, string, params object[])",
                                                            "System.Globalization.CultureInfo.CurrentCulture",
                                                            "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(15, 16, "string.Format(string, object, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider2()",
                                                            "string.Format(System.IFormatProvider, string, params object[])",
                                                            "System.Globalization.CultureInfo.CurrentCulture",
                                                            "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(20, 16, "string.Format(string, object, object, object)",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider3()",
                                                            "string.Format(System.IFormatProvider, string, params object[])",
                                                            "System.Globalization.CultureInfo.CurrentCulture",
                                                            "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(25, 16, "string.Format(string, params object[])",
                                                            "IFormatProviderStringTest.SpecifyIFormatProvider4()",
                                                            "string.Format(System.IFormatProvider, string, params object[])",
                                                            "System.Globalization.CultureInfo.CurrentCulture",
                                                            "System.Globalization.CultureInfo.InvariantCulture"));
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
                                                           "IFormatProviderOverloads.LeadingIFormatProviderReturningString(System.IFormatProvider, string)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.TrailingIFormatProviderReturningString(string)",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                           "IFormatProviderOverloads.TrailingIFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(string, params object[])",
                                                           "IFormatProviderStringTest.SpecifyIFormatProvider()",
                                                           "IFormatProviderOverloads.UserDefinedParamsMatchMethodOverload(System.IFormatProvider, string, params object[])",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"));
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
GetIFormatProviderAlternateRuleCSharpResultAt(8, 17, "System.Convert.ToInt32(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "System.Convert.ToInt32(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateRuleCSharpResultAt(9, 18, "System.Convert.ToInt64(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "System.Convert.ToInt64(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.LeadingIFormatProvider(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.LeadingIFormatProvider(System.IFormatProvider, string)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.TrailingIFormatProvider(string)",
                                                     "IFormatProviderStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.TrailingIFormatProvider(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"));
        }

        [Fact]
        public void CA1305_StringReturningUICultureIFormatProvider_CSharp()
        {
            this.PrintActualDiagnosticsOnFailure = true;
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
GetIFormatProviderAlternateStringRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(10, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "System.Globalization.CultureInfo.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(11, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "System.Globalization.CultureInfo.InstalledUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateStringRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(12, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "System.Threading.Thread.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "System.Threading.Thread.CurrentUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureStringRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningStringTest.TestMethod()",
                                                           "System.Globalization.CultureInfo.InstalledUICulture",
                                                           "IFormatProviderOverloads.IFormatProviderReturningString(string, System.IFormatProvider, System.IFormatProvider)",
                                                           "System.Globalization.CultureInfo.CurrentCulture",
                                                           "System.Globalization.CultureInfo.InvariantCulture"));
        }

        [Fact]
        public void CA1305_NonStringReturningUICultureIFormatProvider_CSharp()
        {
            this.PrintActualDiagnosticsOnFailure = true;
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
GetIFormatProviderAlternateRuleCSharpResultAt(10, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureRuleCSharpResultAt(10, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "System.Globalization.CultureInfo.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateRuleCSharpResultAt(11, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureRuleCSharpResultAt(11, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "System.Globalization.CultureInfo.InstalledUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderAlternateRuleCSharpResultAt(12, 9, "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureRuleCSharpResultAt(12, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "System.Threading.Thread.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "System.Threading.Thread.CurrentUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"),
GetIFormatProviderUICultureRuleCSharpResultAt(13, 9, "UICultureAsIFormatProviderReturningNonStringTest.TestMethod()",
                                                     "System.Globalization.CultureInfo.InstalledUICulture",
                                                     "IFormatProviderOverloads.IFormatProviderReturningNonString(string, System.IFormatProvider, System.IFormatProvider)",
                                                     "System.Globalization.CultureInfo.CurrentCulture",
                                                     "System.Globalization.CultureInfo.InvariantCulture"));
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
    }
}");
        }

        private DiagnosticResult GetIFormatProviderAlternateStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateStringRule, arg1, arg2, arg3, arg4, arg5);
        }

        private DiagnosticResult GetIFormatProviderAlternateRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.IFormatProviderAlternateRule, arg1, arg2, arg3, arg4, arg5);
        }

        private DiagnosticResult GetIFormatProviderUICultureStringRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureStringRule, arg1, arg2, arg3, arg4, arg5);
        }

        private DiagnosticResult GetIFormatProviderUICultureRuleCSharpResultAt(int line, int column, string arg1, string arg2, string arg3, string arg4, string arg5)
        {
            return GetCSharpResultAt(line, column, SpecifyIFormatProviderAnalyzer.UICultureRule, arg1, arg2, arg3, arg4, arg5);
        }
    }
}
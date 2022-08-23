// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Usage.CSharpImplementGenericMathInterfacesCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class ImplementGenericMathInterfacesCorrectlyTests
    {
        [Fact]
        public async Task IParsableNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System;

public readonly struct MyDate : IParsable<{|#0:DateOnly|}> // 'IParsable' interface requires the derived type 'MyDate' used for the 'TSelf' type parameter
{
    public static DateOnly Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out DateOnly result)
    {
        throw new NotImplementedException();
    }
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.CRTPRule).WithLocation(0).WithArguments("IParsable", "MyDate")).RunAsync();
        }

        [Fact]
        public async Task IParsableImplementedCorrectlyNotWarn()
        {
            await PopulateTestCs(@"
using System;

public readonly struct MyDate : IParsable<MyDate>
{
    public static MyDate Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out MyDate result)
    {
        throw new NotImplementedException();
    }
}

public class Test : ISpanParsable<Test>
{
    public static Test Parse(ReadOnlySpan<char> s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Test result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out Test result)
    {
        throw new NotImplementedException();
    }

    static Test IParsable<Test>.Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }
}

public readonly struct MyDate<TSelf> : IParsable<TSelf> where TSelf : IParsable<TSelf>
{
    public static TSelf Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out TSelf result)
    {
        throw new NotImplementedException();
    }
}").RunAsync();
        }

        [Fact]
        public async Task ISpanParsableNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System;

public class Test : ISpanParsable<{|#0:DateOnly|}> // 'ISpanParsable' interface requires the derived type 'Test' used for the 'TSelf' type parameter
{
    public static DateOnly Parse(ReadOnlySpan<char> s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out DateOnly result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out DateOnly result)
    {
        throw new NotImplementedException();
    }

    static DateOnly IParsable<DateOnly>.Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }
}", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.CRTPRule).WithLocation(0).WithArguments("ISpanParsable", "Test")).RunAsync();
        }

        [Fact]
        public async Task IAdditionOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

public class Test : IAdditionOperators<Test, MyTest, long>
{
    public static long operator +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }

    public static long operator checked +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }
}
public class MyTest : IAdditionOperators<[|Test|], MyTest, long> // 'IAdditionOperators' interface requires the derived type 'MyTest' used for the 'TSelf' type parameter
{    public static long operator +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }

    public static long operator checked +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }


}").RunAsync();
        }

        private static VerifyCS.Test PopulateTestCs(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.Preview
            };
            test.ExpectedDiagnostics.AddRange(expected);
            return test;
        }
    }
}



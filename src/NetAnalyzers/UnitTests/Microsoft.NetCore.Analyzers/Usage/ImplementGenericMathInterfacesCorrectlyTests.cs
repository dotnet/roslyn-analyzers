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
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("IParsable", "MyDate", "TSelf")).RunAsync();
        }

        [Fact]
        public async Task CustomInterfaceWithKnownNameImplementedNotWarn()
        {
            await PopulateTestCs(@"
using System;

namespace MyNamespace
{
    public interface IParsable<TSelf> where TSelf : IParsable<TSelf>
    { }

    public readonly struct MyDate : IParsable<MyDate>
    { }

    public readonly struct MyDate2 : IParsable<MyDate>
    { }
}").RunAsync();
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
}", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("ISpanParsable", "Test", "TSelf")).RunAsync();
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

        [Fact]
        public async Task ParentClassImplementedIParsableShouldWarn()
        {
            await PopulateTestCs(@"
using System;

class Foo<TMe> : IParsable<TMe> where TMe : IParsable<TMe>
{
    public static TMe Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse(string s, IFormatProvider provider, out TMe result)
    {
        throw new NotImplementedException();
    }
}
class WrongImplementation : Foo<{|#0:int|}> { } // 'IParsable' interface requires the derived type 'WrongImplementation' used for the 'TMe' type parameter

class CorrectImplementation : Foo<CorrectImplementation> { }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("IParsable", "WrongImplementation", "TMe")).RunAsync();
        }

        [Fact]
        public async Task ParentInterfaceImplementedIParsableShouldWarn()
        {
            await PopulateTestCs(@"
using System;

interface IMyInterface<TMe> : IParsable<TMe> where TMe : IParsable<TMe>
{ }

class WrongImplementation : IMyInterface<{|#0:int|}>
{
    public static int Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out int result)
    {
        throw new NotImplementedException();
    }
}

class CorrectImplementation : IMyInterface<CorrectImplementation>
{
    public static CorrectImplementation Parse(string s, IFormatProvider provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string s, IFormatProvider provider, out CorrectImplementation result)
    {
        throw new NotImplementedException();
    }
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("IParsable", "WrongImplementation", "TMe")).RunAsync();
        }

        [Fact]
        public async Task DerivedUsedBaseTypeaAsTypeParameters()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Base : IAdditionOperators<Base, int, int>
{
    public static int operator +(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator checked +(Base left, int right)
    {
        throw new NotImplementedException();
    }
}

class Derived : Base, IAdditionOperators<{|#0:Base|}, int, int> // 'IAdditionOperators' interface requires the derived type 'Derived' used for the 'TSelf' type parameter
{
    static int IAdditionOperators<Base, int, int>.operator +(Base left, int right)
    {
        throw new NotImplementedException();
    }

    static int IAdditionOperators<Base, int, int>.operator checked +(Base left, int right)
    {
        throw new NotImplementedException();
    }
}

class DerivedCorrect : Base, IAdditionOperators<DerivedCorrect, int, int>
{
    public static int operator +(DerivedCorrect left, int right)
    {
        throw new NotImplementedException();
    }
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("IAdditionOperators", "Derived", "TSelf")).RunAsync();
        }

        [Fact]
        public async Task BaseUsedDerivedTypeAsTypeParameter()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Base : IAdditionOperators<{|#0:Derived|}, int, int> // 'IAdditionOperators' interface requires the derived type 'Base' used for the 'TSelf' type parameter
{
    static int IAdditionOperators<Derived, int, int>.operator +(Derived left, int right)
    {
        throw new NotImplementedException();
    }
}

class Derived : Base, IAdditionOperators<Derived, int, int>
{
    public static int operator +(Derived left, int right)
    {
        throw new NotImplementedException();
    }
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMInterfacesRule).WithLocation(0).WithArguments("IAdditionOperators", "Base", "TSelf")).RunAsync();
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



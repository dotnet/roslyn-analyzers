﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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

public readonly struct MyDate : IParsable<{|#0:DateOnly|}> // The 'IParsable<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'MyDate' 
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
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IParsable<TSelf>", "TSelf", "MyDate")).RunAsync();
        }

        [Fact]
        public async Task ISpanParsableNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System;

public class Test : ISpanParsable<{|#0:DateOnly|}> // The 'ISpanParsable<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'Test' 
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
}", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("ISpanParsable<TSelf>", "TSelf", "Test")).RunAsync();
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
public class MyTest : IAdditionOperators<{|#0:Test|}, MyTest, long> // The 'IAdditionOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'MyTest' 
{    
    public static long operator +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }

    public static long operator checked +(Test left, MyTest right)
    {
        throw new NotImplementedException();
    }
}", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IAdditionOperators<TSelf, TOther, TResult>", "TSelf", "MyTest")).RunAsync();
        }

        [Fact]
        public async Task IAdditiveIdentityNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

public class Additive : IAdditiveIdentity<{|#0:int|}, int> // The 'IAdditiveIdentity<TSelf, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'Additive'
{
    public static int AdditiveIdentity => throw new NotImplementedException();
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IAdditiveIdentity<TSelf, TResult>", "TSelf", "Additive")).RunAsync();
        }

        [Fact]
        public async Task IBinaryFloatingPointIeee754NotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : IBinaryFloatingPointIeee754<{|#0:double|}> // The 'IBinaryFloatingPointIeee754<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IBinaryFloatingPointIeee754<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task IBinaryIntegerNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : IBinaryInteger<{|#0:uint|}> // The 'IBinaryInteger<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IBinaryInteger<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task IBinaryNumberNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : IBinaryNumber<{|#0:uint|}> // The 'IBinaryNumber<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IBinaryNumber<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task DerivedUsedBaseTypeAsTypeParameter()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Base : IComparisonOperators<Base, int, int>
{
    public static int operator ==(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator !=(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator <(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator >(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator <=(Base left, int right)
    {
        throw new NotImplementedException();
    }

    public static int operator >=(Base left, int right)
    {
        throw new NotImplementedException();
    }
}

class Derived : Base, IComparisonOperators<{|#0:Base|}, int, int> // The 'IComparisonOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'Derived'
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IComparisonOperators<TSelf, TOther, TResult>", "TSelf", "Derived")).RunAsync();
        }

        [Fact]
        public async Task BaseUsedDerivedTypeAsTypeParameter()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Base : IBitwiseOperators<{|#0:Derived|}, int, int> // The 'IBitwiseOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'Base'
{
    static int IBitwiseOperators<Derived, int, int>.operator ~(Derived value)
    {
        throw new NotImplementedException();
    }

    static int IBitwiseOperators<Derived, int, int>.operator &(Derived left, int right)
    {
        throw new NotImplementedException();
    }

    static int IBitwiseOperators<Derived, int, int>.operator |(Derived left, int right)
    {
        throw new NotImplementedException();
    }

    static int IBitwiseOperators<Derived, int, int>.operator ^(Derived left, int right)
    {
        throw new NotImplementedException();
    }
}

class Derived : Base, IBitwiseOperators<Derived, int, int>
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IBitwiseOperators<TSelf, TOther, TResult>", "TSelf", "Base")).RunAsync();
        }

        [Fact]
        public async Task ParentClassImplementedIParsableShouldWarn()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Foo<TMe> : IDecrementOperators<TMe> where TMe : IDecrementOperators<TMe>
{
    static TMe IDecrementOperators<TMe>.operator --(TMe value)
    {
        throw new NotImplementedException();
    }
}
class WrongImplementation : Foo<{|#0:int|}> { } // The 'Foo<TMe>' interface requires the 'TMe' type parameter to be filled with the derived type 'WrongImplementation' 

class CorrectImplementation : Foo<CorrectImplementation> { }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForClass).WithLocation(0).WithArguments("Foo<TMe>", "TMe", "WrongImplementation")).RunAsync();
        }

        [Fact]
        public async Task IDivisionOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IDivisionOperators<{|#0:int|}, int, int> // The 'IDivisionOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IDivisionOperators<TSelf, TOther, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IEqualityOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyEquality : IEqualityOperators<{|#0:int|}, int, bool> // The 'IEqualityOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyEquality' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IEqualityOperators<TSelf, TOther, TResult>", "TSelf", "IMyEquality")).RunAsync();
        }

        [Fact]
        public async Task IExponentialFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyExponential : IExponentialFunctions<{|#0:double|}> // The 'IExponentialFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyExponential' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IExponentialFunctions<TSelf>", "TSelf", "IMyExponential")).RunAsync();
        }

        [Fact]
        public async Task IFloatingPointIeee754NotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyFloat : IFloatingPointIeee754<{|#0:float|}> // The 'IFloatingPointIeee754<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyFloat' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IFloatingPointIeee754<TSelf>", "TSelf", "IMyFloat")).RunAsync();
        }

        [Fact]
        public async Task IFloatingPointNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyFloat : IFloatingPoint<{|#0:float|}> // The 'IFloatingPoint<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyFloat' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IFloatingPoint<TSelf>", "TSelf", "IMyFloat")).RunAsync();
        }

        [Fact]
        public async Task IHyperbolicFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyHyperbolic : IHyperbolicFunctions<{|#0:float|}> // The 'IHyperbolicFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyHyperbolic' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IHyperbolicFunctions<TSelf>", "TSelf", "IMyHyperbolic")).RunAsync();
        }

        [Fact]
        public async Task IIncrementOperatorsNotImplementedCorrectlyInBaseChain()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

class Base1<T> : IIncrementOperators<T> where T : IIncrementOperators<T>
{
    static T IIncrementOperators<T>.operator ++(T value)
    {
        throw new NotImplementedException();
    }
}

class Base2<T> : Base1<T> where T : IIncrementOperators<T> { }

class Wrong : Base2<{|#0:int|}> 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForClass).WithLocation(0).WithArguments("Base2<T>", "T", "Wrong")).RunAsync();
        }

        [Fact]
        public async Task ILogarithmicFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyLogarithm : ILogarithmicFunctions<{|#0:float|}> // The 'ILogarithmicFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyLogarithm' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("ILogarithmicFunctions<TSelf>", "TSelf", "IMyLogarithm")).RunAsync();
        }

        [Fact]
        public async Task IMinMaxValueNotImplementedCorrectlyInRecord()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

public record MyRecord : IMinMaxValue<{|#0:float|}> // The 'IMinMaxValue<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'MyRecord'
{
    public static float MaxValue => throw new NotImplementedException();

    public static float MinValue => throw new NotImplementedException();
}
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IMinMaxValue<TSelf>", "TSelf", "MyRecord")).RunAsync();
        }

        [Fact]
        public async Task IModulusOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IModulusOperators<{|#0:int|}, int, int> // The 'IDivisionOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IModulusOperators<TSelf, TOther, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IMultiplicativeIdentityNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IMultiplicativeIdentity<{|#0:int|}, int> // The 'IMultiplicativeIdentity<TSelf, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IMultiplicativeIdentity<TSelf, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IMultiplyOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IMultiplyOperators<{|#0:int|}, int, int> // The 'IMultiplyOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IMultiplyOperators<TSelf, TOther, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task INumberBaseNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : INumberBase<{|#0:float|}> // The 'INumberBase<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("INumberBase<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task INumberNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : INumber<{|#0:float|}> // The 'INumber<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("INumber<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task IPowerFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyPower : IPowerFunctions<{|#0:float|}> // The 'IPowerFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyPower' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IPowerFunctions<TSelf>", "TSelf", "IMyPower")).RunAsync();
        }

        [Fact]
        public async Task IRootFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyRoot : IRootFunctions<{|#0:float|}> // The 'IRootFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyRoot' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IRootFunctions<TSelf>", "TSelf", "IMyRoot")).RunAsync();
        }

        [Fact]
        public async Task ISignedNumberNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : ISignedNumber<{|#0:float|}> // The 'ISignedNumber<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("ISignedNumber<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task ITrigonometricFunctionsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : ITrigonometricFunctions<{|#0:float|}> // The 'ITrigonometricFunctions<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("ITrigonometricFunctions<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task IShiftOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IShiftOperators<{|#0:int|}, int, int> // The 'IShiftOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IShiftOperators<TSelf, TOther, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IUnaryNegationOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IUnaryNegationOperators<{|#0:int|}, int> // The 'IUnaryNegationOperators<TSelf, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IUnaryNegationOperators<TSelf, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IUnaryPlusOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : IUnaryPlusOperators<{|#0:int|}, int> // The 'IUnaryPlusOperators<TSelf, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IUnaryPlusOperators<TSelf, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task ISubtractionOperatorsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyInterface : ISubtractionOperators<{|#0:int|}, int, int> // The 'ISubtractionOperators<TSelf, TOther, TResult>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyInterface' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("ISubtractionOperators<TSelf, TOther, TResult>", "TSelf", "IMyInterface")).RunAsync();
        }

        [Fact]
        public async Task IUnsignedNumberNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : IUnsignedNumber<{|#0:uint|}> // The 'IUnsignedNumber<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IUnsignedNumber<TSelf>", "TSelf", "IMyNumber")).RunAsync();
        }

        [Fact]
        public async Task IFloatingPointConstantsNotImplementedCorrectly()
        {
            await PopulateTestCs(@"
using System.Numerics;

interface IMyNumber : IFloatingPointConstants<{|#0:double|}> // The 'IFloatingPointConstants<TSelf>' interface requires the 'TSelf' type parameter to be filled with the derived type 'IMyNumber' 
{ }
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IFloatingPointConstants<TSelf>", "TSelf", "IMyNumber")).RunAsync();
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
        public async Task InterfacesImplementedCorrectlyNotWarn()
        {
            await PopulateTestCs(@"
using System;
using System.Numerics;

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
}
public record MyRecord : IMinMaxValue<MyRecord>
{
    public static MyRecord MaxValue => throw new NotImplementedException();

    public static MyRecord MinValue => throw new NotImplementedException();
}").RunAsync();
        }

        [Fact]
        public async Task ParentInterfaceImplementedIParsableShouldWarn()
        {
            await PopulateTestCs(@"
using System;

interface IMyInterface<TMe> : IParsable<TMe> where TMe : IParsable<TMe>
{ }

class WrongImplementation : IMyInterface<{|#0:int|}> // The 'IMyInterface<TMe>' interface requires the 'TMe' type parameter to be filled with the derived type 'WrongImplementation'
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
", VerifyCS.Diagnostic(ImplementGenericMathInterfacesCorrectly.GMIRuleForInterface).WithLocation(0).WithArguments("IMyInterface<TMe>", "TMe", "WrongImplementation")).RunAsync();
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



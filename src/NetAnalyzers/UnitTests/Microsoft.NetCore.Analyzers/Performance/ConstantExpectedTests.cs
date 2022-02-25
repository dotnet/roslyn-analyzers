// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.NetCore.Analyzers.Performance;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpConstantExpectedAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Performance
{
    public sealed class ConstantExpectedTests
    {
        [Theory]
        [InlineData("char", "char.MinValue", "char.MaxValue")]
        [InlineData("sbyte", "sbyte.MinValue", "sbyte.MaxValue")]
        [InlineData("short", "short.MinValue", "short.MaxValue")]
        [InlineData("int", "int.MinValue", "int.MaxValue")]
        [InlineData("long", "long.MinValue", "long.MaxValue")]
        [InlineData("nint", "int.MinValue", "int.MaxValue")]
        [InlineData("byte", "byte.MinValue", "byte.MaxValue")]
        [InlineData("ushort", "ushort.MinValue", "ushort.MaxValue")]
        [InlineData("uint", "uint.MinValue", "uint.MaxValue")]
        [InlineData("ulong", "ulong.MinValue", "ulong.MaxValue")]
        [InlineData("nuint", "uint.MinValue", "uint.MaxValue")]
        [InlineData("float", "float.MinValue", "float.MaxValue")]
        [InlineData("double", "double.MinValue", "double.MaxValue")]
        public static async Task TestConstantExpectedSupportedUnmanagedTypesAsync(string type, string minValue, string maxValue)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod1([ConstantExpected] {type} val)
    {{
    }}
    public static void TestMethod2([ConstantExpected(Min={minValue})] {type} val)
    {{
    }}
    public static void TestMethod3([ConstantExpected(Max={maxValue})] {type} val)
    {{
    }}
    public static void TestMethod4([ConstantExpected(Min={minValue}, Max={maxValue})] {type} val)
    {{
    }}
    public static void TestMethod5([ConstantExpected(Min=null)] {type} val)
    {{
    }}
    public static void TestMethod6([ConstantExpected(Max=null)] {type} val)
    {{
    }}
    public static void TestMethod7([ConstantExpected(Min=null, Max=null)] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput);
        }

        [Fact]
        public static async Task TestConstantExpectedSupportedComplexTypesAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethodString([ConstantExpected] string val)
    {
    }
    public static void TestMethodGeneric<T>([ConstantExpected] T val)
    {
    }
    
    public static class GenenricClass<T>
    {
        public static void TestMethodGeneric([ConstantExpected] T val)
        {
        }
    }
}
";
            await TestCSAsync(csInput);
        }

        [Theory]
        [InlineData("char")]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("nint")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("nuint")]
        [InlineData("float")]
        [InlineData("double")]
        [InlineData("string")]
        public static async Task TestConstantExpectedSupportedComplex2TypesAsync(string type)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public interface ITest<T>
    {{
        T Method(T operand1, [ConstantExpected] T operand2);
    }}
    public interface ITest2<T>
    {{
        T Method(T operand1, [ConstantExpected] T operand2);
    }}
    public abstract class AbstractTest<T>
    {{
        public abstract T Method2(T operand1, [ConstantExpected] T operand2);
    }}

    public class Generic : AbstractTest<{type}>, ITest<{type}>, ITest2<{type}>
    {{
        public {type} Method({type} operand1, {{|#0:{type} operand2|}}) => throw new NotImplementedException();
        {type} ITest2<{type}>.Method({type} operand1, {{|#1:{type} operand2|}}) => throw new NotImplementedException();
        public override {type} Method2({type} operand1, {{|#2:{type} operand2|}}) => throw new NotImplementedException();
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(0),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(1),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(2));
        }

        [Fact]
        public static async Task TestConstantExpectedSupportedComplex3TypesAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public interface ITest<T>
    {
        T Method(T operand1, [ConstantExpected] T operand2);
    }
    public interface ITest2<T>
    {
        T Method(T operand1, [ConstantExpected] T operand2);
    }
    public abstract class AbstractTest<T>
    {
        public abstract T Method2(T operand1, [ConstantExpected] T operand2);
    }
    public class GenericForward<T> : AbstractTest<T>, ITest<T>, ITest2<T>
    {
        public T Method(T operand1, {|#0:T operand2|}) => throw new NotImplementedException();
        T ITest2<T>.Method(T operand1, {|#1:T operand2|}) => throw new NotImplementedException();
        public override T Method2(T operand1, {|#2:T operand2|}) => throw new NotImplementedException();
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(0),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(1),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeExpectedRule)
                        .WithLocation(2));
        }

        [Theory]
        [InlineData("", "", "object", "object")]
        [InlineData("", "", "Test", "Test")]
        [InlineData("", "", "Guid", "System.Guid")]
        [InlineData("", "", "decimal", "decimal")]
        [InlineData("", "", "byte[]", "byte[]")]
        [InlineData("", "", "(int, long)", "(int, long)")]
        [InlineData("<T>", "", "T[]", "T[]")]
        [InlineData("", "<T>", "T[]", "T[]")]
        public static async Task TestConstantExpectedUnsupportedTypesAsync(string classGeneric, string methodGeneric, string type, string diagnosticType)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test{classGeneric}
{{
    public static void TestMethod{methodGeneric}([{{|#0:ConstantExpected|}}] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(0)
                        .WithArguments(diagnosticType));
        }

        [Theory]
        [InlineData("object")]
        [InlineData("Test")]
        [InlineData("Guid")]
        [InlineData("decimal")]
        [InlineData("byte[]")]
        [InlineData("(int, long)")]
        public static async Task TestConstantExpectedUnsupportedIgnoredComplexTypesAsync(string type)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public interface ITest<T>
    {{
        T Method(T operand1, [ConstantExpected] T operand2);
    }}
    public interface ITest2<T>
    {{
        T Method(T operand1, [ConstantExpected] T operand2);
    }}
    public abstract class AbstractTest<T>
    {{
        public abstract T Method2(T operand1, [ConstantExpected] T operand2);
    }}
    public class Generic : AbstractTest<{type}>, ITest<{type}>, ITest2<{type}>
    {{
        public {type} Method({type} operand1, {type} operand2) => throw new NotImplementedException();
        {type} ITest2<{type}>.Method({type} operand1, {type} operand2) => throw new NotImplementedException();
        public override {type} Method2({type} operand1, {type} operand2) => throw new NotImplementedException();
    }}
}}
";
            await TestCSAsync(csInput);
        }

        [Theory]
        [InlineData("char", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("sbyte", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("short", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("int", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("long", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("nint", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("byte", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("ushort", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("uint", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("ulong", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("nuint", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("bool", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("float", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        [InlineData("double", "\"a\"", "\"a\"", "\"a\"", "\"a\"")]
        public static async Task TestConstantExpectedIncompatibleConstantTypeErrorAsync(string type, string min1, string min2, string max2, string max3)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod([ConstantExpected({{|#0:Min = {min1}|}})] {type} val)
    {{
    }}
    public static void TestMethod2([ConstantExpected({{|#1:Min = {min2}|}}, {{|#2:Max = {max2}|}})] {type} val)
    {{
    }}
    public static void TestMethod3([ConstantExpected({{|#3:Max = {max3}|}})] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(0)
                        .WithArguments("Min", type),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(1)
                        .WithArguments("Min", type),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(2)
                        .WithArguments("Max", type),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(3)
                        .WithArguments("Max", type));
        }

        [Theory]
        [InlineData("", "", "string", "true", "false", "string")]
        [InlineData("<T>", "", "T", "\"min\"", "false", "generic")]
        [InlineData("", "<T>", "T", "\"min\"", "false", "generic")]
        public static async Task TestConstantExpectedIncompatibleConstantMinMaxTypeErrorAsync(string classGeneric, string methodGeneric, string type, string badMinValue, string badMaxValue, string diagnosticText)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test{classGeneric}
{{
    public static void TestMethod1{methodGeneric}([{{|#0:ConstantExpected(Min = {badMinValue})|}}] {type} val)
    {{
    }}
    public static void TestMethod2{methodGeneric}([{{|#1:ConstantExpected(Min = {badMinValue}, Max = {badMaxValue})|}}] {type} val)
    {{
    }}
    public static void TestMethod3{methodGeneric}([{{|#2:ConstantExpected(Max = {badMaxValue})|}}] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(0)
                        .WithArguments(diagnosticText),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(1)
                        .WithArguments(diagnosticText),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(2)
                        .WithArguments(diagnosticText));
        }

        [Theory]
        [InlineData("char", "'Z'", "'A'")]
        [InlineData("sbyte", "1", "0")]
        [InlineData("short", "1", "0")]
        [InlineData("int", "1", "0")]
        [InlineData("long", "1", "0")]
        [InlineData("nint", "1", "0")]
        [InlineData("byte", "1", "0")]
        [InlineData("ushort", "1", "0")]
        [InlineData("uint", "1", "0")]
        [InlineData("ulong", "1", "0")]
        [InlineData("nuint", "1", "0")]
        [InlineData("float", "1", "0")]
        [InlineData("double", "1", "0")]
        public static async Task TestConstantExpectedInvertedConstantTypeErrorAsync(string type, string min, string max)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod([{{|#0:ConstantExpected(Min = {min}, Max = {max})|}}] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvertedRangeRule)
                        .WithLocation(0));
        }

        [Theory]
        [InlineData("sbyte", sbyte.MinValue, sbyte.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("short", short.MinValue, short.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("int", int.MinValue, int.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("long", long.MinValue, long.MaxValue, "ulong.MaxValue", "ulong.MaxValue", "ulong.MaxValue", "ulong.MaxValue")]
        [InlineData("nint", int.MinValue, int.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("byte", byte.MinValue, byte.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("ushort", ushort.MinValue, ushort.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("uint", uint.MinValue, uint.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("ulong", ulong.MinValue, ulong.MaxValue, "long.MinValue", "long.MinValue", "-1", "-1")]
        [InlineData("nuint", uint.MinValue, uint.MaxValue, "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue")]
        [InlineData("float", float.MinValue, float.MaxValue, "double.MinValue", "double.MinValue", "double.MaxValue", "double.MaxValue")]
        public static async Task TestConstantExpectedInvalidBoundsAsync(string type, object min, object max, string min1, string min2, string max2,
                string max3)
        {
            string minString = min.ToString();
            string maxString = max.ToString();
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod([ConstantExpected({{|#0:Min = {min1}|}})] {type} val)
    {{
    }}
    public static void TestMethod2([ConstantExpected({{|#1:Min = {min2}|}}, {{|#2:Max = {max2}|}})] {type} val)
    {{
    }}
    public static void TestMethod3([ConstantExpected({{|#3:Max = {max3}|}})] {type} val)
    {{
    }}
    public static void TestMethod4([ConstantExpected({{|#4:Min = false|}}, {{|#5:Max = {max2}|}})] {type} val)
    {{
    }}
    public static void TestMethod5([ConstantExpected({{|#6:Min = {min2}|}}, {{|#7:Max = true|}})] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(0)
                    .WithArguments("Min", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(1)
                    .WithArguments("Min", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(2)
                    .WithArguments("Max", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(3)
                    .WithArguments("Max", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                    .WithLocation(4)
                    .WithArguments("Min", type),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(5)
                    .WithArguments("Max", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                    .WithLocation(6)
                    .WithArguments("Min", minString, maxString),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                    .WithLocation(7)
                    .WithArguments("Max", type));
        }

        [Theory]
        [InlineData("char", "'A'", "'Z'", "'A'", "(char)('A'+'\\u0001')")]
        [InlineData("sbyte", "10", "20", "10", "2*5")]
        [InlineData("short", "10", "20", "10", "2*5")]
        [InlineData("int", "10", "20", "10", "2*5")]
        [InlineData("long", "10", "20", "10", "2*5")]
        [InlineData("nint", "10", "20", "10", "2*5")]
        [InlineData("byte", "10", "20", "10", "2*5")]
        [InlineData("ushort", "10", "20", "10", "2*5")]
        [InlineData("uint", "10", "20", "10", "2*5")]
        [InlineData("ulong", "10", "20", "10", "2*5")]
        [InlineData("nuint", "10", "20", "10", "2*5")]
        [InlineData("float", "10", "20", "10", "2*5")]
        [InlineData("double", "10", "20", "10", "2*5")]
        [InlineData("bool", "true", "true", "true", "!false")]
        [InlineData("string", "null", "null", "\"true\"", "\"false\"")]
        public static async Task TestArgumentConstantAsync(string type, string minValue, string maxValue, string value, string expression)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod()
    {{
        TestMethodWithConstant({value});
        TestMethodWithConstant({expression});
        TestMethodWithConstrainedConstant({value});
        TestMethodWithConstrainedConstant({expression});
        TestMethodGeneric<{type}>({value});
        TestMethodGeneric<{type}>({expression});
        GenericClass<{type}>.TestMethodGeneric({value});
        GenericClass<{type}>.TestMethodGeneric({expression});
    }}
    public static void TestMethodWithConstant([ConstantExpected] {type} val)
    {{
    }}
    public static void TestMethodWithConstrainedConstant([ConstantExpected(Min = {minValue}, Max = {maxValue})] {type} val)
    {{
    }}
    public static void TestMethodGeneric<T>([ConstantExpected] T val)
    {{
    }}
    
    public static class GenericClass<T>
    {{
        public static void TestMethodGeneric([ConstantExpected] T val)
        {{
        }}
    }}
}}
";
            await TestCSAsync(csInput);
        }

        [Theory]
        [InlineData("char")]
        [InlineData("sbyte")]
        [InlineData("short")]
        [InlineData("int")]
        [InlineData("long")]
        [InlineData("nint")]
        [InlineData("byte")]
        [InlineData("ushort")]
        [InlineData("uint")]
        [InlineData("ulong")]
        [InlineData("nuint")]
        [InlineData("float")]
        [InlineData("double")]
        [InlineData("string")]
        public static async Task TestArgumentNotConstantAsync(string type)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod({type} nonConstant)
    {{
        TestMethodWithConstant({{|#0:nonConstant|}});
        TestMethodGeneric<{type}>({{|#1:nonConstant|}});
        GenenricClass<{type}>.TestMethodGeneric({{|#2:nonConstant|}});
    }}
    public static void TestMethodWithConstant([ConstantExpected] {type} val)
    {{
    }}
    public static void TestMethodGeneric<T>([ConstantExpected] T val)
    {{
    }}
    
    public static class GenenricClass<T>
    {{
        public static void TestMethodGeneric([ConstantExpected] T val)
        {{
        }}
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(0),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(1),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(2));
        }

        [Theory]
        [InlineData("char", "'B'", "'C'", "'D'")]
        [InlineData("sbyte", "3", "4", "5")]
        [InlineData("short", "3", "4", "5")]
        [InlineData("int", "3", "4", "5")]
        [InlineData("long", "3", "4", "5")]
        [InlineData("nint", "3", "4", "5")]
        [InlineData("byte", "3", "4", "5")]
        [InlineData("ushort", "3", "4", "5")]
        [InlineData("uint", "3", "4", "5")]
        [InlineData("ulong", "3", "4", "5")]
        [InlineData("nuint", "3", "4", "5")]
        [InlineData("float", "3", "4", "5")]
        [InlineData("double", "3", "4", "5")]
        public static async Task TestArgumentOutOfBoundsConstantAsync(string type, string min, string max, string testValue)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod()
    {{
        TestMethodWithConstant({{|#0:{testValue}|}});
    }}
    public static void TestMethodWithConstant([ConstantExpected(Min={min}, Max={max})] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantOutOfBoundsRule)
                        .WithLocation(0)
                        .WithArguments(min.Trim('\''), max.Trim('\'')));
        }

        [Fact]
        public static async Task TestArgumentInvalidGenericTypeParameterConstantAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod(int[] nonConstant)
    {
        TestMethodGeneric<int[]>(nonConstant); // ignore scenario
        GenenricClass<int[]>.TestMethodGeneric(nonConstant); // ignore scenario
    }
    public static void TestMethodGeneric<T>([ConstantExpected] T val)
    {
    }
    
    public static class GenenricClass<T>
    {
        public static void TestMethodGeneric([ConstantExpected] T val)
        {
        }
    }
}
";
            await TestCSAsync(csInput);
        }

        [Theory]
        [InlineData("char", "'B'", "'C'")]
        [InlineData("sbyte", "3", "4")]
        [InlineData("short", "3", "4")]
        [InlineData("int", "3", "4")]
        [InlineData("long", "3", "4")]
        [InlineData("nint", "3", "4")]
        [InlineData("byte", "3", "4")]
        [InlineData("ushort", "3", "4")]
        [InlineData("uint", "3", "4")]
        [InlineData("ulong", "3", "4")]
        [InlineData("nuint", "3", "4")]
        [InlineData("float", "3", "4")]
        [InlineData("double", "3", "4")]
        public static async Task TestConstantCompositionAsync(string type, string min, string max)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethod([ConstantExpected] {type} constant)
    {{
        TestMethodWithConstant(constant);
    }}
    public static void TestMethodWithConstant([ConstantExpected] {type} val)
    {{
    }}
    public static void TestMethodConstrained([ConstantExpected(Min = {min}, Max = {max})] {type} constant)
    {{
        TestMethodWithConstant(constant);
        TestMethodWithConstrainedConstant(constant);
    }}
    public static void TestMethodWithConstrainedConstant([ConstantExpected(Min = {min}, Max = {max})] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput);
        }

        [Fact]
        public static async Task TestConstantCompositionStringAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod([ConstantExpected] string constant)
    {
        TestMethodWithConstant(constant);
    }
    public static void TestMethodWithConstant([ConstantExpected] string val)
    {
    }
}
";
            await TestCSAsync(csInput);
        }

        [Theory]
        [InlineData("char", "'B'", "'C'", "'D'")]
        [InlineData("sbyte", "3", "4", "5")]
        [InlineData("short", "3", "4", "5")]
        [InlineData("int", "3", "4", "5")]
        [InlineData("long", "3", "4", "5")]
        [InlineData("nint", "3", "4", "5")]
        [InlineData("byte", "3", "4", "5")]
        [InlineData("ushort", "3", "4", "5")]
        [InlineData("uint", "3", "4", "5")]
        [InlineData("ulong", "3", "4", "5")]
        [InlineData("nuint", "3", "4", "5")]
        [InlineData("float", "3", "4", "5")]
        [InlineData("double", "3", "4", "5")]
        public static async Task TestConstantCompositionOutOfBoundsAsync(string type, string min, string max, string outOfBoundMax)
        {
            string csInput = @$"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{{
    public static void TestMethodConstrained([{{|#0:ConstantExpected(Min = {min}, Max = {outOfBoundMax})|}}] {type} constant)
    {{
        TestMethodWithConstrainedConstant(constant);
    }}
    public static void TestMethodWithConstrainedConstant([ConstantExpected(Min = {min}, Max = {max})] {type} val)
    {{
    }}
}}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeOutOfBoundsRule)
                        .WithLocation(0)
                        .WithArguments(min.Trim('\''), max.Trim('\'')));
        }

        [Fact]
        public static async Task TestConstantCompositionNotSameTypeAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod([{|#0:ConstantExpected|}] long constant)
    {
        TestMethodWithConstant((int)constant);
    }
    public static void TestMethod([{|#1:ConstantExpected|}] short constant)
    {
        TestMethodWithConstant(constant);
    }
    public static void TestMethodWithConstant([ConstantExpected] int val)
    {
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeNotSameTypeRule)
                        .WithLocation(0)
                        .WithArguments("int"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeNotSameTypeRule)
                        .WithLocation(1)
                        .WithArguments("int"));
        }

        private static async Task TestCSAsync(string source, params DiagnosticResult[] diagnosticResults)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                LanguageVersion = CSharp.LanguageVersion.Preview,
            };
            // TODO: remove when the type is avaiable
            test.TestState.Sources.Add(s_attributeSource);
            test.ExpectedDiagnostics.AddRange(diagnosticResults);
            await test.RunAsync();
        }

        private static readonly string s_attributeSource = @"#nullable enable
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ConstantExpectedAttribute : Attribute
    {
        public object? Min { get; set; }
        public object? Max { get; set; }
    }
}";
    }
}

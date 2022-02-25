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
        [InlineData("byte", "byte.MinValue", "byte.MaxValue")]
        [InlineData("ushort", "ushort.MinValue", "ushort.MaxValue")]
        [InlineData("uint", "uint.MinValue", "uint.MaxValue")]
        [InlineData("ulong", "ulong.MinValue", "ulong.MaxValue")]
        [InlineData("nuint", "uint.MinValue", "uint.MaxValue")]
        [InlineData("sbyte", "sbyte.MinValue", "sbyte.MaxValue")]
        [InlineData("short", "short.MinValue", "short.MaxValue")]
        [InlineData("int", "int.MinValue", "int.MaxValue")]
        [InlineData("long", "long.MinValue", "long.MaxValue")]
        [InlineData("nint", "int.MinValue", "int.MaxValue")]
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

        [Fact]
        public static async Task TestConstantExpectedUnsupportedTypesAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethodObject([{|#0:ConstantExpected|}] object val)
    {
    }
    public static void TestMethodCustomClass([{|#1:ConstantExpected|}] Test val)
    {
    }
    public static void TestMethodDecimal([{|#2:ConstantExpected|}] decimal val)
    {
    }
    public static void TestMethodByteArray([{|#3:ConstantExpected|}] byte[] val)
    {
    }
    public static void TestMethodGenericArray<T>([{|#4:ConstantExpected|}] T[] val)
    {
    }
    public static void TestMethodValueTuple([{|#5:ConstantExpected|}] ValueTuple<int, long> val)
    {
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(0)
                        .WithArguments("object"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(1)
                        .WithArguments("Test"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(2)
                        .WithArguments("decimal"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(3)
                        .WithArguments("byte[]"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(4)
                        .WithArguments("T[]"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidTypeRule)
                        .WithLocation(5)
                        .WithArguments("(int, long)"));
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

        [Fact]
        public static async Task TestConstantExpectedIncompatibleConstantMinMaxTypeErrorAsync()
        {
            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethodString1([{|#0:ConstantExpected(Min = true)|}] string val)
    {
    }
    public static void TestMethodString2([{|#1:ConstantExpected(Min = true, Max = 5f)|}] string val)
    {
    }
    public static void TestMethodString3([{|#2:ConstantExpected(Max = 10.0)|}] string val)
    {
    }

    public static void TestMethodGeneric1<T>([{|#3:ConstantExpected(Min = ""min"")|}] T val)
    {
    }    
    public static void TestMethodGeneric2<T>([{|#4:ConstantExpected(Min = ""min"", Max = '1')|}] T val)
    {
    }    
    public static void TestMethodGeneric3<T>([{|#5:ConstantExpected(Max = ulong.MaxValue)|}] T val)
    {
    }    
    public static class GenenricClass<T>
    {
        public static void TestMethodGeneric1([{|#6:ConstantExpected(Min = ""min"")|}] T val)
        {
        }
        public static void TestMethodGeneric2([{|#7:ConstantExpected(Min = ""min"", Max = ""a"")|}] T val)
        {
        }
        public static void TestMethodGeneric3([{|#8:ConstantExpected(Max = ""a"")|}] T val)
        {
        }
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(0)
                        .WithArguments("string"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(1)
                        .WithArguments("string"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(2)
                        .WithArguments("string"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(3)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(4)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(5)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(6)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(7)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(8)
                        .WithArguments("generic"));
        }

        [Fact]
        public static async Task TestConstantExpectedInvalidBoundsAsync()
        {
            string[][] setArray = {
                new[]
                {
                    "byte", byte.MinValue.ToString(), byte.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "ushort", ushort.MinValue.ToString(), ushort.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "uint", uint.MinValue.ToString(), uint.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "ulong", ulong.MinValue.ToString(), ulong.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "-1", "-1"
                },
                new[]
                {
                    "nuint", uint.MinValue.ToString(), uint.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "sbyte", sbyte.MinValue.ToString(), sbyte.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "short", short.MinValue.ToString(), short.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "int", int.MinValue.ToString(), int.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "long", long.MinValue.ToString(), long.MaxValue.ToString(),
                    "ulong.MaxValue", "ulong.MaxValue", "ulong.MaxValue", "ulong.MaxValue"
                },
                new[]
                {
                    "nint", int.MinValue.ToString(), int.MaxValue.ToString(),
                    "long.MinValue", "long.MinValue", "long.MaxValue", "long.MaxValue"
                },
                new[]
                {
                    "float", float.MinValue.ToString(), float.MaxValue.ToString(),
                    "double.MinValue", "double.MinValue", "double.MaxValue", "double.MaxValue"
                }
            };

            foreach (string[] set in setArray)
            {
                await TestTheoryAsync(set[0], set[1], set[2], set[3], set[4], set[5], set[6]);
            }

            static async Task TestTheoryAsync(string type, string min, string max, string min1, string min2, string max2,
                string max3)
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
                        .WithArguments("Min", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(1)
                        .WithArguments("Min", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(2)
                        .WithArguments("Max", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(3)
                        .WithArguments("Max", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(4)
                        .WithArguments("Min", type),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(5)
                        .WithArguments("Max", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(6)
                        .WithArguments("Min", min, max),
                    VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(7)
                        .WithArguments("Max", type));
            }
        }

        [Theory]
        [InlineData("char", "'A'", "'Z'", "'A'", "(char)('A'+'\\u0001')")]
        [InlineData("byte", "10", "20", "10", "2*5")]
        [InlineData("ushort", "10", "20", "10", "2*5")]
        [InlineData("uint", "10", "20", "10", "2*5")]
        [InlineData("ulong", "10", "20", "10", "2*5")]
        [InlineData("nuint", "10", "20", "10", "2*5")]
        [InlineData("sbyte", "10", "20", "10", "2*5")]
        [InlineData("short", "10", "20", "10", "2*5")]
        [InlineData("int", "10", "20", "10", "2*5")]
        [InlineData("long", "10", "20", "10", "2*5")]
        [InlineData("nint", "10", "20", "10", "2*5")]
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

        [Fact]
        public static async Task TestArgumentNotConstantAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod(int nonConstant)
    {
        TestMethodWithConstant({|#0:nonConstant|});
        TestMethodGeneric<int>({|#1:nonConstant|});
        GenenricClass<int>.TestMethodGeneric({|#2:nonConstant|});
    }
    public static void TestMethodWithConstant([ConstantExpected] int val)
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
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(0),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(1),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                        .WithLocation(2));
        }

        [Fact]
        public static async Task TestArgumentStringNotConstantAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod(string nonConstant)
    {
        TestMethodWithConstant({|#0:nonConstant|});
        TestMethodGeneric<string>({|#1:nonConstant|});
        GenenricClass<string>.TestMethodGeneric({|#2:nonConstant|});
    }
    public static void TestMethodWithConstant([ConstantExpected] string val)
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
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                    .WithLocation(0),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                    .WithLocation(1),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantNotConstantRule)
                    .WithLocation(2));
        }

        [Fact]
        public static async Task TestArgumentOutOfRangeConstantAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod()
    {
        TestMethodWithConstant({|#0:11|});
    }
    public static void TestMethodWithConstant([ConstantExpected(Min=0, Max=10)] int val)
    {
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.ConstantOutOfBoundsRule)
                        .WithLocation(0)
                        .WithArguments("0", "10"));
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

        [Fact]
        public static async Task TestConstantCompositionAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod([ConstantExpected] int constant)
    {
        TestMethodWithConstant(constant);
    }
    public static void TestMethodWithConstant([ConstantExpected] int val)
    {
    }
    public static void TestMethod2([ConstantExpected(Min = 10, Max = 20)] int constant)
    {
        TestMethodWithConstant(constant);
        TestMethod2WithConstrainedConstant(constant);
    }
    public static void TestMethod2WithConstant([ConstantExpected] int val)
    {
    }
    public static void TestMethod2WithConstrainedConstant([ConstantExpected(Min = 10, Max = 20)] int val)
    {
    }
}
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

        [Fact]
        public static async Task TestConstantCompositionOutOfRangeAsync()
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
    public static void TestMethod2([{|#2:ConstantExpected(Min = 10, Max = 21)|}] int constant)
    {
        TestMethod2WithConstrainedConstant(constant);
    }
    public static void TestMethod2WithConstrainedConstant([ConstantExpected(Min = 10, Max = 20)] int val)
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
                        .WithArguments("int"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.AttributeOutOfBoundsRule)
                        .WithLocation(2)
                        .WithArguments("10", "20"));
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

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
        [Fact]
        public static async Task TestConstantExpectedSupportedTypesAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethodByte([ConstantExpected] byte val)
    {
    }
    public static void TestMethodSByte([ConstantExpected] sbyte val)
    {
    }
    public static void TestMethodUInt16([ConstantExpected] ushort val)
    {
    }
    public static void TestMethodInt16([ConstantExpected] short val)
    {
    }
    public static void TestMethodUInt32([ConstantExpected] uint val)
    {
    }
    public static void TestMethodInt32([ConstantExpected] int val)
    {
    }
    public static void TestMethodUInt64([ConstantExpected] ulong val)
    {
    }
    public static void TestMethodInt64([ConstantExpected] long val)
    {
    }
    public static void TestMethodFloat([ConstantExpected] float val)
    {
    }
    public static void TestMethodDouble([ConstantExpected] double val)
    {
    }
    public static void TestMethodBoolean([ConstantExpected] bool val)
    {
    }
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

        [Fact]
        public static async Task TestConstantExpectedIncompatibleConstantTypeErrorAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod([ConstantExpected({|#0:Min = ""min""|})] int val)
    {
    }
    public static void TestMethod2([ConstantExpected({|#1:Min = ""min""|}, {|#2:Max = ""a""|})] int val)
    {
    }
    public static void TestMethod3([ConstantExpected({|#3:Max = true|})] short val)
    {
    }
    public static void TestMethodString([{|#4:ConstantExpected(Max = true)|}] string val)
    {
    }
    public static void TestMethodGeneric<T>([{|#5:ConstantExpected(Min = ""min"", Max = ""a"")|}] T val)
    {
    }
    
    public static class GenenricClass<T>
    {
        public static void TestMethodGeneric([{|#6:ConstantExpected(Min = ""min"", Max = ""a"")|}] T val)
        {
        }
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(0)
                        .WithArguments("Min", "int"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(1)
                        .WithArguments("Min", "int"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(2)
                        .WithArguments("Max", "int"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantTypeRule)
                        .WithLocation(3)
                        .WithArguments("Max", "short"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(4)
                        .WithArguments("string"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(5)
                        .WithArguments("generic"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.IncompatibleConstantForMinMaxRule)
                        .WithLocation(6)
                        .WithArguments("generic"));
        }

        [Fact]
        public static async Task TestConstantExpectedInvalidTypeRangeValueAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod([ConstantExpected({|#0:Min = 256|})] byte val)
    {
    }
    public static void TestMethod2([ConstantExpected({|#1:Min = -256|}, {|#2:Max = 256|})] byte val)
    {
    }
    public static void TestMethod3([ConstantExpected({|#3:Min = double.MinValue|}, {|#4:Max = double.MaxValue|})] float val)
    {
    }
}
";
            await TestCSAsync(csInput,
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(0)
                        .WithArguments("Min", "0", "255"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(1)
                        .WithArguments("Min", "0", "255"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(2)
                        .WithArguments("Max", "0", "255"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(3)
                        .WithArguments("Min", "-3.4028235E+38", "3.4028235E+38"),
                VerifyCS.Diagnostic(ConstantExpectedAnalyzer.InvalidBoundsRule)
                        .WithLocation(4)
                        .WithArguments("Max", "-3.4028235E+38", "3.4028235E+38"));
        }

        [Fact]
        public static async Task TestArgumentConstantAsync()
        {

            string csInput = @"
using System;
using System.Diagnostics.CodeAnalysis;
#nullable enable

public class Test
{
    public static void TestMethod()
    {
        TestMethodWithConstant(10);
        TestMethodWithConstrainedConstant(10);
        TestMethodWithConstrainedConstant(2*5);
        TestMethodGeneric<int>(10);
        GenenricClass<int>.TestMethodGeneric(10);
    }
    public static void TestMethodWithConstant([ConstantExpected] int val)
    {
    }
    public static void TestMethodWithConstrainedConstant([ConstantExpected(Min = 10, Max = 20)] int val)
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
        public static async Task TestArgumentInvalidGenericTypeParamterConstantAsync()
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

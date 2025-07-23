// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpUseCrossPlatformIntrinsicsAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpUseCrossPlatformIntrinsicsFixer>;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    using static UseCrossPlatformIntrinsicsAnalyzer;

    public partial class CSharpUseCrossPlatformIntrinsicsTests
    {
        [Fact]
        public async Task Fixer_opRightShiftArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => {|#1:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => {|#2:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => {|#3:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => {|#4:AdvSimd.ShiftRightArithmeticScalar(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => x >> y;
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => x >> y;
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => x >> y;
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(4),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opRightShiftArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:AdvSimd.ShiftRightArithmetic(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#4:AdvSimd.ShiftRightArithmetic(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >> y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(4),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opRightShiftWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:PackedSimd.ShiftRightArithmetic(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:PackedSimd.ShiftRightArithmetic(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:PackedSimd.ShiftRightArithmetic(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#4:PackedSimd.ShiftRightArithmetic(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >> y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(4),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opRightShiftx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Sse2.ShiftRightArithmetic(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#2:Sse2.ShiftRightArithmetic(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#3:Avx512F.VL.ShiftRightArithmetic(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opRightShiftx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx2.ShiftRightArithmetic(x, y)|};
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#2:Avx2.ShiftRightArithmetic(x, y)|};
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#3:Avx512F.VL.ShiftRightArithmetic(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >> y;
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >> y;
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opRightShiftx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx512BW.ShiftRightArithmetic(x, y)|};
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#2:Avx512F.ShiftRightArithmetic(x, y)|};
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#3:Avx512F.ShiftRightArithmetic(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >> y;
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >> y;
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opRightShift]).WithLocation(3),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
    }
}

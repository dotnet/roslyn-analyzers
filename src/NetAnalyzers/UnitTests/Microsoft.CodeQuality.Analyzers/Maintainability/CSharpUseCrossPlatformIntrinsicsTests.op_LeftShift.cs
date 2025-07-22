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

    public class CSharpUseCrossPlatformIntrinsicsTests
    {
        [Fact]
        public async Task Fixer_opLeftShiftArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => {|#2:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => {|#3:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, [ConstantExpected(Max = (byte)(15))] byte  y) => {|#4:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => {|#5:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<uint> M(Vector64<uint> x, [ConstantExpected(Max = (byte)(31))] byte  y) => {|#6:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => {|#7:AdvSimd.ShiftLeftLogicalScalar(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, [ConstantExpected(Max = (byte)(63))] byte  y) => {|#8:AdvSimd.ShiftLeftLogicalScalar(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x << y;
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => x << y;
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => x << y;
    Vector64<ushort> M(Vector64<ushort> x, [ConstantExpected(Max = (byte)(15))] byte  y) => x << y;
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => x << y;
    Vector64<uint> M(Vector64<uint> x, [ConstantExpected(Max = (byte)(31))] byte  y) => x << y;
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => x << y;
    Vector64<ulong> M(Vector64<ulong> x, [ConstantExpected(Max = (byte)(63))] byte  y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opLeftShiftArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#2:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#3:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#4:AdvSimd.ShiftLeftLogical(x, y)|};
    // The overload for Vector128<int> doesn't exist
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#6:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#7:AdvSimd.ShiftLeftLogical(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#8:AdvSimd.ShiftLeftLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x << y;
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x << y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    // The overload for Vector128<int> doesn't exist
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    // The overload for Vector128<int> doesn't exist
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opLeftShiftWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:PackedSimd.ShiftLeft(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#2:PackedSimd.ShiftLeft(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#3:PackedSimd.ShiftLeft(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#4:PackedSimd.ShiftLeft(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#5:PackedSimd.ShiftLeft(x, y)|};
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#6:PackedSimd.ShiftLeft(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#7:PackedSimd.ShiftLeft(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#8:PackedSimd.ShiftLeft(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x << y;
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x << y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opLeftShiftx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Sse2.ShiftLeftLogical(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Sse2.ShiftLeftLogical(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Sse2.ShiftLeftLogical(x, y)|};
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Sse2.ShiftLeftLogical(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Sse2.ShiftLeftLogical(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Sse2.ShiftLeftLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opLeftShiftx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx2.ShiftLeftLogical(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Avx2.ShiftLeftLogical(x, y)|};
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Avx2.ShiftLeftLogical(x, y)|};
    Vector256<uint> M(Vector256<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Avx2.ShiftLeftLogical(x, y)|};
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Avx2.ShiftLeftLogical(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Avx2.ShiftLeftLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector256<ushort> M(Vector256<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector256<uint> M(Vector256<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
    Vector256<ulong> M(Vector256<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opLeftShiftx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx512BW.ShiftLeftLogical(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Avx512BW.ShiftLeftLogical(x, y)|};
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Avx512F.ShiftLeftLogical(x, y)|};
    Vector512<uint> M(Vector512<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Avx512F.ShiftLeftLogical(x, y)|};
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Avx512F.ShiftLeftLogical(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Avx512F.ShiftLeftLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector512<ushort> M(Vector512<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x << y;
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector512<uint> M(Vector512<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x << y;
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
    Vector512<ulong> M(Vector512<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x << y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opLeftShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
    }
}

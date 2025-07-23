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
        public async Task Fixer_opUnsignedRightShiftArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => {|#2:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => {|#3:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, [ConstantExpected(Max = (byte)(15))] byte  y) => {|#4:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => {|#5:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<uint> M(Vector64<uint> x, [ConstantExpected(Max = (byte)(31))] byte  y) => {|#6:AdvSimd.ShiftRightLogical(x, y)|};
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => {|#7:AdvSimd.ShiftRightLogicalScalar(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, [ConstantExpected(Max = (byte)(63))] byte  y) => {|#8:AdvSimd.ShiftRightLogicalScalar(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >>> y;
    Vector64<sbyte> M(Vector64<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte  y) => x >>> y;
    Vector64<short> M(Vector64<short> x, [ConstantExpected(Max = (byte)(15))] byte  y) => x >>> y;
    Vector64<ushort> M(Vector64<ushort> x, [ConstantExpected(Max = (byte)(15))] byte  y) => x >>> y;
    Vector64<int> M(Vector64<int> x, [ConstantExpected(Max = (byte)(31))] byte  y) => x >>> y;
    Vector64<uint> M(Vector64<uint> x, [ConstantExpected(Max = (byte)(31))] byte  y) => x >>> y;
    Vector64<long> M(Vector64<long> x, [ConstantExpected(Max = (byte)(63))] byte  y) => x >>> y;
    Vector64<ulong> M(Vector64<ulong> x, [ConstantExpected(Max = (byte)(63))] byte  y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnsignedRightShiftArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#2:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#3:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#4:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#5:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#6:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#7:AdvSimd.ShiftRightLogical(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#8:AdvSimd.ShiftRightLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >>> y;
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >>> y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnsignedRightShiftWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#1:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => {|#2:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#3:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#4:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#5:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#6:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#7:PackedSimd.ShiftRightLogical(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#8:PackedSimd.ShiftRightLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >>> y;
    Vector128<sbyte> M(Vector128<sbyte> x, [ConstantExpected(Max = (byte)(7))] byte y) => x >>> y;
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnsignedRightShiftx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Sse2.ShiftRightLogical(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Sse2.ShiftRightLogical(x, y)|};
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Sse2.ShiftRightLogical(x, y)|};
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Sse2.ShiftRightLogical(x, y)|};
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Sse2.ShiftRightLogical(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Sse2.ShiftRightLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<ushort> M(Vector128<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector128<int> M(Vector128<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<uint> M(Vector128<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector128<long> M(Vector128<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
    Vector128<ulong> M(Vector128<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnsignedRightShiftx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx2.ShiftRightLogical(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Avx2.ShiftRightLogical(x, y)|};
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Avx2.ShiftRightLogical(x, y)|};
    Vector256<uint> M(Vector256<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Avx2.ShiftRightLogical(x, y)|};
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Avx2.ShiftRightLogical(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Avx2.ShiftRightLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector256<ushort> M(Vector256<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector256<int> M(Vector256<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector256<uint> M(Vector256<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector256<long> M(Vector256<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
    Vector256<ulong> M(Vector256<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnsignedRightShiftx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#1:Avx512BW.ShiftRightLogical(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => {|#2:Avx512BW.ShiftRightLogical(x, y)|};
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#3:Avx512F.ShiftRightLogical(x, y)|};
    Vector512<uint> M(Vector512<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => {|#4:Avx512F.ShiftRightLogical(x, y)|};
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#5:Avx512F.ShiftRightLogical(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => {|#6:Avx512F.ShiftRightLogical(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector512<ushort> M(Vector512<ushort> x, [ConstantExpected(Max = (byte)(15))] byte y) => x >>> y;
    Vector512<int> M(Vector512<int> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector512<uint> M(Vector512<uint> x, [ConstantExpected(Max = (byte)(31))] byte y) => x >>> y;
    Vector512<long> M(Vector512<long> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
    Vector512<ulong> M(Vector512<ulong> x, [ConstantExpected(Max = (byte)(63))] byte y) => x >>> y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnsignedRightShift]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            }.RunAsync();
        }
    }
}

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
        public async Task Fixer_opMultiplyArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.Multiply(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.Multiply(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.Multiply(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.Multiply(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.Multiply(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.Multiply(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#7:AdvSimd.Multiply(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#8:AdvSimd.MultiplyScalar(x, y)|};

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.MultiplyScalar(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x * y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x * y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x * y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x * y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x * y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x * y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x * y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x * y;

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.MultiplyScalar(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opMultiplyArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.Multiply(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.Multiply(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.Multiply(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.Multiply(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.Multiply(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.Multiply(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#7:AdvSimd.Multiply(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#8:AdvSimd.Arm64.Multiply(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x * y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x * y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x * y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x * y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x * y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x * y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x * y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x * y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opMultiplyWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#1:PackedSimd.Multiply(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#2:PackedSimd.Multiply(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#3:PackedSimd.Multiply(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#4:PackedSimd.Multiply(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#5:PackedSimd.Multiply(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#6:PackedSimd.Multiply(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#7:PackedSimd.Multiply(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#8:PackedSimd.Multiply(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x * y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x * y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x * y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x * y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x * y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x * y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x * y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x * y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opMultiplyx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#1:Sse2.MultiplyLow(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#2:Sse2.MultiplyLow(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#3:Sse41.MultiplyLow(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#4:Sse41.MultiplyLow(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#5:Avx512DQ.VL.MultiplyLow(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#6:Avx512DQ.VL.MultiplyLow(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#7:Sse.Multiply(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#8:Sse2.Multiply(x, y)|};

    Vector128<long> N(Vector128<int> x, Vector128<int> y) => Sse41.Multiply(x, y);
    Vector128<ulong> N(Vector128<uint> x, Vector128<uint> y) => Sse2.Multiply(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x * y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x * y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x * y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x * y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x * y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x * y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x * y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x * y;

    Vector128<long> N(Vector128<int> x, Vector128<int> y) => Sse41.Multiply(x, y);
    Vector128<ulong> N(Vector128<uint> x, Vector128<uint> y) => Sse2.Multiply(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opMultiplyx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#1:Avx2.MultiplyLow(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#2:Avx2.MultiplyLow(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#3:Avx2.MultiplyLow(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#4:Avx2.MultiplyLow(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#5:Avx512DQ.VL.MultiplyLow(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#6:Avx512DQ.VL.MultiplyLow(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#7:Avx.Multiply(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#8:Avx.Multiply(x, y)|};

    Vector256<long> N(Vector256<int> x, Vector256<int> y) => Avx2.Multiply(x, y);
    Vector256<ulong> N(Vector256<uint> x, Vector256<uint> y) => Avx2.Multiply(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x * y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x * y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x * y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x * y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x * y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x * y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x * y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x * y;

    Vector256<long> N(Vector256<int> x, Vector256<int> y) => Avx2.Multiply(x, y);
    Vector256<ulong> N(Vector256<uint> x, Vector256<uint> y) => Avx2.Multiply(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opMultiplyx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#1:Avx512BW.MultiplyLow(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#2:Avx512BW.MultiplyLow(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#3:Avx512F.MultiplyLow(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#4:Avx512F.MultiplyLow(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#5:Avx512DQ.MultiplyLow(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#6:Avx512DQ.MultiplyLow(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#7:Avx512F.Multiply(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#8:Avx512F.Multiply(x, y)|};

    Vector512<long> N(Vector512<int> x, Vector512<int> y) => Avx512F.Multiply(x, y);
    Vector512<ulong> N(Vector512<uint> x, Vector512<uint> y) => Avx512F.Multiply(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x * y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x * y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x * y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x * y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x * y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x * y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x * y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x * y;

    Vector512<long> N(Vector512<int> x, Vector512<int> y) => Avx512F.Multiply(x, y);
    Vector512<ulong> N(Vector512<uint> x, Vector512<uint> y) => Avx512F.Multiply(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opMultiply]).WithLocation(8),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
    }
}

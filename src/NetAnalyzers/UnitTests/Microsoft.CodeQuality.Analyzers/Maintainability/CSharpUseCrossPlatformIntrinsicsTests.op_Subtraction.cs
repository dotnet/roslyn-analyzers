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
        public async Task Fixer_opSubtractionArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.Subtract(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.Subtract(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.Subtract(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.Subtract(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.Subtract(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.Subtract(x, y)|};
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => {|#7:AdvSimd.SubtractScalar(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => {|#8:AdvSimd.SubtractScalar(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#9:AdvSimd.Subtract(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#10:AdvSimd.SubtractScalar(x, y)|};

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.SubtractScalar(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x - y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x - y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x - y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x - y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x - y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x - y;
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => x - y;
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => x - y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x - y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x - y;

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.SubtractScalar(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opSubtractionArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.Subtract(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.Subtract(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.Subtract(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.Subtract(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.Subtract(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.Subtract(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:AdvSimd.Subtract(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:AdvSimd.Subtract(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:AdvSimd.Subtract(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:AdvSimd.Arm64.Subtract(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x - y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x - y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x - y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x - y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x - y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x - y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x - y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x - y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x - y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x - y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opSubtractionWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:PackedSimd.Subtract(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:PackedSimd.Subtract(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:PackedSimd.Subtract(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:PackedSimd.Subtract(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:PackedSimd.Subtract(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:PackedSimd.Subtract(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:PackedSimd.Subtract(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:PackedSimd.Subtract(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:PackedSimd.Subtract(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:PackedSimd.Subtract(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x - y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x - y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x - y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x - y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x - y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x - y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x - y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x - y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x - y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x - y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opSubtractionx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:Sse2.Subtract(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:Sse2.Subtract(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:Sse2.Subtract(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:Sse2.Subtract(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:Sse2.Subtract(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:Sse2.Subtract(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:Sse2.Subtract(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:Sse2.Subtract(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:Sse.Subtract(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:Sse2.Subtract(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x - y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x - y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x - y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x - y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x - y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x - y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x - y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x - y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x - y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x - y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opSubtractionx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => {|#1:Avx2.Subtract(x, y)|};
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => {|#2:Avx2.Subtract(x, y)|};
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#3:Avx2.Subtract(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#4:Avx2.Subtract(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#5:Avx2.Subtract(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#6:Avx2.Subtract(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#7:Avx2.Subtract(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#8:Avx2.Subtract(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#9:Avx.Subtract(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#10:Avx.Subtract(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => x - y;
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => x - y;
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x - y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x - y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x - y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x - y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x - y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x - y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x - y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x - y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opSubtractionx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => {|#1:Avx512BW.Subtract(x, y)|};
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => {|#2:Avx512BW.Subtract(x, y)|};
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#3:Avx512BW.Subtract(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#4:Avx512BW.Subtract(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#5:Avx512F.Subtract(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#6:Avx512F.Subtract(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#7:Avx512F.Subtract(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#8:Avx512F.Subtract(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#9:Avx512F.Subtract(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#10:Avx512F.Subtract(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => x - y;
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => x - y;
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x - y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x - y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x - y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x - y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x - y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x - y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x - y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x - y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opSubtraction]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
    }
}

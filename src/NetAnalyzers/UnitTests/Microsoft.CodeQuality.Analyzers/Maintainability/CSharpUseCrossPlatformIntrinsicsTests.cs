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
        #region op_Addition
        [Fact]
        public async Task Fixer_opAdditionArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.Add(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.Add(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.Add(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.Add(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.Add(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.Add(x, y)|};
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => {|#7:AdvSimd.AddScalar(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => {|#8:AdvSimd.AddScalar(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#9:AdvSimd.Add(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#10:AdvSimd.AddScalar(x, y)|};

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.AddScalar(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x + y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x + y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x + y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x + y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x + y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x + y;
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => x + y;
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => x + y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x + y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x + y;

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.AddScalar(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opAdditionArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.Add(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.Add(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.Add(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.Add(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.Add(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.Add(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:AdvSimd.Add(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:AdvSimd.Add(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:AdvSimd.Add(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:AdvSimd.Arm64.Add(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x + y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x + y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x + y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x + y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x + y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x + y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x + y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x + y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x + y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x + y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opAdditionWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:PackedSimd.Add(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:PackedSimd.Add(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:PackedSimd.Add(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:PackedSimd.Add(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:PackedSimd.Add(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:PackedSimd.Add(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:PackedSimd.Add(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:PackedSimd.Add(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:PackedSimd.Add(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:PackedSimd.Add(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x + y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x + y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x + y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x + y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x + y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x + y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x + y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x + y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x + y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x + y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opAdditionx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:Sse2.Add(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:Sse2.Add(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:Sse2.Add(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:Sse2.Add(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:Sse2.Add(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:Sse2.Add(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:Sse2.Add(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:Sse2.Add(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:Sse.Add(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:Sse2.Add(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x + y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x + y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x + y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x + y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x + y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x + y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x + y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x + y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x + y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x + y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opAdditionx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => {|#1:Avx2.Add(x, y)|};
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => {|#2:Avx2.Add(x, y)|};
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#3:Avx2.Add(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#4:Avx2.Add(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#5:Avx2.Add(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#6:Avx2.Add(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#7:Avx2.Add(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#8:Avx2.Add(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#9:Avx.Add(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#10:Avx.Add(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => x + y;
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => x + y;
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x + y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x + y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x + y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x + y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x + y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x + y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x + y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x + y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opAdditionx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => {|#1:Avx512BW.Add(x, y)|};
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => {|#2:Avx512BW.Add(x, y)|};
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#3:Avx512BW.Add(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#4:Avx512BW.Add(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#5:Avx512F.Add(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#6:Avx512F.Add(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#7:Avx512F.Add(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#8:Avx512F.Add(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#9:Avx512F.Add(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#10:Avx512F.Add(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => x + y;
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => x + y;
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x + y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x + y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x + y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x + y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x + y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x + y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x + y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x + y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opAddition]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_BitwiseAnd
        [Fact]
        public async Task Fixer_opBitwiseAndArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.And(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.And(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.And(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.And(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.And(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.And(x, y)|};
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => {|#7:AdvSimd.And(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => {|#8:AdvSimd.And(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#9:AdvSimd.And(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#10:AdvSimd.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x & y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x & y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x & y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x & y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x & y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x & y;
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => x & y;
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => x & y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x & y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseAndArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.And(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.And(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.And(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.And(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.And(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.And(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:AdvSimd.And(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:AdvSimd.And(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:AdvSimd.And(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:AdvSimd.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x & y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x & y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x & y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x & y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x & y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x & y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x & y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x & y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x & y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseAndWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:PackedSimd.And(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:PackedSimd.And(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:PackedSimd.And(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:PackedSimd.And(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:PackedSimd.And(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:PackedSimd.And(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:PackedSimd.And(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:PackedSimd.And(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:PackedSimd.And(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:PackedSimd.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x & y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x & y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x & y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x & y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x & y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x & y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x & y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x & y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x & y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseAndx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:Sse2.And(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:Sse2.And(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:Sse2.And(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:Sse2.And(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:Sse2.And(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:Sse2.And(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:Sse2.And(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:Sse2.And(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:Sse.And(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:Sse2.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x & y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x & y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x & y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x & y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x & y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x & y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x & y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x & y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x & y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseAndx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => {|#1:Avx2.And(x, y)|};
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => {|#2:Avx2.And(x, y)|};
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#3:Avx2.And(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#4:Avx2.And(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#5:Avx2.And(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#6:Avx2.And(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#7:Avx2.And(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#8:Avx2.And(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#9:Avx.And(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#10:Avx.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => x & y;
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => x & y;
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x & y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x & y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x & y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x & y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x & y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x & y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x & y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseAndx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => {|#1:Avx512F.And(x, y)|};
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => {|#2:Avx512F.And(x, y)|};
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#3:Avx512F.And(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#4:Avx512F.And(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#5:Avx512F.And(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#6:Avx512F.And(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#7:Avx512F.And(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#8:Avx512F.And(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#9:Avx512DQ.And(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#10:Avx512DQ.And(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => x & y;
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => x & y;
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x & y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x & y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x & y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x & y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x & y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x & y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x & y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x & y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseAnd]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_BitwiseOr
        [Fact]
        public async Task Fixer_opBitwiseOrArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.Or(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.Or(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.Or(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.Or(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.Or(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.Or(x, y)|};
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => {|#7:AdvSimd.Or(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => {|#8:AdvSimd.Or(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#9:AdvSimd.Or(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#10:AdvSimd.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x | y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x | y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x | y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x | y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x | y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x | y;
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => x | y;
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => x | y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x | y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseOrArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.Or(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.Or(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.Or(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.Or(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.Or(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.Or(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:AdvSimd.Or(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:AdvSimd.Or(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:AdvSimd.Or(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:AdvSimd.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x | y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x | y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x | y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x | y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x | y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x | y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x | y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x | y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x | y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseOrWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:PackedSimd.Or(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:PackedSimd.Or(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:PackedSimd.Or(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:PackedSimd.Or(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:PackedSimd.Or(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:PackedSimd.Or(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:PackedSimd.Or(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:PackedSimd.Or(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:PackedSimd.Or(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:PackedSimd.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x | y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x | y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x | y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x | y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x | y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x | y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x | y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x | y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x | y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseOrx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:Sse2.Or(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:Sse2.Or(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:Sse2.Or(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:Sse2.Or(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:Sse2.Or(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:Sse2.Or(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:Sse2.Or(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:Sse2.Or(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:Sse.Or(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:Sse2.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x | y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x | y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x | y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x | y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x | y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x | y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x | y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x | y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x | y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseOrx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => {|#1:Avx2.Or(x, y)|};
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => {|#2:Avx2.Or(x, y)|};
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#3:Avx2.Or(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#4:Avx2.Or(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#5:Avx2.Or(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#6:Avx2.Or(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#7:Avx2.Or(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#8:Avx2.Or(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#9:Avx.Or(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#10:Avx.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => x | y;
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => x | y;
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x | y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x | y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x | y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x | y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x | y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x | y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x | y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opBitwiseOrx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => {|#1:Avx512F.Or(x, y)|};
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => {|#2:Avx512F.Or(x, y)|};
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#3:Avx512F.Or(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#4:Avx512F.Or(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#5:Avx512F.Or(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#6:Avx512F.Or(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#7:Avx512F.Or(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#8:Avx512F.Or(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#9:Avx512DQ.Or(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#10:Avx512DQ.Or(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => x | y;
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => x | y;
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x | y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x | y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x | y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x | y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x | y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x | y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x | y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x | y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opBitwiseOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_Division
        [Fact]
        public async Task Fixer_opDivisionArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#1:AdvSimd.Arm64.Divide(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#2:AdvSimd.DivideScalar(x, y)|};

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.DivideScalar(x, y);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x / y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x / y;

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.DivideScalar(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opDivisionArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#1:AdvSimd.Arm64.Divide(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#2:AdvSimd.Arm64.Divide(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opDivisionWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#1:PackedSimd.Divide(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#2:PackedSimd.Divide(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opDivisionx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#1:Sse.Divide(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#2:Sse2.Divide(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opDivisionx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#1:Avx.Divide(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#2:Avx.Divide(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x / y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opDivisionx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#1:Avx512F.Divide(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#2:Avx512F.Divide(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x / y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_ExclusiveOr
        [Fact]
        public async Task Fixer_opExclusiveOrArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => {|#1:AdvSimd.Xor(x, y)|};
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => {|#2:AdvSimd.Xor(x, y)|};
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => {|#3:AdvSimd.Xor(x, y)|};
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => {|#4:AdvSimd.Xor(x, y)|};
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => {|#5:AdvSimd.Xor(x, y)|};
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => {|#6:AdvSimd.Xor(x, y)|};
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => {|#7:AdvSimd.Xor(x, y)|};
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => {|#8:AdvSimd.Xor(x, y)|};
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => {|#9:AdvSimd.Xor(x, y)|};
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => {|#10:AdvSimd.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x, Vector64<byte> y) => x ^ y;
    Vector64<sbyte> M(Vector64<sbyte> x, Vector64<sbyte> y) => x ^ y;
    Vector64<short> M(Vector64<short> x, Vector64<short> y) => x ^ y;
    Vector64<ushort> M(Vector64<ushort> x, Vector64<ushort> y) => x ^ y;
    Vector64<int> M(Vector64<int> x, Vector64<int> y) => x ^ y;
    Vector64<uint> M(Vector64<uint> x, Vector64<uint> y) => x ^ y;
    Vector64<long> M(Vector64<long> x, Vector64<long> y) => x ^ y;
    Vector64<ulong> M(Vector64<ulong> x, Vector64<ulong> y) => x ^ y;
    Vector64<float> M(Vector64<float> x, Vector64<float> y) => x ^ y;
    Vector64<double> M(Vector64<double> x, Vector64<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opExclusiveOrArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:AdvSimd.Xor(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:AdvSimd.Xor(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:AdvSimd.Xor(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:AdvSimd.Xor(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:AdvSimd.Xor(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:AdvSimd.Xor(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:AdvSimd.Xor(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:AdvSimd.Xor(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:AdvSimd.Xor(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:AdvSimd.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x ^ y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x ^ y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x ^ y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x ^ y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x ^ y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x ^ y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x ^ y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x ^ y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x ^ y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opExclusiveOrWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:PackedSimd.Xor(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:PackedSimd.Xor(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:PackedSimd.Xor(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:PackedSimd.Xor(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:PackedSimd.Xor(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:PackedSimd.Xor(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:PackedSimd.Xor(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:PackedSimd.Xor(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:PackedSimd.Xor(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:PackedSimd.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x ^ y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x ^ y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x ^ y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x ^ y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x ^ y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x ^ y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x ^ y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x ^ y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x ^ y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opExclusiveOrx86V128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => {|#1:Sse2.Xor(x, y)|};
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => {|#2:Sse2.Xor(x, y)|};
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => {|#3:Sse2.Xor(x, y)|};
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => {|#4:Sse2.Xor(x, y)|};
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => {|#5:Sse2.Xor(x, y)|};
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => {|#6:Sse2.Xor(x, y)|};
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => {|#7:Sse2.Xor(x, y)|};
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => {|#8:Sse2.Xor(x, y)|};
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => {|#9:Sse.Xor(x, y)|};
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => {|#10:Sse2.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<byte> M(Vector128<byte> x, Vector128<byte> y) => x ^ y;
    Vector128<sbyte> M(Vector128<sbyte> x, Vector128<sbyte> y) => x ^ y;
    Vector128<short> M(Vector128<short> x, Vector128<short> y) => x ^ y;
    Vector128<ushort> M(Vector128<ushort> x, Vector128<ushort> y) => x ^ y;
    Vector128<int> M(Vector128<int> x, Vector128<int> y) => x ^ y;
    Vector128<uint> M(Vector128<uint> x, Vector128<uint> y) => x ^ y;
    Vector128<long> M(Vector128<long> x, Vector128<long> y) => x ^ y;
    Vector128<ulong> M(Vector128<ulong> x, Vector128<ulong> y) => x ^ y;
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x ^ y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opExclusiveOrx86V256Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => {|#1:Avx2.Xor(x, y)|};
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => {|#2:Avx2.Xor(x, y)|};
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => {|#3:Avx2.Xor(x, y)|};
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => {|#4:Avx2.Xor(x, y)|};
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => {|#5:Avx2.Xor(x, y)|};
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => {|#6:Avx2.Xor(x, y)|};
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => {|#7:Avx2.Xor(x, y)|};
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => {|#8:Avx2.Xor(x, y)|};
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => {|#9:Avx.Xor(x, y)|};
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => {|#10:Avx.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<byte> M(Vector256<byte> x, Vector256<byte> y) => x ^ y;
    Vector256<sbyte> M(Vector256<sbyte> x, Vector256<sbyte> y) => x ^ y;
    Vector256<short> M(Vector256<short> x, Vector256<short> y) => x ^ y;
    Vector256<ushort> M(Vector256<ushort> x, Vector256<ushort> y) => x ^ y;
    Vector256<int> M(Vector256<int> x, Vector256<int> y) => x ^ y;
    Vector256<uint> M(Vector256<uint> x, Vector256<uint> y) => x ^ y;
    Vector256<long> M(Vector256<long> x, Vector256<long> y) => x ^ y;
    Vector256<ulong> M(Vector256<ulong> x, Vector256<ulong> y) => x ^ y;
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x ^ y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opExclusiveOrx86V512Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => {|#1:Avx512F.Xor(x, y)|};
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => {|#2:Avx512F.Xor(x, y)|};
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => {|#3:Avx512F.Xor(x, y)|};
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => {|#4:Avx512F.Xor(x, y)|};
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => {|#5:Avx512F.Xor(x, y)|};
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => {|#6:Avx512F.Xor(x, y)|};
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => {|#7:Avx512F.Xor(x, y)|};
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => {|#8:Avx512F.Xor(x, y)|};
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => {|#9:Avx512DQ.Xor(x, y)|};
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => {|#10:Avx512DQ.Xor(x, y)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<byte> M(Vector512<byte> x, Vector512<byte> y) => x ^ y;
    Vector512<sbyte> M(Vector512<sbyte> x, Vector512<sbyte> y) => x ^ y;
    Vector512<short> M(Vector512<short> x, Vector512<short> y) => x ^ y;
    Vector512<ushort> M(Vector512<ushort> x, Vector512<ushort> y) => x ^ y;
    Vector512<int> M(Vector512<int> x, Vector512<int> y) => x ^ y;
    Vector512<uint> M(Vector512<uint> x, Vector512<uint> y) => x ^ y;
    Vector512<long> M(Vector512<long> x, Vector512<long> y) => x ^ y;
    Vector512<ulong> M(Vector512<ulong> x, Vector512<ulong> y) => x ^ y;
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x ^ y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x ^ y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opExclusiveOr]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_LeftShift
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
        #endregion

        #region op_Multiply
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
        #endregion

        #region op_OnesComplement
        [Fact]
        public async Task Fixer_opOnesComplementArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x) => {|#1:AdvSimd.Not(x)|};
    Vector64<sbyte> M(Vector64<sbyte> x) => {|#2:AdvSimd.Not(x)|};
    Vector64<short> M(Vector64<short> x) => {|#3:AdvSimd.Not(x)|};
    Vector64<ushort> M(Vector64<ushort> x) => {|#4:AdvSimd.Not(x)|};
    Vector64<int> M(Vector64<int> x) => {|#5:AdvSimd.Not(x)|};
    Vector64<uint> M(Vector64<uint> x) => {|#6:AdvSimd.Not(x)|};
    Vector64<long> M(Vector64<long> x) => {|#7:AdvSimd.Not(x)|};
    Vector64<ulong> M(Vector64<ulong> x) => {|#8:AdvSimd.Not(x)|};
    Vector64<float> M(Vector64<float> x) => {|#9:AdvSimd.Not(x)|};
    Vector64<double> M(Vector64<double> x) => {|#10:AdvSimd.Not(x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<byte> M(Vector64<byte> x) => ~x;
    Vector64<sbyte> M(Vector64<sbyte> x) => ~x;
    Vector64<short> M(Vector64<short> x) => ~x;
    Vector64<ushort> M(Vector64<ushort> x) => ~x;
    Vector64<int> M(Vector64<int> x) => ~x;
    Vector64<uint> M(Vector64<uint> x) => ~x;
    Vector64<long> M(Vector64<long> x) => ~x;
    Vector64<ulong> M(Vector64<ulong> x) => ~x;
    Vector64<float> M(Vector64<float> x) => ~x;
    Vector64<double> M(Vector64<double> x) => ~x;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opOnesComplementArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x) => {|#1:AdvSimd.Not(x)|};
    Vector128<sbyte> M(Vector128<sbyte> x) => {|#2:AdvSimd.Not(x)|};
    Vector128<short> M(Vector128<short> x) => {|#3:AdvSimd.Not(x)|};
    Vector128<ushort> M(Vector128<ushort> x) => {|#4:AdvSimd.Not(x)|};
    Vector128<int> M(Vector128<int> x) => {|#5:AdvSimd.Not(x)|};
    Vector128<uint> M(Vector128<uint> x) => {|#6:AdvSimd.Not(x)|};
    Vector128<long> M(Vector128<long> x) => {|#7:AdvSimd.Not(x)|};
    Vector128<ulong> M(Vector128<ulong> x) => {|#8:AdvSimd.Not(x)|};
    Vector128<float> M(Vector128<float> x) => {|#9:AdvSimd.Not(x)|};
    Vector128<double> M(Vector128<double> x) => {|#10:AdvSimd.Not(x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<byte> M(Vector128<byte> x) => ~x;
    Vector128<sbyte> M(Vector128<sbyte> x) => ~x;
    Vector128<short> M(Vector128<short> x) => ~x;
    Vector128<ushort> M(Vector128<ushort> x) => ~x;
    Vector128<int> M(Vector128<int> x) => ~x;
    Vector128<uint> M(Vector128<uint> x) => ~x;
    Vector128<long> M(Vector128<long> x) => ~x;
    Vector128<ulong> M(Vector128<ulong> x) => ~x;
    Vector128<float> M(Vector128<float> x) => ~x;
    Vector128<double> M(Vector128<double> x) => ~x;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opOnesComplementWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x) => {|#1:PackedSimd.Not(x)|};
    Vector128<sbyte> M(Vector128<sbyte> x) => {|#2:PackedSimd.Not(x)|};
    Vector128<short> M(Vector128<short> x) => {|#3:PackedSimd.Not(x)|};
    Vector128<ushort> M(Vector128<ushort> x) => {|#4:PackedSimd.Not(x)|};
    Vector128<int> M(Vector128<int> x) => {|#5:PackedSimd.Not(x)|};
    Vector128<uint> M(Vector128<uint> x) => {|#6:PackedSimd.Not(x)|};
    Vector128<long> M(Vector128<long> x) => {|#7:PackedSimd.Not(x)|};
    Vector128<ulong> M(Vector128<ulong> x) => {|#8:PackedSimd.Not(x)|};
    Vector128<float> M(Vector128<float> x) => {|#9:PackedSimd.Not(x)|};
    Vector128<double> M(Vector128<double> x) => {|#10:PackedSimd.Not(x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<byte> M(Vector128<byte> x) => ~x;
    Vector128<sbyte> M(Vector128<sbyte> x) => ~x;
    Vector128<short> M(Vector128<short> x) => ~x;
    Vector128<ushort> M(Vector128<ushort> x) => ~x;
    Vector128<int> M(Vector128<int> x) => ~x;
    Vector128<uint> M(Vector128<uint> x) => ~x;
    Vector128<long> M(Vector128<long> x) => ~x;
    Vector128<ulong> M(Vector128<ulong> x) => ~x;
    Vector128<float> M(Vector128<float> x) => ~x;
    Vector128<double> M(Vector128<double> x) => ~x;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(6),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(7),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(8),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(9),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opOnesComplement]).WithLocation(10),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_RightShift
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
        #endregion

        #region op_Subtraction
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
        #endregion

        #region op_UnaryNegation
        [Fact]
        public async Task Fixer_opUnaryNegationArmV64Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<sbyte> M(Vector64<sbyte> x) => {|#1:AdvSimd.Negate(x)|};
    Vector64<short> M(Vector64<short> x) => {|#2:AdvSimd.Negate(x)|};
    Vector64<int> M(Vector64<int> x) => {|#3:AdvSimd.Negate(x)|};
    Vector64<long> M(Vector64<long> x) => {|#4:AdvSimd.Arm64.NegateScalar(x)|};
    Vector64<float> M(Vector64<float> x) => {|#5:AdvSimd.Negate(x)|};
    Vector64<double> M(Vector64<double> x) => {|#6:AdvSimd.NegateScalar(x)|};

    Vector64<float> N(Vector64<float> x) => AdvSimd.NegateScalar(x);
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector64<sbyte> M(Vector64<sbyte> x) => -x;
    Vector64<short> M(Vector64<short> x) => -x;
    Vector64<int> M(Vector64<int> x) => -x;
    Vector64<long> M(Vector64<long> x) => -x;
    Vector64<float> M(Vector64<float> x) => -x;
    Vector64<double> M(Vector64<double> x) => -x;

    Vector64<float> N(Vector64<float> x) => AdvSimd.NegateScalar(x);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnaryNegationArmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x) => {|#1:AdvSimd.Negate(x)|};
    Vector128<short> M(Vector128<short> x) => {|#2:AdvSimd.Negate(x)|};
    Vector128<int> M(Vector128<int> x) => {|#3:AdvSimd.Negate(x)|};
    Vector128<long> M(Vector128<long> x) => {|#4:AdvSimd.Arm64.Negate(x)|};
    Vector128<float> M(Vector128<float> x) => {|#5:AdvSimd.Negate(x)|};
    Vector128<double> M(Vector128<double> x) => {|#6:AdvSimd.Arm64.Negate(x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x) => -x;
    Vector128<short> M(Vector128<short> x) => -x;
    Vector128<int> M(Vector128<int> x) => -x;
    Vector128<long> M(Vector128<long> x) => -x;
    Vector128<float> M(Vector128<float> x) => -x;
    Vector128<double> M(Vector128<double> x) => -x;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }

        [Fact]
        public async Task Fixer_opUnaryNegationWasmV128Async()
        {
            // lang=C#-test
            const string testCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x) => {|#1:PackedSimd.Negate(x)|};
    Vector128<short> M(Vector128<short> x) => {|#2:PackedSimd.Negate(x)|};
    Vector128<int> M(Vector128<int> x) => {|#3:PackedSimd.Negate(x)|};
    Vector128<long> M(Vector128<long> x) => {|#4:PackedSimd.Negate(x)|};
    Vector128<float> M(Vector128<float> x) => {|#5:PackedSimd.Negate(x)|};
    Vector128<double> M(Vector128<double> x) => {|#6:PackedSimd.Negate(x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<sbyte> M(Vector128<sbyte> x) => -x;
    Vector128<short> M(Vector128<short> x) => -x;
    Vector128<int> M(Vector128<int> x) => -x;
    Vector128<long> M(Vector128<long> x) => -x;
    Vector128<float> M(Vector128<float> x) => -x;
    Vector128<double> M(Vector128<double> x) => -x;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(4),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(5),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opUnaryNegation]).WithLocation(6),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
        #endregion

        #region op_UnsignedRightShift
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
        #endregion
    }
}

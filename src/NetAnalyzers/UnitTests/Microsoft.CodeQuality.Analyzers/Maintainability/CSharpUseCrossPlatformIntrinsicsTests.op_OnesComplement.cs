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
    }
}

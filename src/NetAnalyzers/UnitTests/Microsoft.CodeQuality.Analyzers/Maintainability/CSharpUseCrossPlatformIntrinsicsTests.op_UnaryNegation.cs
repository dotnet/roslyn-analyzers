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
    }
}

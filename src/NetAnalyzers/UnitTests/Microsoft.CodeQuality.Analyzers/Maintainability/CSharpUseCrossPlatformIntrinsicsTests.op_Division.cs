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

    Vector64<float> R(Vector64<float> x, Vector64<float> y) => {|#3:AdvSimd.Arm64.Divide(right: y, left: x)|};
    Vector64<double> R(Vector64<double> x, Vector64<double> y) => {|#4:AdvSimd.DivideScalar(right: y, left: x)|};

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

    Vector64<float> R(Vector64<float> x, Vector64<float> y) => x / y;
    Vector64<double> R(Vector64<double> x, Vector64<double> y) => x / y;

    Vector64<float> N(Vector64<float> x, Vector64<float> y) => AdvSimd.DivideScalar(x, y);
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
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

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => {|#3:AdvSimd.Arm64.Divide(right: y, left: x)|};
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => {|#4:AdvSimd.Arm64.Divide(right: y, left: x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
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

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => {|#3:PackedSimd.Divide(right: y, left: x)|};
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => {|#4:PackedSimd.Divide(right: y, left: x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Wasm;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
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

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => {|#3:Sse.Divide(right: y, left: x)|};
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => {|#4:Sse2.Divide(right: y, left: x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector128<float> M(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> M(Vector128<double> x, Vector128<double> y) => x / y;

    Vector128<float> R(Vector128<float> x, Vector128<float> y) => x / y;
    Vector128<double> R(Vector128<double> x, Vector128<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
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

    Vector256<float> R(Vector256<float> x, Vector256<float> y) => {|#3:Avx.Divide(right: y, left: x)|};
    Vector256<double> R(Vector256<double> x, Vector256<double> y) => {|#4:Avx.Divide(right: y, left: x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector256<float> M(Vector256<float> x, Vector256<float> y) => x / y;
    Vector256<double> M(Vector256<double> x, Vector256<double> y) => x / y;

    Vector256<float> R(Vector256<float> x, Vector256<float> y) => x / y;
    Vector256<double> R(Vector256<double> x, Vector256<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
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

    Vector512<float> R(Vector512<float> x, Vector512<float> y) => {|#3:Avx512F.Divide(right: y, left: x)|};
    Vector512<double> R(Vector512<double> x, Vector512<double> y) => {|#4:Avx512F.Divide(right: y, left: x)|};
}";
            // lang=C#-test
            const string fixedCode = @"using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class C
{
    Vector512<float> M(Vector512<float> x, Vector512<float> y) => x / y;
    Vector512<double> M(Vector512<double> x, Vector512<double> y) => x / y;

    Vector512<float> R(Vector512<float> x, Vector512<float> y) => x / y;
    Vector512<double> R(Vector512<double> x, Vector512<double> y) => x / y;
}";

            await new VerifyCS.Test
            {
                TestCode = testCode,
                ExpectedDiagnostics = {
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(1),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(2),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(3),
                    VerifyCS.Diagnostic(Rules[(int)RuleKind.opDivision]).WithLocation(4),
                },
                FixedCode = fixedCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }.RunAsync();
        }
    }
}

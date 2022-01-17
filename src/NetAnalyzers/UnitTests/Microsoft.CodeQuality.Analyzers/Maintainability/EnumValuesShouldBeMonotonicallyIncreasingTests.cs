// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpEnumValuesShouldBeMonotonicallyIncreasing,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class EnumValuesShouldBeMonotonicallyIncreasingTests
    {
        [Fact]
        public async Task EnumIsOrdered()
        {
            var code = @"
enum E
{
    A = 1,
    B = 2,
    C = 2,
    D = 4,
    E = A | C,
    F = A | B | C,
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task EnumIsNotOrdered()
        {
            var code = @"
enum E
{
    C = 2,
    [|A|] = 1,
    B = 2,
    D = 4,
    E = A | C,
    F = A | B | C,
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task EnumIsNotOrdered_ImplicitValues()
        {
            var code = @"
enum E
{
    A,
    B,
    C,
    [|D|] = 1,
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }
    }
}

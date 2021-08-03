// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.AvoidConstArraysAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class AvoidConstArraysTests
    {
        [Fact]
        public async Task IdentifyConstArrays_Explicit_Init()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class A
{
    public void B()
    {
        Console.WriteLine(new int[]{ 1, 2, 3 });
    }
}
");
        }

        [Fact]
        public async Task IdentifyConstArrays_Implicit_Init()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class A
{
    public void B()
    {
        Console.WriteLine(new[]{ 1, 2, 3 });
    }
}
");
        }
    }
}
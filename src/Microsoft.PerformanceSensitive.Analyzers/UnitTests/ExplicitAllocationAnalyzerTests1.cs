// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.PerformanceSensitive.Analyzers;
using Xunit;
using VerifyCS = Microsoft.PerformanceSensitive.Analyzers.UnitTests.CSharpPerformanceCodeFixVerifier<
    Microsoft.PerformanceSensitive.Analyzers.ExplicitAllocationAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class ExplicitAllocationAnalyzerTests1
    {
        [Fact]
        public async Task ExplicitAllocation_ImplicitArrayCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        int[] intData = new[] { 123, 32, 4 };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                // Test0.cs(9,25): info HAA0504: Implicit new array creation allocation
                VerifyCS.Diagnostic(ExplicitAllocationAnalyzer.NewArrayRule).WithLocation(9, 25));
        }
        

        [Fact]
        public async Task ExplicitAllocation_ArrayCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        int[] intData = new int[] { 123, 32, 4 };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                // Test0.cs(9,25): info HAA0501: Explicit new array type allocation
                VerifyCS.Diagnostic(ExplicitAllocationAnalyzer.NewArrayRule).WithLocation(9, 25));
        }
    }
}

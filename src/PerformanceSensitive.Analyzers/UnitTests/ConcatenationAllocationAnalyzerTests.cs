// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class ConcatenationAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void ConcatenationAllocation_Basic1()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        string s0 = ""hello"" + 0.ToString() + ""world"" + 1.ToString();
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        [Fact]
        public void ConcatenationAllocation_Basic2()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        string s2 = ""ohell"" + 2.ToString() + ""world"" + 3.ToString() + 4.ToString();
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,21): warning HAA0201: Considering using StringBuilder
                        GetCSharpResultAt(9, 21, ConcatenationAllocationAnalyzer.StringConcatenationAllocationRule));
        }

        [Theory]
        [InlineData("string s0 = nameof(System.String) + '-';")]
        [InlineData("string s0 = nameof(System.String) + true;")]
        [InlineData("string s0 = nameof(System.String) + new System.IntPtr();")]
        [InlineData("string s0 = nameof(System.String) + new System.UIntPtr();")]
        public void ConcatenationAllocation_DoNotWarnForOptimizedValueTypes(string statement)
        {
            var source = $@"using System;
using Roslyn.Utilities;

public class MyClass
{{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {{
        {statement}
    }}
}}";
            VerifyCSharp(source, withAttribute: true);
        }

        [Theory]
        [InlineData(@"const string s0 = nameof(System.String) + ""."" + nameof(System.String);")]
        [InlineData(@"const string s0 = nameof(System.String) + ""."";")]
        [InlineData(@"string s0 = nameof(System.String) + ""."" + nameof(System.String);")]
        [InlineData(@"string s0 = nameof(System.String) + ""."";")]
        public void ConcatenationAllocation_DoNotWarnForConst(string statement)
        {
            var source = $@"using System;
using Roslyn.Utilities;

public class MyClass
{{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {{
        {statement}
    }}
}}";
            VerifyCSharp(source, withAttribute: true);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConcatenationAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

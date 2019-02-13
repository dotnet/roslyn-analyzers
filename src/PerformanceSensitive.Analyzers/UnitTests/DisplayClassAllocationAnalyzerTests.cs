// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class DisplayClassAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void DisplayClassAllocation_AnonymousMethodExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

class Test
{
    static void Main()
    {
        Action action = CreateAction<int>(5);
    }

    [PerformanceSensitive(""uri"")]
    static Action CreateAction<T>(T item)
    {
        T test = default(T);
        int counter = 0;
        return delegate
        {
            counter++;
            Console.WriteLine(""counter={0}"", counter);
        };
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(15,13): warning HAA0302: The compiler will emit a class that will hold this as a field to allow capturing of this closure
                        GetCSharpResultAt(15, 13, DisplayClassAllocationAnalyzer.ClosureCaptureRule),
                        // Test0.cs(16,16): warning HAA0303: Considering moving this out of the generic method
                        GetCSharpResultAt(16, 16, DisplayClassAllocationAnalyzer.LambaOrAnonymousMethodInGenericMethodRule),
                        // Test0.cs(16,16): warning HAA0301: Heap allocation of closure Captures: counter
                        GetCSharpResultAt(16, 16, DisplayClassAllocationAnalyzer.ClosureDriverRule, "counter"));
        }

        [Fact]
        public void DisplayClassAllocation_SimpleLambdaExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System;
using System.Linq;
using Roslyn.Utilities;

public class Testing<T>
{
    [PerformanceSensitive(""uri"")]
    public Testing()
    {
        int[] intData = new[] { 123, 32, 4 };
        int min = 31;
        var results = intData.Where(i => i > min).ToList();
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(12,13): warning HAA0302: The compiler will emit a class that will hold this as a field to allow capturing of this closure
                        GetCSharpResultAt(12, 13, DisplayClassAllocationAnalyzer.ClosureCaptureRule),
                        // Test0.cs(13,39): warning HAA0301: Heap allocation of closure Captures: min
                        GetCSharpResultAt(13, 39, DisplayClassAllocationAnalyzer.ClosureDriverRule, "min"));
        }

        [Fact]
        public void DisplayClassAllocation_ParenthesizedLambdaExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System;
using System.Linq;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        var words = new[] { ""foo"", ""bar"", ""baz"", ""beer"" };
        var actions = new List<Action>();
        foreach (string word in words) // <-- captured closure
        {
            actions.Add(() => Console.WriteLine(word)); // <-- reason for closure capture
        }
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(13,25): warning HAA0302: The compiler will emit a class that will hold this as a field to allow capturing of this closure
                        GetCSharpResultAt(13, 25, DisplayClassAllocationAnalyzer.ClosureCaptureRule),
                        // Test0.cs(15,28): warning HAA0301: Heap allocation of closure Captures: word
                        GetCSharpResultAt(15, 28, DisplayClassAllocationAnalyzer.ClosureDriverRule, "word"));
        }

        [Fact]
        public void DisplayClassAllocation_DoNotReportForNonCapturingAnonymousMethod()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Sorter(int[] arr) 
    {
        System.Array.Sort(arr, delegate(int x, int y) { return x - y; });
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        [Fact]
        public void DisplayClassAllocation_DoNotReportForNonCapturingLambda()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Sorter(int[] arr) 
    {
        System.Array.Sort(arr, (x, y) => x - y);
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        [Fact]
        public void DisplayClassAllocation_ReportForCapturingAnonymousMethod()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Sorter(int[] arr) 
    {
        int z = 2;
        System.Array.Sort(arr, delegate(int x, int y) { return x - z; });
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,13): warning HAA0302: The compiler will emit a class that will hold this as a field to allow capturing of this closure
                        GetCSharpResultAt(9, 13, DisplayClassAllocationAnalyzer.ClosureCaptureRule),
                        // Test0.cs(10,32): warning HAA0301: Heap allocation of closure Captures: z
                        GetCSharpResultAt(10, 32, DisplayClassAllocationAnalyzer.ClosureDriverRule, "z"));
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DisplayClassAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

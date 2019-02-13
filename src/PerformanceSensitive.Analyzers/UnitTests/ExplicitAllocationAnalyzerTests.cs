// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public partial class ExplicitAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void ExplicitAllocation_InitializerExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @struct = new TestStruct { Name = ""Bob"" };
        var @class = new TestClass { Name = ""Bob"" };
    }
}

public struct TestStruct
{
    public string Name { get; set; }
}

public class TestClass
{
    public string Name { get; set; }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,13): info HAA0505: Initializer reference type allocation
                        GetCSharpResultAt(10, 13, ExplicitAllocationAnalyzer.InitializerCreationRule),
                        // Test0.cs(10,22): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(10, 22, ExplicitAllocationAnalyzer.NewObjectRule));
        }

        [Fact]
        public void ExplicitAllocation_ImplicitArrayCreationExpressionSyntax()
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
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,25): info HAA0504: Implicit new array creation allocation
                        GetCSharpResultAt(9, 25, ExplicitAllocationAnalyzer.ImplicitArrayCreationRule));
        }

        [Fact]
        public void ExplicitAllocation_AnonymousObjectCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var temp = new { A = 123, Name = ""Test"", };
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,20): info HAA0503: Explicit new anonymous object allocation
                        GetCSharpResultAt(9, 20, ExplicitAllocationAnalyzer.AnonymousNewObjectRule));
        }

        [Fact]
        public void ExplicitAllocation_ArrayCreationExpressionSyntax()
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
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,25): info HAA0501: Explicit new array type allocation
                        GetCSharpResultAt(9, 25, ExplicitAllocationAnalyzer.NewArrayRule));
        }

        [Fact]
        public void ExplicitAllocation_ObjectCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var allocation = new String('a', 10);
        var noAllocation = new DateTime();
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,26): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(9, 26, ExplicitAllocationAnalyzer.NewObjectRule));
        }

        [Fact]
        public void ExplicitAllocation_LetClauseSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        int[] intData = new[] { 123, 32, 4 };
        var result = (from a in intData
                      let b = a * 3
                      select b).ToList();
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,25): info HAA0504: Implicit new array creation allocation
                        GetCSharpResultAt(10, 25, ExplicitAllocationAnalyzer.ImplicitArrayCreationRule),
                        // Test0.cs(12,23): info HAA0506: Let clause induced allocation
                        GetCSharpResultAt(12, 23, ExplicitAllocationAnalyzer.LetCauseRule));
        }

        [Fact]
        public void ExplicitAllocation_AllSyntax()
        {
            var sampleProgram =
@"using System;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @struct = new TestStruct { Name = ""Bob"" };
        var @class = new TestClass { Name = ""Bob"" };

        int[] intDataImplicit = new[] { 123, 32, 4 };

        var temp = new { A = 123, Name = ""Test"", };

        int[] intDataExplicit = new int[] { 123, 32, 4 };

        var allocation = new String('a', 10);
        var noAllocation = new DateTime();

        int[] intDataLinq = new int[] { 123, 32, 4 };
        var result = (from a in intDataLinq
                      let b = a * 3
                      select b).ToList();
    }
}

public struct TestStruct
{
    public string Name { get; set; }
}

public class TestClass
{
    public string Name { get; set; }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(12,13): info HAA0505: Initializer reference type allocation
                        GetCSharpResultAt(12, 13, ExplicitAllocationAnalyzer.InitializerCreationRule),
                        // Test0.cs(12,22): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(12, 22, ExplicitAllocationAnalyzer.NewObjectRule),
                        // Test0.cs(14,33): info HAA0504: Implicit new array creation allocation
                        GetCSharpResultAt(14, 33, ExplicitAllocationAnalyzer.ImplicitArrayCreationRule),
                        // Test0.cs(16,20): info HAA0503: Explicit new anonymous object allocation
                        GetCSharpResultAt(16, 20, ExplicitAllocationAnalyzer.AnonymousNewObjectRule),
                        // Test0.cs(18,33): info HAA0501: Explicit new array type allocation
                        GetCSharpResultAt(18, 33, ExplicitAllocationAnalyzer.NewArrayRule),
                        // Test0.cs(20,26): info HAA0502: Explicit new reference type allocation
                        GetCSharpResultAt(20, 26, ExplicitAllocationAnalyzer.NewObjectRule),
                        // Test0.cs(23,29): info HAA0501: Explicit new array type allocation
                        GetCSharpResultAt(23, 29, ExplicitAllocationAnalyzer.NewArrayRule),
                        // Test0.cs(25,23): info HAA0506: Let clause induced allocation
                        GetCSharpResultAt(25, 23, ExplicitAllocationAnalyzer.LetCauseRule));
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExplicitAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

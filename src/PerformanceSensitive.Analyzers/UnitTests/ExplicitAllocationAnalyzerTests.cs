// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class ExplicitAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void ExplicitAllocation_InitializerExpressionSyntax()
        {
            var sampleProgram =
@"using System;

var @struct = new TestStruct { Name = ""Bob"" };
var @class = new TestClass { Name = ""Bob"" };

public struct TestStruct
{
    public string Name { get; set; }
}

public class TestClass
{
    public string Name { get; set; }
}";

            var analyser = new ExplicitAllocationAnalyzer();
            // SyntaxKind.ObjectInitializerExpression IS linked to InitializerExpressionSyntax (naming is a bit confusing)
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ObjectInitializerExpression));

            Assert.Equal(2, info.Allocations.Length);
            // Diagnostic: (4,14): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 4, character: 14);

            // Diagnostic: (4,5): info HeapAnalyzerInitializerCreationRule: Initializer reference type allocation ***
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.InitializerCreationRule.Id, line: 4, character: 5);
        }

        [Fact]
        public void ExplicitAllocation_ImplicitArrayCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;

int[] intData = new[] { 123, 32, 4 };";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ImplicitArrayCreationExpression));

            Assert.Single(info.Allocations);
            // Diagnostic: (3,17): info HeapAnalyzerImplicitNewArrayCreationRule: Implicit new array creation allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.ImplicitArrayCreationRule.Id, line: 3, character: 17);
        }

        [Fact]
        public void ExplicitAllocation_AnonymousObjectCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System;

var temp = new { A = 123, Name = ""Test"", };";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression));

            Assert.Single(info.Allocations);
            // Diagnostic: (3,12): info HeapAnalyzerExplicitNewAnonymousObjectRule: Explicit new anonymous object allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.AnonymousNewObjectRule.Id, line: 3, character: 12);
        }

        [Fact]
        public void ExplicitAllocation_ArrayCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;

int[] intData = new int[] { 123, 32, 4 };";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ArrayCreationExpression));

            Assert.Single(info.Allocations);
            // Diagnostic: (3,17): info HeapAnalyzerExplicitNewArrayRule: Implicit new array creation allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewArrayRule.Id, line: 3, character: 17);
        }

        [Fact]
        public void ExplicitAllocation_ObjectCreationExpressionSyntax()
        {
            var sampleProgram =
@"using System;

var allocation = new String('a', 10);
var noAllocation = new DateTime();";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ObjectCreationExpression));

            Assert.Single(info.Allocations);
            // Diagnostic: (3,18): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 3, character: 18);
        }

        [Fact]
        public void ExplicitAllocation_LetClauseSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System.Linq;

int[] intData = new[] { 123, 32, 4 };
var result = (from a in intData
              let b = a * 3
              select b).ToList();
";

            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.LetClause));

            Assert.Equal(2, info.Allocations.Length);
            // Diagnostic: (4,17): info HeapAnalyzerImplicitNewArrayCreationRule: Implicit new array creation allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.ImplicitArrayCreationRule.Id, line: 4, character: 17);

            // Diagnostic: (6,15): info HeapAnalyzerLetClauseRule: Let clause induced allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.LetCauseRule.Id, line: 6, character: 15);
        }

        [Fact]
        public void ExplicitAllocation_AllSyntax()
        {
            var sampleProgram =
@"using System;
using System.Collections.Generic;
using System.Linq;

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

public struct TestStruct
{
    public string Name { get; set; }
}

public class TestClass
{
    public string Name { get; set; }
}";

            // This test is here so that we use SyntaxKindsOfInterest explicitly, to make sure it works
            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ObjectCreationExpression, SyntaxKind.AnonymousObjectCreationExpression, SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression, SyntaxKind.ComplexElementInitializerExpression, SyntaxKind.ObjectInitializerExpression, SyntaxKind.ArrayCreationExpression, SyntaxKind.ImplicitArrayCreationExpression, SyntaxKind.LetClause));

            Assert.Equal(8, info.Allocations.Length);
            // Diagnostic: (6,14): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 6, character: 14);
            // Diagnostic: (6,5): info HeapAnalyzerInitializerCreationRule: Initializer reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.InitializerCreationRule.Id, line: 6, character: 5);
            // Diagnostic: (8,25): info HeapAnalyzerImplicitNewArrayCreationRule: Implicit new array creation allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.ImplicitArrayCreationRule.Id, line: 8, character: 25);
            // Diagnostic: (10,12): info HeapAnalyzerExplicitNewAnonymousObjectRule: Explicit new anonymous object allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.AnonymousNewObjectRule.Id, line: 10, character: 12);
            // Diagnostic: (12,25): info HeapAnalyzerExplicitNewArrayRule: Explicit new array type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewArrayRule.Id, line: 12, character: 25);
            // Diagnostic: (14,18): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 14, character: 18);
            // Diagnostic: (17,21): info HeapAnalyzerExplicitNewArrayRule: Explicit new array type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.NewArrayRule.Id, line: 17, character: 21);
            // Diagnostic: (19,15): info HeapAnalyzerLetClauseRule: Let clause induced allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: ExplicitAllocationAnalyzer.LetCauseRule.Id, line: 19, character: 15);
        }
    }
}

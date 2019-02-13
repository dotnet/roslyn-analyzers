// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    /// <summary>
    /// Taken from http://stackoverflow.com/questions/7995606/boxing-occurrence-in-c-sharp
    /// </summary>
    internal class StackOverflowAnswerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void Converting_any_value_type_to_System_Object_type()
        {
            var @script =
                    @"struct S { }
                    object box = new S();";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(SyntaxKind.ObjectCreationExpression, SyntaxKind.AnonymousObjectCreationExpression, SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression, SyntaxKind.ComplexElementInitializerExpression, SyntaxKind.ObjectInitializerExpression, SyntaxKind.ArrayCreationExpression, SyntaxKind.ImplicitArrayCreationExpression, SyntaxKind.LetClause));
            Assert.Single(info.Allocations);
            // Diagnostic: (2,34): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 2, character: 34);
        }

        [Fact]
        public void Converting_any_value_type_to_System_ValueType_type()
        {
            var @script =
                    @"struct S { }
                    System.ValueType box = new S();";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(SyntaxKind.ObjectCreationExpression, SyntaxKind.AnonymousObjectCreationExpression, SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression, SyntaxKind.ComplexElementInitializerExpression, SyntaxKind.ObjectInitializerExpression, SyntaxKind.ArrayCreationExpression, SyntaxKind.ImplicitArrayCreationExpression, SyntaxKind.LetClause));
            Assert.Single(info.Allocations);
            // Diagnostic: (2,44): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 2, character: 44);
        }

        [Fact]
        public void Converting_any_enumeration_type_to_System_Enum_type()
        {
            var @script =
                @"enum E { A }
                System.Enum box = E.A;";
            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.ReturnStatement,
                SyntaxKind.YieldReturnStatement,
                SyntaxKind.CastExpression,
                SyntaxKind.AsExpression,
                SyntaxKind.CoalesceExpression,
                SyntaxKind.ConditionalExpression,
                SyntaxKind.ForEachStatement,
                SyntaxKind.EqualsValueClause,
                SyntaxKind.Argument));
            Assert.Single(info.Allocations);
            // Diagnostic: (2,35): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 2, character: 35);
        }

        [Fact]
        public void Converting_any_value_type_into_interface_reference()
        {
            var @script =
                @"interface I { }
                struct S : I { }
                I box = new S();";
            var analyser = new ExplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(SyntaxKind.ObjectCreationExpression, SyntaxKind.AnonymousObjectCreationExpression, SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression, SyntaxKind.ComplexElementInitializerExpression, SyntaxKind.ObjectInitializerExpression, SyntaxKind.ArrayCreationExpression, SyntaxKind.ImplicitArrayCreationExpression, SyntaxKind.LetClause));
            Assert.Single(info.Allocations);
            // Diagnostic: (3,25): info HeapAnalyzerExplicitNewObjectRule: Explicit new reference type allocation
            AssertEx.ContainsDiagnostic(info.Allocations, ExplicitAllocationAnalyzer.NewObjectRule.Id, line: 3, character: 25);
        }

        [Fact]
        public void Non_constant_value_types_in_CSharp_string_concatenation()
        {
            var @script =
                @"System.DateTime c = System.DateTime.Now;;
                string s1 = ""char value will box"" + c;";
            var analyser = new ConcatenationAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(SyntaxKind.AddExpression, SyntaxKind.AddAssignmentExpression));
            Assert.Single(info.Allocations);
            //Diagnostic: (2,53): warning HeapAnalyzerBoxingRule: Value type (char) is being boxed to a reference type for a string concatenation.
            AssertEx.ContainsDiagnostic(info.Allocations, ConcatenationAllocationAnalyzer.ValueTypeToReferenceTypeInAStringConcatenationRule.Id, line: 2, character: 53);
        }

        [Fact]
        public void Creating_delegate_from_value_type_instance_method()
        {
            var @script =
                @"using System;
                struct S { public void M() {} }
                Action box = new S().M;";
            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.ReturnStatement,
                SyntaxKind.YieldReturnStatement,
                SyntaxKind.CastExpression,
                SyntaxKind.AsExpression,
                SyntaxKind.CoalesceExpression,
                SyntaxKind.ConditionalExpression,
                SyntaxKind.ForEachStatement,
                SyntaxKind.EqualsValueClause,
                SyntaxKind.Argument));
            Assert.Equal(2, info.Allocations.Length);
            // Diagnostic: (3,30): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 3, character: 30);
            // Diagnostic: (3,30): warning HeapAnalyzerDelegateOnStructRule: Struct instance method being used for delegate creation, this will result in a boxing instruction
            AssertEx.ContainsDiagnostic(info.Allocations, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule.Id, line: 3, character: 30);
        }

        [Fact]
        public void Calling_non_overridden_virtual_methods_on_value_types()
        {
            var @script =
                @"enum E { A }
                E.A.GetHashCode();";
            var analyser = new CallSiteImplicitAllocationAnalyzer();
            var info = ProcessCode(analyser, @script, ImmutableArray.Create(SyntaxKind.InvocationExpression));
            Assert.Single(info.Allocations);
            // Diagnostic: (2,17): warning HeapAnalyzerValueTypeNonOverridenCallRule: Non-overriden virtual method call on a value type adds a boxing or constrained instruction
            AssertEx.ContainsDiagnostic(info.Allocations, CallSiteImplicitAllocationAnalyzer.ValueTypeNonOverridenCallRule.Id, line: 2, character: 17);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

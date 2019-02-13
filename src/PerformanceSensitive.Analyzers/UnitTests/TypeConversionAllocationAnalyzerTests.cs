// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Test.Utilities;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class TypeConversionAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void TypeConversionAllocation_ArgumentSyntax()
        {
            VerifyCSharp(@"
using System;
using Roslyn.Utilities;

public class MyObject
{
    public MyObject(object obj)
    {
    }

    private void ObjCall(object obj)
    {
    }

    [PerformanceSensitive(""uri"")]
    public void Foo()
    {
        ObjCall(10); // Allocation
        _ = new MyObject(10); // Allocation
    }
}",
            withAttribute: true,
            GetCSharpResultAt(18, 17, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule),
            GetCSharpResultAt(19, 26, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule)
            );
        }

        [Fact]
        public void TypeConversionAllocation_ArgumentSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @class = new MyClass();
        @class.ProcessFunc(fooObjCall); // implicit, so Allocation
        @class.ProcessFunc(new Func<object, string>(fooObjCall)); // Explicit, so NO Allocation
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @struct = new MyStruct();
        @struct.ProcessFunc(fooObjCall); // implicit, so Allocation
        @struct.ProcessFunc(new Func<object, string>(fooObjCall)); // Explicit, so NO Allocation
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                // Test0.cs(10,28): warning HAA0603: This will allocate a delegate instance
                GetCSharpResultAt(10, 28, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                // Test0.cs(27,29): warning HAA0603: This will allocate a delegate instance
                GetCSharpResultAt(27, 29, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                // Test0.cs(27,29): warning HAA0602: Struct instance method being used for delegate creation, this will result in a boxing instruction
                GetCSharpResultAt(27, 29, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule));
        }

        [Fact]
        public void TypeConversionAllocation_ReturnStatementSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyObject
{
    public Object Obj1 
    { 
        [PerformanceSensitive(""uri"")]
        get { return 0; } 
    }

    [PerformanceSensitive(""uri"")]
    public Object Obj2 
    { 
        get { return 0; } 
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,22): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(9, 22, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule),
                        // Test0.cs(15,22): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(15, 22, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_ReturnStatementSyntax_NoAlloc()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyObject
{
    [PerformanceSensitive(""uri"")]
    public Object ObjNoAllocation1 { get { return 0.ToString(); } }

    public Object ObjNoAllocation2 
    { 
        [PerformanceSensitive(""uri"")]
        get { return 0.ToString(); } 
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        [Fact]
        public void TypeConversionAllocation_YieldStatementSyntax()
        {
            var sampleProgram =
@"using System;
using System.Collections.Generic;
using Roslyn.Utilities;

public class MyClass
{
    public void Foo()
    {
        foreach (var item in GetItems())
        {
        }

        foreach (var item in GetItemsNoAllocation())
        {
        }
    }

    [PerformanceSensitive(""uri"")]
    public IEnumerable<object> GetItems()
    {
        yield return 0; // Allocation
        yield break;
    }
    
    [PerformanceSensitive(""uri"")]
    public IEnumerable<int> GetItemsNoAllocation()
    {
        yield return 0; // NO Allocation (IEnumerable<int>)
        yield break;
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(21,22): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(21, 22, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_BinaryExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo()
    {
        object x = ""blah"";
        object a1 = x ?? 0; // Allocation
        object a2 = x ?? 0.ToString(); // No Allocation

        var b1 = 10 as object; // Allocation
        var b2 = 10.ToString() as object; // No Allocation
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,26): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(10, 26, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule),
                        // Test0.cs(13,18): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(13, 18, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_BinaryExpressionSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> temp = null;
        var result1 = temp ?? fooObjCall; // implicit, so Allocation
        var result2 = temp ?? new Func<object, string>(fooObjCall); // Explicit, so NO Allocation
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> temp = null;
        var result1 = temp ?? fooObjCall; // implicit, so Allocation
        var result2 = temp ?? new Func<object, string>(fooObjCall); // Explicit, so NO Allocation
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";


            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,31): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(10, 31, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(26,31): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(26, 31, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(26,31): warning HAA0602: Struct instance method being used for delegate creation, this will result in a boxing instruction
                        GetCSharpResultAt(26, 31, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule));
        }

        [Fact]
        public void TypeConversionAllocation_EqualsValueClauseSyntax()
        {
            // for (object i = 0;;)
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo()
    {
        for (object i = 0;;) // Allocation
        {
        }

        for (int i = 0;;) // NO Allocation
        {
        }
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,25): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(9, 25, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_EqualsValueClauseSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func2 = fooObjCall; // implicit, so Allocation
        Func<object, string> func1 = new Func<object, string>(fooObjCall); // Explicit, so NO Allocation
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func2 = fooObjCall; // implicit, so Allocation
        Func<object, string> func1 = new Func<object, string>(fooObjCall); // Explicit, so NO Allocation
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,38): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(9, 38, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(24,38): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(24, 38, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(24,38): warning HAA0602: Struct instance method being used for delegate creation, this will result in a boxing instruction
                        GetCSharpResultAt(24, 38, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule));
        }

        [Fact]
        [WorkItem(2, "https://github.com/mjsabby/RoslynClrHeapAllocationAnalyzer/issues/2")]
        public void TypeConversionAllocation_EqualsValueClause_ExplicitMethodGroupAllocation_Bug()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Action methodGroup = this.Method;
    }

    private void Method()
    {
    }
}

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Action methodGroup = this.Method;
    }

    private void Method()
    {
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,30): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(9, 30, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(22,30): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(22, 30, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule),
                        // Test0.cs(22,30): warning HAA0602: Struct instance method being used for delegate creation, this will result in a boxing instruction
                        GetCSharpResultAt(22, 30, TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule));
        }

        [Fact]
        public void TypeConversionAllocation_ConditionalExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        object obj = ""test"";
        object test1 = true ? 0 : obj; // Allocation
        object test2 = true ? 0.ToString() : obj; // NO Allocation
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(10,31): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(10, 31, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_CastExpressionSyntax()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var f1 = (object)5; // Allocation
        var f2 = (object)""5""; // NO Allocation
    }
}";

            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(9,26): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(9, 26, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_ArgumentWithImplicitStringCastOperator()
        {
            const string programWithoutImplicitCastOperator = @"
using System;
using Roslyn.Utilities;

public struct AStruct
{
    [PerformanceSensitive(""uri"")]
    public static void Dump(AStruct astruct)
    {
        System.Console.WriteLine(astruct);
    }
}";

            VerifyCSharp(programWithoutImplicitCastOperator, withAttribute: true,
                        // Test0.cs(10,34): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(10, 34, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));

            const string programWithImplicitCastOperator = @"
using System;
using Roslyn.Utilities;

public struct AStruct
{
    public readonly string WrappedString;

    public AStruct(string s)
    {
        WrappedString = s ?? """";
    }

    [PerformanceSensitive(""uri"")]
    public static void Dump(AStruct astruct)
    {
        System.Console.WriteLine(astruct);
    }

    [PerformanceSensitive(""uri"")]
    public static implicit operator string(AStruct astruct)
    {
        return astruct.WrappedString;
    }
}";
            VerifyCSharp(programWithImplicitCastOperator, withAttribute: true);
        }


        [Fact]
        public void TypeConversionAllocation_YieldReturnImplicitStringCastOperator()
        {
            const string programWithoutImplicitCastOperator = @"
using System;
using Roslyn.Utilities;

public struct AStruct
{
    [PerformanceSensitive(""uri"")]
    public System.Collections.Generic.IEnumerator<object> GetEnumerator()
    {
        yield return this;
    }
}";

            VerifyCSharp(programWithoutImplicitCastOperator, withAttribute: true,
                        // Test0.cs(10,22): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(10, 22, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));

            const string programWithImplicitCastOperator = @"
using System;
using Roslyn.Utilities;

public struct AStruct
{
    [PerformanceSensitive(""uri"")]
    public System.Collections.Generic.IEnumerator<string> GetEnumerator()
    {
        yield return this;
    }

    public static implicit operator string(AStruct astruct)
    {
        return """";
    }
}";

            VerifyCSharp(programWithImplicitCastOperator, withAttribute: true);
        }

        [Fact]
        public void TypeConversionAllocation_InterpolatedStringWithInt_BoxingWarning()
        {
            var source = @"
using System;
using Roslyn.Utilities;

class Program
{
    [PerformanceSensitive(""uri"")]
    void Foo()
    {
        string s = $""{1}"";
    }
}";

            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(10,23): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(10, 23, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_InterpolatedStringWithString_NoWarning()
        {
            var sampleProgram = @"string s = $""{1.ToString()}"";";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Interpolation));

            Assert.Empty(info.Allocations);
        }

        [Theory]
        [InlineData(@"private readonly System.Func<string, bool> fileExists =        System.IO.File.Exists;")]
        [InlineData(@"private System.Func<string, bool> fileExists { get; } =        System.IO.File.Exists;")]
        [InlineData(@"private static System.Func<string, bool> fileExists { get; } = System.IO.File.Exists;")]
        [InlineData(@"private static readonly System.Func<string, bool> fileExists = System.IO.File.Exists;")]
        public void TypeConversionAllocation_DelegateAssignmentToReadonly_DoNotWarn(string snippet)
        {
            var source = $@"
using System;
using Roslyn.Utilities;

class Program
{{
    [PerformanceSensitive(""uri"")]
    {snippet}
}}";

            VerifyCSharp(source, withAttribute: true,
                        // Test0.cs(8,68): info HeapAnalyzerReadonlyMethodGroupAllocationRule: This will allocate a delegate instance
                        GetCSharpResultAt(8, 68, TypeConversionAllocationAnalyzer.ReadonlyMethodGroupAllocationRule));
        }

        [Fact]
        public void TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithBoxing()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    [PerformanceSensitive(""uri"")]
    object Obj => 1;
}";

            VerifyCSharp(snippet, withAttribute: true,
                        // Test0.cs(8,19): warning HAA0601: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
                        GetCSharpResultAt(8, 19, TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule));
        }

        [Fact]
        public void TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithoutBoxing()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    [PerformanceSensitive(""uri"")]
    object Obj => 1.ToString();
}";

            VerifyCSharp(snippet, withAttribute: true);
        }

        [Fact]
        public void TypeConversionAllocation_ExpressionBodiedPropertyDelegate()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    void Function(int i) { } 

    [PerformanceSensitive(""uri"")]
    Action<int> Obj => Function;
}";

            VerifyCSharp(snippet, withAttribute: true,
                        // Test0.cs(10,24): warning HAA0603: This will allocate a delegate instance
                        GetCSharpResultAt(10, 24, TypeConversionAllocationAnalyzer.MethodGroupAllocationRule));
        }

        [Fact]
        public void TypeConversionAllocation_ExpressionBodiedPropertyExplicitDelegate_NoWarning()
        {
            // Tests that an explicit delegate creation does not trigger HAA0603. It should be handled by HAA0502.
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    void Function(int i) { } 

    [PerformanceSensitive(""uri"")]
    Action<int> Obj => new Action<int>(Function);
}";

            VerifyCSharp(snippet, withAttribute: true);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TypeConversionAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.PerformanceSensitiveAnalyzers;
using Test.Utilities;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.PerformanceSensitiveAnalyzers.UnitTests.CSharpPerformanceCodeFixVerifier<
    Microsoft.CodeAnalysis.CSharp.PerformanceSensitiveAnalyzers.TypeConversionAllocationAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.PerformanceSensitive.Analyzers.UnitTests
{
    public class TypeConversionAllocationAnalyzerTests
    {
        [Fact]
        public async Task TypeConversionAllocation_Argument()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(18, 17),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(19, 26)
            );
        }

        [Fact]
        public async Task TypeConversionAllocation_Argument_ClassWithImplicitDelegate()
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
        @class.ProcessFunc(fooObjCall);
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(10, 28));
        }

        [Fact]
        public async Task TypeConversionAllocation_Argument_ClassWithExplicitDelegate()
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
        // Explicit allocation, no warning from this analyzer
        @class.ProcessFunc(new Func<object, string>(fooObjCall)); 
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_Argument_StructWithImplicitDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @struct = new MyStruct();
        @struct.ProcessFunc(fooObjCall);
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(10, 29),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(10, 29));
        }

        [Fact]
        public async Task TypeConversionAllocation_Argument_StructWithExplicitDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        var @struct = new MyStruct();
        @struct.ProcessFunc(new Func<object, string>(fooObjCall));
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj) => null;
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(10, 54));
        }

        [Fact]
        public async Task TypeConversionAllocation_ReturnStatementAsync()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(9, 22),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(15, 22));
        }
        [Fact]
        public async Task TypeConversionAllocation_ReturnStatement_NoAlloction()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_YieldStatement()
        {
            var sampleProgram =
@"using System;
using System.Collections.Generic;
using Roslyn.Utilities;

public class MyClass
{
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 22));
        }

        [Fact]
        public async Task TypeConversionAllocation_BinaryExpression()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 26),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(13, 18));
        }

        [Fact]
        public async Task TypeConversionAllocation_BinaryExpression_ClassWithImplicitDelegate()
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
        var result1 = temp ?? fooObjCall;
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(10, 31));
        }

        [Fact]
        public async Task TypeConversionAllocation_BinaryExpression_ClassWithExplicitDelegate()
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
        var result2 = temp ?? new Func<object, string>(fooObjCall);
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_BinaryExpression_StructWithImplicitDelegate()
        {
            var sampleProgram =
        @"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> temp = null;
        var result1 = temp ?? fooObjCall;
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(10, 31),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(10, 31));
        }

        [Fact]
        public async Task TypeConversionAllocation_BinaryExpression_StructWithExplicitDelegate()
        {
            var sampleProgram =
        @"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> temp = null;
        var result2 = temp ?? new Func<object, string>(fooObjCall);
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(10, 56));
        }

        [Fact]
        public async Task TypeConversionAllocation_EqualsValueClause()
        {
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(9, 25));
        }

        [Fact]
        public async Task TypeConversionAllocation_EqualsValueClause_ClassWithDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func2 = fooObjCall;
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(9, 38));
        }

        [Fact]
        public async Task TypeConversionAllocation_EqualsValueClause_ClassWithExplicitDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func1 = new Func<object, string>(fooObjCall);
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_EqualsValueClause_StructWithImplicitDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func2 = fooObjCall;
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(9, 38),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(9, 38));
        }

        [Fact]
        public async Task TypeConversionAllocation_EqualsValueClause_StructWithExplicitDelegate()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public struct MyStruct
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        Func<object, string> func1 = new Func<object, string>(fooObjCall);
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(9, 63));
        }

        [Fact]
        [WorkItem(2, "https://github.com/Microsoft/RoslynClrHeapAllocationAnalyzer/issues/2")]
        public async Task TypeConversionAllocation_EqualsValueClause_ExplicitMethodGroupAllocation_Bug()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(9, 30),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(22, 30),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(22, 30));
        }

        [Fact]
        public async Task TypeConversionAllocation_ConditionalExpression()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 31));
        }

        [Fact]
        public async Task TypeConversionAllocation_CastExpression()
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

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(9, 26));
        }


        [Fact]
        public async Task TypeConversionAllocation_Argument_WithoutImplicitStringCastOperator()
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

            await VerifyCS.VerifyAnalyzerAsync(programWithoutImplicitCastOperator,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 34));
        }

        [Fact]
        public async Task TypeConversionAllocation_Argument_WithImplicitStringCastOperator()
        {
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
            await VerifyCS.VerifyAnalyzerAsync(programWithImplicitCastOperator);
        }


        [Fact]
        public async Task TypeConversionAllocation_YieldReturnWithoutImplicitStringCastOperator()
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

            await VerifyCS.VerifyAnalyzerAsync(programWithoutImplicitCastOperator,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 22));
        }

        [Fact]
        public async Task TypeConversionAllocation_YieldReturnImplicitStringCastOperator()
        {
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

            await VerifyCS.VerifyAnalyzerAsync(programWithImplicitCastOperator);
        }

        [Fact]
        public async Task TypeConversionAllocation_InterpolatedStringWithInt_BoxingWarning()
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

            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 23));
        }

#if false
        [Fact]
        public void TypeConversionAllocation_InterpolatedStringWithString_NoWarning()
        {
            var sampleProgram = @"string s = $""{1.ToString()}"";";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Interpolation));

            Assert.Empty(info.Allocations);
        }
#endif

        [Theory]
        [InlineData(@"private readonly System.Func<string, bool> fileExists =        System.IO.File.Exists;")]
        [InlineData(@"private System.Func<string, bool> fileExists { get; } =        System.IO.File.Exists;")]
        [InlineData(@"private static System.Func<string, bool> fileExists { get; } = System.IO.File.Exists;")]
        [InlineData(@"private static readonly System.Func<string, bool> fileExists = System.IO.File.Exists;")]
        public async Task TypeConversionAllocation_DelegateAssignmentToReadonly(string snippet)
        {
            var source = $@"
using System;
using Roslyn.Utilities;

class Program
{{
    [PerformanceSensitive(""uri"")]
    {snippet}
}}";

            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(8, 68));
        }

        [Fact]
        public async Task TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithBoxing()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    [PerformanceSensitive(""uri"")]
    object Obj => 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(snippet,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(8, 19));
        }

        [Fact]
        public async Task TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithoutBoxing()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    [PerformanceSensitive(""uri"")]
    object Obj => 1.ToString();
}";

            await VerifyCS.VerifyAnalyzerAsync(snippet);
        }

        [Fact]
        public async Task TypeConversionAllocation_ExpressionBodiedPropertyDelegate()
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

            await VerifyCS.VerifyAnalyzerAsync(snippet,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(10, 24));
        }

        [Fact]
        public async Task TypeConversionAllocation_ExpressionBodiedPropertyExplicitDelegate()
        {
            const string snippet = @"
using System;
using Roslyn.Utilities;

class Program
{
    void Function(int i) { } 

    [PerformanceSensitive(""uri"")]
    Action<int> Obj => new Action<int>(Function); // Explicit allocation, no warning from this analyzer
}";

            await VerifyCS.VerifyAnalyzerAsync(snippet);
        }

        [Fact]
        [WorkItem(7995606, "http://stackoverflow.com/questions/7995606/boxing-occurrence-in-c-sharp")]
        public async Task Converting_any_enumeration_type_to_System_Enum_type()
        {
            var source = @"
using Roslyn.Utilities;

enum E { A }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        System.Enum box = E.A;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(11, 27));
        }

        [Fact]
        [WorkItem(7995606, "http://stackoverflow.com/questions/7995606/boxing-occurrence-in-c-sharp")]
        public async Task Creating_delegate_from_value_type_instance_method()
        {
            var source = @"
using System;
using Roslyn.Utilities;

struct S { public void M() {} }

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        Action box = new S().M;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.MethodGroupAllocationRule).WithLocation(12, 22),
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule).WithLocation(12, 22));
        }

        [Fact]
        [WorkItem(66, "https://github.com/Microsoft/RoslynClrHeapAllocationAnalyzer/issues/66")]
        public async void TypeConversionAllocation_ForwardingActionOnStruct_DoNotWarn()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

struct Foo {
    [PerformanceSensitive(""uri"")]
    void Perform(Action process) { Forward(process); }
    void Forward(Action process) { process(); }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async void TypeConversionAllocation_GenericStructParameter_BoxingWarning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo
{
    [PerformanceSensitive(""uri"")]
    void A<T>(T a) where T : struct
    {
        object box = a;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 22));
        }

        [Fact]
        public async void TypeConversionAllocation_GenericParameter_Warning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo
{
    [PerformanceSensitive(""uri"")]
    void A<T>(T a)
    {
        object box = a;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(10, 22));
        }

        [Fact]
        public async void TypeConversionAllocation_GenericClassParameter_NoWarning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo
{
    [PerformanceSensitive(""uri"")]
    void A<T>(T a) where T : class
    {
        object box = a;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async void TypeConversionAllocation_GenericStructParameterWithImplicitConversion_NoWarning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo<T> where T : struct
{
    [PerformanceSensitive(""uri"")]
    void A(T value)
    {
        Foo<T> noBox = value;
    }

    public static implicit operator Foo<T>(T value)
    {
        return new Foo<T>();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async void TypeConversionAllocation_GenericParameterWithImplicitConversion_NoWarning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo<T>
{
    [PerformanceSensitive(""uri"")]
    void A(T value)
    {
        Foo<T> noBox = value;
    }

    public static implicit operator Foo<T>(T value)
    {
        return new Foo<T>();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async void TypeConversionAllocation_StringConcatenationOfValueType_NoWarning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

struct Bar {}

class Foo
{
    [PerformanceSensitive(""uri"")]
    void A()
    {
        string x = ""foo"" + 1;
        
        Bar bar = new Bar();
        string y = ""foo"" + bar;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_LambdasAndAnonymousMethod_NoHAA0603()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        string s = ""foo"";
        Func<object, string> func1 = o => s;
        Func<object, string> func2 = delegate(object o) { return s; };
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }


        [Fact]
        public async Task TypeConversionAllocation_StringCharConcatenation_NoWarning()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        string x = """";
        x += 'x';
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram);
        }

        [Fact]
        public async Task TypeConversionAllocation_BoxingExtensionMethodReceiver_Warning()
        {
            var sampleProgram =
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

public struct A : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Testing()
    {
        A a = new A();
        int i = a.Last();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule).WithLocation(26, 17));
        }

        [Fact]
        public async void TypeConversionAllocation_LambdaReturnConversion_Warning()
        {
            const string sampleProgram = @"
using System;
using Roslyn.Utilities;

class Foo
{
    [PerformanceSensitive(""uri"")]
    void A()
    {
        B(() => false);
    }

    void B(Func<object> callback)
    { 
    }
}";
            var expectedMessage = string.Format(CultureInfo.InvariantCulture, (string)TypeConversionAllocationAnalyzer.LambdaReturnConversionRule.MessageFormat, "bool", "object");
            var expectedLambdaDiagnostic = RuleWithMessage(TypeConversionAllocationAnalyzer.LambdaReturnConversionRule, expectedMessage);

            await VerifyCS.VerifyAnalyzerAsync(sampleProgram,
                VerifyCS.Diagnostic(expectedLambdaDiagnostic).WithLocation(10, 17));
        }

        private static DiagnosticDescriptor RuleWithMessage(DiagnosticDescriptor d, string message)
        {
            return new DiagnosticDescriptor(d.Id, d.Title, message, d.Category, d.DefaultSeverity, d.IsEnabledByDefault, d.Description, d.HelpLinkUri, d.CustomTags.ToArray());
        }
    }
}
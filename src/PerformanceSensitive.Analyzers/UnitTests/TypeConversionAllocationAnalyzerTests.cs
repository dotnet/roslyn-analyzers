using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Linq;

namespace ClrHeapAllocationAnalyzer.Test
{
    [TestClass]
    public class TypeConversionAllocationAnalyzerTests : AllocationAnalyzerTests
    {
        [TestMethod]
        public void TypeConversionAllocation_ArgumentSyntax()
        {
            var sampleProgram =
@"using System;

var result = fooObjCall(10); // Allocation
var temp = new MyObject(10); // Allocation

private string fooObjCall(object obj)
{
    return obj.ToString();
}

public class MyObject
{
    private Object Obj;

    public MyObject(object obj)
    {
        this.Obj = obj;
    }
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Argument));

            Assert.AreEqual(2, info.Allocations.Count);
            // Diagnostic: (3,25): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable ***
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 3, character: 25);
            // Diagnostic: (4,25): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable ***
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 4, character: 25);
        }

        [TestMethod]
        public void TypeConversionAllocation_ArgumentSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;

public class MyClass
{
    public void Testing()
    {
        var @class = new MyClass();
        @class.ProcessFunc(fooObjCall); // implicit, so Allocation
        @class.ProcessFunc(new Func<object, string>(fooObjCall)); // Explicit, so NO Allocation
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}

public struct MyStruct
{
    public void Testing()
    {
        var @struct = new MyStruct();
        @struct.ProcessFunc(fooObjCall); // implicit, so Allocation
        @struct.ProcessFunc(new Func<object, string>(fooObjCall)); // Explicit, so NO Allocation
    }

    public void ProcessFunc(Func<object, string> func)
    {
    }

    private string fooObjCall(object obj)
    {
        return obj.ToString();
    }
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Argument));

            Assert.AreEqual(3, info.Allocations.Count);
            // Diagnostic: (8,28): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 8, character: 28);
            // Diagnostic: (27,29): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 27, character: 29);
            // Diagnostic: (27,29): warning HeapAnalyzerDelegateOnStructRule: Struct instance method being used for delegate creation, this will result in a boxing instruction
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule.Id, line: 27, character: 29);
        }

        [TestMethod]
        public void TypeConversionAllocation_ReturnStatementSyntax()
        {
            var sampleProgram =
@"using System;

var result1 = new MyObject().Obj; // Allocation
var result2 = new MyObject().ObjNoAllocation; // Allocation

public class MyObject
{
    public Object Obj { get { return 0; } }

    public Object ObjNoAllocation { get { return 0.ToString(); } }
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ReturnStatement));

            Assert.AreEqual(1, info.Allocations.Count);

            // Diagnostic: (7,38): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 8, character: 38);
        }

        [TestMethod]
        public void TypeConversionAllocation_YieldStatementSyntax()
        {
            var sampleProgram =
@"using System;
using System.Collections.Generic;

foreach (var item in GetItems())
{
}

foreach (var item in GetItemsNoAllocation())
{
}

public IEnumerable<object> GetItems()
{
    yield return 0; // Allocation
    yield break;
}

public IEnumerable<int> GetItemsNoAllocation()
{
    yield return 0; // NO Allocation (IEnumerable<int>)
    yield break;
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.YieldReturnStatement));

            Assert.AreEqual(1, info.Allocations.Count);

            // Diagnostic: (14,18): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 14, character: 18);
            // TODO this is a false positive
            // Diagnostic: (8,22): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
        }

        [TestMethod]
        public void TypeConversionAllocation_BinaryExpressionSyntax()
        {
            var sampleProgram =
@"using System;

object x = ""blah"";
object a1 = x ?? 0; // Allocation
object a2 = x ?? 0.ToString(); // No Allocation

var b1 = 10 as object; // Allocation
var b2 = 10.ToString() as object; // No Allocation
";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.CoalesceExpression, SyntaxKind.AsExpression));

            Assert.AreEqual(2, info.Allocations.Count);

            // Diagnostic: (4,17): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 4, character: 18);
            // Diagnostic: (7,9): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 7, character: 10);
        }

        [TestMethod]
        public void TypeConversionAllocation_BinaryExpressionSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;

public class MyClass
{
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

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.CoalesceExpression, SyntaxKind.AsExpression));

            Assert.AreEqual(3, info.Allocations.Count);

            // Diagnostic: (8,31): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 8, character: 31);
            // Diagnostic: (23,31): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 23, character: 31);
            // Diagnostic: (23,31): warning HeapAnalyzerDelegateOnStructRule: Struct instance method being used for delegate creation, this will result in a boxing instruction
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule.Id, line: 23, character: 31);
        }

        [TestMethod]
        public void TypeConversionAllocation_EqualsValueClauseSyntax()
        {
            // for (object i = 0;;)
            var sampleProgram =
@"using System;

for (object i = 0;;) // Allocation
{
}

for (int i = 0;;) // NO Allocation
{
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(
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

            Assert.AreEqual(1, info.Allocations.Count);

            // Diagnostic: (3,17): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 3, character: 17);
        }

        [TestMethod]
        public void TypeConversionAllocation_EqualsValueClauseSyntax_WithDelegates()
        {
            var sampleProgram =
@"using System;

public class MyClass
{
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

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.CoalesceExpression, SyntaxKind.EqualsValueClause));

            Assert.AreEqual(3, info.Allocations.Count);

            // Diagnostic: (7,38): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 7, character: 38);
            // Diagnostic: (21,38): warning HeapAnalyzerMethodGroupAllocationRule: This will allocate a delegate instance
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 21, character: 38);
            // Diagnostic: (21,38): warning HeapAnalyzerDelegateOnStructRule: Struct instance method being used for delegate creation, this will result in a boxing instruction
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.DelegateOnStructInstanceRule.Id, line: 21, character: 38);
            // TODO this is a false positive
            // Diagnostic: (22,63): warning HeapAnalyzerDelegateOnStructRule: Struct instance method being used for delegate creation, this will result in a boxing instruction
        }

        [TestMethod]
        public void TypeConversionAllocation_EqualsValueClause_ExplicitMethodGroupAllocation_Bug()
        {
            // See https://github.com/mjsabby/RoslynClrHeapAllocationAnalyzer/issues/2
            var sampleProgram =
@"using System;

public class MyClass
{
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
    public void Testing()
    {
        Action methodGroup = this.Method;
    }

    private void Method()
    {
    }
}";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.EqualsValueClause));

            Assert.AreEqual(3, info.Allocations.Count);
        }

        [TestMethod]
        public void TypeConversionAllocation_ConditionalExpressionSyntax()
        {
            var sampleProgram =
@"using System;

object obj = ""test"";
object test1 = true ? 0 : obj; // Allocation
object test2 = true ? 0.ToString() : obj; // NO Allocation
";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ConditionalExpression));

            Assert.AreEqual(1, info.Allocations.Count);

            // Diagnostic: (4,23): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 4, character: 23);
        }

        [TestMethod]
        public void TypeConversionAllocation_CastExpressionSyntax()
        {
            var sampleProgram =
@"using System;

var f1 = (object)5; // Allocation
var f2 = (object)""5""; // NO Allocation
";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.CastExpression));

            Assert.AreEqual(1, info.Allocations.Count);

            // Diagnostic: (3,18): warning HeapAnalyzerBoxingRule: Value type to reference type conversion causes boxing at call site (here), and unboxing at the callee-site. Consider using generics if applicable
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 3, character: 18);
        }

        [TestMethod]
        public void TypeConversionAllocation_ArgumentWithImplicitStringCastOperator() {
            const string programWithoutImplicitCastOperator = @"
                public struct AStruct
                {
                    public static void Dump(AStruct astruct)
                    {
                        System.Console.WriteLine(astruct);
                    }
                }
            ";

            const string programWithImplicitCastOperator = @"
                public struct AStruct
                {
                    public readonly string WrappedString;

                    public AStruct(string s)
                    {
                        WrappedString = s ?? """";
                    }

                    public static void Dump(AStruct astruct)
                    {
                        System.Console.WriteLine(astruct);
                    }

                    public static implicit operator string(AStruct astruct)
                    {
                        return astruct.WrappedString;
                    }
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();

            var info0 = ProcessCode(analyzer, programWithoutImplicitCastOperator, ImmutableArray.Create(SyntaxKind.Argument));
            AssertEx.ContainsDiagnostic(info0.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 6, character: 50);
            
            var info1 = ProcessCode(analyzer, programWithImplicitCastOperator, ImmutableArray.Create(SyntaxKind.Argument));
            Assert.AreEqual(0, info1.Allocations.Count);
        }


        [TestMethod]
        public void TypeConversionAllocation_YieldReturnImplicitStringCastOperator() {
            const string programWithoutImplicitCastOperator = @"
                public struct AStruct
                {
                    public System.Collections.Generic.IEnumerator<object> GetEnumerator()
                    {
                        yield return this;
                    }
                }
            ";

            const string programWithImplicitCastOperator = @"
                public struct AStruct
                {
                    public System.Collections.Generic.IEnumerator<string> GetEnumerator()
                    {
                        yield return this;
                    }

                    public static implicit operator string(AStruct astruct)
                    {
                        return """";
                    }
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();

            var info0 = ProcessCode(analyzer, programWithoutImplicitCastOperator, ImmutableArray.Create(SyntaxKind.Argument));
            AssertEx.ContainsDiagnostic(info0.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 6, character: 38);

            var info1 = ProcessCode(analyzer, programWithImplicitCastOperator, ImmutableArray.Create(SyntaxKind.Argument));
            Assert.AreEqual(0, info1.Allocations.Count);
        }

        [TestMethod]
        public void TypeConversionAllocation_InterpolatedStringWithInt_BoxingWarning() {
            var sampleProgram = @"string s = $""{1}"";";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Interpolation));

            Assert.AreEqual(1, info.Allocations.Count);
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 1, character: 15);
        }

        [TestMethod]
        public void TypeConversionAllocation_InterpolatedStringWithString_NoWarning() {
            var sampleProgram = @"string s = $""{1.ToString()}"";";

            var analyser = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.Interpolation));

            Assert.AreEqual(0, info.Allocations.Count);
        }

        [TestMethod]
        public void TypeConversionAllocation_DelegateAssignmentToReadonly_DoNotWarn()
        {
            string[] snippets =
            {
                @"private readonly System.Func<string, bool> fileExists = System.IO.File.Exists;",
                @"private static readonly System.Func<string, bool> fileExists = System.IO.File.Exists;",
                @"private System.Func<string, bool> fileExists { get; } = System.IO.File.Exists;",
                @"private static System.Func<string, bool> fileExists { get; } = System.IO.File.Exists;"
            };

            var analyzer = new TypeConversionAllocationAnalyzer();
            foreach (var snippet in snippets)
            {
                var info = ProcessCode(analyzer, snippet, ImmutableArray.Create(SyntaxKind.Argument));
                Assert.AreEqual(1, info.Allocations.Count(x => x.Id == TypeConversionAllocationAnalyzer.ReadonlyMethodGroupAllocationRule.Id), snippet);
            }
        }

        [TestMethod]
        public void TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithBoxing() {
            const string snippet = @"
                class Program
                {
                    object Obj => 1;
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyzer, snippet, ImmutableArray.Create(
                SyntaxKind.ArrowExpressionClause));
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.ValueTypeToReferenceTypeConversionRule.Id, line: 4, character: 35);
        }

        [TestMethod]
        public void TypeConversionAllocation_ExpressionBodiedPropertyBoxing_WithoutBoxing() {
            const string snippet = @"
                class Program
                {
                    object Obj => 1.ToString();
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyzer, snippet, ImmutableArray.Create(
                SyntaxKind.ArrowExpressionClause));
            Assert.AreEqual(0, info.Allocations.Count);
        }

        [TestMethod]
        public void TypeConversionAllocation_ExpressionBodiedPropertyDelegate() {
            const string snippet = @"
                using System;
                class Program
                {
                    void Function(int i) { } 

                    Action<int> Obj => Function;
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyzer, snippet, ImmutableArray.Create(
                SyntaxKind.ArrowExpressionClause));
            AssertEx.ContainsDiagnostic(info.Allocations, id: TypeConversionAllocationAnalyzer.MethodGroupAllocationRule.Id, line: 7, character: 40);
        }

        [TestMethod]
        [Description("Tests that an explicit delegate creation does not trigger HAA0603. " +
            "It should be handled by HAA0502.")]
        public void TypeConversionAllocation_ExpressionBodiedPropertyExplicitDelegate_NoWarning() {
            const string snippet = @"
                using System;
                class Program
                {
                    void Function(int i) { } 

                    Action<int> Obj => new Action<int>(Function);
                }
            ";

            var analyzer = new TypeConversionAllocationAnalyzer();
            var info = ProcessCode(analyzer, snippet, ImmutableArray.Create(
                SyntaxKind.ArrowExpressionClause));
            Assert.AreEqual(0, info.Allocations.Count);
        }
    }
}

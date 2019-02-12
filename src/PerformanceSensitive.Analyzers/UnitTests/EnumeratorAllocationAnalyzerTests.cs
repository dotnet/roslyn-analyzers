using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;

namespace ClrHeapAllocationAnalyzer.Test
{
    [TestClass]
    public class EnumeratorAllocationAnalyzerTests : AllocationAnalyzerTests
    {
        [TestMethod]
        public void EnumeratorAllocation_Basic()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System;
using System.Linq;

int[] intData = new[] { 123, 32, 4 };
IList<int> iListData = new[] { 123, 32, 4 };
List<int> listData = new[] { 123, 32, 4 }.ToList();

foreach (var i in intData)
{
    Console.WriteLine(i);
}

foreach (var i in listData)
{
    Console.WriteLine(i);
}

foreach (var i in iListData) // Allocations (line 19)
{
    Console.WriteLine(i);
}

foreach (var i in (IEnumerable<int>)intData) // Allocations (line 24)
{
    Console.WriteLine(i);
}";

            var analyser = new EnumeratorAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ForEachStatement));

            Assert.AreEqual(2, info.Allocations.Count);
            // Diagnostic: (19,16): warning HeapAnalyzerEnumeratorAllocationRule: Non-ValueType enumerator may result in a heap allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule.Id, line: 19, character: 16);
            // Diagnostic: (24,16): warning HeapAnalyzerEnumeratorAllocationRule: Non-ValueType enumerator may result in a heap allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule.Id, line: 24, character: 16);
        }

        [TestMethod]
        public void EnumeratorAllocation_Advanced()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System;

// These next 3 are from the YouTube video 
foreach (object a in new[] { 1, 2, 3}) // Allocations 'new [] { 1. 2, 3}'
{
    Console.WriteLine(a.ToString());
}

IEnumerable<string> fx1 = default(IEnumerable<string>);
foreach (var f in fx1) // Allocations 'in'
{
}

List<string> fx2 = default(List<string>);
foreach (var f in fx2) // NO Allocations
{
}";

            var analyser = new EnumeratorAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ForEachStatement, SyntaxKind.InvocationExpression));

            Assert.AreEqual(1, info.Allocations.Count);
            // Diagnostic: (11,16): warning HeapAnalyzerEnumeratorAllocationRule: Non-ValueType enumerator may result in a heap allocation
            AssertEx.ContainsDiagnostic(info.Allocations, id: EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule.Id, line: 11, character: 16);
        }

        [TestMethod]
        public void EnumeratorAllocation_Via_InvocationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System.Collections;
using System;

var enumeratorRaw = GetIEnumerableRaw();
while (enumeratorRaw.MoveNext())
{
    Console.WriteLine(enumeratorRaw.Current.ToString());
}

var enumeratorRawViaIEnumerable = GetIEnumeratorViaIEnumerable();
while (enumeratorRawViaIEnumerable.MoveNext())
{
    Console.WriteLine(enumeratorRawViaIEnumerable.Current.ToString());
}

private IEnumerator GetIEnumerableRaw()
{
    return new[] { 123, 32, 4 }.GetEnumerator();
}

private IEnumerator<int> GetIEnumeratorViaIEnumerable()
{
    int[] intData = new[] { 123, 32, 4 };
    return (IEnumerator<int>)intData.GetEnumerator();
}";

            var analyser = new EnumeratorAllocationAnalyzer();
            var expectedNodes = ImmutableArray.Create(SyntaxKind.InvocationExpression);
            var info = ProcessCode(analyser, sampleProgram, expectedNodes);

            Assert.AreEqual(1, info.Allocations.Count);
            // Diagnostic: (11,35): warning HeapAnalyzerEnumeratorAllocationRule: Non-ValueType enumerator may result in a heap allocation ***
            AssertEx.ContainsDiagnostic(info.Allocations, id: EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule.Id, line: 11, character: 35);
        }

        [TestMethod]
        public void EnumeratorAllocation_IterateOverString_NoWarning()
        {
            var sampleProgram = "foreach (char c in \"foo\") { }";

            var analyser = new EnumeratorAllocationAnalyzer();
            var info = ProcessCode(analyser, sampleProgram, ImmutableArray.Create(SyntaxKind.ForEachStatement));

            Assert.AreEqual(0, info.Allocations.Count);
        }
    }
}

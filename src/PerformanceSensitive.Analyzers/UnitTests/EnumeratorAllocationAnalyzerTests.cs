// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using PerformanceSensitive.CSharp.Analyzers;
using Xunit;

namespace PerformanceSensitive.Analyzers.UnitTests
{
    public class EnumeratorAllocationAnalyzerTests : AllocationAnalyzerTestsBase
    {
        [Fact]
        public void EnumeratorAllocation_Basic()
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
        }
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(25,24): warning HAA0401: Non-ValueType enumerator may result in a heap allocation
                        GetCSharpResultAt(25, 24, EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule),
                        // Test0.cs(30,24): warning HAA0401: Non-ValueType enumerator may result in a heap allocation
                        GetCSharpResultAt(30, 24, EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule));
        }

        [Fact]
        public void EnumeratorAllocation_Advanced()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
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
        }
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(17,24): warning HAA0401: Non-ValueType enumerator may result in a heap allocation
                        GetCSharpResultAt(17, 24, EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule));
        }

        [Fact]
        public void EnumeratorAllocation_Via_InvocationExpressionSyntax()
        {
            var sampleProgram =
@"using System.Collections.Generic;
using System.Collections;
using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
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
    }

    private IEnumerator GetIEnumerableRaw()
    {
        return new[] { 123, 32, 4 }.GetEnumerator();
    }

    private IEnumerator<int> GetIEnumeratorViaIEnumerable()
    {
        int[] intData = new[] { 123, 32, 4 };
        return (IEnumerator<int>)intData.GetEnumerator();
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true,
                        // Test0.cs(17,43): warning HAA0401: Non-ValueType enumerator may result in a heap allocation
                        GetCSharpResultAt(17, 43, EnumeratorAllocationAnalyzer.ReferenceTypeEnumeratorRule));
        }

        [Fact]
        public void EnumeratorAllocation_IterateOverString_NoWarning()
        {
            var sampleProgram =
@"using System;
using Roslyn.Utilities;

public class MyClass
{
    [PerformanceSensitive(""uri"")]
    public void Foo() 
    {
        foreach (char c in ""foo"") { };
    }
}";
            VerifyCSharp(sampleProgram, withAttribute: true);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnumeratorAllocationAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            throw new System.NotImplementedException();
        }
    }
}

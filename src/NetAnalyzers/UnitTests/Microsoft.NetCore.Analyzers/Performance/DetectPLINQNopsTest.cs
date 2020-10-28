// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DetectPLINQNopsAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.DetectPLINQNopsFixer>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DetectPLINQNopsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public sealed class DetectPLINQNopsTest
    {
        [Fact]
        public async Task AsParallelToListInForeach_SingleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()"));
        }

        [Fact]
        public async Task AsParallelAtStart_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in Enumerable.Range(0,1).AsParallel().Select(x => x*2).ToList());}
    }
}");
        }

        [Fact]
        public async Task AsParallelToListAtEndOfGenericMethod_SingleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test<T>(IEnumerable<T> enumerable) { foreach(var s in {|#0:enumerable.AsParallel().ToList()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("enumerable.AsParallel().ToList()"));
        }

        [Fact]
        public async Task AsParallelAtEndOfGenericMethod_SingleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
            public void Test<T>(IEnumerable<T> enumerable) { foreach(var s in {|#0:enumerable.AsParallel()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("enumerable.AsParallel()"));
        }

        [Fact]
        public async Task VB_AsParallelAtEnd_SingleDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ConsoleApplication1
    Class TypeName
        Public Sub Test()
            For Each s In {|#0:Enumerable.Range(0, 1).Select(Function(x) x * 2).AsParallel()|}.ToList()
            Next
        End Sub
    End Class
End Namespace", VerifyVB.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0, 1).Select(Function(x) x * 2).AsParallel()"));
        }

        [Fact]
        public async Task AsParallelToArrayInForeach_SingleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToArray()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToArray()"));
        }

        [Fact]
        public async Task AsParallelToListInForeach_SingleFix()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()"),
    @"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in Enumerable.Range(0,1).Select(x => x*2));}
    }
}");
        }

        [Fact]
        public async Task AsParallelInForeach_SingleFix()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel()|});}
    }
}", VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel()"),
    @"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in Enumerable.Range(0,1).Select(x => x*2));}
    }
}");
        }

        [Fact]
        public async Task AsParallelToSetInForeach_SingleFix()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToHashSet()|});}
        public void Test2() { foreach(var s in {|#1:Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToDictionary(x => x, y=> y)|});}
    }
}", new[]{VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToHashSet()"),
                VerifyCS.Diagnostic(DetectPLINQNopsAnalyzer.DefaultRule).WithLocation(1).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToDictionary(x => x, y=> y)")},
    @"
using System;
using System.Collections.Generic;
using System.Linq;
namespace ConsoleApplication1
{
    class TypeName
    {   
        public void Test() { foreach(var s in Enumerable.Range(0,1).Select(x => x*2).ToHashSet());}
        public void Test2() { foreach(var s in Enumerable.Range(0,1).Select(x => x*2).ToDictionary(x => x, y=> y));}
    }
}");
        }
    }
}

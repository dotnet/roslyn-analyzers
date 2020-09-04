// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Usage;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.DetectPLINQNops,
    Microsoft.NetCore.Analyzers.Usage.DetectPLINQNopsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.DetectPLINQNops,
    Microsoft.NetCore.Analyzers.Usage.DetectPLINQNopsFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Usage
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
    }", VerifyCS.Diagnostic(DetectPLINQNops.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()"));
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
    }", VerifyCS.Diagnostic(DetectPLINQNops.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToArray()"));
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
    }", VerifyCS.Diagnostic(DetectPLINQNops.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel().ToList()"),
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
    }", VerifyCS.Diagnostic(DetectPLINQNops.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel()"),
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
        public async Task AsParallelAtEnd_SingleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
                public void Test() {foreach(var s in {|#0:Enumerable.Range(0,1).Select(x => x*2).AsParallel()|});}
        }
    }", VerifyCS.Diagnostic(DetectPLINQNops.DefaultRule).WithLocation(0).WithArguments("Enumerable.Range(0,1).Select(x => x*2).AsParallel()"));
        }
    }
}

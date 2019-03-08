// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.Runtime;
using Microsoft.NetCore.VisualBasic.Analyzers.Runtime;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer();
        }

        [Fact]
        public void CA1826FixEnumerableFirstExtensionCallCSharp()
        {
            VerifyCSharpFix(@"
using System;
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var list = GetList();
        var matrix = new[] { list };
        var f1 = list.First();
        var f2 = GetList().First();
        var f3 = matrix[0].First();
        Console.WriteLine(list.First());
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var list = GetList();
        var matrix = new[] { list };
        var f1 = list[0];
        var f2 = GetList()[0];
        var f3 = matrix[0][0];
        Console.WriteLine(list[0]);
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA1826FixEnumerableFirstStaticCallCSharp()
        {
            VerifyCSharpFix(@"
using System;
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var list = GetList();
        var matrix = new[] { list };
        var f1 = Enumerable.First(list);
        var f2 = Enumerable.First(GetList());
        var f3 = Enumerable.First(matrix[0]);
        Console.WriteLine(Enumerable.First(list));
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var list = GetList();
        var matrix = new[] { list };
        var f1 = list[0];
        var f2 = GetList()[0];
        var f3 = matrix[0][0];
        Console.WriteLine(list[0]);
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA1826FixEnumerableFirstMethodChainCallWrongFormattingCSharp()
        {
            //this unit test documents a problematic edge case which needs to be discussed and addressed

            VerifyCSharpFix(@"
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var f = GetList()
            .First();

        var s = GetList()
            .First()
            .ToString();
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", @"
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        var f = GetList()[0]
;

        var s = GetList()[0]
            .ToString();
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CA1826FixEnumerableFirstProblematicSyntaxCallCSharp()
        {
            //this unit test documents a problematic edge case which needs to be discussed and addressed

            VerifyCSharpFix(@"
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        GetList().First();
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", @"
using System.Linq;
using System.Collections.Generic;
class C
{
    void M()
    {
        GetList()[0];
    }

    IReadOnlyList<int> GetList()
    {
        return new List<int> { 1, 2, 3 };
    }
}
", allowNewCompilerDiagnostics: true, validationMode: TestValidationMode.AllowCompileErrors);
        }
    }
}
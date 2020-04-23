// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferContainsKeyOrContainsValue,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferContainsKeyOrContainsValueTests
    {
        [Theory]
        [InlineData("System.Collections.Generic.IDictionary")]
        [InlineData("System.Collections.Generic.IReadOnlyDictionary")]
        [InlineData("System.Collections.Generic.Dictionary")]
        [InlineData("System.Collections.Generic.SortedDictionary")]
        [InlineData("System.Collections.Concurrent.ConcurrentDictionary")]
        [InlineData("System.Collections.Immutable.IImmutableDictionary")]
        [InlineData("System.Collections.Immutable.ImmutableDictionary")]
        [InlineData("System.Collections.Immutable.ImmutableSortedDictionary")]
        [InlineData("System.Collections.ObjectModel.ReadOnlyDictionary")]
        public async Task CA1835_KeysContainsAnyDictionary_Diagnostic(string dictionaryFullTypeName)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
using System.Linq;

public class C
{{
    public void M({dictionaryFullTypeName}<int, int> dic)
    {{
        [|dic.Keys.Contains(42)|];
    }}
}}");
        }

        [Theory]
        [InlineData("System.Collections.Generic.Dictionary")]
        [InlineData("System.Collections.Generic.SortedDictionary")]
        [InlineData("System.Collections.Immutable.ImmutableDictionary")]
        [InlineData("System.Collections.Immutable.ImmutableSortedDictionary")]
        public async Task CA1835_ValuesContainsDictionaryWithContainsValue_Diagnostic(string dictionaryFullTypeName)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
using System.Linq;

public class C
{{
    public void M({dictionaryFullTypeName}<int, int> dic)
    {{
        [|dic.Values.Contains(42)|];
    }}
}}");
        }

        [Theory]
        [InlineData("System.Collections.Generic.IDictionary")]
        [InlineData("System.Collections.Generic.IReadOnlyDictionary")]
        [InlineData("System.Collections.Concurrent.ConcurrentDictionary")]
        [InlineData("System.Collections.Immutable.IImmutableDictionary")]
        [InlineData("System.Collections.ObjectModel.ReadOnlyDictionary")]
        public async Task CA1835_ValuesContainsDictionaryWithoutContainsValue_NoDiagnostic(string dictionaryFullTypeName)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
using System.Linq;

public class C
{{
    public void M({dictionaryFullTypeName}<int, int> dic)
    {{
        dic.Values.Contains(42);
    }}
}}");
        }

        [Fact]
        public async Task CA1835_KeysContainsNotADictionary_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class A
{
    public IEnumerable<int> Keys { get; }
}

public class C
{
    public void M(A a)
    {
        a.Keys.Contains(42);
    }
}");
        }

        [Fact]
        public async Task CA1835_ValuesContainsNotADictionary_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Collections.Generic;
using System.Linq;

public class A
{
    public IEnumerable<int> Values { get; }
}

public class C
{
    public void M(A a)
    {
        a.Values.Contains(42);
    }
}");
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.UseSymbolComparerInCollectionsAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.UseSymbolComparerInCollectionsFix>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.UseSymbolComparerInCollectionsAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.UseSymbolComparerInCollectionsFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class UseSymbolComparerInCollectionsTests
    {
        private const string SymbolEqualityComparerStubCSharp =
@"
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis
{
    public class SymbolEqualityComparer : IEqualityComparer<ISymbol>
    {
        public static readonly SymbolEqualityComparer Default = new SymbolEqualityComparer();

        private SymbolEqualityComparer()
        {
        }

        public bool Equals(ISymbol x, ISymbol y)
        {
            throw new System.NotImplementedException();
        }

        public int GetHashCode(ISymbol obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task ImmutableArray_BinarySearch_UseComparer(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    int Method(ImmutableArray<{symbolType}> arr, {symbolType} value) {{
        return [|arr.BinarySearch(value)|];
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    int Method(ImmutableArray<{symbolType}> arr, {symbolType} value) {{
        return arr.BinarySearch(value, SymbolEqualityComparer.Default);
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { source, SymbolEqualityComparerStubCSharp } },
                FixedState = { Sources = { fixedSource, SymbolEqualityComparerStubCSharp } },
            }.RunAsync();
        }

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task ImmutableDictionary_BinarySearch_UseComparer(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    ImmutableDictionary<{symbolType}, string> Method() {{
        return [|ImmutableDictionary.Create<{symbolType}, string>()|];
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    ImmutableDictionary<{symbolType}, string> Method() {{
        return ImmutableDictionary.Create<{symbolType}, string>(SymbolEqualityComparer.Default);
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { source, SymbolEqualityComparerStubCSharp } },
                FixedState = { Sources = { fixedSource, SymbolEqualityComparerStubCSharp } },
            }.RunAsync();
        }
    }
}

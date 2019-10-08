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
        public async Task ImmutableDictionary_Create_UseComparer(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method() {{
        return [|ImmutableDictionary.Create<{symbolType}, string>()|];
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method() {{
        return ImmutableDictionary.Create<{symbolType}, string>(SymbolEqualityComparer.Default);
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { source, SymbolEqualityComparerStubCSharp } },
                FixedState = { Sources = { fixedSource, SymbolEqualityComparerStubCSharp } }
            }.RunAsync();
        }

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task ImmutableDictionary_CreateBuilder_UseComparer(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method() {{
        return [|ImmutableDictionary.CreateBuilder<{symbolType}, string>()|];
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method() {{
        return ImmutableDictionary.CreateBuilder<{symbolType}, string>(SymbolEqualityComparer.Default);
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { source, SymbolEqualityComparerStubCSharp } },
                FixedState = { Sources = { fixedSource, SymbolEqualityComparerStubCSharp } }
            }.RunAsync();
        }

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task ImmutableDictionary_ToImmutableDictionary_UseComparer(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method(ImmutableDictionary<{symbolType}, string> dict) {{
        return [|dict.ToImmutableDictionary()|];
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

class TestClass {{
    object Method(ImmutableDictionary<{symbolType}, string> dict) {{
        return [|dict.ToImmutableDictionary(SymbolEqualityComparer.Default)|];
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { source, SymbolEqualityComparerStubCSharp } },
                FixedState = { Sources = { fixedSource, SymbolEqualityComparerStubCSharp } }
            }.RunAsync();
        }
    }
}

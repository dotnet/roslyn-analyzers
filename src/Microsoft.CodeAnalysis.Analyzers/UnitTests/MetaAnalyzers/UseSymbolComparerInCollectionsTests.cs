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

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task Enumerable_Contains_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    bool Method(IEnumerable<{symbolType}> e, {symbolType} s) {{
        return [|e.Contains(s)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    bool Method(IEnumerable<{symbolType}> e, {symbolType} s) {{
        return e.Contains(s, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_Distinct_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return [|e.Distinct()|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return e.Distinct(SymbolEqualityComparer.Default);
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
        public async Task Enumerable_GroupBy_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return [|e.GroupBy(
            s => s,
            s => s,
            (s1, s2) => s1
            )|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return e.GroupBy(
            s => s,
            s => s,
            (s1, s2) => s1
, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_GroupJoin_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.GroupJoin(
            e2,
            s => s,
            s => s,
            (s1, s2) => s1
            )|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return e1.GroupJoin(
            e2,
            s => s,
            s => s,
            (s1, s2) => s1
, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_Intersect_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.Intersect(e2)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return e1.Intersect(e2, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_Join_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.Join(
            e2,
            s => s,
            s => s,
            (s1, s2) => s1)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.Join(
            e2,
            s => s,
            s => s,
            (s1, s2) => s1, SymbolEqualityComparer.Default)|];
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
        public async Task Enumerable_Union_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.Union(e2)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return e1.Union(e2, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_SequenceEqual_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return [|e1.SequenceEqual(e2)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e1, IEnumerable<{symbolType}> e2) {{
        return e1.SequenceEqual(e2, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_ToDictionary_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return [|e.ToDictionary(
                s => s,
                s => s)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return e.ToDictionary(
                s => s,
                s => s, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_ToLookup_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return [|e.ToLookup(
                s => s,
                s => s)|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return e.ToLookup(
                s => s,
                s => s, SymbolEqualityComparer.Default);
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
        public async Task Enumerable_ToHashSet_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return [|e.ToHashSet()|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method(IEnumerable<{symbolType}> e) {{
        return e.ToHashSet(SymbolEqualityComparer.Default);
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
        public async Task Dictionary_Construct_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method() {{
        return [|new Dictionary<{symbolType}, {symbolType}>()|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method() {{
        return new Dictionary<{symbolType}, {symbolType}>(SymbolEqualityComparer.Default);
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
        public async Task HashSet_Construct_UseComparer(string symbolType)
        {
            var source = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method() {{
        return [|new HashSet<{symbolType}>()|];
    }}
}}
";
            var fixedSource = $@"
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

class TestClass {{
    object Method() {{
        return new HashSet<{symbolType}>(SymbolEqualityComparer.Default);
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

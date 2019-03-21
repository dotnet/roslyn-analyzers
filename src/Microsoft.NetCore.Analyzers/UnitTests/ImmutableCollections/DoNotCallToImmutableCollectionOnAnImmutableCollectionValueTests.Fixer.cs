// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.ImmutableCollections;
using Microsoft.NetCore.VisualBasic.Analyzers.ImmutableCollections;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;
using System.Collections.Immutable;
using Test.Utilities.MinimalImplementations;

namespace Microsoft.NetCore.Analyzers.ImmutableCollections.UnitTests
{
    public class DoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer();
        }

        public static readonly TheoryData<string> CollectionNames_Arity1 = new TheoryData<string>
        {
            nameof(ImmutableArray),
            nameof(ImmutableHashSet),
            nameof(ImmutableList),
            nameof(ImmutableSortedSet)
        };

        public static readonly TheoryData<string> CollectionNames_Arity2 = new TheoryData<string>
        {
            nameof(ImmutableDictionary),
            nameof(ImmutableSortedDictionary)
        };

        [Theory]
        [MemberData(nameof(CollectionNames_Arity1))]
        public void CA2009_Arity1_CSharp(string collectionName)
        {
            var initial = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

static class Extensions
{{
     public static {collectionName}<TSource> To{collectionName}<TSource>(this IEnumerable<TSource> items)
     {{
         return default({collectionName}<TSource>);
     }}
}}

class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        var x = p1.To{collectionName}().To{collectionName}();
        var y = p3.To{collectionName}();
    }}
}}";

            var expected = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

static class Extensions
{{
     public static {collectionName}<TSource> To{collectionName}<TSource>(this IEnumerable<TSource> items)
     {{
         return default({collectionName}<TSource>);
     }}
}}

class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        var x = p1.To{collectionName}();
        var y = p3;
    }}
}}";
            VerifyCSharpFix(new[] { initial, ImmutableCollectionsSource.CSharp }, new[] { expected, ImmutableCollectionsSource.CSharp });
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity1))]
        public void CA2009_Arity1_Basic(string collectionName)
        {
            var initial = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Module Extensions
	<System.Runtime.CompilerServices.Extension> _
	Public Function To{collectionName}(Of TSource)(items As IEnumerable(Of TSource)) As {collectionName}(Of TSource)
		Return Nothing
	End Function
End Module

Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		Dim x = p1.To{collectionName}().To{collectionName}()
		Dim y = p3.To{collectionName}()
	End Sub
End Class";

            var expected = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Module Extensions
	<System.Runtime.CompilerServices.Extension> _
	Public Function To{collectionName}(Of TSource)(items As IEnumerable(Of TSource)) As {collectionName}(Of TSource)
		Return Nothing
	End Function
End Module

Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		Dim x = p1.To{collectionName}()
		Dim y = p3
	End Sub
End Class";
            VerifyBasicFix(new[] { initial, ImmutableCollectionsSource.Basic }, new[] { expected, ImmutableCollectionsSource.Basic });
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void CA2009_Arity2_CSharp(string collectionName)
        {
            var initial = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

static class Extensions
{{
     public static {collectionName}<TKey, TValue> To{collectionName}<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
     {{
         return default({collectionName}<TKey, TValue>);
     }}
}}

class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        var x = p1.To{collectionName}().To{collectionName}();
        var y = p3.To{collectionName}();
    }}
}}";

            var expected = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

static class Extensions
{{
     public static {collectionName}<TKey, TValue> To{collectionName}<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
     {{
         return default({collectionName}<TKey, TValue>);
     }}
}}

class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        var x = p1.To{collectionName}();
        var y = p3;
    }}
}}";
            VerifyCSharpFix(new[] { initial, ImmutableCollectionsSource.CSharp }, new[] { expected, ImmutableCollectionsSource.CSharp });
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void CA2009_Arity2_Basic(string collectionName)
        {
            var initial = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Module Extensions
	<System.Runtime.CompilerServices.Extension> _
	Public Function To{collectionName}(Of TKey, TValue)(items As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As {collectionName}(Of TKey, TValue)
		Return Nothing
	End Function
End Module

Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		Dim x = p1.To{collectionName}().To{collectionName}()
		Dim y = p3.To{collectionName}()
	End Sub
End Class";

            var expected = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Module Extensions
	<System.Runtime.CompilerServices.Extension> _
	Public Function To{collectionName}(Of TKey, TValue)(items As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As {collectionName}(Of TKey, TValue)
		Return Nothing
	End Function
End Module

Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		Dim x = p1.To{collectionName}()
		Dim y = p3
	End Sub
End Class";
            VerifyBasicFix(new[] { initial, ImmutableCollectionsSource.Basic }, new[] { expected, ImmutableCollectionsSource.Basic });
        }
    }
}
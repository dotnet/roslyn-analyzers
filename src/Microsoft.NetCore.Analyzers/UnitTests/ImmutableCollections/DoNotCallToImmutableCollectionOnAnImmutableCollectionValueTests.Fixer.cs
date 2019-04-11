// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer();
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

class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        var a = p1.To{collectionName}().To{collectionName}();
        var b = p3.To{collectionName}();
        var c = ImmutableExtensions.To{collectionName}(ImmutableExtensions.To{collectionName}(p1));
        var d = ImmutableExtensions.To{collectionName}(p3);
    }}
}}";

            var expected = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        var a = p1.To{collectionName}();
        var b = p3;
        var c = ImmutableExtensions.To{collectionName}(p1);
        var d = p3;
    }}
}}";
            VerifyCSharpFix(new[] { initial, ImmutableCollectionsSource.CSharp }, new[] { expected, ImmutableCollectionsSource.CSharp }, referenceFlags: ReferenceFlags.RemoveImmutable);
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity1))]
        public void CA2009_Arity1_Basic(string collectionName)
        {
            var initial = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		Dim a = p1.To{collectionName}().To{collectionName}()
		Dim b = p3.To{collectionName}()
		Dim c = ImmutableExtensions.To{collectionName}(ImmutableExtensions.To{collectionName}(p1))
		Dim d = ImmutableExtensions.To{collectionName}(p3)
	End Sub
End Class";

            var expected = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		Dim a = p1.To{collectionName}()
		Dim b = p3
		Dim c = ImmutableExtensions.To{collectionName}(p1)
		Dim d = p3
	End Sub
End Class";
            VerifyBasicFix(new[] { initial, ImmutableCollectionsSource.Basic }, new[] { expected, ImmutableCollectionsSource.Basic }, referenceFlags: ReferenceFlags.RemoveImmutable);
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void CA2009_Arity2_CSharp(string collectionName)
        {
            var initial = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        var a = p1.To{collectionName}().To{collectionName}();
        var b = p3.To{collectionName}();
        var c = ImmutableExtensions.To{collectionName}(ImmutableExtensions.To{collectionName}(p1));
        var d = ImmutableExtensions.To{collectionName}(p3);
    }}
}}";

            var expected = $@"
using System.Collections.Generic;
using System.Collections.Immutable;

class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        var a = p1.To{collectionName}();
        var b = p3;
        var c = ImmutableExtensions.To{collectionName}(p1);
        var d = p3;
    }}
}}";
            VerifyCSharpFix(new[] { initial, ImmutableCollectionsSource.CSharp }, new[] { expected, ImmutableCollectionsSource.CSharp }, referenceFlags: ReferenceFlags.RemoveImmutable);
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void CA2009_Arity2_Basic(string collectionName)
        {
            var initial = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		Dim a = p1.To{collectionName}().To{collectionName}()
		Dim b = p3.To{collectionName}()
		Dim c = ImmutableExtensions.To{collectionName}(ImmutableExtensions.To{collectionName}(p1))
		Dim d = ImmutableExtensions.To{collectionName}(p3)
	End Sub
End Class";

            var expected = $@"
Imports System.Collections.Generic
Imports System.Collections.Immutable

Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		Dim a = p1.To{collectionName}()
		Dim b = p3
		Dim c = ImmutableExtensions.To{collectionName}(p1)
		Dim d = p3
	End Sub
End Class";
            VerifyBasicFix(new[] { initial, ImmutableCollectionsSource.Basic }, new[] { expected, ImmutableCollectionsSource.Basic }, referenceFlags: ReferenceFlags.RemoveImmutable);
        }
    }
}
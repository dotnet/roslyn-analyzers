// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.ImmutableCollections.UnitTests
{
    public class DoNotCallToImmutableCollectionOnAnImmutableCollectionValueTests : DiagnosticAnalyzerTestBase
    {
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

        #region CSharpCollectionsDefinition

        private const string CSharpCollectionsDefinition = @"
using System.Collections.Generic;
using System.Collections.Immutable;
using static System.Collections.Immutable.ImmutableExtensions;

namespace System.Collections.Immutable
{
    public sealed partial class ImmutableArray<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ImmutableList<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ImmutableHashSet<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ImmutableSortedSet<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ImmutableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ImmutableSortedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public static class ImmutableExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerable<T> source)
        {
            return null;
        }

        public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return null;
        }

        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
        {
            return null;
        }

        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return null;
        }

        public static ImmutableHashSet<T> ToImmutableHashSet<T>(this IEnumerable<T> source)
        {
            return null;
        }

        public static ImmutableHashSet<T> ToImmutableHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return null;
        }

        public static ImmutableSortedSet<T> ToImmutableSortedSet<T>(this IEnumerable<T> source)
        {
            return null;
        }

        public static ImmutableSortedSet<T> ToImmutableSortedSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return null;
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return null;
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
        {
            return null;
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return null;
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
        {
            return null;
        }
    }
}
";
        private const string VisualBasicCollectionsDefinition = @"
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Immutable
Imports System.Collections.Immutable.ImmutableExtensions
Imports System.Runtime.CompilerServices

Namespace System.Collections.Immutable

    Partial Public NotInheritable Class ImmutableArray(Of T)
        Implements IEnumerable(Of T)

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Partial Public NotInheritable Class ImmutableList(Of T)
        Implements IEnumerable(Of T)
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Partial Public NotInheritable Class ImmutableHashSet(Of T)
        Implements IEnumerable(Of T)
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Partial Public NotInheritable Class ImmutableSortedSet(Of T)
        Implements IEnumerable(Of T)
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Partial Public NotInheritable Class ImmutableDictionary(Of TKey, TValue)
        Implements IEnumerable(Of KeyValuePair(Of TKey, TValue))
        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Partial Public NotInheritable Class ImmutableSortedDictionary(Of TKey, TValue)
        Implements IEnumerable(Of KeyValuePair(Of TKey, TValue))
        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class

    Module ImmutableExtensions
        <Extension()>
        Function ToImmutableArray(Of T)(ByVal source As IEnumerable(Of T)) As ImmutableArray(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableArray(Of T)(ByVal source As IEnumerable(Of T), ByVal comparer As IEqualityComparer(Of T)) As ImmutableArray(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableList(Of T)(ByVal source As IEnumerable(Of T)) As ImmutableList(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableList(Of T)(ByVal source As IEnumerable(Of T), ByVal comparer As IEqualityComparer(Of T)) As ImmutableList(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableHashSet(Of T)(ByVal source As IEnumerable(Of T)) As ImmutableHashSet(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableHashSet(Of T)(ByVal source As IEnumerable(Of T), ByVal comparer As IEqualityComparer(Of T)) As ImmutableHashSet(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableSortedSet(Of T)(ByVal source As IEnumerable(Of T)) As ImmutableSortedSet(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableSortedSet(Of T)(ByVal source As IEnumerable(Of T), ByVal comparer As IEqualityComparer(Of T)) As ImmutableSortedSet(Of T)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableDictionary(Of TKey, TValue)(ByVal source As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As ImmutableDictionary(Of TKey, TValue)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableDictionary(Of TKey, TValue)(ByVal source As IEnumerable(Of KeyValuePair(Of TKey, TValue)), ByVal keyComparer As IEqualityComparer(Of TKey)) As ImmutableDictionary(Of TKey, TValue)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableSortedDictionary(Of TKey, TValue)(ByVal source As IEnumerable(Of KeyValuePair(Of TKey, TValue))) As ImmutableSortedDictionary(Of TKey, TValue)
            Return Nothing
        End Function

        <Extension()>
        Function ToImmutableSortedDictionary(Of TKey, TValue)(ByVal source As IEnumerable(Of KeyValuePair(Of TKey, TValue)), ByVal keyComparer As IEqualityComparer(Of TKey)) As ImmutableSortedDictionary(Of TKey, TValue)
            Return Nothing
        End Function
    End Module
End Namespace
";
        #endregion

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer();
        }

        #region No Diagnostic Tests

        [Theory]
        [MemberData(nameof(CollectionNames_Arity1))]
        public void NoDiagnosticCases_Arity1(string collectionName)
        {
            VerifyCSharp(CSharpCollectionsDefinition + $@"
class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        // Allowed
        p1.To{collectionName}();
        p2.To{collectionName}();

        // No dataflow
        IEnumerable<int> l1 = p3;
        l1.To{collectionName}();
    }}
}}
");

            VerifyBasic(VisualBasicCollectionsDefinition + $@"
Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		' Allowed
		p1.To{collectionName}()
		p2.To{collectionName}()

		' No dataflow
		Dim l1 As IEnumerable(Of Integer) = p3
		l1.To{collectionName}()
	End Sub
End Class
");
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void NoDiagnosticCases_Arity2(string collectionName)
        {
            VerifyCSharp(CSharpCollectionsDefinition + $@"
class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        // Allowed
        p1.To{collectionName}();
        p2.To{collectionName}();

        // No dataflow
        IEnumerable<KeyValuePair<int, int>> l1 = p3;
        l1.To{collectionName}();
    }}
}}
");

            VerifyBasic(VisualBasicCollectionsDefinition + $@"
Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		' Allowed
		p1.To{collectionName}()
		p2.To{collectionName}()

		' No dataflow
		Dim l1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)) = p3
		l1.To{collectionName}()
	End Sub
End Class
");
        }

        #endregion

        #region Diagnostic Tests

        [Theory]
        [MemberData(nameof(CollectionNames_Arity1))]
        public void DiagnosticCases_Arity1(string collectionName)
        {
            VerifyCSharp(CSharpCollectionsDefinition + $@"
class C
{{
    public void M(IEnumerable<int> p1, List<int> p2, {collectionName}<int> p3)
    {{
        p1.To{collectionName}().To{collectionName}();
        p3.To{collectionName}();
    }}
}}
",
    // Test0.cs(154,9): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetCSharpResultAt(154, 9, collectionName),
    // Test0.cs(155,9): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetCSharpResultAt(155, 9, collectionName));

            VerifyBasic(VisualBasicCollectionsDefinition + $@"
Class C
	Public Sub M(p1 As IEnumerable(Of Integer), p2 As List(Of Integer), p3 As {collectionName}(Of Integer))
		p1.To{collectionName}().To{collectionName}()
		p3.To{collectionName}()
	End Sub
End Class
",
    // Test0.vb(143,3): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetBasicResultAt(143, 3, collectionName),
    // Test0.vb(144,3): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetBasicResultAt(144, 3, collectionName));
        }

        [Theory]
        [MemberData(nameof(CollectionNames_Arity2))]
        public void DiagnosticCases_Arity2(string collectionName)
        {
            VerifyCSharp(CSharpCollectionsDefinition + $@"
class C
{{
    public void M(IEnumerable<KeyValuePair<int, int>> p1, List<KeyValuePair<int, int>> p2, {collectionName}<int, int> p3)
    {{
        p1.To{collectionName}().To{collectionName}();
        p3.To{collectionName}();
    }}
}}
",
    // Test0.cs(154,9): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetCSharpResultAt(154, 9, collectionName),
    // Test0.cs(155,9): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetCSharpResultAt(155, 9, collectionName));

            VerifyBasic(VisualBasicCollectionsDefinition + $@"
Class C
	Public Sub M(p1 As IEnumerable(Of KeyValuePair(Of Integer, Integer)), p2 As List(Of KeyValuePair(Of Integer, Integer)), p3 As {collectionName}(Of Integer, Integer))
		p1.To{collectionName}().To{collectionName}()
		p3.To{collectionName}()
	End Sub
End Class
",
    // Test0.vb(143,3): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetBasicResultAt(143, 3, collectionName),
    // Test0.vb(144,3): warning RS0012: Do not call ToImmutableCollection on an ImmutableCollection value
    GetBasicResultAt(144, 3, collectionName));
        }

        #endregion

        private static DiagnosticResult GetCSharpResultAt(int line, int column, string collectionName)
        {
            return GetCSharpResultAt(
                line,
                column,
                DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer.RuleId,
                string.Format(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableCollectionOnAnImmutableCollectionValueMessage, $"To{collectionName}", collectionName));
        }

        private static DiagnosticResult GetBasicResultAt(int line, int column, string collectionName)
        {
            return GetBasicResultAt(
                line,
                column,
                DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer.RuleId,
                string.Format(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableCollectionOnAnImmutableCollectionValueMessage, $"To{collectionName}", collectionName));
        }
    }
}
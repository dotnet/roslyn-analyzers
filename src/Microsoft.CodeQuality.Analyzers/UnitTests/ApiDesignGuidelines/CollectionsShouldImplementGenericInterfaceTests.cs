// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class CollectionsShouldImplementGenericInterfaceTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new CollectionsShouldImplementGenericInterfaceAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CollectionsShouldImplementGenericInterfaceAnalyzer();
        }

        [Fact]
        public void Test_WithCollectionBase()
        {
            VerifyCSharp(@"
using System.Collections;

public class TestClass : CollectionBase { }",
                GetCSharpResultAt(4, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
Imports System.Collections

Public Class TestClass 
    Inherits CollectionBase
End Class
",
                GetBasicResultAt(4, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void Test_WithCollectionBase_Internal()
        {
            VerifyCSharp(@"
using System.Collections;

internal class TestClass : CollectionBase { }");

            VerifyBasic(@"
Imports System.Collections

Friend Class TestClass 
    Inherits CollectionBase
End Class
");
        }

        [Fact]
        public void Test_WithCollection()
        {
            VerifyCSharp(@"
using System;
using System.Collections;

public class TestClass : ICollection
{
    public int Count => 0;
    public object SyncRoot => null;
    public bool IsSynchronized => false;

    public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
    public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
}
",
                GetCSharpResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
Imports System
Imports System.Collections

Public Class TestClass
	Implements ICollection

    Public ReadOnly Property Count As Integer Implements ICollection.Count
    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException
    End Sub
End Class
",
                GetBasicResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void Test_WithCollection_Internal()
        {
            VerifyCSharp(@"
using System;
using System.Collections;

internal class TestClass : ICollection
{
    public int Count => 0;
    public object SyncRoot => null;
    public bool IsSynchronized => false;

    public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
    public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections

Friend Class TestClass
	Implements ICollection

    Public ReadOnly Property Count As Integer Implements ICollection.Count
    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException
    End Sub
End Class
");
        }

        [Fact]
        public void Test_WithEnumerable()
        {
            VerifyCSharp(@"
using System;
using System.Collections;

public class TestClass : IEnumerable
{
    public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
}
",
                GetCSharpResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
Imports System
Imports System.Collections

Public Class TestClass
    Implements IEnumerable

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function
End Class
",
                GetBasicResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithList()
        {
            VerifyCSharp(@"
using System;
using System.Collections;

public class TestClass : IList
{
    public int Count => 0;
    public object SyncRoot => null;
    public bool IsSynchronized => false;
    public bool IsReadOnly => false;
    public bool IsFixedSize => false;

    public object this[int index]
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
    public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
    public int Add(object value) { throw new NotImplementedException(); }
    public bool Contains(object value) { throw new NotImplementedException(); }
    public void Clear() { throw new NotImplementedException(); }
    public int IndexOf(object value) { throw new NotImplementedException(); }
    public void Insert(int index, object value) { throw new NotImplementedException(); }
    public void Remove(object value) { throw new NotImplementedException(); }
    public void RemoveAt(int index) { throw new NotImplementedException(); }
}
",
                GetCSharpResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
Imports System
Imports System.Collections

Public Class TestClass
	Implements IList

    Default Public Property Item(index As Integer) As Object Implements IList.Item
        Get
            Throw New NotImplementedException
        End Get
        Set
            Throw New NotImplementedException
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection.Count
    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
    Public ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
    Public ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException
    End Sub

    Public Function Add(value As Object) As Integer Implements IList.Add
        Throw New NotImplementedException
    End Function

    Public Function Contains(value As Object) As Boolean Implements IList.Contains
        Throw New NotImplementedException
    End Function

    Public Sub Clear() Implements IList.Clear
        Throw New NotImplementedException
    End Sub

    Public Function IndexOf(value As Object) As Integer Implements IList.IndexOf
        Throw New NotImplementedException
    End Function

    Public Sub Insert(index As Integer, value As Object) Implements IList.Insert
        Throw New NotImplementedException
    End Sub

    Public Sub Remove(value As Object) Implements IList.Remove
        Throw New NotImplementedException
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList.RemoveAt
        Throw New NotImplementedException
    End Sub
End Class
",
                GetBasicResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithGenericCollection()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class TestClass : ICollection<int>
{
    public int Count => 0;
    public bool IsReadOnly => false;

    public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }
    IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
    public void Add(int item) { throw new NotImplementedException(); }
    public void Clear() { throw new NotImplementedException(); }
    public bool Contains(int item) { throw new NotImplementedException(); }
    public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }
    public bool Remove(int item) { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Public Class TestClass
	Implements ICollection(Of Integer)

    Public ReadOnly Property Count As Integer Implements ICollection(Of Integer).Count
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Integer).IsReadOnly

    Public Function IEnumerable_GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub Add(item As Integer) Implements ICollection(Of Integer).Add
        Throw New NotImplementedException
    End Sub

    Public Sub Clear() Implements ICollection(Of Integer).Clear
        Throw New NotImplementedException
    End Sub

    Public Function Contains(item As Integer) As Boolean Implements ICollection(Of Integer).Contains
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Integer(), arrayIndex As Integer) Implements ICollection(Of Integer).CopyTo
        Throw New NotImplementedException
    End Sub

    Public Function Remove(item As Integer) As Boolean Implements ICollection(Of Integer).Remove
        Throw New NotImplementedException
    End Function
End Class
");
        }

        [Fact]
        public void Test_WithGenericEnumerable()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class TestClass : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }
    IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Public Class TestClass
	Implements IEnumerable(Of Integer)

    Public Function IEnumerable_GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function
End Class
");
        }

        [Fact]
        public void Test_WithGenericList()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class TestClass : IList<int>
{
    public int this[int index]
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public int Count => 0;
    public bool IsReadOnly => false;

    public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }
    IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
    public void Add(int item) { throw new NotImplementedException(); }
    public void Clear() { throw new NotImplementedException(); }
    public bool Contains(int item) { throw new NotImplementedException(); }
    public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }
    public bool Remove(int item) { throw new NotImplementedException(); }
    public int IndexOf(int item) { throw new NotImplementedException(); }
    public void Insert(int index, int item) { throw new NotImplementedException(); }
    public void RemoveAt(int index) { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Public Class TestClass
	Implements IList(Of Integer)

    Default Public Property Item(index As Integer) As Integer Implements IList(Of Integer).Item
        Get
            Throw New NotImplementedException
        End Get
        Set
            Throw New NotImplementedException
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection(Of Integer).Count
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Integer).IsReadOnly

    Public Function IEnumerable_GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub Add(item As Integer) Implements ICollection(Of Integer).Add
        Throw New NotImplementedException
    End Sub

    Public Sub Clear() Implements ICollection(Of Integer).Clear
        Throw New NotImplementedException
    End Sub

    Public Function Contains(item As Integer) As Boolean Implements ICollection(Of Integer).Contains
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Integer(), arrayIndex As Integer) Implements ICollection(Of Integer).CopyTo
        Throw New NotImplementedException
    End Sub

    Public Function Remove(item As Integer) As Boolean Implements ICollection(Of Integer).Remove
        Throw New NotImplementedException
    End Function

    Public Function IndexOf(item As Integer) As Integer Implements IList(Of Integer).IndexOf
        Throw New NotImplementedException
    End Function

    Public Sub Insert(index As Integer, item As Integer) Implements IList(Of Integer).Insert
        Throw New NotImplementedException
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList(Of Integer).RemoveAt
        Throw New NotImplementedException
    End Sub
End Class
");
        }

        [Fact]
        public void Test_WithCollectionBaseAndGenerics()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class TestClass : CollectionBase, ICollection<int>, IEnumerable<int>, IList<int>
{
    public int this[int index]
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public bool IsReadOnly => false;

    public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }

    public void Add(int item) { throw new NotImplementedException(); }

    public bool Contains(int item) { throw new NotImplementedException(); }

    public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }

    public bool Remove(int item) { throw new NotImplementedException(); }

    public int IndexOf(int item) { throw new NotImplementedException(); }

    public void Insert(int index, int item) { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Public Class TestClass
	Inherits CollectionBase
    Implements ICollection(Of Integer)
	Implements IEnumerable(Of Integer)
	Implements IList(Of Integer)

    Default Public Property Item(index As Integer) As Integer Implements IList(Of Integer).Item
        Get
            Throw New NotImplementedException
        End Get
        Set
            Throw New NotImplementedException
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection(Of Integer).Count
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Integer).IsReadOnly

    Public Function GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub Add(item As Integer) Implements ICollection(Of Integer).Add
        Throw New NotImplementedException
    End Sub

    Public Sub Clear() Implements ICollection(Of Integer).Clear
        Throw New NotImplementedException
    End Sub

    Public Function Contains(item As Integer) As Boolean Implements ICollection(Of Integer).Contains
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Integer(), arrayIndex As Integer) Implements ICollection(Of Integer).CopyTo
        Throw New NotImplementedException
    End Sub

    Public Function Remove(item As Integer) As Boolean Implements ICollection(Of Integer).Remove
        Throw New NotImplementedException
    End Function

    Public Function IndexOf(item As Integer) As Integer Implements IList(Of Integer).IndexOf
        Throw New NotImplementedException
    End Function

    Public Sub Insert(index As Integer, item As Integer) Implements IList(Of Integer).Insert
        Throw New NotImplementedException
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList(Of Integer).RemoveAt
        Throw New NotImplementedException
    End Sub
End Class
");
        }

        [Fact]
        public void Test_WithCollectionAndGenericCollection()
        {
            VerifyCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class TestClass : ICollection, ICollection<int>
{
    int ICollection<int>.Count
    {
        get { throw new NotImplementedException(); }
    }

    int ICollection.Count
    {
        get { throw new NotImplementedException(); }
    }

    public bool IsReadOnly => false;
    public object SyncRoot => null;
    public bool IsSynchronized => false;

    public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }
    IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
    public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
    public void Add(int item) { throw new NotImplementedException(); }
    public void Clear() { throw new NotImplementedException(); }
    public bool Contains(int item) { throw new NotImplementedException(); }
    public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }
    public bool Remove(int item) { throw new NotImplementedException(); }
}
");

            VerifyBasic(@"
Imports System
Imports System.Collections
Imports System.Collections.Generic

Public Class TestClass
	Implements ICollection
	Implements ICollection(Of Integer)

    Public ReadOnly Property ICollection_Count As Integer Implements ICollection(Of Integer).Count

    Public ReadOnly Property Count As Integer Implements ICollection.Count
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Integer).IsReadOnly
    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized

    Public Function IEnumerable_GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException
    End Sub

    Public Sub Add(item As Integer) Implements ICollection(Of Integer).Add
        Throw New NotImplementedException
    End Sub

    Public Sub Clear() Implements ICollection(Of Integer).Clear
        Throw New NotImplementedException
    End Sub

    Public Function Contains(item As Integer) As Boolean Implements ICollection(Of Integer).Contains
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Integer(), arrayIndex As Integer) Implements ICollection(Of Integer).CopyTo
        Throw New NotImplementedException
    End Sub

    Public Function Remove(item As Integer) As Boolean Implements ICollection(Of Integer).Remove
        Throw New NotImplementedException
    End Function
End Class
");
        }

        [Fact]
        public void Test_WithBaseAndDerivedClassFailureCase()
        {
            VerifyCSharp(@"
using System;
using System.Collections;

public class BaseClass : ICollection
{
    public int Count => 0;
    public object SyncRoot => null;
    public bool IsSynchronized => false;

    public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
    public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
}

public class IntCollection : BaseClass
{
}
",
                GetCSharpResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()),
                GetCSharpResultAt(15, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
Imports System
Imports System.Collections

Public Class BaseClass
	Implements ICollection

    Public ReadOnly Property Count As Integer Implements ICollection.Count
    Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
    Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException
    End Function

    Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
        Throw New NotImplementedException
    End Sub
End Class

Public Class IntCollection
	Inherits BaseClass
End Class
",
                GetBasicResultAt(5, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()),
                GetBasicResultAt(21, 14, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }
    }
}
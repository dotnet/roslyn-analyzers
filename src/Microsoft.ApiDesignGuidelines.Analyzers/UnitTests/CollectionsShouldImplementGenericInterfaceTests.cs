// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
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
                        public class TestClass :CollectionBase
                        {
                            public int Count => 0;
                        }",
                        GetCSharpResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
                        Imports System.Collections
                        Public Class TestClass 
                            Inherits CollectionBase
	                        Public ReadOnly Property Count() As Integer
		                        Get
			                        Throw New NotImplementedException()
		                        End Get
	                        End Property
                        End Class",
                        GetBasicResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithCollection()
        {
            VerifyCSharp(@"
                        using System.Collections;
                        public class TestClass :ICollection
                        {
                             public int Count => 0;
                        }",
                        GetCSharpResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
                        Imports System.Collections
                        Public Class TestClass
	                            Implements ICollection
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                        End Class",
                        GetBasicResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithEnumerable()
        {
            VerifyCSharp(@"
                        using System.Collections;
                        public class TestClass :IEnumerable
                        {
                            public int Count => 0;
                        }",
                        GetCSharpResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
                        Imports System.Collections
                        Public Class TestClass
	                            Implements IEnumerable
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                  Throw New NotImplementedException()
		                    End Get
	                    End Property
                        End Class",
                        GetBasicResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithList()
        {
            VerifyCSharp(@"
                        using System.Collections;
                        public class TestClass :IList
                        {
                           public int Count => 0;
                        }",
                        GetCSharpResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));

            VerifyBasic(@"
                        Imports System.Collections
                        Public Class TestClass
	                        Implements IList
	                        Public ReadOnly Property Count() As Integer
		                        Get
			                        Throw New NotImplementedException()
		                        End Get
	                        End Property
                        End Class",
                        GetBasicResultAt(3, 38, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()));
        }

        [Fact]
        public void Test_WithGenericCollection()
        {
            VerifyCSharp(@"
                    using System.Collections.Generic;
                    public class TestClass :ICollection<int>
                    {
                        public int Count => 0;
                    }");

            VerifyBasic(@"
                    Imports System.Collections.Generic
                    Public Class TestClass
	                    Implements ICollection(Of Integer)
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class");
        }

        [Fact]
        public void Test_WithGenericEnumerable()
        {
            VerifyCSharp(@"
                    using System.Collections.Generic;
                    public class TestClass :IEnumerable<int>
                        {
                            public int Count => 0;
                        }");

            VerifyBasic(@"
                    Imports System.Collections.Generic
                    Public Class TestClass
	                    Implements IEnumerable(Of Integer)
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class
                    ");
        }

        [Fact]
        public void Test_WithGenericList()
        {
            VerifyCSharp(@"
                    using System.Collections.Generic;
                    public class TestClass :IList<int>
                        {
                            public int Count => 0;
                        }");

            VerifyBasic(@"
                    Imports System.Collections.Generic
                    Public Class TestClass
	                    Implements IList(Of Integer)
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class
                    ");
        }

        [Fact]
        public void Test_WithCollectionBaseAndGenerics()
        {
            VerifyCSharp(@"
                using System.Collections;
                using System.Collections.Generic;
                public class TestClass :CollectionBase, ICollection<int>, IEnumerable<int> , IList<int>
                    {
                        public int Count => 0;
                    }");

            VerifyBasic(@"
                Imports System.Collections
                Imports System.Collections.Generic
                Public Class TestClass
	                Inherits CollectionBase
	                Implements ICollection(Of Integer)
	                Implements IEnumerable(Of Integer)
	                Implements IList(Of Integer)
	                Public ReadOnly Property Count() As Integer
		                Get
			                Throw New NotImplementedException()
		                End Get
	                End Property
                End Class
                ");
        }

        [Fact]
        public void Test_WithCollectionAndGenericCollection()
        {
            VerifyCSharp(@"
                    using System.Collections;
                    using System.Collections.Generic;
                    public class TestClass :ICollection, ICollection<int>
                        {
                            public int Count => 0;
                        }");

            VerifyBasic(@"
                    Imports System.Collections
                    Imports System.Collections.Generic
                    Public Class TestClass
	                    Implements ICollection
	                    Implements ICollection(Of Integer)
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class");
        }

        [Fact]
        public void Test_WithBaseAndDerivedClassFailureCase()
        {
            VerifyCSharp(@"
                    using System.Collections;
                    using System.Collections.Generic;
                    public class BaseClass :ICollection
                        {
                            public int Count => 0;
                        }
                    public class IntCollection :BaseClass
                        {
                            public int Count => 0;
                        }",
                        GetCSharpResultAt(4, 34, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()),
                        GetCSharpResultAt(8, 34, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString())
                        );

            VerifyBasic(@"
                    Imports System.Collections
                    Imports System.Collections.Generic
                    Public Class BaseClass
	                    Implements ICollection
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class
                    Public Class IntCollection
	                    Inherits BaseClass
	                    Public ReadOnly Property Count() As Integer
		                    Get
			                    Throw New NotImplementedException()
		                    End Get
	                    End Property
                    End Class",
                       GetBasicResultAt(4, 34, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString()),
                       GetBasicResultAt(12, 34, CollectionsShouldImplementGenericInterfaceAnalyzer.RuleId, CollectionsShouldImplementGenericInterfaceAnalyzer.Rule.MessageFormat.ToString())
                       );
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public class MarkAllNonSerializableFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        #region CA2235

        [Fact]
        [WorkItem(858655, "DevDiv")]
        public void CA2235WithOnlyPrimitiveFields()
        {
            VerifyCSharp(@"
                using System;
    
                [Serializable]
                public class CA2235WithOnlyPrimitiveFields
                {
                    public string s1;
                    internal string s2;
                    private string s3;

                    public int i1;
                    internal int i2;
                    private int i3;

                    public bool b1;
                    internal bool b2;
                    private bool b3;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Public Class CA2235WithOnlyPrimitiveFields 

                    Public s1 As String
                    Friend s2 As String
                    Private s3 As String

                    Public i1 As Integer
                    Friend i2 As Integer
                    Private i3 As Integer

                    Public b1 As Boolean
                    Friend b2 As Boolean
                    Private b3 As Boolean
                End Class");
        }

        [Fact]
        public void CA2235WithConstPrimitiveFields()
        {
            VerifyCSharp(@"
                using System;
    
                [Serializable]
                public class CA2235WithConstPrimitiveFields
                {
                    public const int i1 = 42;
                    internal const int i2 = 54;
                    private const int i3 = 96;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Public Class CA2235WithConstPrimitiveFields 

                    Public Const i1 As Integer = 42
                    Friend Const i2 As Integer = 54
                    Private Const i3 As Integer = 96
                End Class");
        }

        [Fact]
        public void CA2235WithPrimitiveGetOnlyProperties()
        {
            VerifyCSharp(@"
                using System;
    
                [Serializable]
                public class CA2235WithPrimitiveGetOnlyProperties
                {
                    public int I1 { get; } = 42;
                    internal int I2 { get; } = 54;
                    private int I3 { get; } = 96;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Public Class CA2235WithPrimitiveGetOnlyProperties 

                    Public ReadOnly Property I1 As Integer
                        Get
                            Return 42
                        End Get
                    End Property

                    Friend ReadOnly Property I2 As Integer
                        Get
                            Return 54
                        End Get
                    End Property

                    Private ReadOnly Property I3 As Integer
                        Get
                            Return 96
                        End Get
                    End Property

                   ' Using auto-implemented property syntax
                    Public Property AI1 As Integer = 42
                    Friend Property AI2 As Integer = 54
                    Private Property AI3 As Integer = 96
                End Class");
        }

        [Fact]
        public void CA2235WithSerializableLibraryTypes()
        {
            VerifyCSharp(@"
                using System;
                using System.Text.RegularExpressions;

                [Serializable]
                public class CA2235WithSerializableLibraryTypes
                {
                    public Regex R = new Regex(@""\w+"");
                    public Nullable<int> NI = new Nullable<int>(42);
                    public bool? NB = true;
                    public Version V = new Version(1, 1, 12, 2);
                }");

            VerifyBasic(@"
                Imports System
                Imports System.Text.RegularExpressions

                <Serializable>
                Public Class CA2235WithSerializableLibraryTypes
                    Public R As Regex = New Regex(""\w+"")
                    Public NI As Nullable(Of Integer) = new Nullable(Of Integer)(42)
                    Public NB As Boolean? = true
                    Public V As Version = New Version(1, 1, 12, 2)
                End Class");
        }

        [Fact]
        [WorkItem(279, "https://github.com/dotnet/roslyn-analyzers/issues/279")]
        public void CA2235WithNonSerialized()
        {
            VerifyCSharp(@"
                using System;
    
                [Serializable]
                public class CA2235WithOnlyPrimitiveFields
                {
                        [NonSerialized]
                        public Action<string> SomeAction;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Public Class CA2235WithOnlyPrimitiveFields 
                        <NonSerialized>
                        Public SomeAction As Action(Of String)
                End Class");
        }

        [Fact]
        public void CA2235WithOnlySerializableFields()
        {
            VerifyCSharp(@"
                using System;
                [Serializable]
                public class SerializableType { }
    
                [Serializable]
                public class CA2235WithOnlySerializableFields
                {
                    public SerializableType s1;
                    internal SerializableType s2;
                    private SerializableType s3;
                }");

            VerifyBasic(@"
                Imports System
                <Serializable>
                Public Class SerializableType
                End Class

                <Serializable>
                Public Class CA2235WithOnlySerializableFields 

                    Public s1 As SerializableType
                    Friend s2 As SerializableType
                    Private s3 As SerializableType
                End Class");
        }

        [Fact]
        public void CA2235WithNonPublicNonSerializableFields()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                public class SerializableType { }
    
                [Serializable]
                public class CA2235WithNonPublicNonSerializableFields
                {
                    public SerializableType s1;
                    internal NonSerializableType s2;
                    private NonSerializableType s3;
                }",
                GetCA2235CSharpResultAt(12, 50, "s2", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235CSharpResultAt(13, 49, "s3", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"));

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class
                <Serializable>
                Public Class SerializableType
                End Class

                <Serializable>
                Public Class CA2235WithNonPublicNonSerializableFields 
                    Public s1 As SerializableType
                    Friend s2 As NonSerializableType
                    Private s3 As NonSerializableType
                End Class",
                GetCA2235BasicResultAt(12, 28, "s2", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235BasicResultAt(13, 29, "s3", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"));
        }

        [Fact]
        public void CA2235WithNonPublicNonSerializableFieldsWithScope()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                public class SerializableType { }
    
                [|[Serializable]
                public class CA2235WithNonPublicNonSerializableFields
                {
                    public SerializableType s1;
                    internal NonSerializableType s2;
                    private NonSerializableType s3;
                }|]

                [Serializable]
                public class Sample
                {
                    public SerializableType s1;
                    internal NonSerializableType s2;
                    private NonSerializableType s3;
                }",
                GetCA2235CSharpResultAt(12, 50, "s2", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235CSharpResultAt(13, 49, "s3", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"));

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class
                <Serializable>
                Public Class SerializableType
                End Class

                [|<Serializable>
                Public Class CA2235WithNonPublicNonSerializableFields 
                    Public s1 As SerializableType
                    Friend s2 As NonSerializableType
                    Private s3 As NonSerializableType
                End Class|]

                <Serializable>
                Public Class Sample 
                    Public s1 As SerializableType
                    Friend s2 As NonSerializableType
                    Private s3 As NonSerializableType
                End Class",
                GetCA2235BasicResultAt(12, 28, "s2", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235BasicResultAt(13, 29, "s3", "CA2235WithNonPublicNonSerializableFields", "NonSerializableType"));
        }

        [Fact]
        public void CA2235InternalWithNonPublicNonSerializableFields()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                public class SerializableType { }
    
                [Serializable]
                internal class CA2235InternalWithNonPublicNonSerializableFields
                {
                    public NonSerializableType s1;
                    internal SerializableType s2;
                    private NonSerializableType s3;
                }",
                GetCA2235CSharpResultAt(11, 48, "s1", "CA2235InternalWithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235CSharpResultAt(13, 49, "s3", "CA2235InternalWithNonPublicNonSerializableFields", "NonSerializableType"));

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class
                <Serializable>
                Public Class SerializableType
                End Class

                <Serializable>
                Friend Class CA2235InternalWithNonPublicNonSerializableFields 
                    Public s1 As NonSerializableType
                    Friend s2 As SerializableType
                    Private s3 As NonSerializableType
                End Class",
                GetCA2235BasicResultAt(11, 28, "s1", "CA2235InternalWithNonPublicNonSerializableFields", "NonSerializableType"),
                GetCA2235BasicResultAt(13, 29, "s3", "CA2235InternalWithNonPublicNonSerializableFields", "NonSerializableType"));
        }

        [Fact]
        public void CA2235WithNonSerializableAutoProperties()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                public class SerializableType { }
    
                [Serializable]
                internal class CA2235WithNonSerializableAutoProperties
                {
                    public SerializableType s1;
                    internal NonSerializableType s2 {get; set; }
                }",
                GetCA2235CSharpResultAt(12, 50, "s2", "CA2235WithNonSerializableAutoProperties", "NonSerializableType"));

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class
                <Serializable>
                Public Class SerializableType
                End Class

                <Serializable>
                Friend Class CA2235WithNonSerializableAutoProperties 
                    Public s1 As SerializableType
                    Friend Property s2 As NonSerializableType
                End Class",
                GetCA2235BasicResultAt(12, 37, "s2", "CA2235WithNonSerializableAutoProperties", "NonSerializableType"));
        }

        [Fact]
        public void CA2235WithArrayType()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                public class SerializableType { }
    
                [Serializable]
                internal class CA2235WithNonSerializableArray
                {
                    public SerializableType[] s1;
                    internal NonSerializableType[] s2;
                }",
                // Test0.cs(12,52): warning CA2235: Field s2 is a member of type CA2235WithNonSerializableArray which is serializable but is of type NonSerializableType[] which is not serializable
                GetCA2235CSharpResultAt(12, 52, "s2", "CA2235WithNonSerializableArray", "NonSerializableType[]"));

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class
                <Serializable>
                Public Class SerializableType
                End Class

                <Serializable>
                Friend Class CA2235WithNonSerializableArray 
                    Public s1 As SerializableType()
                    Friend Property s2 As NonSerializableType()
                End Class",
                // Test0.vb(12,37): warning CA2235: Field s2 is a member of type CA2235WithNonSerializableArray which is serializable but is of type NonSerializableType() which is not serializable
                GetCA2235BasicResultAt(12, 37, "s2", "CA2235WithNonSerializableArray", "NonSerializableType()"));
        }

        [Fact]
        public void CA2235WithEnumType()
        {
            VerifyCSharp(@"
                using System;
                internal enum E1
                {
                    F1 = 0
                }
    
                internal enum E2 : long
                {
                    F1 = 0
                }
    
                [Serializable]
                internal class CA2235WithEnumFields
                {
                    public E1 s1;
                    internal E2 s2;
                }");

            VerifyBasic(@"
                Imports System
                Friend Enum E1
                    F1 = 0
                End Enum

                Friend Enum E2 As Long
                    F1 = 0
                End Enum

                <Serializable>
                Friend Class CA2235WithEnumFields
                    Public s1 As E1
                    Friend Property s2 As E2
                End Class");
        }

        [Fact, WorkItem(1510, "https://github.com/dotnet/roslyn-analyzers/issues/1510")]
        public void CA2235WithDateTimeType()
        {
            VerifyCSharp(@"
                using System;

                [Serializable]
                internal class CA2235WithDateTimeField
                {
                    public DateTime s1;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Friend Class CA2235WithDateTimeField
                    Public s1 As DateTime
                End Class");
        }

        [Fact, WorkItem(1510, "https://github.com/dotnet/roslyn-analyzers/issues/1510")]
        public void CA2235WithNullableType()
        {
            VerifyCSharp(@"
                using System;

                [Serializable]
                internal class CA2235WithNullableField
                {
                    public long? s1;
                }");

            VerifyBasic(@"
                Imports System

                <Serializable>
                Friend Class CA2235WithNullableField
                    Public s1 As Long?
                End Class");
        }

        [Fact]
        public void CA2235WithSpecialSerializableTypeFields()
        {
            // Interface, type parameter and delegate fields are always considered serializable. 
            VerifyCSharp(@"
                using System;
                interface I
                {
                }
    
                [Serializable]
                internal class GenericType<T>
                {
                    public I s1;
                    internal T s2;
                    private delegate void s3();
                }");

            VerifyBasic(@"
                Imports System
                Friend Interface I
                End Interface

                <Serializable>
                Friend Class GenericType(Of T)
                    Public s1 As I
                    Friend s2 As T
                    Private Delegate Sub s3()
                End Class");
        }

        [Fact, WorkItem(1883, "https://github.com/dotnet/roslyn-analyzers/issues/1883")]
        public void CA2235WithNonInstanceFieldsOfNonSerializableType()
        {
            VerifyCSharp(@"
                using System;
                public class NonSerializableType { }

                [Serializable]
                internal class CA2235WithNonInstanceFieldsOfNonSerializableType
                {
                    public const NonSerializableType s1 = null;
                    public static NonSerializableType s2;
                }");

            VerifyBasic(@"
                Imports System
                Public Class NonSerializableType
                End Class

                <Serializable>
                Friend Class CA2235WithNonInstanceFieldsOfNonSerializableType 
                    Public Const s1 As Object = Nothing
                    Public Shared s2 As NonSerializableType
                End Class");
        }

        [Fact, WorkItem(1970, "https://github.com/dotnet/roslyn-analyzers/issues/1970")]
        public void CA2235WithISerializableImplementation()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;
                public class NonSerializableType { }

                [Serializable]
                internal class CA2235WithISerializableImplementation : ISerializable
                {
                    private NonSerializableType _nonSerializable;
                    public CA2235WithISerializableImplementation(SerializationInfo info, StreamingContext context) { }
                    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
                }");

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization
                Public Class NonSerializableType
                End Class

                <Serializable>
                Friend Class CA2235WithISerializableImplementation
                    Implements ISerializable
                    Private _nonSerializable As NonSerializableType
                    Private Sub New(Info As SerializationInfo, Context As StreamingContext)
                    End Sub
                    Private Sub GetObjectData(Info As SerializationInfo, Context As StreamingContext) Implements ISerializable.GetObjectData
                    End Sub
                End Class");
        }

        internal static readonly string CA2235Message = MicrosoftNetFrameworkAnalyzersResources.MarkAllNonSerializableFieldsMessage;

        private static DiagnosticResult GetCA2235CSharpResultAt(int line, int column, string fieldName, string containerName, string typeName)
        {
            return GetCSharpResultAt(line, column, SerializationRulesDiagnosticAnalyzer.RuleCA2235Id, string.Format(CA2235Message, fieldName, containerName, typeName));
        }

        private static DiagnosticResult GetCA2235BasicResultAt(int line, int column, string fieldName, string containerName, string typeName)
        {
            return GetBasicResultAt(line, column, SerializationRulesDiagnosticAnalyzer.RuleCA2235Id, string.Format(CA2235Message, fieldName, containerName, typeName));
        }

        #endregion
    }
}

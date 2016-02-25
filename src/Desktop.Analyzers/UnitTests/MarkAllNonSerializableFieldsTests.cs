// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Diagnostics.Test.Utilities;

namespace Desktop.Analyzers.UnitTests
{
    public partial class MarkAllNonSerializableFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        [WorkItem(858655, "DevDiv")]
        #region CA2235

        [Fact]
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

                    Public Property I1 As Integer
                        Get
                            Return 42
                        End Get
                    End Property

                    Friend Property I2 As Integer
                        Get
                            Return 54
                        End Get
                    End Property

                    Private Const i3 As Integer
                        Get
                            Return 96
                        End Get
                    End Property

                   ' Using auto-implemented property syntax
                    Public Property I1 As Integer = 42
                    Friend Property I2 As Integer = 54
                    Private Const i3 As Integer = 96
                End Class");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/3898")]
        public void CA2235WithSerializableLibraryTypes()
        {
            VerifyCSharp(@"
                using System;
                using System.Text.RegularExpressions;

                [Serializable]
                public class CA2235WithSerializableLibraryTypes
                {
                    public Regex R = new Regex(""\w+"");
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
                    Public NI As Nullable(Of Integer)  = new Nullable(Of Integer)(42)
                    Public NB As Boolean? = true
                    Public V As Version = New Version(1, 1, 12, 2)
                }");
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
                        public Action<string> SomeAction;

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

                    Public s1 As SerializableType;
                    Friend s2 As SerializableType;
                    Private s3 As SerializableType;
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
                    Public s1 As SerializableType;
                    Friend s2 As NonSerializableType;
                    Private s3 As NonSerializableType;
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
                    Public s1 As SerializableType;
                    Friend s2 As NonSerializableType;
                    Private s3 As NonSerializableType;
                End Class|]

                <Serializable>
                Public Class Sample 
                    Public s1 As SerializableType;
                    Friend s2 As NonSerializableType;
                    Private s3 As NonSerializableType;
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
                    Public s1 As NonSerializableType;
                    Friend s2 As SerializableType;
                    Private s3 As NonSerializableType;
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

        internal static readonly string CA2235Name = SerializationRulesDiagnosticAnalyzer.RuleCA2235Id;
        internal static readonly string CA2235Message = DesktopAnalyzersResources.MarkAllNonSerializableFieldsMessage;

        private static DiagnosticResult GetCA2235CSharpResultAt(int line, int column, string fieldName, string containerName, string typeName)
        {
            return GetCSharpResultAt(line, column, CA2235Name, string.Format(CA2235Message, fieldName, containerName, typeName));
        }

        private static DiagnosticResult GetCA2235BasicResultAt(int line, int column, string fieldName, string containerName, string typeName)
        {
            return GetBasicResultAt(line, column, CA2235Name, string.Format(CA2235Message, fieldName, containerName, typeName));
        }
        #endregion
    }
}

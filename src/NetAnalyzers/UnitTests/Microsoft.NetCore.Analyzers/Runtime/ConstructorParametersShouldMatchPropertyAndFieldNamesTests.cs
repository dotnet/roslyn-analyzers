// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ConstructorParametersShouldMatchPropertyAndFieldNamesTests
    {
        #region Class Props Do Not Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatch_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstDrop|}, object {|#1:secondDrop|})
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstDrop", "FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondDrop", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatch_ConstructorParametersShouldMatchPropertyNames_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public Property FirstProp() As Integer
                    Public Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstDrop|} as Integer, {|#1:secondDrop|} as Object)
                        Me.FirstProp = firstDrop
                        Me.SecondProp = secondDrop
                    End Sub
                End Class",
                CA1071BasicPropertyResultAt(0, "C1", "firstDrop", "FirstProp"),
                CA1071BasicPropertyResultAt(1, "C1", "secondDrop", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                public class C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    public C1(int firstDrop, object secondDrop)
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchNotJsonCtor_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Public Class C1
                    Public Property firstProp() As Integer
                    Public Property secondProp() as Object

                    Public Sub New(firstDrop as Integer, secondDrop as Object)
                        Me.firstProp = firstDrop
                        Me.secondProp = secondDrop
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchReversedWords_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:dropFirst|}, object {|#1:dropSecond|})
                    {
                        this.FirstProp = dropFirst;
                        this.SecondProp = dropSecond;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "dropFirst", "FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "dropSecond", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchReversedWords_ConstructorParametersShouldMatchPropertyNames_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public Property FirstProp() As Integer
                    Public Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:dropFirst|} as Integer, {|#1:dropSecond|} as Object)
                        Me.FirstProp = dropFirst
                        Me.SecondProp = dropSecond
                    End Sub
                End Class",
                CA1071BasicPropertyResultAt(0, "C1", "dropFirst", "FirstProp"),
                CA1071BasicPropertyResultAt(1, "C1", "dropSecond", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchAndTupleAssignment_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int _FirstProp { get; }
                    public object _SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        (this._FirstProp, this._SecondProp) = (firstProp, secondProp);
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstProp", "_FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondProp", "_SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            // This is the current behavior on deserialization - JsonPropertyName is ignored by the JsonConstructor's logic.
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    [JsonPropertyName(""FirstProp"")]
                    public int _FirstProp { get; }

                    [JsonPropertyName(""SecondProp"")]
                    public object _SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        this._FirstProp = firstProp;
                        this._SecondProp = secondProp;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstProp", "_FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondProp", "_SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchPropertyNames_Basic()
        {
            // This is the current behavior on deserialization - JsonPropertyName is ignored by the JsonConstructor's logic.
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    <JsonPropertyName(""FirstProp"")>
                    Public Property _FirstProp() As Integer

                    <JsonPropertyName(""SecondProp"")>
                    Public Property _SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstProp|} as Integer, {|#1:secondProp|} as Object)
                        Me._FirstProp = firstProp
                        Me._SecondProp = secondProp
                    End Sub
                End Class",
                CA1071BasicPropertyResultAt(0, "C1", "firstProp", "_FirstProp"),
                CA1071BasicPropertyResultAt(1, "C1", "secondProp", "_SecondProp"));
        }

        #endregion

        #region Class Props Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_ClassPropsMatch_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int firstProp, object secondProp)
                    {
                        this.FirstProp = firstProp;
                        this.SecondProp = secondProp;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassPropsMatch_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public Property firstProp() As Integer
                    Public Property secondProp() as Object

                    <JsonConstructor>
                    Public Sub New(firstProp as Integer, secondProp as Object)
                        Me.firstProp = firstProp
                        Me.secondProp = secondProp
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassPropsMatchButPrivate_ConstructorParametersShouldShouldMatchPublicProperties_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    private int FirstProp { get; }
                    private object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        this.FirstProp = firstProp;
                        this.SecondProp = secondProp;
                    }
                }",
                CA1071CSharpPublicPropertyResultAt(0, "C1", "firstProp", "FirstProp"),
                CA1071CSharpPublicPropertyResultAt(1, "C1", "secondProp", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsMatchButPrivate_ConstructorParametersShouldMatchPublicProperties_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Private Property FirstProp() As Integer
                    Private Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstProp|} as Integer, {|#1:secondProp|} as Object)
                        Me.FirstProp = firstProp
                        Me.SecondProp = secondProp
                    End Sub
                End Class",
                CA1071BasicPublicPropertyResultAt(0, "C1", "firstProp", "FirstProp"),
                CA1071BasicPublicPropertyResultAt(1, "C1", "secondProp", "SecondProp"));
        }

        #endregion

        #region Class Fields Do Not Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatch_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstIField|}, object {|#1:secondIField|})
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstIField", "firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondIField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatch_ConstructorParametersShouldMatchFieldNames_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField as Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstIField|} as Integer, {|#1:secondIField|} as Object)
                        Me.firstField = firstIField
                        Me.secondField = secondIField
                    End Sub
                End Class",
                CA1071BasicFieldResultAt(0, "C1", "firstIField", "firstField"),
                CA1071BasicFieldResultAt(1, "C1", "secondIField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                public class C1
                {
                    public int firstField;
                    public object secondField;

                    public C1(int firstIField, object secondIField)
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchNotJsonCtor_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Public Class C1
                    Public firstField as Integer
                    Public secondField as Object

                    Public Sub New(firstIField as Integer, secondIField as Object)
                        Me.firstField = firstIField
                        Me.secondField = secondIField
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchReversedWords_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:fieldFirst|}, object {|#1:fieldSecond|})
                    {
                        this.firstField = fieldFirst;
                        this.secondField = fieldSecond;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "fieldFirst", "firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "fieldSecond", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchReversedWords_ConstructorParametersShouldMatchFieldNames_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField as Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New({|#0:fieldFirst|} as Integer, {|#1:fieldSecond|} as Object)
                        Me.firstField = fieldFirst
                        Me.secondField = fieldSecond
                    End Sub
                End Class",
                CA1071BasicFieldResultAt(0, "C1", "fieldFirst", "firstField"),
                CA1071BasicFieldResultAt(1, "C1", "fieldSecond", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchAndTupleAssignment_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int _firstField;
                    public object _secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        (_firstField, _secondField) = (firstField, secondField);
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstField", "_firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondField", "_secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            // This is the current behavior on deserialization - JsonPropertyName is ignored by the JsonConstructor's logic.
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    [JsonPropertyName(""firstField"")]
                    public int _firstField;

                    [JsonPropertyName(""secondField"")]
                    public object _secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this._firstField = firstField;
                        this._secondField = secondField;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstField", "_firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondField", "_secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchFieldNames_Basic()
        {
            // This is the current behavior on deserialization - JsonFieldertyName is ignored by the JsonConstructor's logic.
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    <JsonPropertyName(""firstField"")>
                    Public _firstField As Integer

                    <JsonPropertyName(""secondField"")>
                    Public _secondField as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstField|} as Integer, {|#1:secondField|} as Object)
                        Me._firstField = firstField
                        Me._secondField = secondField
                    End Sub
                End Class",
                CA1071BasicFieldResultAt(0, "C1", "firstField", "_firstField"),
                CA1071BasicFieldResultAt(1, "C1", "secondField", "_secondField"));
        }

        #endregion

        #region Class Fields Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_ClassFieldsMatchNoJsonInclude_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchNoJsonInclude_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField as Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New(firstField as Integer, secondField as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchHasJsonInclude_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    [JsonInclude]
                    public int firstField;
                    
                    [JsonInclude]
                    public object secondField;

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchHasJsonInclude_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    <JsonInclude>
                    Public firstField as Integer
                    
                    <JsonInclude>
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New(firstField as Integer, secondField as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class");
        }


        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                public class C1
                {
                    private int firstField;
                    private object secondField;

                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateNotJsonCtor_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Public Class C1
                    Private firstField as Integer
                    Private secondField as Object

                    Public Sub New(firstField as Integer, secondField as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateNoJsonInclude_ConstructorParametersShouldMatchPublicFields_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;
                
                public class C1
                {
                    private int firstField;
                    private object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }",
                CA1071CSharpPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071CSharpPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateNoJsonInclude_ConstructorParametersShouldMatchPublicFields_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization
            
                Public Class C1
                    Private firstField as Integer
                    Private secondField as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstField|} as Integer, {|#1:secondField|} as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class",
                CA1071BasicPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071BasicPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateHasJsonInclude_ConstructorParametersShouldMatchPublicFields_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    [JsonInclude]
                    private int firstField;

                    [JsonInclude]
                    private object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }",
                CA1071CSharpPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071CSharpPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchButPrivateHasJsonInclude_ConstructorParametersShouldMatchPublicFields_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    <JsonInclude>
                    Private firstField as Integer

                    <JsonInclude>
                    Private secondField as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstField|} as Integer, {|#1:secondField|} as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class",
                CA1071BasicPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071BasicPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        #endregion

        #region Record Props Do Not Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_RecordPropsDoNotMatch_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstDrop|}, object {|#1:secondDrop|})
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstDrop", "FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondDrop", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_RecordPropsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                public record C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    public C1(int firstDrop, object secondDrop)
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordPropsDoNotMatchReversedWords_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:dropFirst|}, object {|#1:dropSecond|})
                    {
                        this.FirstProp = dropFirst;
                        this.SecondProp = dropSecond;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "dropFirst", "FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "dropSecond", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_RecordPropsDoNotMatchAndTupleAssignment_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstDrop|}, object {|#1:secondDrop|})
                    {
                        (this.FirstProp, this.SecondProp) = (firstDrop, secondDrop);
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstDrop", "FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondDrop", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_RecordPropsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchPropertyNames_CSharp()
        {
            // This is the current behavior on deserialization - JsonPropertyName is ignored by the JsonConstructor's logic.
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    [JsonPropertyName(""FirstProp"")]
                    public int _FirstProp { get; }

                    [JsonPropertyName(""SecondProp"")]
                    public object _SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        this._FirstProp = firstProp;
                        this._SecondProp = secondProp;
                    }
                }",
                CA1071CSharpPropertyResultAt(0, "C1", "firstProp", "_FirstProp"),
                CA1071CSharpPropertyResultAt(1, "C1", "secondProp", "_SecondProp"));
        }

        #endregion

        #region Record Props Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_RecordPropsMatch_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int FirstProp { get; }
                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int firstProp, object secondProp)
                    {
                        this.FirstProp = firstProp;
                        this.SecondProp = secondProp;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordPropsMatchButPrivate_ConstructorParametersShouldShouldMatchPublicProperties_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    private int FirstProp { get; }
                    private object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        this.FirstProp = firstProp;
                        this.SecondProp = secondProp;
                    }
                }",
                CA1071CSharpPublicPropertyResultAt(0, "C1", "firstProp", "FirstProp"),
                CA1071CSharpPublicPropertyResultAt(1, "C1", "secondProp", "SecondProp"));
        }

        #endregion

        #region Record Fields Do Not Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_RecordFieldsDoNotMatch_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstIField|}, object {|#1:secondIField|})
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstIField", "firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondIField", "secondField"));
        }

        [Fact]
        public async Task CA1071_RecordFieldsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                public record C1
                {
                    public int firstField;
                    public object secondField;

                    public C1(int firstIField, object secondIField)
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordFieldsDoNotMatchReversedWords_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:fieldFirst|}, object {|#1:fieldSecond|})
                    {
                        this.firstField = fieldFirst;
                        this.secondField = fieldSecond;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "fieldFirst", "firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "fieldSecond", "secondField"));
        }

        [Fact]
        public async Task CA1071_RecordFieldsDoNotMatchAndTupleAssignment_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstIField|}, object {|#1:secondIField|})
                    {
                        (this.firstField, this.secondField) = (firstIField, secondIField);
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstIField", "firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondIField", "secondField"));
        }

        [Fact]
        public async Task CA1071_RecordFieldsDoNotMatchButMatchWithJsonPropertyName_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            // This is the current behavior on deserialization - JsonPropertyName is ignored by the JsonConstructor's logic.
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    [JsonPropertyName(""firstField"")]
                    public int _firstField;

                    [JsonPropertyName(""secondField"")]
                    public object _secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this._firstField = firstField;
                        this._secondField = secondField;
                    }
                }",
                CA1071CSharpFieldResultAt(0, "C1", "firstField", "_firstField"),
                CA1071CSharpFieldResultAt(1, "C1", "secondField", "_secondField"));
        }

        #endregion

        #region Record Fields Match Referenced Parameter Names

        [Fact]
        public async Task CA1071_RecordFieldsMatchNoJsonInclude_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordFieldsMatchHasJsonInclude_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    [JsonInclude]
                    public int firstField;
                    
                    [JsonInclude]
                    public object secondField;

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordFieldsMatchButPrivateNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                public record C1
                {
                    private int firstField;
                    private object secondField;

                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_RecordFieldsMatchButPrivateNoJsonInclude_ConstructorParametersShouldMatchPublicFields_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    private int firstField;
                    private object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }",
                CA1071CSharpPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071CSharpPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        [Fact]
        public async Task CA1071_RecordFieldsMatchButPrivateHasJsonInclude_ConstructorParametersShouldMatchPublicFields_CSharp()
        {
            await VerifyCSharp9AnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public record C1
                {
                    [JsonInclude]
                    private int firstField;

                    [JsonInclude]
                    private object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }",
                CA1071CSharpPublicFieldResultAt(0, "C1", "firstField", "firstField"),
                CA1071CSharpPublicFieldResultAt(1, "C1", "secondField", "secondField"));
        }

        #endregion

        #region Test Helpers

        private static async Task VerifyCSharpAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestCode = source,
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private static async Task VerifyCSharp9AnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                LanguageVersion = LanguageVersion.CSharp9,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestCode = source,
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private static async Task VerifyBasicAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var basicTest = new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestCode = source,
            };

            basicTest.ExpectedDiagnostics.AddRange(expected);

            await basicTest.RunAsync();
        }

        private DiagnosticResult CA1071CSharpPropertyResultAt(int markupKey, params string[] arguments)
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyNameRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071BasicPropertyResultAt(int markupKey, params string[] arguments)
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyNameRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071CSharpPublicPropertyResultAt(int markupKey, params string[] arguments)
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyPublicRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071BasicPublicPropertyResultAt(int markupKey, params string[] arguments)
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyPublicRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071CSharpPublicFieldResultAt(int markupKey, params string[] arguments)
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.FieldPublicRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071BasicPublicFieldResultAt(int markupKey, params string[] arguments)
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.FieldPublicRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071CSharpFieldResultAt(int markupKey, params string[] arguments)
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.FieldRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071BasicFieldResultAt(int markupKey, params string[] arguments)
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.FieldRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        #endregion
    }
}
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
        public async Task CA1071_ClassPropsDoNotMatch_ConstructorParametersShouldMatchPropertyNames_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property FirstProp() As Integer
                    Property SecondProp() as Object

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
                Property firstProp() As Integer
                Property secondProp() as Object

                Public Sub New(firstDrop as Integer, secondDrop as Object)
                    Me.firstProp = firstDrop
                    Me.secondProp = secondDrop
                End Sub
            End Class");
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
        public async Task CA1071_ClassPropsMatchAndTupleAssignment_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }

                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int {|#0:firstProp|}, object {|#1:secondProp|})
                    {
                        (this.FirstProp, this.SecondProp) = (firstProp, secondProp);
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassPropsMatch_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property firstProp() As Integer
                    Property secondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstProp|} as Integer, {|#1:secondProp|} as Object)
                        Me.firstProp = firstProp
                        Me.secondProp = secondProp
                    End Sub
                End Class");
        }

        [Fact(Skip = "Not sure if this syntax is possible in VB.")]
        public async Task CA1071_ClassPropsMatchAndTupleAssignment_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property firstProp() As Integer
                    Property secondProp() as Object

                    <JsonConstructor>
                    Public Sub New({|#0:firstProp|} as Integer, {|#1:secondProp|} as Object)
                        new Tuple(As String, Object)(Me.firstProp, Me.secondProp) = new Tuple(As String, Object)(firstDrop, secondDrop)
                    End Sub
                End Class");
        }

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
        public async Task CA1071_ClassFieldsMatch_NoDiagnostics_CSharp()
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
                }"
            );
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatchAndTupleAssignment_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int {|#0:firstField|}, object {|#1:secondField|})
                    {
                        (firstField, secondField) = (firstField, secondField);
                    }
                }"
            );
        }

        [Fact]
        public async Task CA1071_ClassFieldsMatch_NoDiagnostics_Basic()
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
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyRule)
               .WithLocation(markupKey)
               .WithArguments(arguments);

        private DiagnosticResult CA1071BasicPropertyResultAt(int markupKey, params string[] arguments)
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer.PropertyRule)
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
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyNamesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyNamesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ConstructorParametersShouldMatchPropertyNamesTests
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
                    public C1(int firstDrop, object secondDrop)
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }",
            CA1071CSharpPropertyResultAt(11, 35, "C1", "firstDrop", "FirstProp"),
            CA1071CSharpPropertyResultAt(11, 53, "C1", "secondDrop", "SecondProp"));
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
                    public C1(int firstDrop, object secondDrop)
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }",
            CA1071CSharpPropertyResultAt(11, 35, "C1", "firstDrop", "FirstProp"),
            CA1071CSharpPropertyResultAt(11, 53, "C1", "secondDrop", "SecondProp"));
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
    Public Sub New(firstDrop as Integer, secondDrop as Object)
        Me.FirstProp = firstDrop
        Me.SecondProp = secondDrop
    End Sub
End Class",
            CA2243BasicPropertyResultAt(9, 20, "C1", "firstDrop", "FirstProp"),
            CA2243BasicPropertyResultAt(9, 42, "C1", "secondDrop", "SecondProp"));
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

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
Imports System.Text.Json.Serialization

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
                using System.Text.Json.Serialization;

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
        public async Task CA1071_ClassPropsMatch_NoDiagnostics_Basic()
        {
            await VerifyBasicAnalyzerAsync(@"
Imports System.Text.Json.Serialization

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
        public async Task CA1071_ClassFieldsDoNotMatch_ConstructorParametersShouldMatchFieldNames_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField;
                    public object secondField;

                    [JsonConstructor]
                    public C1(int firstIField, object secondIField)
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }",
            CA1071CSharpFieldResultAt(10, 35, "C1", "firstIField", "firstField"),
            CA1071CSharpFieldResultAt(10, 55, "C1", "secondIField", "secondField"));
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
    Public Sub New(firstIField as Integer, secondIField as Object)
        Me.firstField = firstIField
        Me.secondField = secondIField
    End Sub
End Class",
            CA2243BasicFieldResultAt(9, 20, "C1", "firstIField", "firstField"),
            CA2243BasicFieldResultAt(9, 44, "C1", "secondIField", "secondField"));
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
                    public C1(int firstIField, object secondIField)
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }",
            CA1071CSharpFieldResultAt(11, 35, "C1", "firstIField", "firstField"),
            CA1071CSharpFieldResultAt(11, 55, "C1", "secondIField", "secondField"));
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatchNotJsonCtor_NoDiagnostics_CSharp()
        {
            await VerifyCSharpAnalyzerAsync(@"
                using System.Text.Json.Serialization;

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
Imports System.Text.Json.Serialization

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
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithSystemTextJson50,
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
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithSystemTextJson50,
                TestCode = source,
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        private static async Task VerifyBasicAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var basicTest = new VerifyVB.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithSystemTextJson50,
                TestCode = source,
            };

            basicTest.ExpectedDiagnostics.AddRange(expected);

            await basicTest.RunAsync();
        }

        private DiagnosticResult CA1071CSharpPropertyResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyNamesAnalyzer.PropertyRule)
               .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
               .WithArguments(arguments);

        private DiagnosticResult CA2243BasicPropertyResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyNamesAnalyzer.PropertyRule)
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);
        private DiagnosticResult CA1071CSharpFieldResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
           => VerifyCS.Diagnostic(ConstructorParametersShouldMatchPropertyNamesAnalyzer.FieldRule)
               .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
               .WithArguments(arguments);

        private DiagnosticResult CA2243BasicFieldResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic(ConstructorParametersShouldMatchPropertyNamesAnalyzer.FieldRule)
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ConstructorParametersShouldMatchPropertyAndFieldNamesFixerTests
    {
        [Fact]
        public async Task CA1071_ClassSinglePropDoesNotMatch_CSharp()
        {
            await VerifyCSharpCodeFixAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }

                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int [|firstDrop|], object secondProp)
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondProp;
                    }
                }",
                @"
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
        public async Task CA1071_ClassSinglePropDoesNotMatch_Basic()
        {
            await VerifyBasicCodeFixAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property FirstProp() As Integer
                    Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New([|firstDrop|] as Integer, secondProp as Object)
                        Me.FirstProp = firstDrop
                        Me.SecondProp = secondProp
                    End Sub
                End Class",
                @"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property FirstProp() As Integer
                    Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New(firstProp as Integer, secondProp as Object)
                        Me.FirstProp = firstProp
                        Me.SecondProp = secondProp
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassPropsDoNotMatch_CSharp()
        {
            await VerifyCSharpCodeFixAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int FirstProp { get; }

                    public object SecondProp { get; }

                    [JsonConstructor]
                    public C1(int [|firstDrop|], object [|secondDrop|])
                    {
                        this.FirstProp = firstDrop;
                        this.SecondProp = secondDrop;
                    }
                }",
                @"
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
        public async Task CA1071_ClassPropsDoNotMatch_Basic()
        {
            await VerifyBasicCodeFixAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property FirstProp() As Integer
                    Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New([|firstDrop|] as Integer, [|secondDrop|] as Object)
                        Me.FirstProp = firstDrop
                        Me.SecondProp = secondDrop
                    End Sub
                End Class",
                @"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Property FirstProp() As Integer
                    Property SecondProp() as Object

                    <JsonConstructor>
                    Public Sub New(firstProp as Integer, secondProp as Object)
                        Me.FirstProp = firstProp
                        Me.SecondProp = secondProp
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassSingleFieldDoesNotMatch_CSharp()
        {
            await VerifyCSharpCodeFixAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField { get; }

                    public object secondField { get; }

                    [JsonConstructor]
                    public C1(int [|firstIField|], object secondField)
                    {
                        this.firstField = firstIField;
                        this.secondField = secondField;
                    }
                }",
                @"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField { get; }

                    public object secondField { get; }

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassSingleFieldDoesNotMatch_Basic()
        {
            await VerifyBasicCodeFixAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField As Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New([|firstIField|] as Integer, secondField as Object)
                        Me.firstField = firstIField
                        Me.secondField = secondField
                    End Sub
                End Class",
                @"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField As Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New(firstField as Integer, secondField as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class");
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatch_CSharp()
        {
            await VerifyCSharpCodeFixAsync(@"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField { get; }

                    public object secondField { get; }

                    [JsonConstructor]
                    public C1(int [|firstIField|], object [|secondIField|])
                    {
                        this.firstField = firstIField;
                        this.secondField = secondIField;
                    }
                }",
                @"
                using System.Text.Json.Serialization;

                public class C1
                {
                    public int firstField { get; }

                    public object secondField { get; }

                    [JsonConstructor]
                    public C1(int firstField, object secondField)
                    {
                        this.firstField = firstField;
                        this.secondField = secondField;
                    }
                }");
        }

        [Fact]
        public async Task CA1071_ClassFieldsDoNotMatch_Basic()
        {
            await VerifyBasicCodeFixAsync(@"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField As Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New([|firstIField|] as Integer, [|secondIField|] as Object)
                        Me.firstField = firstIField
                        Me.secondField = secondIField
                    End Sub
                End Class",
                @"
                Imports System.Text.Json.Serialization

                Public Class C1
                    Public firstField As Integer
                    Public secondField as Object

                    <JsonConstructor>
                    Public Sub New(firstField as Integer, secondField as Object)
                        Me.firstField = firstField
                        Me.secondField = secondField
                    End Sub
                End Class");
        }

        private static async Task VerifyCSharpCodeFixAsync(string source, string expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestCode = source,
                FixedCode = expected,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            };

            await csharpTest.RunAsync();
        }

        private static async Task VerifyBasicCodeFixAsync(string source, string expected)
        {
            var basicTest = new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                TestCode = source,
                FixedCode = expected,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            };

            await basicTest.RunAsync();
        }
    }
}
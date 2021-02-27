// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
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
        public async Task CA1071_ClassSinglePropDoesNotMatch_ConstructorParametersShouldMatchPropertyNames_CSharp()
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
        public async Task CA1071_ClassPropsDoNotMatch_ConstructorParametersShouldMatchPropertyNames_CSharp()
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
    }
}
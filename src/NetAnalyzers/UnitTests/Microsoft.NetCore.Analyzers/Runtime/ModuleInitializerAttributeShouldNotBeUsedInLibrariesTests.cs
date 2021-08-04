// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ModuleInitializerAttributeShouldNotBeUsedInLibraries,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.ModuleInitializerAttributeShouldNotBeUsedInLibraries,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class ModuleInitializerAttributeShouldNotBeUsedInLibrariesTests
    {
        [Theory]
        [InlineData("public", "public", false)]
        [InlineData("public", "internal", false)]
        [InlineData("internal", "public", false)]
        [InlineData("internal", "internal", false)]
        [InlineData("public", "public", true)]
        [InlineData("public", "internal", true)]
        [InlineData("internal", "public", true)]
        [InlineData("internal", "internal", true)]
        public async Task CA2255ModuleInitializerOnMethod(string classModifier, string methodModifier, bool useAsync = false)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        {classModifier} class CA2255ModuleInitializerOnMethod
                        {{
                            [{{|CA2255:System.Runtime.CompilerServices.ModuleInitializer|}}]
                            {methodModifier} static {(useAsync ? "async" : "")} void Initialize() {{ }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Theory]
        [InlineData("public", "public", false)]
        [InlineData("public", "internal", false)]
        [InlineData("internal", "public", false)]
        [InlineData("internal", "internal", false)]
        [InlineData("public", "public", true)]
        [InlineData("public", "internal", true)]
        [InlineData("internal", "public", true)]
        [InlineData("internal", "internal", true)]
        public async Task CA2255ModuleInitializerOnMethod_WithParens(string classModifier, string methodModifier, bool useAsync = false)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        {classModifier} class CA2255ModuleInitializerOnMethod
                        {{
                            [{{|CA2255:System.Runtime.CompilerServices.ModuleInitializer()|}}]
                            {methodModifier} static {(useAsync ? "async" : "")} void Initialize() {{ }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Theory]
        [InlineData("public", "public", false)]
        [InlineData("public", "internal", false)]
        [InlineData("internal", "public", false)]
        [InlineData("internal", "internal", false)]
        [InlineData("public", "public", true)]
        [InlineData("public", "internal", true)]
        [InlineData("internal", "public", true)]
        [InlineData("internal", "internal", true)]
        public async Task CA2255ModuleInitializerOnMethod_Suppressed(string classModifier, string methodModifier, bool useAsync = false)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        $@"
                        #pragma warning disable CA2255
                        {classModifier} class CA2255ModuleInitializerOnMethod
                        {{
                            [System.Runtime.CompilerServices.ModuleInitializer]
                            {methodModifier} static {(useAsync ? "async" : "")} void Initialize() {{ }}
                        }}
                        #pragma warning restore CA2255
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Fact]
        public async Task CA2255DoesNotApply_ToPrivateMethods()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        public class CA2255ModuleInitializerOnMethod
                        {{
                            [{{|CS8814:System.Runtime.CompilerServices.ModuleInitializer|}}]
                            private static void Initialize() {{ }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Fact]
        public async Task CA2255DoesNotApply_ToNonVoidMethods()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        public class CA2255ModuleInitializerOnMethod
                        {{
                            [{{|CS8815:System.Runtime.CompilerServices.ModuleInitializer|}}]
                            public static bool Initialize() {{ return true; }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }

        [Fact]
        public async Task CA2255DoesNotApply_ToInstanceMethods()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        public class CA2255ModuleInitializerOnMethod
                        {{
                            [{{|CS8815:System.Runtime.CompilerServices.ModuleInitializer|}}]
                            public void Initialize() {{ }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }


        [Fact]
        public async Task CA2255DoesNotApply_ToGenericMethods()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    Sources = {
                        @$"
                        public class CA2255ModuleInitializerOnMethod<T>
                        {{
                            [{{|CS8816:System.Runtime.CompilerServices.ModuleInitializer|}}]
                            public static void Initialize<T>() {{ }}
                        }}
                        "
                    }
                },
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            }.RunAsync();
        }
    }
}

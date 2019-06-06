// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.GenericMethodsShouldProvideTypeParameterAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.GenericMethodsShouldProvideTypeParameterAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class GenericMethodsShouldProvideTypeParameterTests
    {
        [Fact]
        public async Task PublicGenericMethod_NoParameters_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void [|Method|]<T>() { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub [|Method|](Of T)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PublicGenericMethod_Parameters_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void Method<T>(T o) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub Method(Of T)(o As T)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task ProtectedGenericMethod_NoParameters_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void [|Method|]<T>() { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    protected Sub [|Method|](Of T)
                    End Sub
                End Class");
        }


        [Fact]
        public async Task ProtectedGenericMethod_Parameters_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void Method<T>(T o) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Sub Method(Of T)(o As T)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PrivateGenericMethod_NoParameters_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private void Method<T>() { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Sub Method(Of T)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task GenericMethod_SomeParameters_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void [|Method|]<T1, T2>(T1 o) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub [|Method|](Of T1, T2)(o As T1)
                    End Sub
                End Class");
        }
    }
}
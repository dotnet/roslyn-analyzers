// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotExposeGenericListsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotExposeGenericListsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotExposeGenericListsTests
    {
        [Fact]
        public async Task PublicListProperty_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Collections.Generic.List<int> [|Ints|] { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Property [|Ints|] As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task PrivateListProperty_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Collections.Generic.List<int> Ints { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Property Ints As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task ProtectedListProperty_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Collections.Generic.List<int> [|Ints|] { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Property [|Ints|] As System.Collections.Generic.List(Of Integer)
                End Class");
        }


        [Fact]
        public async Task PublicListField_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Collections.Generic.List<int> [|Ints|];
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public [|Ints|] As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task PrivateListField_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Collections.Generic.List<int> Ints;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Ints As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task ProtectedListField_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Collections.Generic.List<int> [|Ints|];
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected [|Ints|] As System.Collections.Generic.List(Of Integer)
                End Class");
        }


        [Fact]
        public async Task PublicListReturn_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Collections.Generic.List<int> [|Ints|]() { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Function [|Ints|]() As System.Collections.Generic.List(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task PrivateListReturn_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Collections.Generic.List<int> Ints() { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Function Ints() As System.Collections.Generic.List(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task ProtectedListReturn_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Collections.Generic.List<int> [|Ints|]() { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Function [|Ints|]() As System.Collections.Generic.List(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }


        [Fact]
        public async Task PublicListParameter_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void Ints(System.Collections.Generic.List<int> [|ints|]) { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub Ints([|ints|] As System.Collections.Generic.List(Of Integer))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PrivateListParameter_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private void Ints(System.Collections.Generic.List<int> ints) { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Sub Ints(ints As System.Collections.Generic.List(Of Integer))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task ProtectedListParameter_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void Ints(System.Collections.Generic.List<int> [|ints|]) { throw null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Sub Ints([|ints|] As System.Collections.Generic.List(Of Integer))
                    End Sub
                End Class");
        }

    }
}
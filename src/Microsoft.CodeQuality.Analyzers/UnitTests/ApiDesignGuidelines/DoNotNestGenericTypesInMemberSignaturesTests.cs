// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotNestGenericTypesInMemberSignaturesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotNestGenericTypesInMemberSignaturesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotNestGenericTypesInMemberSignaturesTests
    {
        [Fact]
        public async Task PublicNestedProperty_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<System.Action<int>> [|Actions|] { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Property [|Actions|] As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task PublicNonNestedProperty_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<int> Action { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Property Action As System.Action(Of Integer)
                End Class");
        }


        [Fact]
        public async Task PrivateNestedProperty_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Action<System.Action<int>> Actions { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Property Actions As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task ProtectedNestedProperty_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Action<System.Action<int>> [|Actions|] { get; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Property [|Actions|] As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task PublicNestedField_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<System.Action<int>> [|Actions|];
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public [|Actions|] As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task PublicNonNestedField_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<int> Action;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Action As System.Action(Of Integer)
                End Class");
        }


        [Fact]
        public async Task PrivateNestedField_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Action<System.Action<int>> Actions;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Actions As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task ProtectedNestedField_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Action<System.Action<int>> [|Actions|];
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected [|Actions|] As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task PublicNestedReturn_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<System.Action<int>> [|Actions|]() { return null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Function [|Actions|] As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task PublicNonNestedReturn_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public System.Action<int> Actions() { return null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Function Actions As System.Action(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }


        [Fact]
        public async Task PrivateNestedReturn_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private System.Action<System.Action<int>> Actions() { return null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Function Actions As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task ProtectedNestedReturn_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected System.Action<System.Action<int>> [|Actions|]() { return null; }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Function [|Actions|] As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task PublicNestedParameter_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void DoActions(System.Action<System.Action<int>> [|action|]) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub DoActions([|action|] As System.Action(Of System.Action(Of Integer)))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PublicNonNestedParameter_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void DoActions(System.Action<int> action) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub DoActions(action As System.Action(Of Integer))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PrivateNestedParameter_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private void DoActions(System.Action<System.Action<int>> action) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Sub DoActions(action As System.Action(Of System.Action(Of Integer)))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task ProtectedNestedParameter_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void DoActions(System.Action<System.Action<int>> [|action|]) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Sub DoActions([|action|] As System.Action(Of System.Action(Of Integer)))
                    End Sub
                End Class");
        }
    }
}
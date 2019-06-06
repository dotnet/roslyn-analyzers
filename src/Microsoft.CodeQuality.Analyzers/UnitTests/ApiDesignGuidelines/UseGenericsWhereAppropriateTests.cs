// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UseGenericsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UseGenericsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class UseGenericsWhereAppropriate
    {
        [Fact]
        public async Task PublicRefObject_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void [|Swap|](ref object o1, ref object o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub [|Swap|](ByRef o1 As Object, ByRef o2 As Object)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PrivateRefObject_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private void Swap(ref object o1, ref object o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Private Sub Swap(ByRef o1 As Object, ByRef o2 As Object)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task ProtectedRefObject_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void [|Swap|](ref object o1, ref object o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Protected Sub [|Swap|](ByRef o1 As Object, ByRef o2 As Object)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PublicRefInt_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void Swap(ref int o1, ref int o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub Swap(ByRef o1 As Integer, ByRef o2 As Integer)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PublicValObject_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void Swap(object o1, object o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class [MyClass]
                    Public Sub Swap(o1 As Object, o2 As Object)
                    End Sub
                End Class");
        }
    }
}
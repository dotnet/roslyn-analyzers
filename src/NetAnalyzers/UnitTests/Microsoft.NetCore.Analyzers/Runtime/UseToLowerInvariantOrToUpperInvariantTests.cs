// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseToLowerInvariantOrToUpperInvariantAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicUseToLowerInvariantOrToUpperInvariantAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseToLowerInvariantOrToUpperInvariantTests
    {
        #region Diagnostic tests

        [Fact]
        public async Task CA1311_ToLowerTest_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
class C
{
    void Method()
    {
        string a = ""test"";
        a.[|ToLower|]();
        a?.[|ToLower|]();
    }
}
");
        }

        [Fact]
        public async Task CA1311_ToLowerTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.[|ToLower|]()
        a?.[|ToLower|]()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1311_ToUpperTest_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
class C
{
    void Method()
    {
        string a = ""test"";
        a.[|ToUpper|]();
        a?.[|ToUpper|]();
    }
}
");
        }

        [Fact]
        public async Task CA1311_ToUpperTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.[|ToUpper|]()
        a?.[|ToUpper|]()
    End Sub
End Class
");
        }

        #endregion
    }
}
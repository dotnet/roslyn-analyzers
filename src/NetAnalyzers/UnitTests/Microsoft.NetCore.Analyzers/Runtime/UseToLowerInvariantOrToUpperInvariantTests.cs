// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseToLowerInvariantOrToUpperInvariantAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicUseToLowerInvariantOrToUpperInvariant,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseToLowerInvariantOrToUpperInvariantTests
    {
        #region Helper methods

        private DiagnosticResult CSharpResult(int line, int column)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);
#pragma warning restore RS0030 // Do not used banned APIs

        private DiagnosticResult BasicResult(int line, int column)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic()
                .WithLocation(line, column);
#pragma warning restore RS0030 // Do not used banned APIs

        #endregion

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
        a.ToLower();
        a?.ToLower();
    }
}
",
        CSharpResult(7, 11),
        CSharpResult(8, 11)
);
        }

        [Fact]
        public async Task CA1311_ToLowerTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.ToLower()
        a?.ToLower()
    End Sub
End Class
",
        BasicResult(5, 11),
        BasicResult(6, 12)
);
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
        a.ToUpper();
        a?.ToUpper();
    }
}
",
        CSharpResult(7, 11),
        CSharpResult(8, 11)
);
        }

        [Fact]
        public async Task CA1311_ToUpperTest_Basic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Class C
    Sub Method()
        Dim a As String = ""test""
        a.ToUpper()
        a?.ToUpper()
    End Sub
End Class
",
        BasicResult(5, 11),
        BasicResult(6, 12)
);
        }

        #endregion
    }
}
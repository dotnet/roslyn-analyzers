// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseToLowerInvariantOrToUpperInvariantAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseToLowerInvariantOrToUpperInvariantFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicUseToLowerInvariantOrToUpperInvariantAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicUseToLowerInvariantOrToUpperInvariantFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class UseToLowerInvariantOrToUpperInvariantFixerTests
    {
        [Fact]
        public async Task CA1311_FixToLowerCSharpAsync()
        {
            await VerifyCS.VerifyCodeFixAsync(
                @"
class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToLower|]();
        a?.[|ToLower|]();
    }
}
",
                @"
class C
{
    void M()
    {
        var a = ""test"";
        a.ToLowerInvariant();
        a?.ToLowerInvariant();
    }
}
");
        }

        [Fact]
        public async Task CA1311_FixToLowerBasicAsync()
        {
            await VerifyVB.VerifyCodeFixAsync(
                @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToLower|]()
        a?.[|ToLower|]()
    End Sub
End Class
",
                @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToLowerInvariant()
        a?.ToLowerInvariant()
    End Sub
End Class
");
        }

        [Fact]
        public async Task CA1311_FixToUpperCSharpAsync()
        {
            await VerifyCS.VerifyCodeFixAsync(
                @"
class C
{
    void M()
    {
        var a = ""test"";
        a.[|ToUpper|]();
        a?.[|ToUpper|]();
    }
}
",
                @"
class C
{
    void M()
    {
        var a = ""test"";
        a.ToUpperInvariant();
        a?.ToUpperInvariant();
    }
}
");
        }

        [Fact]
        public async Task CA1311_FixToUpperBasicAsync()
        {
            await VerifyVB.VerifyCodeFixAsync(
                @"
Class C
    Sub M()
        Dim a = ""test""
        a.[|ToUpper|]()
        a?.[|ToUpper|]()
    End Sub
End Class
",
                @"
Class C
    Sub M()
        Dim a = ""test""
        a.ToUpperInvariant()
        a?.ToUpperInvariant()
    End Sub
End Class
");
        }
    }
}

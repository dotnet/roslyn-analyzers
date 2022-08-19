// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.CSharp.Analyzers.CSharpSymbolIsBannedInAnalyzersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.VisualBasic.Analyzers.BasicSymbolIsBannedInAnalyzersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests
{
    public class SymbolIsBannedInAnalyzersTests
    {
        [Fact]
        public async Task UseBannedApi_CSharp()
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp31,
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestCode = @"
using System.IO;

class C
{
    void M()
    {
        _ = File.Exists(""something"");
    }
}
",
                ExpectedDiagnostics =
                {
                    // /0/Test0.cs(8,13): warning RS0051: The symbol 'File' is banned for use by analyzers: do not do file IO in analyzers
                    VerifyCS.Diagnostic().WithSpan(8, 13, 8, 37).WithArguments("File", ": do not do file IO in analyzers"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_Basic()
        {
            await new VerifyVB.Test
            {
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp31,
                LanguageVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                TestCode = @"
Imports System.IO

Class C
    Function M()
        File.Exists(""something"")
    End Function
End Class
",
                ExpectedDiagnostics =
                {
                    // /0/Test0.vb(6,9): warning RS0051: The symbol 'File' is banned for use by analyzers: do not do file IO in analyzers
                    VerifyVB.Diagnostic().WithSpan(6, 9, 6, 33).WithArguments("File", ": do not do file IO in analyzers"),
                }
            }.RunAsync();
        }
    }
}

﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
        public async Task UseBannedApi_EnforcementEnabled_CSharp()
        {
            await new VerifyCS.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestCode = @"
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
class MyAnalyzer
{
}

class C
{
    void M()
    {
        _ = File.Exists(""something"");
    }
}
",
                ExpectedDiagnostics = {
                    // /0/Test0.cs(15,13): error RS1035: The symbol 'File' is banned for use by analyzers: do not do file IO in analyzers
                    VerifyCS.Diagnostic("RS1035").WithSpan(15, 13, 15, 37).WithArguments("File", ": do not do file IO in analyzers"),
                },
                TestState = {
                    AnalyzerConfigFiles = { ("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.enforce_analyzer_banned_apis = true
"), },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_EnforcementNotSpecified_CSharp()
        {
            await new VerifyCS.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestCode = @"
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
class MyAnalyzer
{
}

class C
{
    void M()
    {
        _ = File.Exists(""something"");
    }
}
",
                ExpectedDiagnostics = {
                    // /0/Test0.cs(7,7): error RS1036: 'MyAnalyzer': A project containing analyzers or source generators should specify the editorconfig setting 'dotnet_code_quality.enforce_analyzer_banned_apis = true'.
                    VerifyCS.Diagnostic("RS1036").WithSpan(7, 7, 7, 17).WithArguments("MyAnalyzer"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_EnforcementDisabled_CSharp()
        {
            await new VerifyCS.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestCode = @"
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
class MyAnalyzer
{
}

class C
{
    void M()
    {
        _ = File.Exists(""something"");
    }
}
",
                TestState = {
                    AnalyzerConfigFiles = { ("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.enforce_analyzer_banned_apis = false
"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_EnforcementEnabled_Basic()
        {
            await new VerifyVB.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                TestCode = @"
Imports System.IO
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Class MyDiagnosticAnalyzer
End Class

Class C
    Function M()
        File.Exists(""something"")
    End Function
End Class
",
                ExpectedDiagnostics =
                {
                    // /0/Test0.vb(12,9): error RS1035: The symbol 'File' is banned for use by analyzers: do not do file IO in analyzers
                    VerifyVB.Diagnostic("RS1035").WithSpan(12, 9, 12, 33).WithArguments("File", ": do not do file IO in analyzers"),
                },
                TestState = {
                    AnalyzerConfigFiles = { ("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.enforce_analyzer_banned_apis = true
"),
                    },
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_EnforcementNotSpecified_Basic()
        {
            await new VerifyVB.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                TestCode = @"
Imports System.IO
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Class MyDiagnosticAnalyzer
End Class

Class C
    Function M()
        File.Exists(""something"")
    End Function
End Class
",
                ExpectedDiagnostics =
                {
                    // /0/Test0.vb(7,7): error RS1036: 'MyDiagnosticAnalyzer': A project containing analyzers or source generators should specify the editorconfig setting 'dotnet_code_quality.enforce_analyzer_banned_apis = true'.
                    VerifyVB.Diagnostic("RS1036").WithSpan(7, 7, 7, 27).WithArguments("MyDiagnosticAnalyzer"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task UseBannedApi_EnforcementDisabled_Basic()
        {
            await new VerifyVB.Test
            {
                LanguageVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest,
                TestCode = @"
Imports System.IO
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Class MyDiagnosticAnalyzer
End Class

Class C
    Function M()
        File.Exists(""something"")
    End Function
End Class
",
                TestState = {
                    AnalyzerConfigFiles = { ("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.enforce_analyzer_banned_apis = false
"),
                    },
                }
            }.RunAsync();
        }
    }
}

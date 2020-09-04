// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.InteropServices.PreferIsExactSpellingIsTrueForKnownApisAnalyzer,
    Microsoft.NetCore.Analyzers.InteropServices.PreferIsExactSpellingIsTrueForKnownApisFixer>;
namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public sealed class PreferIsExactSpellingIsTrueForKnownApisTest
    {
        private DiagnosticResult CA1839_DefaultRule(int line, int column, params string[] arguments)
           => VerifyCS.Diagnostic(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.DefaultRule)
               .WithLocation(line, column)
               .WithArguments(arguments);

        private DiagnosticResult CA1839_WideRule(int line, int column, params string[] arguments)
           => VerifyCS.Diagnostic(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.WideRule)
               .WithLocation(line, column)
               .WithArguments(arguments);

        [Fact]
        public async Task CA1839CSharpTest()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""user32.dll"")]
    static extern void CallMsgFilter(); // should have exactSpelling and W rule
    [DllImport(""user32.dll"")]
    static extern void CallMsgFilterW(); // should have exactSpelling
    [DllImport(""user32.dll"", EntryPoint = ""CallMsgFilterW"")]
    static extern void CallMyMessageFilter(); // should have exactSpelling
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterA(); // should have nothing, exactspelling present and is present in known api
    [DllImport(""user32.dll"")]
    static extern void abcdefg(); // should have nothing, method unknown
    [DllImport(""testunknown.dll"")]
    static extern void abcdefg12(); // should have nothing, dll unknown
}
",
                CA1839_WideRule(7, 24, "CallMsgFilter"),
                CA1839_DefaultRule(9, 24, "CallMsgFilterW"),
                CA1839_DefaultRule(11, 24, "CallMsgFilterW"));

            await VerifyCS.VerifyCodeFixAsync(@"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""advapi32.dll"")]
    static extern void ProcessIdleTasks(); // should have exactSpelling and W rule
    [DllImport(""user32.dll"")]
    static extern void CallMsgFilterW(); // should have exactSpelling
    [DllImport(""user32.dll"", EntryPoint = ""CallMsgFilterW"")]
    static extern void CallMyMessageFilter(); // should have exactSpelling
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterA(); // should have nothing, exactspelling present and is present in known api
    [DllImport(""user32.dll"")]
    static extern void abcdefg(); // should have nothing, method unknown
    [DllImport(""testunknown.dll"")]
    static extern void abcdefg12(); // should have nothing, dll unknown
}
",
                new[]{CA1839_WideRule(7, 24, "ProcessIdleTasks"),
                CA1839_DefaultRule(9, 24, "CallMsgFilterW"),
                CA1839_DefaultRule(11, 24, "CallMsgFilterW") }, @"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""advapi32.dll"", ExactSpelling = true)]
    static extern void ProcessIdleTasksW(); // should have exactSpelling and W rule
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterW(); // should have exactSpelling
    [DllImport(""user32.dll"", EntryPoint = ""CallMsgFilterW"", ExactSpelling = true)]
    static extern void CallMyMessageFilter(); // should have exactSpelling
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterA(); // should have nothing, exactspelling present and is present in known api
    [DllImport(""user32.dll"")]
    static extern void abcdefg(); // should have nothing, method unknown
    [DllImport(""testunknown.dll"")]
    static extern void abcdefg12(); // should have nothing, dll unknown
}
");

        }
    }
}
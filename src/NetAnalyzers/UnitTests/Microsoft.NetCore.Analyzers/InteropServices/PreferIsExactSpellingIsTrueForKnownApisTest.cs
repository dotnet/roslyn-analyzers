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
        private DiagnosticResult CA1839_DefaultRule(int markupId, params string[] arguments)
           => VerifyCS.Diagnostic(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.DefaultRule)
               .WithLocation(markupId)
               .WithArguments(arguments);

        private DiagnosticResult CA1839_WideRule(int markupId, params string[] arguments)
           => VerifyCS.Diagnostic(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.WideRule)
               .WithLocation(markupId)
               .WithArguments(arguments);

        [Fact]
        public async Task CA1839CSharpTest()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""advapi32.dll"", CharSet = CharSet.Unicode)]
    static extern void {|#0:ProcessIdleTasksW|}(); // should have exactSpelling
    [DllImport(""user32.dll"")]
    static extern void {|#1:CharToOemBuff|}(); // should have exactSpelling and A suffix
    [DllImport(""user32.dll"", EntryPoint = ""GetWindowModuleFileName"")]
    static extern void {|#2:CustomMethodNameRename|}(); // should have exactSpelling and a suffix, name derived from attribute
    [DllImport(""user32.dll"", EntryPoint = ""CharToOemA"", ExactSpelling = false)]
    static extern void {|#3:CallMyMessageFilterCuston|}(); // should have exactSpelling true
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterA(); // should have nothing, exactspelling present and is present in known api
    [DllImport(""user32.dll"")]
    static extern void abcdefg(); // should have nothing, method unknown
    [DllImport(""testunknown.dll"")]
    static extern void abcdefg12(); // should have nothing, dll unknown
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void BroadcastSystemMessageA(); // should have nothing
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void BroadcastSystemMessage(); // should have nothing, perhaps we want to call method without a or w suffix
    static void nonExtern() {} // should have nothing, not extern
    static extern void onlyExtern(); // should have nothing, attribute missing
}
",
                new[]{CA1839_WideRule(0, "ProcessIdleTasksW"),
                CA1839_DefaultRule(1, "CharToOemBuff"),
                CA1839_DefaultRule(2, "GetWindowModuleFileNameA"),
                CA1839_DefaultRule(3, "CharToOemA") }, @"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""advapi32.dll"", CharSet = CharSet.Unicode, ExactSpelling = true)]
    static extern void ProcessIdleTasksW(); // should have exactSpelling
    [DllImport(""user32.dll"", ExactSpelling = true, EntryPoint = ""CharToOemBuffA"")]
    static extern void CharToOemBuff(); // should have exactSpelling and A suffix
    [DllImport(""user32.dll"", EntryPoint = ""GetWindowModuleFileNameA"", ExactSpelling = true)]
    static extern void CustomMethodNameRename(); // should have exactSpelling and a suffix, name derived from attribute
    [DllImport(""user32.dll"", EntryPoint = ""CharToOemA"", ExactSpelling = true)]
    static extern void CallMyMessageFilterCuston(); // should have exactSpelling true
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void CallMsgFilterA(); // should have nothing, exactspelling present and is present in known api
    [DllImport(""user32.dll"")]
    static extern void abcdefg(); // should have nothing, method unknown
    [DllImport(""testunknown.dll"")]
    static extern void abcdefg12(); // should have nothing, dll unknown
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void BroadcastSystemMessageA(); // should have nothing
    [DllImport(""user32.dll"", ExactSpelling = true)]
    static extern void BroadcastSystemMessage(); // should have nothing, perhaps we want to call method without a or w suffix
    static void nonExtern() {} // should have nothing, not extern
    static extern void onlyExtern(); // should have nothing, attribute missing
}
");
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SuspiciousCastFromCharToIntAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpSuspiciousCastFromCharToIntFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class SuspiciousCastFromCharToIntTests
    {
        [Theory]
        [InlineData("'a'")]
        [InlineData("_char")]
        [InlineData("GetChar()")]
        public Task StringSplit_CharInt32StringSplitOptions_Diagnostic_CS(string charArgument)
        {
            string source = $@"
public class Testopolis
{{
    private char _char;
    private char GetChar() => _char;
    public void M()
    {{
        nameof(M).Split('c', {{|#0:{charArgument}|}}, System.StringSplitOptions.None);
    }}
}}";
            string fixedSource = $@"
public class Testopolis
{{
    private char _char;
    private char GetChar() => _char;
    public void M()
    {{
        nameof(M).Split(new char[] {{ 'c', {charArgument} }}, System.StringSplitOptions.None);
    }}
}}";
            var diagnostics = new[]
            {
                VerifyCS.Diagnostic(Rule).WithLocation(0)
            };

            return VerifyCS.VerifyCodeFixAsync(source, diagnostics, fixedSource);
        }

        [Theory]
        [InlineData(
            "'c', {|#0:'a'|}, System.StringSplitOptions.RemoveEmptyEntries",
            "new char[] { 'c', 'a' }, System.StringSplitOptions.RemoveEmptyEntries")]
        [InlineData(
            "{|#0:count: 'a'|}, separator: 'c', options: System.StringSplitOptions.RemoveEmptyEntries",
            "new char[] { 'c', 'a' }, options: System.StringSplitOptions.RemoveEmptyEntries")]
        [InlineData(
            "options: System.StringSplitOptions.None, separator: 'c', {|#0:count: 'a'|}",
            "new char[] { 'c', 'a' }, options: System.StringSplitOptions.None")]
        public Task StringSplit_CharInt32StringSplitOptions_NamedArguments_Diagnostic_CS(string testArgumentList, string fixedArgumentList)
        {
            string source = $@"
public class Testopolis
{{
    private char _char;
    private char GetChar() => _char;
    public void M()
    {{
        nameof(M).Split({testArgumentList});
    }}
}}";
            string fixedSource = $@"
public class Testopolis
{{
    private char _char;
    private char GetChar() => _char;
    public void M()
    {{
        nameof(M).Split({fixedArgumentList});
    }}
}}";
            var diagnostic = VerifyCS.Diagnostic(Rule).WithLocation(0);

            return VerifyCS.VerifyCodeFixAsync(source, diagnostic, fixedSource);
        }

        [Theory]
        [InlineData("s, {|#0:c|}, System.StringSplitOptions.None", "new string[] { s, c.ToString() }, System.StringSplitOptions.None")]
        [InlineData("s, {|#0:c|}", "new string[] { s, c.ToString() }, System.StringSplitOptions.None")]
        [InlineData("{|#0:count: c|}, separator: s", "new string[] { s, c.ToString() }, System.StringSplitOptions.None")]
        [InlineData("options: System.StringSplitOptions.None, {|#0:count: c|}, separator: s", "new string[] { s, c.ToString() }, options: System.StringSplitOptions.None")]
        [InlineData("s, {|#0:'a'|}", @"new string[] { s, ""a"" }, System.StringSplitOptions.None")]
        public Task StringSplit_StringInt32StringSplitOptions_NamedArguments_Diagnostic_CS(string testArgumentList, string fixedArgumentList)
        {
            string format = @"
public class Testopolis
{{
    public void M(string s, char c)
    {{
        nameof(M).Split({0});
    }}
}}";
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { string.Format(CultureInfo.InvariantCulture, format, testArgumentList) },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithLocation(0) }
                },
                FixedState =
                {
                    Sources = { string.Format(CultureInfo.InvariantCulture, format, fixedArgumentList) },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50
                }
            };

            return test.RunAsync();
        }

        [Theory]
        [InlineData("{|#0:c|}", "c.ToString()")]
        [InlineData("{|#0:'a'|}", @"""a""")]
        [InlineData("{|#0:capacity: c|}", "c.ToString()")]
        [InlineData("{|#0:capacity: 'a'|}", @"""a""")]
        public Task StringBuilder_Int32_Diagnostic_CS(string testArgumentList, string fixedArgumentList)
        {
            string format = @"
using System.Text;
public class Testopolis
{{
    public void M(string s, char c, int n)
    {{
        new StringBuilder({0});
    }}
}}";
            var test = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { string.Format(CultureInfo.InvariantCulture, format, testArgumentList) },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                    ExpectedDiagnostics = { VerifyCS.Diagnostic(Rule).WithLocation(0) }
                },
                FixedState =
                {
                    Sources = { string.Format(CultureInfo.InvariantCulture, format, fixedArgumentList) },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net50
                }
            };

            return test.RunAsync();
        }

        private static DiagnosticDescriptor Rule => SuspiciousCastFromCharToIntAnalyzer.Rule;
    }
}

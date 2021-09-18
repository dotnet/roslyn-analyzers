// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Globalization;
using System.Linq;
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
        [InlineData(
            "'c', {|#0:'a'|}, System.StringSplitOptions.RemoveEmptyEntries",
            "new char[] { 'c', 'a' }, System.StringSplitOptions.RemoveEmptyEntries")]
        [InlineData(
            "{|#0:count: 'a'|}, separator: 'c', options: System.StringSplitOptions.RemoveEmptyEntries",
            "new char[] { 'c', 'a' }, options: System.StringSplitOptions.RemoveEmptyEntries")]
        [InlineData(
            "options: System.StringSplitOptions.None, separator: 'c', {|#0:count: 'a'|}",
            "new char[] { 'c', 'a' }, options: System.StringSplitOptions.None")]
        public Task StringSplit_CharInt32StringSplitOptions_Fixed_CS(string testArgumentList, string fixedArgumentList)
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
        public Task StringSplit_StringInt32StringSplitOptions_Fixed_CS(string testArgumentList, string fixedArgumentList)
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
        [InlineData("ss, c, o")]
        [InlineData("cc, c, o")]
        [InlineData("cc, c")]
        public Task StringSplit_NonProblematic_NoDiagnostic_CS(string argumentList)
        {
            string source = $@"
using System;
public class Testopolis
{{
    public void M(string s, char c, string[] ss, char[] cc, StringSplitOptions o)
    {{
        nameof(M).Split({argumentList});
    }}
}}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("c, n, o")]
        [InlineData("c, (int)c, o")]
        [InlineData("s, n, o")]
        [InlineData("s, (int)c, o")]
        public Task StringSplit_NoImplicitConversion_NoDiagnostic_CS(string argumentList)
        {
            string source = $@"
using System;
public class Testopolis
{{
    public void M(string s, char c, int n, StringSplitOptions o)
    {{
        nameof(M).Split({argumentList});
    }}
}}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("{|#0:c|}", "c.ToString()")]
        [InlineData("{|#0:'a'|}", @"""a""")]
        [InlineData("{|#0:capacity: c|}", "c.ToString()")]
        [InlineData("{|#0:capacity: 'a'|}", @"""a""")]
        public Task StringBuilderCtor_Int32_Fixed_CS(string testArgumentList, string fixedArgumentList)
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

        [Theory]
        [InlineData("{|#0:c|}, n")]
        [InlineData("n, {|#0:c|}")]
        [InlineData("{|#0:c|}, {|#1:c|}", 2)]
        [InlineData("s, {|#0:c|}")]
        [InlineData("s, n, n, {|#0:c|}")]
        [InlineData("s, {|#0:c|}, n, n")]
        [InlineData("s, {|#0:c|}, {|#1:c|}, {|#2:c|}", 3)]
        public Task StringBuilderCtor_NonFixable_Diagnostic_CS(string argumentList, int diagnosticsCount = 1)
        {
            string source = $@"
using System.Text;
public class Testopolis
{{
    public void M(string s, char c, int n)
    {{
        new StringBuilder({argumentList});
    }}
}}";
            var diagnostics = Enumerable.Range(0, diagnosticsCount).Select(x => VerifyCS.Diagnostic(Rule).WithLocation(x)).ToArray();

            return VerifyCS.VerifyAnalyzerAsync(source, diagnostics);
        }

        [Theory]
        [InlineData("n")]
        [InlineData("(int)c")]
        [InlineData("n, n")]
        [InlineData("(int)c, (int)c")]
        [InlineData("s, n")]
        [InlineData("s, (int)c")]
        [InlineData("s, n, n, n")]
        [InlineData("s, (int)c, n, (int)c")]
        public Task StringBuilderCtor_NoImplicitConversions_NoDiagnostic_CS(string argumentList)
        {
            string source = $@"
using System.Text;
public class Testopolis
{{
    public void M(string s, char c, int n)
    {{
        new StringBuilder({argumentList});
    }}
}}";

            return VerifyCS.VerifyAnalyzerAsync(source);
        }

        private static DiagnosticDescriptor Rule => SuspiciousCastFromCharToIntAnalyzer.Rule;
    }
}

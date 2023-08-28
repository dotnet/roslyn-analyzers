// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUseSearchValuesAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUseSearchValuesFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class UseSearchValuesTests
    {
        [Fact]
        public async Task TestIndexOfAnyAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp7_3,
                """
                using System;

                internal sealed class Test
                {
                    private const string ShortConstStringTypeMember = "foo";
                    private const string LongConstStringTypeMember = "aeiouA";
                    private static string NonConstStringProperty => "aeiouA";
                    private string NonConstStringInstanceField = "aeiouA";
                    private static string NonConstStringStaticField = "aeiouA";
                    private readonly string NonConstStringReadonlyInstanceField = "aeiouA";
                    private static readonly string NonConstStringReadonlyStaticField = "aeiouA";
                    private const char ConstChar = 'A';
                    private static char NonConstChar => 'A';
                    private const byte ConstByte = (byte)'A';
                    private static byte NonConstByte => (byte)'A';
                    private static readonly char[] ShortStaticReadonlyCharArrayField = new[] { 'a', 'e', 'i', 'o', 'u' };
                    private static readonly char[] LongStaticReadonlyCharArrayField = new[] { 'a', 'e', 'i', 'o', 'u', 'A' };
                    private static readonly char[] LongStaticReadonlyExplicitCharArrayField = new char[] { 'a', 'e', 'i', 'o', 'u', 'A' };
                    private readonly char[] InstanceReadonlyCharArrayField = new[] { 'a', 'e', 'i', 'o', 'u', 'A' };
                    private char[] InstanceSettableCharArrayField = new[] { 'a', 'e', 'i', 'o', 'u', 'A' };
                    private ReadOnlySpan<char> ShortReadOnlySpanOfCharRVAProperty => new[] { 'a', 'e', 'i', 'o', 'u' };
                    private ReadOnlySpan<char> LongReadOnlySpanOfCharRVAProperty => new[] { 'a', 'e', 'i', 'o', 'u', 'A' };
                    private ReadOnlySpan<byte> ShortReadOnlySpanOfByteRVAProperty => new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u' };
                    private ReadOnlySpan<byte> LongReadOnlySpanOfByteRVAProperty => new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' };

                    private void TestMethod(ReadOnlySpan<char> chars, ReadOnlySpan<byte> bytes)
                    {
                        const string ShortConstStringLocal = "foo";
                        const string LongConstStringLocal = "aeiouA";
                        string NonConstStringLocal = "aeiouA";

                        _ = chars.IndexOfAny("aeiou");
                        _ = chars.IndexOfAny([|"aeiouA"|]);
                        _ = chars.IndexOfAny("aeiouA" + NonConstStringProperty);
                        _ = chars.IndexOfAny("aeiouA" + NonConstStringLocal);

                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u' });
                        _ = chars.IndexOfAny([|new[] { 'a', 'e', 'i', 'o', 'u', 'A' }|]);
                        _ = chars.IndexOfAny([|new char[] { 'a', 'e', 'i', 'o', 'u', 'A' }|]);
                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u', NonConstChar });

                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u' });
                        _ = bytes.IndexOfAny([|new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }|]);
                        _ = bytes.IndexOfAny([|new byte[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }|]);
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', NonConstByte });

                        _ = chars.IndexOfAny(ShortConstStringTypeMember);
                        _ = chars.IndexOfAny([|LongConstStringTypeMember|]);
                        _ = chars.IndexOfAny(NonConstStringProperty);

                        _ = chars.IndexOfAny(ShortConstStringLocal);
                        _ = chars.IndexOfAny([|LongConstStringLocal|]);
                        _ = chars.IndexOfAny(NonConstStringLocal);

                        _ = chars.IndexOfAny(ShortStaticReadonlyCharArrayField);
                        _ = chars.IndexOfAny([|LongStaticReadonlyCharArrayField|]);
                        _ = chars.IndexOfAny([|LongStaticReadonlyExplicitCharArrayField|]);
                        _ = chars.IndexOfAny([|InstanceReadonlyCharArrayField|]);
                        _ = chars.IndexOfAny(InstanceSettableCharArrayField);

                        _ = chars.IndexOfAny(ShortReadOnlySpanOfCharRVAProperty);
                        _ = chars.IndexOfAny([|LongReadOnlySpanOfCharRVAProperty|]);

                        _ = bytes.IndexOfAny(ShortReadOnlySpanOfByteRVAProperty);
                        _ = bytes.IndexOfAny([|LongReadOnlySpanOfByteRVAProperty|]);


                        // For cases that we'd want to flag, a different analyzer should suggest making the field 'const' first.
                        _ = chars.IndexOfAny(NonConstStringInstanceField);
                        _ = chars.IndexOfAny(NonConstStringStaticField);
                        _ = chars.IndexOfAny(NonConstStringReadonlyInstanceField);
                        _ = chars.IndexOfAny(NonConstStringReadonlyStaticField);


                        // A few cases that could be flagged, but currently aren't:
                        _ = chars.IndexOfAny("aeiou" + 'A');
                        _ = chars.IndexOfAny("aeiou" + "A");
                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u', ConstChar });
                        _ = chars.IndexOfAny("aeiouA" + ShortConstStringTypeMember);
                        _ = chars.IndexOfAny(LongConstStringTypeMember + ShortConstStringTypeMember);
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', ConstByte });
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)ConstChar });
                    }
                }
                """);
        }

        [Fact]
        public async Task TestUtf8StringLiteralsAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp11,
                """
                using System;

                internal sealed class Test
                {
                    private ReadOnlySpan<byte> ShortReadOnlySpanOfByteRVAPropertyU8 => "aeiou"u8;
                    private ReadOnlySpan<byte> LongReadOnlySpanOfByteRVAPropertyU8 => "aeiouA"u8;

                    private void TestMethod(ReadOnlySpan<byte> bytes)
                    {
                        _ = bytes.IndexOfAny("aeiou"u8);
                        _ = bytes.IndexOfAny([|"aeiouA"u8|]);

                        _ = bytes.IndexOfAny(ShortReadOnlySpanOfByteRVAPropertyU8);
                        _ = bytes.IndexOfAny([|LongReadOnlySpanOfByteRVAPropertyU8|]);
                    }
                }
                """);
        }

        [Fact]
        public async Task TestAllIndexOfAnyAndContainsAnyOverloadsAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp11,
                """
                using System;

                internal sealed class Test
                {
                    private void TestMethod(ReadOnlySpan<char> chars, ReadOnlySpan<byte> bytes, Span<char> writableChars)
                    {
                        _ = chars.IndexOfAny([|"aeiouA"|]);
                        _ = chars.IndexOfAnyExcept([|"aeiouA"|]);
                        _ = chars.LastIndexOfAny([|"aeiouA"|]);
                        _ = chars.LastIndexOfAnyExcept([|"aeiouA"|]);
                        _ = chars.ContainsAny([|"aeiouA"|]);
                        _ = chars.ContainsAnyExcept([|"aeiouA"|]);

                        _ = bytes.IndexOfAny([|"aeiouA"u8|]);
                        _ = bytes.IndexOfAnyExcept([|"aeiouA"u8|]);
                        _ = bytes.LastIndexOfAny([|"aeiouA"u8|]);
                        _ = bytes.LastIndexOfAnyExcept([|"aeiouA"u8|]);
                        _ = bytes.ContainsAny([|"aeiouA"u8|]);
                        _ = bytes.ContainsAnyExcept([|"aeiouA"u8|]);

                        _ = writableChars.IndexOfAny([|"aeiouA"|]);
                        _ = writableChars.IndexOfAnyExcept([|"aeiouA"|]);
                    }
                }
                """);
        }

        [Theory]
        [InlineData("const string", "= \"aeiouA\"", true)]
        [InlineData("static readonly char[]", "= new[] { 'a', 'e', 'i', 'o', 'u', 'A' }", false)]
        [InlineData("static readonly byte[]", "= new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }", false)]
        [InlineData("readonly char[]", "= new[] { 'a', 'e', 'i', 'o', 'u', 'A' }", false)]
        [InlineData("readonly byte[]", "= new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }", false)]
        [InlineData("readonly char[]", "= new char[] { 'a', 'e', 'i', 'o', 'u', 'A' }", false)]
        [InlineData("readonly char[]", "= new char[]  { 'a', 'e', 'i', 'o', 'u',  'A' }", false)]
        [InlineData("ReadOnlySpan<char>", "=> new[] { 'a', 'e', 'i', 'o', 'u', 'A' }", false)]
        [InlineData("ReadOnlySpan<byte>", "=> new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }", false)]
        [InlineData("ReadOnlySpan<byte>", "=> \"aeiouA\"u8", false)]
        [InlineData("static ReadOnlySpan<char>", "=> new[] { 'a', 'e', 'i', 'o', 'u', 'A' }", true)]
        [InlineData("static ReadOnlySpan<byte>", "=> new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }", true)]
        [InlineData("static ReadOnlySpan<byte>", "=> \"aeiouA\"u8", true)]
        public async Task TestCodeFixerNamedArguments(string modifiersAndType, string initializer, bool createWillUseMemberReference)
        {
            const string OriginalValuesName = "MyValuesTypeMember";
            const string SearchValuesFieldName = "s_myValuesTypeMemberSearchValues";

            string byteOrChar = modifiersAndType.Contains("byte", StringComparison.Ordinal) ? "byte" : "char";
            string memberDefinition = $"{modifiersAndType} {OriginalValuesName} {initializer};";
            string createExpression = createWillUseMemberReference ? OriginalValuesName : initializer.TrimStart('=', '>', ' ');
            bool isProperty = initializer.Contains("=>", StringComparison.Ordinal);

            string source =
                $$"""
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    {{memberDefinition}}

                    private void TestMethod(ReadOnlySpan<{{byteOrChar}}> span)
                    {
                        _ = span.IndexOfAny([|{{OriginalValuesName}}|]);
                    }
                }
                """;

            string expected =
                $$"""
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private static readonly SearchValues<{{byteOrChar}}> {{SearchValuesFieldName}} = SearchValues.Create({{createExpression}});{{(isProperty ? Environment.NewLine : "")}}
                    {{memberDefinition}}

                    private void TestMethod(ReadOnlySpan<{{byteOrChar}}> span)
                    {
                        _ = span.IndexOfAny({{SearchValuesFieldName}});
                    }
                }
                """;

            var languageVersion = initializer.Contains("u8", StringComparison.Ordinal)
                ? LanguageVersion.CSharp11
                : LanguageVersion.CSharp7_3;

            await VerifyCodeFixAsync(languageVersion, source, expected);
        }

        [Fact]
        public async Task TestCodeFixerLocalStringConst()
        {
            string source =
                """
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        const string Values = "aeiouA";

                        _ = chars.IndexOfAny([|Values|]);
                    }
                }
                """;

            string expected =
                """
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private static readonly SearchValues<char> s_valuesSearchValues = SearchValues.Create("aeiouA");

                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        const string Values = "aeiouA";

                        _ = chars.IndexOfAny(s_valuesSearchValues);
                    }
                }
                """;

            await VerifyCodeFixAsync(LanguageVersion.CSharp7_3, source, expected);
        }

        [Theory]
        [InlineData("\"aeiouA\"")]
        [InlineData("\"aeiouA\"u8")]
        [InlineData("new[] { 'a', 'e', 'i', 'o', 'u', 'A' }")]
        [InlineData("new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }")]
        [InlineData("new char[] { 'a', 'e', 'i', 'o', 'u', 'A' }")]
        [InlineData("new char[]  { 'a', 'e', 'i', 'o',  'u', 'A' }")]
        public async Task TestCodeFixerInlineArguments(string values)
        {
            string byteOrChar = values.Contains("byte", StringComparison.Ordinal) || values.Contains("u8", StringComparison.Ordinal) ? "byte" : "char";
            string searchValuesFieldName = byteOrChar == "byte" ? "s_myBytes" : "s_myChars";

            string source =
                $$"""
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private void TestMethod(ReadOnlySpan<{{byteOrChar}}> span)
                    {
                        _ = span.IndexOfAny([|{{values}}|]);
                    }
                }
                """;

            string expected =
                $$"""
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private static readonly SearchValues<{{byteOrChar}}> {{searchValuesFieldName}} = SearchValues.Create({{values}});

                    private void TestMethod(ReadOnlySpan<{{byteOrChar}}> span)
                    {
                        _ = span.IndexOfAny({{searchValuesFieldName}});
                    }
                }
                """;

            var languageVersion = values.Contains("u8", StringComparison.Ordinal)
                ? LanguageVersion.CSharp11
                : LanguageVersion.CSharp7_3;

            await VerifyCodeFixAsync(languageVersion, source, expected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestCodeFixerAccountsForUsingStatements(bool sourceHasNamespaceUsing)
        {
            const string Namespace = "System.Buffers";

            string usingLine = sourceHasNamespaceUsing ? $"using {Namespace};" : "";
            string namespacePrefix = sourceHasNamespaceUsing ? "" : $"{Namespace}.";

            string source =
                $$"""
                using System;
                {{usingLine}}

                internal sealed class Test
                {
                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        _ = chars.IndexOfAny([|"aeiouA"|]);
                    }
                }
                """;

            string expected =
                $$"""
                using System;
                {{usingLine}}

                internal sealed class Test
                {
                    private static readonly {{namespacePrefix}}SearchValues<char> s_myChars = {{namespacePrefix}}SearchValues.Create("aeiouA");

                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        _ = chars.IndexOfAny(s_myChars);
                    }
                }
                """;

            await VerifyCodeFixAsync(LanguageVersion.CSharp7_3, source, expected);
        }

        [Fact]
        public async Task TestCodeFixerAvoidsMemberNameConflicts()
        {
            string source =
                """
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private static string s_myChars => "";
                    private static readonly string s_myChars1 = "";
                    private static string s_myChars2() => "";
                    private sealed class s_myChars3 { }

                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        _ = chars.IndexOfAny([|"aeiouA"|]);
                    }
                }
                """;

            string expected =
                """
                using System;
                using System.Buffers;

                internal sealed class Test
                {
                    private static readonly SearchValues<char> s_myChars4 = SearchValues.Create("aeiouA");

                    private static string s_myChars => "";
                    private static readonly string s_myChars1 = "";
                    private static string s_myChars2() => "";
                    private sealed class s_myChars3 { }

                    private void TestMethod(ReadOnlySpan<char> chars)
                    {
                        _ = chars.IndexOfAny(s_myChars4);
                    }
                }
                """;

            await VerifyCodeFixAsync(LanguageVersion.CSharp7_3, source, expected);
        }

        private static async Task VerifyAnalyzerAsync(LanguageVersion languageVersion, string source)
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = Net80,
                LanguageVersion = languageVersion,
                TestCode = source,
            }.RunAsync();
        }

        private static async Task VerifyCodeFixAsync(LanguageVersion languageVersion, string source, string expected)
        {
            await new VerifyCS.Test
            {
                ReferenceAssemblies = Net80,
                LanguageVersion = languageVersion,
                TestCode = source,
                FixedCode = expected,
            }.RunAsync();
        }

        // TEMP - need newer version of Microsoft.CodeAnalysis.Analyzer.Testing
        private static readonly Lazy<ReferenceAssemblies> _lazyNet80 =
                new Lazy<ReferenceAssemblies>(() =>
                {
                    if (!NuGet.Frameworks.NuGetFramework.Parse("net8.0").IsPackageBased)
                    {
                        // The NuGet version provided at runtime does not recognize the 'net8.0' target framework
                        throw new NotSupportedException("The 'net8.0' target framework is not supported by this version of NuGet.");
                    }

                    return new ReferenceAssemblies(
                        "net8.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "8.0.0-preview.7.23375.6"),
                        System.IO.Path.Combine("ref", "net8.0"));
                });

        public static ReferenceAssemblies Net80 => _lazyNet80.Value;
    }
}

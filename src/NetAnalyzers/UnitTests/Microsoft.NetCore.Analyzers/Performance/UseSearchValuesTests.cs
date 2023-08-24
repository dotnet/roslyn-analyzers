// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.CSharpUseSearchValuesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class UseSearchValuesTests
    {
        [Fact]
        public async Task TestIndexOfAnyAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp7_3, @"
                using System;

                internal sealed class Test
                {
                    private const string ShortConstStringTypeMember = ""foo"";
                    private const string LongConstStringTypeMember = ""aeiouA"";
                    private static string NonConstStringTypeMember => ""aeiouA"";
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
                        const string ShortConstStringLocal = ""foo"";
                        const string LongConstStringLocal = ""aeiouA"";
                        string NonConstStringLocal = ""aeiouA"";

                        _ = chars.IndexOfAny(""aeiou"");
                        _ = chars.IndexOfAny({|CA1870:""aeiouA""|});
                        _ = chars.IndexOfAny(""aeiouA"" + NonConstStringTypeMember);
                        _ = chars.IndexOfAny(""aeiouA"" + NonConstStringLocal);

                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u' });
                        _ = chars.IndexOfAny({|CA1870:new[] { 'a', 'e', 'i', 'o', 'u', 'A' }|});
                        _ = chars.IndexOfAny({|CA1870:new char[] { 'a', 'e', 'i', 'o', 'u', 'A' }|});
                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u', NonConstChar });

                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u' });
                        _ = bytes.IndexOfAny({|CA1870:new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }|});
                        _ = bytes.IndexOfAny({|CA1870:new byte[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)'A' }|});
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', NonConstByte });

                        _ = chars.IndexOfAny(ShortConstStringTypeMember);
                        _ = chars.IndexOfAny({|CA1870:LongConstStringTypeMember|});
                        _ = chars.IndexOfAny(NonConstStringTypeMember);

                        _ = chars.IndexOfAny(ShortConstStringLocal);
                        _ = chars.IndexOfAny({|CA1870:LongConstStringLocal|});
                        _ = chars.IndexOfAny(NonConstStringLocal);

                        _ = chars.IndexOfAny(ShortStaticReadonlyCharArrayField);
                        _ = chars.IndexOfAny({|CA1870:LongStaticReadonlyCharArrayField|});
                        _ = chars.IndexOfAny({|CA1870:LongStaticReadonlyExplicitCharArrayField|});
                        _ = chars.IndexOfAny({|CA1870:InstanceReadonlyCharArrayField|});
                        _ = chars.IndexOfAny(InstanceSettableCharArrayField);

                        _ = chars.IndexOfAny(ShortReadOnlySpanOfCharRVAProperty);
                        _ = chars.IndexOfAny({|CA1870:LongReadOnlySpanOfCharRVAProperty|});

                        _ = bytes.IndexOfAny(ShortReadOnlySpanOfByteRVAProperty);
                        _ = bytes.IndexOfAny({|CA1870:LongReadOnlySpanOfByteRVAProperty|});


                        // A few cases that could be flagged, but currently aren't:
                        _ = chars.IndexOfAny(""aeiou"" + 'A');
                        _ = chars.IndexOfAny(""aeiou"" + ""A"");
                        _ = chars.IndexOfAny(new[] { 'a', 'e', 'i', 'o', 'u', ConstChar });
                        _ = chars.IndexOfAny(""aeiouA"" + ShortConstStringTypeMember);
                        _ = chars.IndexOfAny(LongConstStringTypeMember + ShortConstStringTypeMember);
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', ConstByte });
                        _ = bytes.IndexOfAny(new[] { (byte)'a', (byte)'e', (byte)'i', (byte)'o', (byte)'u', (byte)ConstChar });
                    }
                }
            ");
        }

        [Fact]
        public async Task TestUtf8StringLiteralsAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp11, @"
                using System;

                internal sealed class Test
                {
                    private ReadOnlySpan<byte> ShortReadOnlySpanOfByteRVAPropertyU8 => ""aeiou""u8;
                    private ReadOnlySpan<byte> LongReadOnlySpanOfByteRVAPropertyU8 => ""aeiouA""u8;

                    private void TestMethod(ReadOnlySpan<byte> bytes)
                    {
                        _ = bytes.IndexOfAny(""aeiou""u8);
                        _ = bytes.IndexOfAny({|CA1870:""aeiouA""u8|});

                        _ = bytes.IndexOfAny(ShortReadOnlySpanOfByteRVAPropertyU8);
                        _ = bytes.IndexOfAny({|CA1870:LongReadOnlySpanOfByteRVAPropertyU8|});
                    }
                }
            ");
        }

        [Fact]
        public async Task TestAllIndexOfAnyAndContainsAnyOverloadsAnalyzer()
        {
            await VerifyAnalyzerAsync(LanguageVersion.CSharp11, @"
                using System;

                internal sealed class Test
                {
                    private void TestMethod(ReadOnlySpan<char> chars, ReadOnlySpan<byte> bytes)
                    {
                        _ = chars.IndexOfAny({|CA1870:""aeiouA""|});
                        _ = chars.IndexOfAnyExcept({|CA1870:""aeiouA""|});
                        _ = chars.LastIndexOfAny({|CA1870:""aeiouA""|});
                        _ = chars.LastIndexOfAnyExcept({|CA1870:""aeiouA""|});
                        _ = chars.ContainsAny({|CA1870:""aeiouA""|});
                        _ = chars.ContainsAnyExcept({|CA1870:""aeiouA""|});

                        _ = bytes.IndexOfAny({|CA1870:""aeiouA""u8|});
                        _ = bytes.IndexOfAnyExcept({|CA1870:""aeiouA""u8|});
                        _ = bytes.LastIndexOfAny({|CA1870:""aeiouA""u8|});
                        _ = bytes.LastIndexOfAnyExcept({|CA1870:""aeiouA""u8|});
                        _ = bytes.ContainsAny({|CA1870:""aeiouA""u8|});
                        _ = bytes.ContainsAnyExcept({|CA1870:""aeiouA""u8|});
                    }
                }
            ");
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

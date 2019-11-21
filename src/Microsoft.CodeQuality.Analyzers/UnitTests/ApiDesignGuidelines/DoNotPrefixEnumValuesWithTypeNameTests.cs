// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotPrefixEnumValuesWithTypeNameAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotPrefixEnumValuesWithTypeNameAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.ApiDesignGuidelines
{
    public class DoNotPrefixEnumValuesWithTypeNameTests
    {
        [Fact]
        public async Task CSharp_NoDiagnostic_NoPrefix()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                { 
                    enum State
                    {
                        Ok = 0,
                        Error = 1,
                        Unknown = 2
                    };
                }");
        }

        [Fact]
        public async Task Basic_NoDiagnostic_NoPrefix()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
                Class A
                    Private Enum State
                        Ok = 0
                        Err = 1
                        Unknown = 2
                    End Enum
                End Class");
        }

        [Fact]
        public async Task CSharp_Diagnostic_EachValuePrefixed()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                {
                    enum State
                    {
                        StateOk = 0,
                        StateError = 1,
                        StateUnknown = 2
                    };
                }",
                GetCSharpResultAt(6, 25, "State"),
                GetCSharpResultAt(7, 25, "State"),
                GetCSharpResultAt(8, 25, "State"));
        }

        [Fact]
        public async Task Basic_Diagnostic_EachValuePrefixed()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
                Class A
                    Private Enum State
                        StateOk = 0
                        StateErr = 1
                        StateUnknown = 2
                    End Enum
                End Class
                ",
                GetBasicResultAt(4, 25, "State"),
                GetBasicResultAt(5, 25, "State"),
                GetBasicResultAt(6, 25, "State"));
        }

        [Fact]
        public async Task CSharp_NoDiagnostic_HalfOfValuesPrefixed()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                {
                    enum State
                    {
                        Ok = 0,
                        StateError = 1,
                        StateUnknown = 2,
                        Invalid = 3
                    };
                }");
        }

        [Fact]
        public async Task CSharp_Diagnostic_ThreeOfFourValuesPrefixed()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                {
                    enum State
                    {
                        StateOk = 0,
                        StateError = 1,
                        StateUnknown = 2,
                        Invalid = 3
                    };
                }",
                GetCSharpResultAt(6, 25, "State"),
                GetCSharpResultAt(7, 25, "State"),
                GetCSharpResultAt(8, 25, "State"));
        }

        [Fact]
        public async Task CSharp_Diagnostic_PrefixCaseDiffers()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                {
                    enum State
                    {
                        stateOk = 0
                    };
                }",
                GetCSharpResultAt(6, 25, "State"));
        }

        [Fact]
        public async Task CSharp_NoDiagnostic_EmptyEnum()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                class A
                {
                    enum State
                    {
                    };
                }");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
            => new DiagnosticResult(DoNotPrefixEnumValuesWithTypeNameAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
            => new DiagnosticResult(DoNotPrefixEnumValuesWithTypeNameAnalyzer.Rule)
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}

﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#pragma warning disable CA1305

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Microsoft.CodeAnalysis.PublicApiAnalyzers.UnitTests
{
    public class AnnotatePublicApiAnalyzerTests
    {
        private async Task VerifyCSharpAsync(string source, string shippedApiText, string unshippedApiText, params DiagnosticResult[] expected)
        {
            var test = new CSharpCodeFixTest<DeclarePublicApiAnalyzer, AnnotatePublicApiFix, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles = { },
                },
                TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck,
            };

            if (shippedApiText != null)
            {
                test.TestState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.ShippedFileName, shippedApiText));
            }

            if (unshippedApiText != null)
            {
                test.TestState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.UnshippedFileName, unshippedApiText));
            }

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        private async Task VerifyCSharpAdditionalFileFixAsync(string source, string oldShippedApiText, string oldUnshippedApiText, string newShippedApiText, string newUnshippedApiText)
        {
            await VerifyAdditionalFileFixAsync(source, oldShippedApiText, oldUnshippedApiText, newShippedApiText, newUnshippedApiText);
        }

        private async Task VerifyAdditionalFileFixAsync(string source, string oldShippedApiText, string oldUnshippedApiText, string newShippedApiText, string newUnshippedApiText)
        {
            var test = new CSharpCodeFixTest<DeclarePublicApiAnalyzer, AnnotatePublicApiFix, XUnitVerifier>();

            test.TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

            test.TestState.Sources.Add(source);
            test.TestState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.ShippedFileName, oldShippedApiText));
            test.TestState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.UnshippedFileName, oldUnshippedApiText));

            test.FixedState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.ShippedFileName, newShippedApiText));
            test.FixedState.AdditionalFiles.Add((DeclarePublicApiAnalyzer.UnshippedFileName, newUnshippedApiText));

            await test.RunAsync();
        }

        #region Fix tests

        // TODO2 test fix all
        [Fact]
        public async Task DoNotAnnotateMemberInUnannotatedUnshippedAPI()
        {
            var source = @"
#nullable enable
public class C
{
    public string? Field;
    //public string Field2; // TODO2
}
";

            var shippedText = @"";
            var unshippedText = @"C
C.C() -> void
C.Field -> string";

            await VerifyCSharpAsync(source, shippedText, unshippedText, System.Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task DoNotAnnotateMemberInUnannotatedShippedAPI()
        {
            var source = @"
#nullable enable
public class C
{
    public string? Field;
    //public string Field2; // TODO2
}
";

            var shippedText = @"C
C.C() -> void
C.Field -> string";
            var unshippedText = @"";

            await VerifyCSharpAsync(source, shippedText, unshippedText, System.Array.Empty<DiagnosticResult>());
        }

        [Fact]
        public async Task AnnotatedMemberInAnnotatedShippedAPI()
        {
            var source = @"
#nullable enable
public class C
{
    public string? OldField;
    public string? {|RS0036:Field|};
    //public string Field2; // TODO2
}
";

            var shippedText = @"#nullable enable
C
C.C() -> void
C.OldField -> string?
C.Field -> string";

            var unshippedText = @"";

            var fixedShippedText = @"#nullable enable
C
C.C() -> void
C.OldField -> string?
C.Field -> string?";

            await VerifyCSharpAdditionalFileFixAsync(source, shippedText, unshippedText, fixedShippedText, newUnshippedApiText: unshippedText);
        }

        [Fact]
        public async Task AnnotatedMemberInAnnotatedUnshippedAPI()
        {
            var source = @"
#nullable enable
public class C
{
    public string? OldField;
    public string? {|RS0036:Field|};
    //public string Field2; // TODO2
}
";

            var shippedText = @"#nullable enable";
            var unshippedText = @"C
C.C() -> void
C.OldField -> string?
C.Field -> string";

            var fixedUnshippedText = @"C
C.C() -> void
C.OldField -> string?
C.Field -> string?";

            await VerifyCSharpAdditionalFileFixAsync(source, shippedText, unshippedText, newShippedApiText: shippedText, fixedUnshippedText);
        }

        #endregion
    }
}

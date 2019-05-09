// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.LinkedEnumerationAnalyzer,
    Roslyn.Diagnostics.Analyzers.LinkedEnumerationCodeFix>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.LinkedEnumerationAnalyzer,
    Roslyn.Diagnostics.Analyzers.LinkedEnumerationCodeFix>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class LinkedEnumerationTests
    {
        [Fact]
        public async Task TestSynchronizeEmptyLinkedEnumeration_CSharp()
        {
            var source = @"
using Roslyn.Utilities;

internal enum Glyph
{
    Value1 = 3,
    Value2,
}

[LinkedEnumeration(typeof(Glyph))]
internal enum {|RS0035:ExternalGlyph|}
{
}
" + LinkedEnumeration.CSharp;
            var fixedSource = @"
using Roslyn.Utilities;

internal enum Glyph
{
    Value1 = 3,
    Value2,
}

[LinkedEnumeration(typeof(Glyph))]
internal enum ExternalGlyph
{
    Value1 = Glyph.Value1,
    Value2 = Glyph.Value2
}
" + LinkedEnumeration.CSharp;

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task TestSynchronizeEmptyLinkedEnumeration_VisualBasic()
        {
            var source = @"
Imports Roslyn.Utilities

Friend Enum Glyph
    Value1 = 3
    Value2
End Enum

<LinkedEnumeration(GetType(Glyph))>
Friend Enum {|BC30280:{|RS0035:ExternalGlyph|}|}
End Enum
" + LinkedEnumeration.VisualBasic;
            var fixedSource = @"
Imports Roslyn.Utilities

Friend Enum Glyph
    Value1 = 3
    Value2
End Enum

<LinkedEnumeration(GetType(Glyph))>
Friend Enum ExternalGlyph
    Value1 = Glyph.Value1
    Value2 = Glyph.Value2
End Enum
" + LinkedEnumeration.VisualBasic;

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async Task TestSynchronizeLinkedEnumerationValues_CSharp()
        {
            var source = @"
using Roslyn.Utilities;

internal enum Glyph
{
    Value1 = 3,
    Value2,
}

[LinkedEnumeration(typeof(Glyph))]
internal enum ExternalGlyph
{
    {|RS0036:Value1|},
    {|RS0036:Value2|},
}
" + LinkedEnumeration.CSharp;
            var fixedSource = @"
using Roslyn.Utilities;

internal enum Glyph
{
    Value1 = 3,
    Value2,
}

[LinkedEnumeration(typeof(Glyph))]
internal enum ExternalGlyph
{
    Value1 = Glyph.Value1,
    Value2,
}
" + LinkedEnumeration.CSharp;
            var batchFixedSource = @"
using Roslyn.Utilities;

internal enum Glyph
{
    Value1 = 3,
    Value2,
}

[LinkedEnumeration(typeof(Glyph))]
internal enum ExternalGlyph
{
    Value1 = Glyph.Value1,
    Value2 = Glyph.Value2,
}
" + LinkedEnumeration.CSharp;

            await new VerifyCS.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                BatchFixedState = { Sources = { batchFixedSource } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestSynchronizeLinkedEnumerationValues_VisualBasic()
        {
            var source = @"
Imports Roslyn.Utilities

Friend Enum Glyph
    Value1 = 3
    Value2
End Enum

<LinkedEnumeration(GetType(Glyph))>
Friend Enum ExternalGlyph
    {|RS0036:Value1|}
    {|RS0036:Value2|}
End Enum
" + LinkedEnumeration.VisualBasic;
            var fixedSource = @"
Imports Roslyn.Utilities

Friend Enum Glyph
    Value1 = 3
    Value2
End Enum

<LinkedEnumeration(GetType(Glyph))>
Friend Enum ExternalGlyph
    Value1 = Glyph.Value1
    Value2
End Enum
" + LinkedEnumeration.VisualBasic;
            var batchFixedSource = @"
Imports Roslyn.Utilities

Friend Enum Glyph
    Value1 = 3
    Value2
End Enum

<LinkedEnumeration(GetType(Glyph))>
Friend Enum ExternalGlyph
    Value1 = Glyph.Value1
    Value2 = Glyph.Value2
End Enum
" + LinkedEnumeration.VisualBasic;

            await new VerifyVB.Test
            {
                TestState = { Sources = { source } },
                FixedState = { Sources = { fixedSource } },
                BatchFixedState = { Sources = { batchFixedSource } },
            }.RunAsync();
        }
    }
}

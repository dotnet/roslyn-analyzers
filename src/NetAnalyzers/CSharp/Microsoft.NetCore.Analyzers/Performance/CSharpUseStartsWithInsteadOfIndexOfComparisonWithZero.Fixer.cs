// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix : UseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix
    {
        protected override SyntaxNode AppendElasticMarker(SyntaxNode replacement)
            => replacement.WithTrailingTrivia(SyntaxFactory.ElasticMarker);
    }
}

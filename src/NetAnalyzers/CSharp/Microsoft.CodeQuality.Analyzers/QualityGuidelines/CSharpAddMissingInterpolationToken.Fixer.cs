﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpAddMissingInterpolationTokenFixer : AbstractAddMissingInterpolationTokenFixer
    {
        private protected override SyntaxNode GetReplacement(SyntaxNode node)
            => SyntaxFactory.ParseExpression("$" + node.ToString());
    }
}
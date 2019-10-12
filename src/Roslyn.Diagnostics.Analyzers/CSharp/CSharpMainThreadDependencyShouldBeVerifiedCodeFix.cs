// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharpMainThreadDependencyShouldBeVerifiedCodeFix))]
    [Shared]
    public class CSharpMainThreadDependencyShouldBeVerifiedCodeFix : AbstractMainThreadDependencyShouldBeVerifiedCodeFix
    {
        protected override bool IsAttributeArgumentNamedVerified(SyntaxNode attributeArgument)
        {
            return attributeArgument is AttributeArgumentSyntax syntax
                && syntax.NameEquals?.Name.Identifier.ValueText == "Verified";
        }
    }
}

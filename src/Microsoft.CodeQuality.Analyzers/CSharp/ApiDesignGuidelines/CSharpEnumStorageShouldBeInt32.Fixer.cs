// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.ApiDesignGuidelines.CSharp.Analyzers
{
    /// <summary> 
    /// CA1028: Enum Storage should be Int32
    /// </summary> 
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpEnumStorageShouldBeInt32Fixer : EnumStorageShouldBeInt32Fixer
    {
        protected override SyntaxNode GetTargetNode(SyntaxNode node)
        {
            var enumDecl = (EnumDeclarationSyntax)node;
            var baseTypeNode = (enumDecl.BaseList.Types.FirstOrDefault() as SimpleBaseTypeSyntax)?.Type;
            return baseTypeNode; 
        }
    }
} 

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2225: Operator overloads have named alternates
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpOperatorOverloadsHaveNamedAlternatesFixer
        : OperatorOverloadsHaveNamedAlternatesFixer<OperatorDeclarationSyntax, ConversionOperatorDeclarationSyntax, ClassDeclarationSyntax, MethodDeclarationSyntax, PropertyDeclarationSyntax>
    {
    }
}
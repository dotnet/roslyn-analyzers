// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using ApiReview.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace ApiReview.CSharp.Analyzers
{
    /// <summary>
    /// CA2001: Avoid calling problematic methods
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpAvoidCallingProblematicMethodsFixer : AvoidCallingProblematicMethodsFixer
    {
    }
}
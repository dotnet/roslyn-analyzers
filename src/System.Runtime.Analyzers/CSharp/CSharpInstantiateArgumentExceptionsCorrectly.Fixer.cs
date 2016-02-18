// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpInstantiateArgumentExceptionsCorrectlyFixer : InstantiateArgumentExceptionsCorrectlyFixer
    {
    }
}
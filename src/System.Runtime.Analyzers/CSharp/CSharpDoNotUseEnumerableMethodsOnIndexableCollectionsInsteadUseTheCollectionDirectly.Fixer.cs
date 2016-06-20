// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Runtime.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Runtime.CSharp.Analyzers
{
    /// <summary>
    /// RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer : DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
    {
    }
}
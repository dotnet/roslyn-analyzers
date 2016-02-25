// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0006: Do not mix attributes from different versions of MEF
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpDoNotMixAttributesFromDifferentVersionsOfMEFFixer : DoNotMixAttributesFromDifferentVersionsOfMEFFixer
    {
    }
}
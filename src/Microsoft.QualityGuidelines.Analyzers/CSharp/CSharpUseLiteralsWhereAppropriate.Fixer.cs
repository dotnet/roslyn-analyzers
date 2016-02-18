// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA1802: Use literals where appropriate
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUseLiteralsWhereAppropriateFixer : UseLiteralsWhereAppropriateFixer
    {
    }
}
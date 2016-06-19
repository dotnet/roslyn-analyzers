// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Desktop.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Desktop.CSharp.Analyzers
{
    /// <summary>
    /// CA1058: Types should not extend certain base types
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpTypesShouldNotExtendCertainBaseTypesFixer : TypesShouldNotExtendCertainBaseTypesFixer
    {
    }
}
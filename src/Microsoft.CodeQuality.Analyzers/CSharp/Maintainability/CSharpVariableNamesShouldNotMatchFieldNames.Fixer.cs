// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.Maintainability.Analyzers;

namespace Microsoft.Maintainability.CSharp.Analyzers
{
    /// <summary>
    /// CA1500: Variable names should not match field names
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpVariableNamesShouldNotMatchFieldNamesFixer : VariableNamesShouldNotMatchFieldNamesFixer
    {
    }
}
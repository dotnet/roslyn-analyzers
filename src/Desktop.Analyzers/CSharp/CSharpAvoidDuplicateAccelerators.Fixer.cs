// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Desktop.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Desktop.CSharp.Analyzers
{
    /// <summary>
    /// CA1301: Avoid duplicate accelerators
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpAvoidDuplicateAcceleratorsFixer : AvoidDuplicateAcceleratorsFixer
    {
    }
}
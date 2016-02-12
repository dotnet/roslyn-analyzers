// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2240: Implement ISerializable correctly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpImplementISerializableCorrectlyFixer : ImplementISerializableCorrectlyFixer
    {
    }
}
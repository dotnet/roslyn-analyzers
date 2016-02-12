// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2207: Initialize value type static fields inline
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpInitializeStaticFieldsInlineFixer : InitializeStaticFieldsInlineFixer<SyntaxKind>
    {
    }
}
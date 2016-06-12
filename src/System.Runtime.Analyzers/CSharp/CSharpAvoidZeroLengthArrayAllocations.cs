// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.CSharp.Analyzers
{
    /// <summary>
    /// RS0007: Avoid zero-length array allocations.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidZeroLengthArrayAllocationsAnalyzer : AvoidZeroLengthArrayAllocationsAnalyzer
    {
        protected override bool IsAttributeSyntax(SyntaxNode node)
        {
            return node is AttributeSyntax;
        }
    }
}
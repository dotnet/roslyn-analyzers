// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ApiReview.Analyzers
{
    /// <summary>
    /// CA2001: Avoid calling problematic methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidCallingProblematicMethodsAnalyzer : AvoidCallingProblematicMethodsAnalyzer
    {
    }
}
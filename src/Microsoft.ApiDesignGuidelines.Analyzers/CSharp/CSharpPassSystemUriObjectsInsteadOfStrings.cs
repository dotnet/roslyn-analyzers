// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2234: Pass system uri objects instead of strings
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpPassSystemUriObjectsInsteadOfStringsAnalyzer : PassSystemUriObjectsInsteadOfStringsAnalyzer
    {
    }
}
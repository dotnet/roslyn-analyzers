// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Collections.Immutable.Analyzers
{
    /// <summary>
    /// RS0012: Do not call ToImmutableArray on an ImmutableArray value
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer : DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer
    {
    }
}
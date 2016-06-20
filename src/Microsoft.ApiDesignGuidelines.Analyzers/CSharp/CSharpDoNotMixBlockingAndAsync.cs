// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.CSharp.Analyzers
{
    /// <summary>
    /// Async006: Don't Mix Blocking and Async
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotMixBlockingAndAsyncAnalyzer : DoNotMixBlockingAndAsyncAnalyzer
    {
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1711: Identifiers should not have incorrect suffix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpIdentifiersShouldNotHaveIncorrectSuffixAnalyzer : IdentifiersShouldNotHaveIncorrectSuffixAnalyzer
    {
    }
}
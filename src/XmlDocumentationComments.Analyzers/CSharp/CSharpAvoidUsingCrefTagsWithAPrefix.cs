// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace XmlDocumentationComments.Analyzers
{
    /// <summary>
    /// RS0010: Avoid using cref tags with a prefix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidUsingCrefTagsWithAPrefixAnalyzer : AvoidUsingCrefTagsWithAPrefixAnalyzer
    {
    }
}
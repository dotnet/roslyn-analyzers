// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2212: Do not mark serviced components with WebMethod
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotMarkServicedComponentsWithWebMethodAnalyzer : DoNotMarkServicedComponentsWithWebMethodAnalyzer
    {
    }
}
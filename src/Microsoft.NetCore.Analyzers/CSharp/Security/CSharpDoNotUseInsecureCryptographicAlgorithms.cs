// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.Analyzers.Security;
using Microsoft.NetCore.Analyzers.Security.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.CSharp.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpDoNotUseInsecureCryptographicAlgorithmsAnalyzer : DoNotUseInsecureCryptographicAlgorithmsAnalyzer
    {
        // This analyzer just exists in the CSharp assembly so that people don't have to redo their rulesets,
        // since DoNotUseInsecureCryptographicAlgorithmsAnalyzer started out as separated by C# and VB.
    }
}

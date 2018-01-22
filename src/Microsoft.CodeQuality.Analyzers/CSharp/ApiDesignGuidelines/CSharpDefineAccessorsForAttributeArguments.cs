// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1019: Define accessors for attribute arguments
    /// 
    /// Cause:
    /// In its constructor, an attribute defines arguments that do not have corresponding properties.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpDefineAccessorsForAttributeArgumentsAnalyzer : DefineAccessorsForAttributeArgumentsAnalyzer
    {
    }
}

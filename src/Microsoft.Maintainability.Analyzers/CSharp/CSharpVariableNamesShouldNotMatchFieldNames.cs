// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Maintainability.Analyzers
{
    /// <summary>
    /// CA1500: Variable names should not match field names
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpVariableNamesShouldNotMatchFieldNamesAnalyzer : VariableNamesShouldNotMatchFieldNamesAnalyzer
    {
    }
}
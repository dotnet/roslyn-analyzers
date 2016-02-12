// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2232: Mark Windows Forms entry points with STAThread
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMarkWindowsFormsEntryPointsWithStaThreadAnalyzer : MarkWindowsFormsEntryPointsWithStaThreadAnalyzer
    {
    }
}
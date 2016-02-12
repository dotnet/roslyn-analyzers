// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.InteropServices.Analyzers
{
    /// <summary>
    /// CA2205: Use managed equivalents of win32 api
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseManagedEquivalentsOfWin32ApiAnalyzer : UseManagedEquivalentsOfWin32ApiAnalyzer
    {
    }
}
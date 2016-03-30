// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1601: Do not use timers that prevent power state changes
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotUseTimersThatPreventPowerStateChangesAnalyzer : DoNotUseTimersThatPreventPowerStateChangesAnalyzer
    {
    }
}
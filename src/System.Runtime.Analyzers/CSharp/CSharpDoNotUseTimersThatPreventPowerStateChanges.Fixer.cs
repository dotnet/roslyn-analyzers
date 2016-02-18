// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1601: Do not use timers that prevent power state changes
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpDoNotUseTimersThatPreventPowerStateChangesFixer : DoNotUseTimersThatPreventPowerStateChangesFixer
    {
    }
}
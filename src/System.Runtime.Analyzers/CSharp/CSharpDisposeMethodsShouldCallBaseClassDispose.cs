// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2215: Dispose Methods Should Call Base Class Dispose
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDisposeMethodsShouldCallBaseClassDisposeAnalyzer : DisposeMethodsShouldCallBaseClassDisposeAnalyzer
    {
    }
}
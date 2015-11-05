// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.InteropServices.Analyzers
{                          
    /// <summary>
    /// RS0015: Always consume the value returned by methods marked with PreserveSigAttribute
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer : AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeAnalyzer
    {

    }
}
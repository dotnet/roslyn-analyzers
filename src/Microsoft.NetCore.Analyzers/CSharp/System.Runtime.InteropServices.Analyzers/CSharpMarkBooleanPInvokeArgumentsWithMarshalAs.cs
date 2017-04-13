// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.InteropServices.CSharp.Analyzers
{
    /// <summary>
    /// CA1414: Mark boolean PInvoke arguments with MarshalAs
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer : MarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer
    {
    }
}
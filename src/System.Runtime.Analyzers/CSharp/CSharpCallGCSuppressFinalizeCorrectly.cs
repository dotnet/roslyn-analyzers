// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1816: Dispose methods should call SuppressFinalize
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpCallGCSuppressFinalizeCorrectlyAnalyzer : CallGCSuppressFinalizeCorrectlyAnalyzer
    {
    }
}
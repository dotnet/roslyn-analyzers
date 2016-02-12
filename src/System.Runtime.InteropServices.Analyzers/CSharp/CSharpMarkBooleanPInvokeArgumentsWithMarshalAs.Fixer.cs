// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Runtime.InteropServices.Analyzers
{
    /// <summary>
    /// CA1414: Mark boolean PInvoke arguments with MarshalAs
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpMarkBooleanPInvokeArgumentsWithMarshalAsFixer : MarkBooleanPInvokeArgumentsWithMarshalAsFixer
    {
    }
}
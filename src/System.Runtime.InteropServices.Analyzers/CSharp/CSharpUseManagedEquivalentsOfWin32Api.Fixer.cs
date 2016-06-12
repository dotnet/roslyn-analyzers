// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Runtime.InteropServices.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace System.Runtime.InteropServices.CSharp.Analyzers
{
    /// <summary>
    /// CA2205: Use managed equivalents of win32 api
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpUseManagedEquivalentsOfWin32ApiFixer : UseManagedEquivalentsOfWin32ApiFixer
    {
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
                                 
using System.Composition;
using System.Diagnostics;  
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Composition.Analyzers
{                                 
    /// <summary>
    /// RS0023: Parts exported with MEFv2 must be marked as Shared
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpPartsExportedWithMEFv2MustBeMarkedAsSharedFixer : PartsExportedWithMEFv2MustBeMarkedAsSharedFixer
    { 

    }
}
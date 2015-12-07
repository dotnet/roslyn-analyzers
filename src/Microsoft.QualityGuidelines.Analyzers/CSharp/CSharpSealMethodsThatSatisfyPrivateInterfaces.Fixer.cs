// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
                                 
using System.Composition;
using System.Diagnostics;  
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.QualityGuidelines.Analyzers
{                                 
    /// <summary>
    /// CA2119: Seal methods that satisfy private interfaces
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpSealMethodsThatSatisfyPrivateInterfacesFixer : SealMethodsThatSatisfyPrivateInterfacesFixer
    { 
        
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
                                 
using System.Composition;
using System.Diagnostics;  
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Desktop.Analyzers
{                                 
    /// <summary>
    /// CA2239: Provide deserialization methods for optional fields
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpProvideDeserializationMethodsForOptionalFieldsFixer : ProvideDeserializationMethodsForOptionalFieldsFixer
    { 

    }
}
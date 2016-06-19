// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Desktop.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Desktop.CSharp.Analyzers
{
    /// <summary>
    /// CA2236: Call base class methods on ISerializable types
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpCallBaseClassMethodsOnISerializableTypesFixer : CallBaseClassMethodsOnISerializableTypesFixer
    {
    }
}
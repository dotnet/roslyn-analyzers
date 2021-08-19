// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.NetFramework.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetFramework.CSharp.Analyzers
{
    /// <summary>
    /// CA1300: Specify MessageBoxOptions
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpSpecifyMessageBoxOptionsFixer : SpecifyMessageBoxOptionsFixer
    {
    }
}
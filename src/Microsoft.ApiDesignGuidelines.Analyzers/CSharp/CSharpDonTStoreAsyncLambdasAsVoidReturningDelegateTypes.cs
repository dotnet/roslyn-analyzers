// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// Async004: Don't Store Async Lambdas as Void Returning Delegate Types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDonTStoreAsyncLambdasAsVoidReturningDelegateTypesAnalyzer : DonTStoreAsyncLambdasAsVoidReturningDelegateTypesAnalyzer
    {
    }
}
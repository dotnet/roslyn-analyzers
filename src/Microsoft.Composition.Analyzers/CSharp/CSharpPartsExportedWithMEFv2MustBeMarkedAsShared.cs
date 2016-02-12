// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0023: Parts exported with MEFv2 must be marked as Shared
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpPartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer : PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer
    {
    }
}
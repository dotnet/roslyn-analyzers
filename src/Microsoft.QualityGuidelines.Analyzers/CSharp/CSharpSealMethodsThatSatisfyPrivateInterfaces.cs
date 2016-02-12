// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA2119: Seal methods that satisfy private interfaces
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpSealMethodsThatSatisfyPrivateInterfacesAnalyzer : SealMethodsThatSatisfyPrivateInterfacesAnalyzer
    {
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    /// <summary>
    /// CA1510: Use cross-platform intrinsics
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseCrossPlatformIntrinsicsAnalyzer : UseCrossPlatformIntrinsicsAnalyzer
    {
    }
}

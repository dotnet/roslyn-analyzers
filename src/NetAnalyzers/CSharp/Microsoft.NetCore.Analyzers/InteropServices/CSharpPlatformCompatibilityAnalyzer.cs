// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.NetCore.Analyzers.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.NetCore.CSharp.Analyzers.InteropServices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpPlatformCompatibilityAnalyzer : PlatformCompatibilityAnalyzer
    {
        protected override bool IsSingleLineComment(SyntaxTrivia trivia)
            => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia);
    }
}

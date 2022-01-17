// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpEnumValuesShouldBeMonotonicallyIncreasing : EnumValuesShouldBeMonotonicallyIncreasing
    {
        private protected sealed override bool ShouldSkipSyntax(SyntaxNode syntax)
        {
            if (syntax is not EnumMemberDeclarationSyntax enumMember)
            {
                return true;
            }

            if (enumMember.EqualsValue is null)
            {
                return false;
            }

            if (enumMember.EqualsValue.Value.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return false;
            }

            return true;
        }
    }
}

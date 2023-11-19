// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Usage;

namespace Microsoft.NetCore.CSharp.Analyzers.Usage
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    internal sealed class CSharpUseVolatileReadWriteFixer : UseVolatileReadWriteFixer
    {
        protected override bool TryGetThreadVolatileReadWriteMemberAccess(SyntaxNode invocation, string methodName, [NotNullWhen(true)] out SyntaxNode? memberAccess)
        {
            memberAccess = null;
            if (invocation is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax m } && m.Name.Identifier.Text == methodName)
            {
                memberAccess = m;

                return true;
            }

            return false;
        }
    }
}
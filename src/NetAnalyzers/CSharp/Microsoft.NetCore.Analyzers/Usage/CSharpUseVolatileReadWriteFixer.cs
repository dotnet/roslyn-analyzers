// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Usage;

namespace Microsoft.NetCore.CSharp.Analyzers.Usage
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    internal sealed class CSharpUseVolatileReadWriteFixer : UseVolatileReadWriteFixer
    {
        protected override bool TryGetThreadVolatileReadWriteArguments(SyntaxNode invocation, string methodName, [NotNullWhen(true)] out IEnumerable<SyntaxNode>? arguments)
        {
            arguments = null;
            if (invocation is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax m } i && m.Name.Identifier.Text == methodName)
            {
                arguments = i.ArgumentList.Arguments.Select(a => a.WithNameColon(null));

                return true;
            }

            return false;
        }
    }
}
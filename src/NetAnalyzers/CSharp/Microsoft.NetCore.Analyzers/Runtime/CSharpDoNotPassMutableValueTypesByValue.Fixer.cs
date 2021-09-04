// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.NetCore.Analyzers.Runtime;
using System.Collections.Generic;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpDoNotPassMutableValueTypesByValueFixer : DoNotPassMutableValueTypesByValueFixer
    {
        private protected override SyntaxNode ConvertToByRefParameter(SyntaxNode parameterNode)
        {
            var cast = (ParameterSyntax)parameterNode;
            var refModifierToken = SyntaxFactory.Token(SyntaxKind.RefKeyword);
            SyntaxToken? inModifierToken = cast.Modifiers.Select(x => (SyntaxToken?)x).FirstOrDefault(x => x!.Value.IsKind(SyntaxKind.InKeyword));
            var newModifiers = (inModifierToken.HasValue ? cast.Modifiers.Remove(inModifierToken.Value) : cast.Modifiers).Add(refModifierToken);

            return cast.WithModifiers(newModifiers);
        }

        private protected override SyntaxNode ConvertToByRefArgument(SyntaxNode argumentNode)
        {
            var cast = (ArgumentSyntax)argumentNode;
            var refKindKeywordToken = SyntaxFactory.Token(SyntaxKind.RefKeyword);

            var result = cast.WithRefKindKeyword(refKindKeywordToken);
            return result;
        }

        private protected override IEnumerable<SyntaxNode> GetArgumentNodes(SyntaxNode root)
        {
            return root.DescendantNodes(x => true)
                .Where(node => node is ArgumentSyntax);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    /// <summary>
    /// CA1801: Review unused parameters
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpReviewUnusedParametersFixer : ReviewUnusedParametersFixer
    {
        public CSharpReviewUnusedParametersFixer(): base(new CSharpNodesProvider()) { }

        private sealed class CSharpNodesProvider : NodesProvider
        {
            public override SyntaxNode GetParameterNodeToRemove(DocumentEditor editor, SyntaxNode node, string name)
            {
                var arguments = ((ObjectCreationExpressionSyntax)node.Parent).ArgumentList.Arguments;
                foreach(var argument in arguments)
                {
                    if (argument.NameColon.Name.ToString() == name)
                    {
                        return node;
                    }
                }

                throw new System.ArgumentException(name);
            }

            public override void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove)
            {
                throw new System.NotImplementedException();
            }

            public override void RemoveNode(DocumentEditor editor, SyntaxNode node)
            {
                editor.RemoveNode(node);
            }
        }
    }
}
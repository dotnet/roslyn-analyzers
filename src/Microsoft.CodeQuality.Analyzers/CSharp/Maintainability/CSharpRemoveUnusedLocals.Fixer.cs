// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    /// <summary>
    /// CA1804: Remove unused locals
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpRemoveUnusedLocalsFixer : RemoveUnusedLocalsFixer
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("CS0168", "CS0219", "CS8321");

        protected override SyntaxNode GetAssignmentStatement(SyntaxNode node)
        {
            node = node.Parent;
            if (node.Kind() == SyntaxKind.SimpleAssignmentExpression)
            {
                return node.Parent;
            }

            return null;
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return new CSharpRemoveLocalFixAllProvider();
        }

        private class CSharpNodesProvider : NodesProvider
        {
            protected override SyntaxNode GetAssignmentStatement(SyntaxNode node)
            {
                node = node.Parent;
                if (node.Kind() == SyntaxKind.SimpleAssignmentExpression)
                {
                    return node.Parent;
                }

                return null;
            }
        }

        private class CSharpRemoveLocalFixAllProvider : FixAllProvider
        {
            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnostics = await GetDiagnostics(fixAllContext);
                return new CSharpRemoveLocalFixAllAction(fixAllContext.Solution, diagnostics);
            }
        }

        internal class CSharpRemoveLocalFixAllAction : RemoveLocalFixAllAction
        {
            public CSharpRemoveLocalFixAllAction(Solution solution, List<KeyValuePair<Document, ImmutableArray<Diagnostic>>> diagnosticsToFix): base(solution, diagnosticsToFix) { }

            protected override void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove)
            {
                var candidateLocalDeclarationsToRemove = new HashSet<LocalDeclarationStatementSyntax>();
                foreach (var variableDeclarator in nodesToRemove.OfType<VariableDeclaratorSyntax>())
                {
                    var localDeclaration = (LocalDeclarationStatementSyntax)variableDeclarator.Parent.Parent;
                    candidateLocalDeclarationsToRemove.Add(localDeclaration);
                }

                foreach (var candidate in candidateLocalDeclarationsToRemove)
                {
                    var hasUsedLocal = false;
                    foreach (var variable in candidate.Declaration.Variables)
                    {
                        if (!nodesToRemove.Contains(variable))
                        {
                            hasUsedLocal = true;
                            break;
                        }
                    }

                    if (!hasUsedLocal)
                    {
                        nodesToRemove.Add(candidate);
                        foreach (var variable in candidate.Declaration.Variables)
                        {
                            nodesToRemove.Remove(variable);
                        }
                    }
                }
            }

            internal override NodesProvider GetNodesProvider()
            {
                // TODO at least do not return new each time. return a static instance if not able to refactor
                return new CSharpNodesProvider();
            }
        }
    }
}
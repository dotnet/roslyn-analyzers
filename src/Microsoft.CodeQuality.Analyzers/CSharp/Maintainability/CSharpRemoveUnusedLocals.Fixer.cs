// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
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
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return new CSharpRemoveLocalFixAllProvider();
        }

        private class CSharpRemoveLocalFixAllProvider : FixAllProvider
        {
            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnostics = new List<KeyValuePair<Document, ImmutableArray<Diagnostic>>>();
                foreach(var document in fixAllContext.Project.Documents)
                {
                    diagnostics.Add(new KeyValuePair<Document, ImmutableArray<Diagnostic>>(document, await fixAllContext.GetDocumentDiagnosticsAsync(document)));
                }
                    
                // TODO change/review title
                return new CSharpRemoveLocalFixAllAction("CSharpRemoveLocalFixAllAction", fixAllContext.Solution, diagnostics);
            }
        }

        internal class CSharpRemoveLocalFixAllAction : RemoveLocalFixAllAction
        {
            public CSharpRemoveLocalFixAllAction(string title, Solution solution, List<KeyValuePair<Document, ImmutableArray<Diagnostic>>> diagnosticsToFix): base(title, solution, diagnosticsToFix) { }

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

            protected override async Task<SyntaxNode> GetNodeToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                // Find the variable declarator identified by the diagnostic.
                var variableDeclarator = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                if (variableDeclarator == null)
                {
                    return null;
                }

                // Bail out if the initializer is non-constant (could have side effects if removed).
                if (variableDeclarator.Initializer != null)
                {
                    var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                    if (!semanticModel.GetConstantValue(variableDeclarator.Initializer.Value).HasValue)
                    {
                        return null;
                    }
                }

                // Bail out for code with syntax errors - parent of a declaration is not a local declaration statement.
                var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
                var localDeclaration = variableDeclaration?.Parent as LocalDeclarationStatementSyntax;
                if (localDeclaration == null)
                {
                    return null;
                }

                // If the statement declares a single variable, the code fix should remove the whole statement.
                // Otherwise, the code fix should remove only this variable declaration.
                SyntaxNode nodeToRemove;
                if (variableDeclaration.Variables.Count == 1)
                {
                    if (!(localDeclaration.Parent is BlockSyntax))
                    {
                        // Bail out for error case where local declaration is not embedded in a block.
                        // Compiler generates errors CS1023 (Embedded statement cannot be a declaration or labeled statement)
                        return null;
                    }

                    nodeToRemove = localDeclaration;
                }
                else
                {
                    nodeToRemove = variableDeclarator;
                }

                return nodeToRemove;
            }
        }
    }
}
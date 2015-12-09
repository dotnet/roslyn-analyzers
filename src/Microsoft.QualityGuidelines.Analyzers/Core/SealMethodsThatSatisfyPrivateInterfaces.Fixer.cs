// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;     
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.QualityGuidelines.Analyzers
{                              
    /// <summary>
    /// CA2119: Seal methods that satisfy private interfaces
    /// </summary>
    public abstract class SealMethodsThatSatisfyPrivateInterfacesFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SealMethodsThatSatisfyPrivateInterfacesAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var gen = SyntaxGenerator.GetGenerator(context.Document);

            foreach (var dx in context.Diagnostics)
            {
                if (dx.Location.IsInSource)
                {
                    var root = dx.Location.SourceTree.GetRoot(context.CancellationToken);
                    var declarationNode = gen.GetDeclaration(root.FindToken(dx.Location.SourceSpan.Start).Parent);
                    if (declarationNode != null)
                    {
                        var symbol = model.GetDeclaredSymbol(declarationNode, context.CancellationToken) as IMethodSymbol;
                        if (symbol != null && symbol.IsOverride)
                        {
                            context.RegisterCodeFix(CodeAction.Create("Seal overridden method", c => SealOverriddenMethodAsync(context.Document, root, declarationNode, c)), dx);
                        }
                    }
                }
            }
        }

        private static Task<Document> SealOverriddenMethodAsync(Document document, SyntaxNode root, SyntaxNode declarationNode, CancellationToken cancellationToken)
        {
            var editor = new SyntaxEditor(root, document.Project.Solution.Workspace);
            editor.SetModifiers(declarationNode, editor.Generator.GetModifiers(declarationNode) + DeclarationModifiers.Sealed);
            return Task.FromResult(document.WithSyntaxRoot(editor.GetChangedRoot()));
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;     
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;

// disable equivalence key warning because it cannot tell that equivalence key is overridden in the base class
#pragma warning disable RS1011

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA2119: Seal methods that satisfy private interfaces
    /// </summary>    
    [ExportCodeFixProvider(LanguageNames.CSharp /*, LanguageNames.VisualBasic*/), Shared]  // note: disabled VB until SyntaxGenerator.WithStatements works
    public sealed class SealMethodsThatSatisfyPrivateInterfacesFixer : CodeFixProvider
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
                        var solution = context.Document.Project.Solution;
                        var symbol = model.GetDeclaredSymbol(declarationNode, context.CancellationToken);

                        if (symbol != null)
                        {
                            if (!(symbol is INamedTypeSymbol))
                            {
                                if (symbol.IsOverride)
                                {
                                    context.RegisterCodeFix(new ChangeModifierAction("Make member not overridable.", "MakeMemberNotOverridable", solution, symbol, DeclarationModifiers.From(symbol) + DeclarationModifiers.Sealed), dx);
                                }
                                else if (symbol.IsVirtual)
                                {
                                    context.RegisterCodeFix(new ChangeModifierAction("Make member not overridable.", "MakeMemberNotOverridable", solution, symbol, DeclarationModifiers.From(symbol) - DeclarationModifiers.Virtual), dx);
                                }
                                else if (symbol.IsAbstract)
                                {
                                    context.RegisterCodeFix(new ChangeModifierAction("Make member not overridable.", "MakeMemberNotOverridable", solution, symbol, DeclarationModifiers.From(symbol) - DeclarationModifiers.Abstract), dx);
                                }

                                // trigger containing type code fixes below
                                symbol = symbol.ContainingType;
                            }

                            // if the diagnostic identified a type then it is the containing type of the member
                            var type = symbol as INamedTypeSymbol;
                            if (type != null)
                            {
                                // cannot make abstract type sealed because they cannot be constructed
                                if (!type.IsAbstract)
                                {
                                    context.RegisterCodeFix(new ChangeModifierAction("Make declaring type sealed.", "MakeDeclaringTypeSealed", solution, type, DeclarationModifiers.From(type) + DeclarationModifiers.Sealed), dx);
                                }

                                context.RegisterCodeFix(new ChangeAccessibilityAction("Make declaring type internal.", "MakeDeclaringTypeInternal", solution, type, Accessibility.Internal), dx);
                            }
                        }
                    }
                }
            }
        }

        private abstract class ChangeSymbolAction :  CodeAction
        {
            private readonly string _title;
            private readonly string _equalenceKey;
            private readonly Solution _solution;
            private readonly ISymbol _symbol;

            public ChangeSymbolAction(string title, string equivalenceKey, Solution solution, ISymbol symbol)
            {
                _title = title;
                _equalenceKey = equivalenceKey;
                _solution = solution;
                _symbol = symbol;
            }

            public override string Title
            {
                get { return _title; }
            }

            public override string EquivalenceKey
            {
                get { return _equalenceKey; }
            }

            public Solution Solution
            {
                get { return _solution; }
            }

            public ISymbol Symbol
            {
                get { return _symbol; }
            }
        }

        private class ChangeModifierAction : ChangeSymbolAction
        {
            private readonly DeclarationModifiers _newModifiers;

            public ChangeModifierAction(string title, string equivalenceKey, Solution solution, ISymbol symbol, DeclarationModifiers newModifiers)
                : base(title, equivalenceKey, solution, symbol)
            {
                _newModifiers = newModifiers;
            }

            protected async override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var editor = SymbolEditor.Create(this.Solution);
                await editor.EditAllDeclarationsAsync(this.Symbol, (e, d) =>
                {
                    e.SetModifiers(d, _newModifiers);

                    if (this.Symbol.IsAbstract && !_newModifiers.IsAbstract && this.Symbol.Kind == SymbolKind.Method)
                    {
                        e.ReplaceNode(d, (_d, g) => g.WithStatements(_d, new SyntaxNode[] { }));
                    }
                }
                , cancellationToken);
                return editor.ChangedSolution;
            }
        }

        private class ChangeAccessibilityAction : ChangeSymbolAction
        {
            private Accessibility _newAccessibility;

            public ChangeAccessibilityAction(string title, string equivalenceKey, Solution solution, ISymbol symbol, Accessibility newAccessibilty)
                : base(title, equivalenceKey, solution, symbol)
            {
                _newAccessibility = newAccessibilty;
            }

            protected async override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var editor = SymbolEditor.Create(this.Solution);
                await editor.EditAllDeclarationsAsync(this.Symbol, (e, d) => e.SetAccessibility(d, _newAccessibility), cancellationToken);
                return editor.ChangedSolution;
            }
        }
    }
}
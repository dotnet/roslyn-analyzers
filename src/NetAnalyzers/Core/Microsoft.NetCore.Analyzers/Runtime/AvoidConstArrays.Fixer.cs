// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1839: Avoid const arrays. Replace with static readonly arrays.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidConstArraysFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidConstArraysAnalyzer.RuleId);

        private static readonly string[] collectionMemberEndings = new[] { "array", "collection", "enumerable", "list" };

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);
            SemanticModel model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, context.CancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            string title = MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle;

            context.RegisterCodeFix(
                new MyCodeAction(
                    title,
                    async c => await ExtractConstArrayAsync(root, node, model, editor, generator, context.Diagnostics.First(), c).ConfigureAwait(false),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static async Task<Document> ExtractConstArrayAsync(SyntaxNode root, SyntaxNode node, SemanticModel model,
            DocumentEditor editor, SyntaxGenerator generator, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            IOperation nodeOperation = model.GetOperation(node, cancellationToken);
            INamedTypeSymbol containingType = model.GetEnclosingSymbol(node.SpanStart, cancellationToken).ContainingType;
            IEnumerable<ISymbol> typeMemberSymbols = containingType.GetMembers();
            IEnumerable<ISymbol> typeMemberFields = typeMemberSymbols.Where(x => x is IFieldSymbol);

            // Get method containing the symbol that is being diagnosed. Should always be in a method
            IMethodBodyOperation? containingMethod = nodeOperation.GetFirstAncestorOfType<IMethodBodyOperation>();
            ISymbol containingMethodSymbol = typeMemberSymbols.First(x => x.DeclaringSyntaxReferences.First().GetSyntax(cancellationToken) == containingMethod!.Syntax);
            Accessibility newMemberAccessibility = containingMethodSymbol.DeclaredAccessibility;
            // NOTE: need to get minimum viable accessibility

            // Get a valid member name for the extracted constant
            IEnumerable<string> memberNames = typeMemberFields.Select(x => x.Name);
            string newMemberName = GetExtractedMemberName(memberNames, diagnostic.Properties["matchingParameter"]);

            // Create the new member
            SyntaxNode newMember = generator.WithName(node, newMemberName);
            newMember = generator.WithModifiers(newMember, DeclarationModifiers.Static | DeclarationModifiers.ReadOnly);
            newMember = generator.WithAccessibility(newMember, newMemberAccessibility); // same as the method accessibility

            // Add the new member to the end of the class fields
            ISymbol lastFieldSymbol = typeMemberFields.LastOrDefault();
            if (lastFieldSymbol != null)
            {
                // Insert after last field if any fields are present
                SyntaxNode lastFieldNode = await lastFieldSymbol.DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                editor.InsertAfter(lastFieldNode, newMember);
            }
            else
            {
                // Insert before first member if no fields are present
                SyntaxNode firstMemberNode = (await containingType.DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken).ConfigureAwait(false))
                        .ChildNodes().First();
                editor.InsertBefore(firstMemberNode, newMember);
            }

            // Replace argument with a reference to our new member
            //editor.ReplaceNode(node, generator.Argument(newMember));

            // Replace root
            editor.ReplaceNode(node, editor.GetChangedRoot());

            // Return changed document
            return editor.GetChangedDocument();
        }

        // The called method's parameter names won't need to be checked in the case that both
        // methods are in the same type as conflicts in paramter names and field names can be
        // resolved by directly referencing the static readonly field everywhere it's needed
        private static string GetExtractedMemberName(IEnumerable<string> memberNames, string parameterName)
        {
            string nameOption = parameterName;
            bool hasCollectionEnding = collectionMemberEndings.Any(x => nameOption.EndsWith(x, true, System.Globalization.CultureInfo.InvariantCulture));

            if (memberNames.Contains(nameOption) && !hasCollectionEnding)
            {
                nameOption += "Array";
            }

            int suffix = 1;
            while (memberNames.Contains(nameOption))
            {
                nameOption += suffix;
                suffix++;
            }

            return nameOption;
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private sealed class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey) :
                base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }

    public static class IOperationExtensions
    {
        public static T? GetFirstAncestorOfType<T>(this IOperation operation, OperationKind kind) where T : IOperation
        {
            while (operation != null)
            {
                if (operation.Kind == kind)
                {
                    return (T)operation;
                }
                operation = operation.Parent;
            }
            return default;
        }

        public static T? GetFirstAncestorOfType<T>(this IOperation operation) where T : IOperation
        {
            while (operation != null)
            {
                if (operation is T outOperation)
                {
                    return outOperation;
                }
                operation = operation.Parent;
            }
            return default;
        }
    }
}
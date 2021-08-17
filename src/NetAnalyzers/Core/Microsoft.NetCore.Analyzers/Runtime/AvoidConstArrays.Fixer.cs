// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
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

        private static readonly ImmutableArray<string> s_collectionMemberEndings = ImmutableArray.Create("array", "collection", "enumerable", "list");

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
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
                    async c => await ExtractConstArrayAsync(node, model, editor, generator, context.Diagnostics.First().Properties, c).ConfigureAwait(false),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static async Task<Document> ExtractConstArrayAsync(SyntaxNode node, SemanticModel model, DocumentEditor editor,
            SyntaxGenerator generator, ImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            IArrayCreationOperation arrayArgument = GetArrayCreationOperation(node, model, cancellationToken, out bool isInvoked);
            IEnumerable<ISymbol> typeFields = model.GetEnclosingSymbol(node.SpanStart, cancellationToken).ContainingType
                .GetMembers().Where(x => x is IFieldSymbol);

            // Get a valid member name for the extracted constant
            string newMemberName = GetExtractedMemberName(typeFields.Select(x => x.Name), properties["paramName"]);

            // Get method containing the symbol that is being diagnosed. Should always be in a method
            IOperation? containingMethodBody = arrayArgument.GetAncestor<IMethodBodyOperation>(OperationKind.MethodBody);
            if (containingMethodBody is null)
            {
                // This is due to VB methods having a different structure than CS methods
                containingMethodBody = arrayArgument.GetAncestor<IBlockOperation>(OperationKind.Block);
            }

            // Create the new member
            SyntaxNode newMember = generator.FieldDeclaration(
                newMemberName,
                generator.TypeExpression(arrayArgument.Type),
                GetAccessibility(model.GetEnclosingSymbol(containingMethodBody!.Syntax.SpanStart, cancellationToken)),
                DeclarationModifiers.Static | DeclarationModifiers.ReadOnly,
                arrayArgument.Syntax
            ).FormatForExtraction(containingMethodBody.Syntax);

            // Add the new extracted member
            ISymbol lastFieldSymbol = typeFields.LastOrDefault();
            if (lastFieldSymbol is not null)
            {
                // Insert after last field if any fields are present
                SyntaxNode lastFieldNode = await lastFieldSymbol.DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                editor.InsertAfter(lastFieldNode, newMember);
            }
            else
            {
                // Insert before first method if no fields are present, as a method already exists
                editor.InsertBefore(containingMethodBody.Syntax, newMember);
            }

            // Replace argument with a reference to our new member
            SyntaxNode identifier = generator.IdentifierName(newMemberName);
            if (isInvoked)
            {
                editor.ReplaceNode(node, generator.WithExpression(identifier, node));
            }
            else
            {
                editor.ReplaceNode(node, generator.Argument(identifier));
            }

            // Return changed document
            return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static IArrayCreationOperation GetArrayCreationOperation(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken,
                out bool isInvoked)
        {
            // If this is a LINQ invocation, the node is already an IArrayCreationOperation
            if (model.GetOperation(node, cancellationToken) is IArrayCreationOperation arrayCreation)
            {
                isInvoked = true;
                return arrayCreation;
            }
            isInvoked = false;
            return (IArrayCreationOperation)model.GetOperation(node.ChildNodes().First(), cancellationToken);
        }

        private static string GetExtractedMemberName(IEnumerable<string> memberNames, string parameterName)
        {
            bool hasCollectionEnding = s_collectionMemberEndings.Any(x => parameterName.EndsWith(x, true, null));

            if (parameterName == "source" // for LINQ, "sourceArray" is clearer than "source"
                || (memberNames.Contains(parameterName) && !hasCollectionEnding))
            {
                parameterName += "Array";
            }

            if (memberNames.Contains(parameterName))
            {
                int suffix = 0;
                while (memberNames.Contains(parameterName + suffix))
                {
                    suffix++;
                }
                return parameterName + suffix;
            }

            return parameterName;
        }

        private static Accessibility GetAccessibility(ISymbol originMethodSymbol)
        {
            if (Enum.TryParse(originMethodSymbol.GetResultantVisibility().ToString(), out Accessibility accessibility))
            {
                return accessibility == Accessibility.Public
                    ? Accessibility.Private // public accessibility not wanted for fields
                    : accessibility;
            }
            return Accessibility.Private;
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

    internal static class SyntaxNodeExtensions
    {
        internal static SyntaxNode FormatForExtraction(this SyntaxNode node, SyntaxNode previouslyContainingNode)
        {
            return node.HasTrailingTrivia ? node : node.WithTrailingTrivia(previouslyContainingNode.GetTrailingTrivia());
        }
    }
}
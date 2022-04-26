// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1853: Avoid constant arrays as arguments. Replace with static readonly arrays.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidConstArraysFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AvoidConstArraysAnalyzer.RuleId);

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

            string title = MicrosoftNetCoreAnalyzersResources.AvoidConstArraysCodeFixTitle;
            context.RegisterCodeFix(CodeAction.Create(
                    title,
                    async c => await ExtractConstArrayAsync(node, model, editor, generator, context.Diagnostics.First().Properties, c).ConfigureAwait(false),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static async Task<Document> ExtractConstArrayAsync(SyntaxNode node, SemanticModel model, DocumentEditor editor,
            SyntaxGenerator generator, ImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            IArrayCreationOperation arrayArgument = GetArrayCreationOperation(node, model, cancellationToken, out bool isInvoked);
            IEnumerable<ISymbol> members = model.GetEnclosingSymbol(node.SpanStart, cancellationToken).ContainingType.GetMembers();

            // Get a valid member name for the extracted constant
            string newMemberName = GetExtractedMemberName(
                members.Where(x => x is IFieldSymbol).Select(x => x.Name),
                properties["paramName"] ?? GetMemberNameFromType(arrayArgument));

            // Get method containing the symbol that is being diagnosed.
            IOperation? methodContext = arrayArgument.GetAncestor<IMethodBodyOperation>(OperationKind.MethodBody);
            methodContext ??= arrayArgument.GetAncestor<IBlockOperation>(OperationKind.Block); // VB methods have a different structure than CS methods

            // Create the new member
            SyntaxNode newMember = generator.FieldDeclaration(
                newMemberName,
                generator.TypeExpression(arrayArgument.Type),
                GetAccessibility(methodContext is null ? null : model.GetEnclosingSymbol(methodContext.Syntax.SpanStart, cancellationToken)),
                DeclarationModifiers.Static | DeclarationModifiers.ReadOnly,
                arrayArgument.Syntax.WithoutTrailingTrivia() // don't include extra trivia before the end of the declaration
            );

            // Add any additional formatting
            if (methodContext is not null)
            {
                newMember = newMember.FormatForExtraction(methodContext.Syntax);
            }

            // Insert the new extracted member before the first member
            SyntaxNode firstMemberNode = await members.First().DeclaringSyntaxReferences.First()
                .GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
            editor.InsertBefore(generator.GetDeclaration(firstMemberNode), newMember);

            // Replace argument with a reference to our new member
            SyntaxNode identifier = generator.IdentifierName(newMemberName);
            if (isInvoked)
            {
                editor.ReplaceNode(node, generator.WithExpression(identifier, node));
            }
            else
            {
                editor.ReplaceNode(node, generator.Argument(identifier)
                    .WithTrailingTrivia(arrayArgument.Syntax.GetTrailingTrivia())); // add any extra trivia that was after the original argument
            }

            // Return changed document
            return editor.GetChangedDocument();
        }

        private static IArrayCreationOperation GetArrayCreationOperation(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken,
                out bool isInvoked)
        {
            // The analyzer only passes a diagnostic for two scenarios, each having an IArrayCreationOperation:
            //      1. The node is an IArgumentOperation that is a direct parent of an IArrayCreationOperation
            //      2. The node is an IArrayCreationOperation already, as it was pulled from an
            //         invocation, like with LINQ extension methods

            // If this is a LINQ invocation, the node is already an IArrayCreationOperation
            if (model.GetOperation(node, cancellationToken) is IArrayCreationOperation arrayCreation)
            {
                isInvoked = true;
                return arrayCreation;
            }
            // Otherwise, we'll get the IArrayCreationOperation from the argument node's child
            isInvoked = false;
            return (IArrayCreationOperation)model.GetOperation(node.ChildNodes().First(), cancellationToken);
        }

        private static string GetExtractedMemberName(IEnumerable<string> memberNames, string parameterName)
        {
            bool hasCollectionEnding = s_collectionMemberEndings.Any(x => parameterName.EndsWith(x, true, CultureInfo.InvariantCulture));

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

        private static string GetMemberNameFromType(IArrayCreationOperation arrayCreationOperation)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return ((IArrayTypeSymbol)arrayCreationOperation.Type).ElementType.OriginalDefinition.Name.ToLowerInvariant() + "Array";
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        private static Accessibility GetAccessibility(ISymbol? methodSymbol)
        {
            if (methodSymbol is not null && Enum.TryParse(methodSymbol.GetResultantVisibility().ToString(), out Accessibility accessibility))
            {
                return accessibility == Accessibility.Public
                    ? Accessibility.Private // public accessibility not wanted for fields
                    : accessibility;
            }
            return Accessibility.Private;
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
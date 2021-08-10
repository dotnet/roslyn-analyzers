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

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            string title = MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle;
            context.RegisterCodeFix(
                new MyCodeAction(
                    title,
                    async c => await ExtractConstArrayAsync(context.Document, context.Diagnostics.First(), c).ConfigureAwait(false),
                    equivalenceKey: title),
                context.Diagnostics);
            await Task.Run(() => { }).ConfigureAwait(false);
        }

        private static async Task<Document> ExtractConstArrayAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);

            // Get method containing the symbol that is being diagnosed. Should always be in a method
            IMethodSymbol containingMethod = model.GetSymbolInfo(root, cancellationToken).Symbol.GetContainingMethod()!;

            // Get a valid member name for the extracted constant
            IEnumerable<string> memberNames = model.GetTypeInfo(root, cancellationToken).Type.GetMembers().Where(x => x is IFieldSymbol).Select(x => x.Name);
            if (!diagnostic.Properties.TryGetValue("matchingParamater", out string matchingParamater))
            {
                matchingParamater = ((IArgumentOperation)model.GetOperation(root)).Parameter.Name;
            }
            string newMemberName = GetExtractedMemberName(memberNames, matchingParamater);

            // Create the new member
            SyntaxNode newMember = generator.WithName(root, newMemberName);
            newMember = generator.WithModifiers(newMember, DeclarationModifiers.Static | DeclarationModifiers.ReadOnly);
            newMember = generator.WithAccessibility(newMember, containingMethod.DeclaredAccessibility); // same as the method accessibility

            // Add the new member to the end of the class fields
            SyntaxNode lastFieldSyntaxNode = generator.GetMembers(root).Last(x => model.GetSymbolInfo(x).Symbol is IFieldSymbol);
            editor.AddMember(lastFieldSyntaxNode, newMember);

            // Replace argument with a reference to our new member
            editor.ReplaceNode(root, generator.Argument(newMember));

            // Return changed document
            return editor.GetChangedDocument();
        }

        // The called method's parameter names won't need to be checked in the case that both
        // methods are in the same type as conflicts in paramter names and field names can be
        // resolved by directly referencing the static readonly field everywhere it's needed
        private static string GetExtractedMemberName(IEnumerable<string> memberNames, string parameterName)
        {
            // Half-shot attempt at getting a unique field name
            string nameOption = parameterName;

            if (memberNames.Contains(nameOption))
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

    public static class ISymbolExtensions
    {
        public static IMethodSymbol? GetContainingMethod(this ISymbol symbol)
        {
            ISymbol current = symbol;
            while (current != null)
            {
                if (current is IMethodSymbol method)
                {
                    return method;
                }
                current = current.ContainingSymbol;
            }
            return default;
        }
    }
}
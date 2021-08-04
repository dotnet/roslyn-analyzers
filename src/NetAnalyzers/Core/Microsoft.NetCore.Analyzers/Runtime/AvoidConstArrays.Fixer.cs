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

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidConstArraysFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidConstArraysAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            await Task.Run(() =>
            {
                string title = MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle;
                context.RegisterCodeFix(
                    new MyCodeAction(
                        title,
                        c => ExtractConstArrayAsync(context.Document, context.Diagnostics, c),
                        equivalenceKey: title),
                    context.Diagnostics);
            }).ConfigureAwait(false);
        }

        private static async Task<Document> ExtractConstArrayAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // The containing method accessibility and the called method accessibility
            // should both be considered when setting the new member's accessibility
            var memberGroup = model.GetMemberGroup(root, cancellationToken).FirstOrDefault();

            string newMemberName = GetExtractedMemberName(
                memberGroup.ContainingType.MemberNames,
                diagnostics.First().Properties["matchingParameter"]
            );

            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);

            SyntaxNode newMember = generator.WithName(root, newMemberName);
            newMember = generator.WithModifiers(newMember, DeclarationModifiers.Static | DeclarationModifiers.ReadOnly);
            newMember = generator.WithAccessibility(newMember, memberGroup.DeclaredAccessibility); // same as the method accessibility

            SyntaxNode lastFieldSyntaxNode = await generator.GetMembers(root).Where(x => x is IFieldSymbol)
                .Select(x => (IFieldSymbol)x).Last().DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
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
}
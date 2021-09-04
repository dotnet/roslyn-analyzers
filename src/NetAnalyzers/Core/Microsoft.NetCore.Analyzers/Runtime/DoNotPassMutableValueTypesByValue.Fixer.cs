// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class DoNotPassMutableValueTypesByValueFixer : CodeFixProvider
    {
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var codeAction = CodeAction.Create(
                Resx.DoNotPassMutableValueTypesByValueCodeFixTitle,
                ChangeParameterAndUpdateLValueCallsites,
                Resx.DoNotPassMutableValueTypesByValueCodeFixTitle);
            context.RegisterCodeFix(codeAction, diagnostic);

            return Task.CompletedTask;

            //  Local functions

            async Task<Solution> ChangeParameterAndUpdateLValueCallsites(CancellationToken token)
            {
                var root = await context.Document.GetSyntaxRootAsync(token).ConfigureAwait(false);
                var model = await context.Document.GetSemanticModelAsync(token).ConfigureAwait(false);
                var parameterNode = root.FindNode(diagnostic!.Location.SourceSpan);
                var parameterSymbol = (IParameterSymbol)model.GetDeclaredSymbol(parameterNode, token);

                var solutionEditor = new SolutionEditor(context.Document.Project.Solution);
                var documentEditor = await solutionEditor.GetDocumentEditorAsync(context.Document.Id, token).ConfigureAwait(false);
                documentEditor.ReplaceNode(parameterNode, AddRefKeywordToParameter(parameterNode));

                await UpdateMatchingArgumentsAsync(solutionEditor, context.Document.Project, parameterSymbol, token).ConfigureAwait(false);

                return solutionEditor.GetChangedSolution();
            }
        }

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DoNotPassMutableValueTypesByValueAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private protected abstract SyntaxNode AddRefKeywordToParameter(SyntaxNode parameterNode);

        private protected abstract IEnumerable<SyntaxNode> GetArgumentNodes(SyntaxNode root);

        private protected abstract Task UpdateMatchingArgumentsAsync(SolutionEditor solutionEditor, Project originalProject, IParameterSymbol parameterSymbol, CancellationToken token);
    }
}

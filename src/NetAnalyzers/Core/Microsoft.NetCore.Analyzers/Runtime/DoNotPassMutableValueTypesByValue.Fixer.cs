// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CodeActions;
using System.Linq;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;

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
                var parameterNode = (await context.Document.GetSyntaxRootAsync(token).ConfigureAwait(false)).FindNode(diagnostic!.Location.SourceSpan);
                var parameterSymbol = (await context.Document.GetSemanticModelAsync(token).ConfigureAwait(false)).GetDeclaredSymbol(parameterNode, token);

                var referencingDocuments = context.Document.Project.Solution.Projects
                    .Where(project => project.Id == context.Document.Project.Id || project.ProjectReferences.Select(x => x.ProjectId).Contains(context.Document.Project.Id))
                    .SelectMany(x => x.Documents);
                var solutionEditor = new SolutionEditor(context.Document.Project.Solution);
                var argumentOperations = new List<IArgumentOperation>();

                foreach (var document in referencingDocuments)
                {
                    var model = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
                    var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
                    argumentOperations.Clear();

                    foreach (var argumentNode in GetArgumentNodes(root))
                    {
                        if (model.GetOperation(argumentNode, token) is IArgumentOperation argumentOperation &&
                            argumentOperation.Parameter.Equals(parameterSymbol, SymbolEqualityComparer.Default) &&
                            IsLValue(argumentOperation))
                        {
                            argumentOperations.Add(argumentOperation);
                        }
                    }

                    if (argumentOperations.Count == 0)
                        continue;

                    var documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, token).ConfigureAwait(false);
                    foreach (var argumentOperation in argumentOperations)
                    {
                        documentEditor.ReplaceNode(argumentOperation.Syntax, ConvertToByRefArgument(argumentOperation.Syntax));
                    }
                }

                {
                    var documentEditor = await solutionEditor.GetDocumentEditorAsync(context.Document.Id, token).ConfigureAwait(false);
                    documentEditor.ReplaceNode(parameterNode, ConvertToByRefParameter(parameterNode));
                }

                return solutionEditor.GetChangedSolution();
            }

            static bool IsLValue(IArgumentOperation argumentOperation)
            {
                var value = argumentOperation.Value;

                return (value is ILocalReferenceOperation localRef && !localRef.Local.IsConst) ||
                    (value is IFieldReferenceOperation fieldRef && !fieldRef.Field.IsReadOnly && !fieldRef.Field.IsConst) ||
                    (value is IParameterReferenceOperation parameterRef && parameterRef.Parameter.RefKind is not RefKind.In) ||
                    (value is IArrayElementReferenceOperation) ||
                    (value is IInvocationOperation invocation && invocation.TargetMethod.ReturnsByRef) ||
                    (value is IPropertyReferenceOperation propertyRef && propertyRef.Property.ReturnsByRef);
            }
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotPassMutableValueTypesByValueAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private protected abstract SyntaxNode ConvertToByRefParameter(SyntaxNode parameterNode);

        private protected abstract SyntaxNode ConvertToByRefArgument(SyntaxNode argumentNode);

        private protected abstract IEnumerable<SyntaxNode> GetArgumentNodes(SyntaxNode root);
    }
}

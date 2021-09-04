// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.NetCore.Analyzers.Runtime;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpDoNotPassMutableValueTypesByValueFixer : DoNotPassMutableValueTypesByValueFixer
    {
        private protected override SyntaxNode AddRefKeywordToParameter(SyntaxNode parameterNode)
        {
            var cast = (ParameterSyntax)parameterNode;
            var refModifierToken = SyntaxFactory.Token(SyntaxKind.RefKeyword);
            SyntaxToken? inModifierToken = cast.Modifiers.Select(x => (SyntaxToken?)x).FirstOrDefault(x => x!.Value.IsKind(SyntaxKind.InKeyword));
            var newModifiers = (inModifierToken.HasValue ? cast.Modifiers.Remove(inModifierToken.Value) : cast.Modifiers).Add(refModifierToken);

            return cast.WithModifiers(newModifiers);
        }

        private protected override IEnumerable<SyntaxNode> GetArgumentNodes(SyntaxNode root)
        {
            return root.DescendantNodes(x => true)
                .Where(node => node is ArgumentSyntax);
        }

        private protected override async Task UpdateMatchingArgumentsAsync(SolutionEditor solutionEditor, Project originalProject, IParameterSymbol parameterSymbol, CancellationToken token)
        {
            //  Find all documents that can access parameterSymbol
            var referencingDocuments = solutionEditor.OriginalSolution.Projects
                .Where(project => project.Id == originalProject.Id || project.ProjectReferences.Select(x => x.ProjectId).Contains(originalProject.Id))
                .SelectMany(x => x.Documents);

            var argumentOperations = new List<IArgumentOperation>();

            foreach (var document in referencingDocuments)
            {
                var model = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
                var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

                foreach (var argumentNode in GetArgumentNodes(root))
                {
                    //  Select all lvalue arguments that match parameterSymbol
                    if (model.GetOperation(argumentNode, token) is IArgumentOperation argumentOperation &&
                        argumentOperation.Parameter.Equals(parameterSymbol, SymbolEqualityComparer.Default) &&
                        IsLValue(argumentOperation))
                    {
                        argumentOperations.Add(argumentOperation);
                    }
                }

                //  We find all matching arguments in the document before fixing so that we can
                //  skip creating the document editor when the document contains no matching arguments.
                if (argumentOperations.Count > 0)
                {
                    var documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, token).ConfigureAwait(false);
                    foreach (var argumentOperation in argumentOperations)
                    {
                        documentEditor.ReplaceNode(argumentOperation.Syntax, AddRefKeywordToArgument(argumentOperation.Syntax));
                    }

                    argumentOperations.Clear();
                }
            }

            return;

            //  Local functions

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

            static SyntaxNode AddRefKeywordToArgument(SyntaxNode argumentNode)
            {
                var cast = (ArgumentSyntax)argumentNode;
                var refKindKeywordToken = SyntaxFactory.Token(SyntaxKind.RefKeyword);

                var result = cast.WithRefKindKeyword(refKindKeywordToken);
                return result;
            }
        }
    }
}

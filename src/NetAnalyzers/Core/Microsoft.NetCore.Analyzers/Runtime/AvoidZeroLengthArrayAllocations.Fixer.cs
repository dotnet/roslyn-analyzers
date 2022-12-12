// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
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
    /// CA1825: Avoid zero-length array allocations.
    /// </summary>
    public abstract class AvoidZeroLengthArrayAllocationsFixer : CodeFixProvider
    {
        protected abstract T AddElasticMarker<T>(T syntaxNode) where T : SyntaxNode;

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AvoidZeroLengthArrayAllocationsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            // In case the ArrayCreationExpressionSyntax is wrapped in an ArgumentSyntax or some other node with the same span,
            // get the innermost node for ties.
            SyntaxNode nodeToFix = root.FindNode(context.Span, getInnermostNodeForTie: true);
            if (nodeToFix == null)
            {
                return;
            }

            string title = MicrosoftNetCoreAnalyzersResources.UseArrayEmpty;
            context.RegisterCodeFix(CodeAction.Create(title,
                                                     async ct => await ConvertToArrayEmptyAsync(context.Document, nodeToFix, ct).ConfigureAwait(false),
                                                     equivalenceKey: title),
                                    context.Diagnostics);
        }

        private async Task<Document> ConvertToArrayEmptyAsync(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            INamedTypeSymbol? arrayTypeSymbol = semanticModel.Compilation.GetSpecialType(SpecialType.System_Array);
            if (arrayTypeSymbol == null)
            {
                return document;
            }

            ITypeSymbol? elementType = GetArrayElementType(nodeToFix, semanticModel, cancellationToken, out var variableName);
            if (elementType == null)
            {
                return document;
            }

            SyntaxNode arrayEmptyInvocation = GenerateArrayEmptyInvocation(generator, arrayTypeSymbol, elementType).WithTriviaFrom(nodeToFix);

            if (variableName is null)
            {
                editor.ReplaceNode(nodeToFix, arrayEmptyInvocation);
            }
            else
            {
                editor.ReplaceNode(nodeToFix, AddElasticMarker(generator.LocalDeclarationStatement(variableName, arrayEmptyInvocation)));
            }

            return editor.GetChangedDocument();
        }

        private static ITypeSymbol? GetArrayElementType(SyntaxNode arrayCreationExpression, SemanticModel semanticModel, CancellationToken cancellationToken, out string? variableName)
        {
            var operation = semanticModel.GetOperation(arrayCreationExpression, cancellationToken);

            // If we have `string[] x = { }`, we get IArrayInitializerOperation whose parent is IArrayCreationOperation.
            if (operation.Kind == OperationKind.ArrayInitializer)
            {
                operation = operation.Parent;
            }

            if (operation is IArrayCreationOperation arrayCreation)
            {
                variableName = null;
                return arrayCreation.GetElementType();
            }
            else if (operation is IVariableDeclaratorOperation { Initializer.Value: IArrayCreationOperation arrayCreationInDeclarator, Symbol.Name: var symbolName })
            {
                variableName = symbolName;
                return arrayCreationInDeclarator.GetElementType();
            }

            variableName = null;
            return null;
        }

        private static SyntaxNode GenerateArrayEmptyInvocation(SyntaxGenerator generator, INamedTypeSymbol arrayTypeSymbol, ITypeSymbol elementType)
        {
            SyntaxNode arrayEmptyName = generator.MemberAccessExpression(
                generator.TypeExpressionForStaticMemberAccess(arrayTypeSymbol),
                generator.GenericName(AvoidZeroLengthArrayAllocationsAnalyzer.ArrayEmptyMethodName, elementType));
            return generator.InvocationExpression(arrayEmptyName);
        }
    }
}

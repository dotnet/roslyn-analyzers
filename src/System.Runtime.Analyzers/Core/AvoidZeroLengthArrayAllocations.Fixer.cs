// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1825: Avoid zero-length array allocations.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidZeroLengthArrayAllocationsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidZeroLengthArrayAllocationsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode nodeToFix = root.FindNode(context.Span);
            if (nodeToFix == null)
            {
                return;
            }

            context.RegisterCodeFix(new MyCodeAction(SystemRuntimeAnalyzersResources.UseArrayEmpty,
                                                     async ct => await ConvertToArrayEmpty(context.Document, nodeToFix, ct).ConfigureAwait(false)),
                                    context.Diagnostics);
        }

        private async Task<Document> ConvertToArrayEmpty(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            SyntaxNode variableDecl = generator.GetDeclaration(nodeToFix, DeclarationKind.Variable);
            SyntaxNode typeNode = generator.GetType(variableDecl);

            var type = semanticModel.GetTypeInfo(typeNode, cancellationToken).Type as IArrayTypeSymbol;
            INamedTypeSymbol arrayTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(AvoidZeroLengthArrayAllocationsAnalyzer.ArrayTypeName);

            SyntaxNode arrayEmptyName = generator.QualifiedName(generator.TypeExpression(arrayTypeSymbol),
                                                         generator.GenericName(AvoidZeroLengthArrayAllocationsAnalyzer.ArrayEmptyMethodName, type.ElementType));
            SyntaxNode arrayEmptyInvocation = generator.InvocationExpression(arrayEmptyName);
            arrayEmptyInvocation = arrayEmptyInvocation.WithLeadingTrivia(nodeToFix.GetLeadingTrivia()).WithTrailingTrivia(nodeToFix.GetTrailingTrivia());
            editor.ReplaceNode(nodeToFix, arrayEmptyInvocation);
            return editor.GetChangedDocument();
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => Title;
        }
    }
}
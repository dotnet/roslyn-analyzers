// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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

            string title = SystemRuntimeAnalyzersResources.UseArrayEmpty;
            context.RegisterCodeFix(new MyCodeAction(title,
                                                     async ct => await ConvertToArrayEmpty(context.Document, nodeToFix, ct).ConfigureAwait(false),
                                                     equivalenceKey: title),
                                    context.Diagnostics);
        }

        private async Task<Document> ConvertToArrayEmpty(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            ITypeSymbol elementType = GetArrayElementType(nodeToFix, semanticModel, cancellationToken);
            SyntaxNode arrayEmptyInvocation = GenerateArrayEmptyInvocation(generator, elementType, semanticModel).WithTriviaFrom(nodeToFix);

            editor.ReplaceNode(nodeToFix, arrayEmptyInvocation);
            return editor.GetChangedDocument();
        }

        private static ITypeSymbol GetArrayElementType(SyntaxNode arrayCreationExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var typeInfo = semanticModel.GetTypeInfo(arrayCreationExpression, cancellationToken);
            var arrayType = (IArrayTypeSymbol)typeInfo.Type;
            return arrayType.ElementType;
        }

        private static SyntaxNode GenerateArrayEmptyInvocation(SyntaxGenerator generator, ITypeSymbol elementType, SemanticModel semanticModel)
        {
            INamedTypeSymbol arrayTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(AvoidZeroLengthArrayAllocationsAnalyzer.ArrayTypeName);
            SyntaxNode arrayEmptyName = generator.QualifiedName(
                generator.TypeExpression(arrayTypeSymbol),
                generator.GenericName(AvoidZeroLengthArrayAllocationsAnalyzer.ArrayEmptyMethodName, elementType));
            return generator.InvocationExpression(arrayEmptyName);
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
=       private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}

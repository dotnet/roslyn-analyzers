// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic == null)
            {
                return Task.CompletedTask;
            }

            var methodPropertyKey = DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.MethodPropertyKey;
            // The fixer is only implemented for "Enumerable.First", "Enumerable.Last" and "Enumerable.Count"
            if (!diagnostic.Properties.TryGetValue(methodPropertyKey, out var method)
                || (method != "First" && method != "Last" && method != "Count"))
            {
                return Task.CompletedTask;
            }

            var title = MicrosoftNetCoreAnalyzersResources.UseIndexer;

            context.RegisterCodeFix(new MyCodeAction(title,
                                        async ct => await UseCollectionDirectly(context.Document, context.Span, method, ct).ConfigureAwait(false),
                                        equivalenceKey: title),
                                    diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> UseCollectionDirectly(Document document, TextSpan span, string methodName, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var invocationNode = root.FindNode(span, getInnermostNodeForTie: true);
            if (invocationNode == null)
            {
                return document;
            }

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (!(semanticModel.GetOperation(invocationNode, cancellationToken) is IInvocationOperation invocationOperation))
            {
                return document;
            }

            var collectionSyntax = invocationOperation.GetInstance();
            if (collectionSyntax == null)
            {
                return document;
            }

            var generator = SyntaxGenerator.GetGenerator(document);

            var elementAccessNode = GetReplacementNode(methodName, generator, collectionSyntax, semanticModel);
            if (elementAccessNode == null)
            {
                return document;
            }

            var newRoot = root.ReplaceNode(invocationNode, elementAccessNode.WithTrailingTrivia(invocationNode.GetTrailingTrivia()));
            return document.WithSyntaxRoot(newRoot);
        }

        private static SyntaxNode GetReplacementNode(string methodName, SyntaxGenerator generator, SyntaxNode collectionSyntax, SemanticModel semanticModel)
        {
            var collectionSyntaxNoTrailingTrivia = collectionSyntax.WithoutTrailingTrivia();

            if (methodName == "First")
            {
                var zeroLiteral = generator.LiteralExpression(0);
                return generator.ElementAccessExpression(collectionSyntaxNoTrailingTrivia, zeroLiteral);
            }

            if (methodName == "Last")
            {
                // TODO: Handle C# 8 index expression (and vb.net equivalent if any)

                if (!HasCountProperty(collectionSyntax, semanticModel))
                {
                    return null;
                }

                // TODO: Handle cases were `collectionSyntax` is an invocation. We would need to create some intermediate variable.
                var countMemberAccess = generator.MemberAccessExpression(collectionSyntaxNoTrailingTrivia, "Count");
                var oneLiteral = generator.LiteralExpression(1);

                // The SubstractExpression method will wrap left and right in parenthesis but those will be automatically removed later on
                var substraction = generator.SubtractExpression(countMemberAccess, oneLiteral);
                return generator.ElementAccessExpression(collectionSyntaxNoTrailingTrivia, substraction);
            }

            if (methodName == "Count")
            {
                return HasCountProperty(collectionSyntax, semanticModel)
                    ? generator.MemberAccessExpression(collectionSyntaxNoTrailingTrivia, "Count")
                    : null;
            }

            Debug.Fail($"Unexpected method name '{methodName}' for {DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.RuleId} code fix.");
            return null;
        }

        private static bool HasCountProperty(SyntaxNode node, SemanticModel model)
        {
            var typeSymbol = model.GetTypeInfo(node).Type;

            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.TypeKind == TypeKind.Class)
            {
                if (HasCountPropertyMember(typeSymbol))
                {
                    return true;
                }

                var currentType = typeSymbol.BaseType;
                while (currentType != null)
                {
                    if (HasCountPropertyMember(currentType))
                    {
                        return true;
                    }

                    currentType = currentType.BaseType;
                }

                return false;
            }

            if (typeSymbol.TypeKind == TypeKind.Interface)
            {
                return HasCountPropertyMember(typeSymbol)
                    ? true
                    : typeSymbol.AllInterfaces.Any(interfaceType => HasCountPropertyMember(interfaceType));
            }

            return false;

            static bool HasCountPropertyMember(ITypeSymbol type) =>
                type.GetMembers("Count").Any(member => member.Kind == SymbolKind.Property);
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}

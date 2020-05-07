// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class PreferStringContainsOverIndexOfFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferStringContainsOverIndexOfAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document doc = context.Document;
            CancellationToken cancellationToken = context.CancellationToken;
            SyntaxNode root = await doc.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root.FindNode(context.Span) is SyntaxNode expression)
            {
                SemanticModel semanticModel = await doc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                var compilation = semanticModel.Compilation;
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemString, out INamedTypeSymbol? stringType) ||
                    !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemChar, out INamedTypeSymbol? charType) ||
                    !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
                {
                    return;
                }

                var operation = semanticModel.GetOperation(expression, cancellationToken);
                // Not offering a code-fix for the variable declaration case
                if (!(operation is IBinaryOperation binaryOperation))
                {
                    return;
                }

                IInvocationOperation invocationOperation;
                IOperation otherOperation;
                if (binaryOperation.LeftOperand is IInvocationOperation)
                {
                    invocationOperation = (IInvocationOperation)binaryOperation.LeftOperand;
                    otherOperation = binaryOperation.RightOperand;
                }
                else
                {
                    invocationOperation = (IInvocationOperation)binaryOperation.RightOperand;
                    otherOperation = binaryOperation.LeftOperand;
                }

                var instanceOperation = invocationOperation.Instance;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle,
                        createChangedDocument: c => ReplaceBinaryOperationWithContains(doc, instanceOperation.Syntax, root, invocationOperation.Arguments, binaryOperation, c),
                        equivalenceKey: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfMessage),
                    context.Diagnostics);
                return;

                async Task<Document?> ReplaceBinaryOperationWithContains(Document document, SyntaxNode syntaxNode, SyntaxNode treeRoot, ImmutableArray<IArgumentOperation> indexOfMethodArguments, IBinaryOperation binaryOperation, CancellationToken cancellationToken)
                {
                    DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                    SyntaxEditor syntaxEditor = new SyntaxEditor(treeRoot, document.Project.Solution.Workspace);
                    SyntaxGenerator generator = editor.Generator;
                    var containsExpression = generator.MemberAccessExpression(syntaxNode, "Contains");
                    SyntaxNode? containsInvocation = null;
                    int numberOfArguments = indexOfMethodArguments.Length;
                    if (numberOfArguments == 1)
                    {
                        var firstArgument = indexOfMethodArguments.First();
                        if (firstArgument.Parameter.Type.Equals(charType))
                        {
                            containsInvocation = generator.InvocationExpression(containsExpression, firstArgument.Syntax);
                        }
                        else
                        {
                            var systemNode = generator.IdentifierName("System");
                            var argument = generator.MemberAccessExpression(generator.MemberAccessExpression(systemNode, "StringComparison"), "CurrentCulture");
                            containsInvocation = generator.InvocationExpression(containsExpression, firstArgument.Syntax, argument);
                        }
                    }
                    else
                    {
                        IArgumentOperation secondArgument = indexOfMethodArguments[1];
                        if (secondArgument.Value.ConstantValue.HasValue && secondArgument.Value.ConstantValue.Value is int intValue)
                        {
                            if ((StringComparison)intValue == StringComparison.Ordinal)
                            {
                                containsInvocation = generator.InvocationExpression(containsExpression, indexOfMethodArguments[0].Syntax);
                            }
                            else
                            {
                                containsInvocation = generator.InvocationExpression(containsExpression, indexOfMethodArguments[0].Syntax, indexOfMethodArguments[1].Syntax);
                            }
                        }
                        else
                        {
                            containsInvocation = generator.InvocationExpression(containsExpression, indexOfMethodArguments[0].Syntax, indexOfMethodArguments[1].Syntax);
                        }
                    }
                    SyntaxNode newIfCondition = containsInvocation;
                    int rightValue = (int)otherOperation.ConstantValue.Value;
                    if (rightValue == -1)
                    {
                        newIfCondition = generator.LogicalNotExpression(containsInvocation);
                    }
                    syntaxEditor.ReplaceNode(binaryOperation.Syntax, newIfCondition);
                    var newRoot = syntaxEditor.GetChangedRoot();
                    return document.WithSyntaxRoot(newRoot);
                }
            }
        }
    }
}

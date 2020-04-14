// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var diagnostics = context.Diagnostics;
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
                if (operation is IBinaryOperation binaryOperation)
                {
                    var leftOperand = binaryOperation.LeftOperand;
                    if (leftOperand is ILocalReferenceOperation localReferenceOperation)
                    {
                        SyntaxNode localDeclarationStatement = root.FindNode(diagnostics.First().AdditionalLocations[0].SourceSpan);
                        var variableDeclarationGroupOperation = (IVariableDeclarationGroupOperation)semanticModel.GetOperation(localDeclarationStatement, cancellationToken);
                        var invocationOperation = (IInvocationOperation)variableDeclarationGroupOperation.Declarations.First().Declarators.First().GetVariableInitializer().Value;
                        HandleInvocationOperation(invocationOperation, variableDeclarationGroupOperation);
                    }
                    else if (leftOperand is IInvocationOperation invocationOperation)
                    {
                        HandleInvocationOperation(invocationOperation);
                    }

                    void HandleInvocationOperation(IInvocationOperation invocationOperation, IVariableDeclarationGroupOperation? variableDeclarationGroupOperation = null)
                    {
                        var instanceOperation = invocationOperation.Instance;
                        SyntaxNode? syntaxNode = null;
                        if (instanceOperation is ILocalReferenceOperation localReferenceOperation)
                        {
                            syntaxNode = localReferenceOperation.Syntax;
                        }
                        else if (instanceOperation is IParameterReferenceOperation parameterReferenceOperation)
                        {
                            syntaxNode = parameterReferenceOperation.Syntax;
                        }
                        else
                        {
                            return;
                        }

                        ImmutableArray<IArgumentOperation> argumentsForContainsInvocation = invocationOperation.Arguments;
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle,
                                createChangedDocument: c => ReplaceBinaryOperationWithContains(doc, syntaxNode, root, argumentsForContainsInvocation, binaryOperation, c, variableDeclarationGroupOperation),
                                equivalenceKey: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfMessage),
                            context.Diagnostics);
                    }

                    async Task<Document?> ReplaceBinaryOperationWithContains(Document document, SyntaxNode localReferenceOrParameterNode, SyntaxNode treeRoot, ImmutableArray<IArgumentOperation> indexOfMethodArguments, IBinaryOperation binaryOperation, CancellationToken cancellationToken, IVariableDeclarationGroupOperation? variableDeclarationGroupOperation = null)
                    {
                        DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                        SyntaxEditor syntaxEditor = new SyntaxEditor(treeRoot, document.Project.Solution.Workspace);
                        SyntaxGenerator generator = editor.Generator;
                        var containsExpression = generator.MemberAccessExpression(localReferenceOrParameterNode, "Contains");
                        List<SyntaxNode> stringComparisonArguments = new List<SyntaxNode>();
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
                                stringComparisonArguments.Add(firstArgument.Syntax);
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
                                if (intValue == (int)StringComparison.Ordinal)
                                {
                                    containsInvocation = generator.InvocationExpression(containsExpression, indexOfMethodArguments[0].Syntax);
                                }
                                else
                                {
                                    containsInvocation = generator.InvocationExpression(containsExpression, indexOfMethodArguments[0].Syntax, indexOfMethodArguments[1].Syntax);
                                }
                            }
                        }
                        SyntaxNode newIfCondition = containsInvocation!;
                        var rightOperand = binaryOperation.RightOperand;
                        int rightValue = (int)rightOperand.ConstantValue.Value;
                        if (rightValue == -1)
                        {
                            newIfCondition = generator.LogicalNotExpression(containsInvocation);
                        }
                        syntaxEditor.ReplaceNode(binaryOperation.Syntax, newIfCondition);
                        if (variableDeclarationGroupOperation != null)
                        {
                            syntaxEditor.RemoveNode(variableDeclarationGroupOperation.Syntax);
                        }
                        var newRoot = syntaxEditor.GetChangedRoot();
                        return document.WithSyntaxRoot(newRoot);
                    }
                }
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
    public class PreferStringContainsOverIndexOfFixer : CodeFixProvider
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
                        SyntaxNode syntaxNode = null;
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

                        var argumentsForContainsInvocation = invocationOperation.Arguments.Select(argument => argument.Syntax);
                        if (argumentsForContainsInvocation.Count() == 1)
                        {
                            if (!semanticModel.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
                            {
                                return;
                            }

                            var ordinal = stringComparisonType.GetMembers("Ordinal").FirstOrDefault();
                            var currentCulture = stringComparisonType.GetMembers("CurrentCulture").FirstOrDefault();
                            if (ordinal == null || currentCulture == null)
                            {
                                return;
                            }

                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle,
                                    createChangedDocument: c => ReplaceBinaryOperationWithContains(doc, syntaxNode, root, argumentsForContainsInvocation, binaryOperation.Syntax, c, variableDeclarationGroupOperation, currentCulture),
                                    equivalenceKey: "PreferStringContainsCurrentCultureOverIndexOfFixer"),
                                context.Diagnostics);

                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle,
                                    createChangedDocument: c => ReplaceBinaryOperationWithContains(doc, syntaxNode, root, argumentsForContainsInvocation, binaryOperation.Syntax, c, variableDeclarationGroupOperation, ordinal),
                                    equivalenceKey: "PreferStringContainsOrdinalOverIndexOfFixer"),
                                context.Diagnostics);

                        }
                        else
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle,
                                    createChangedDocument: c => ReplaceBinaryOperationWithContains(doc, syntaxNode, root, argumentsForContainsInvocation, binaryOperation.Syntax, c, variableDeclarationGroupOperation),
                                    equivalenceKey: "PreferStringContainsOverIndexOfFixer"),
                                context.Diagnostics);
                        }

                    }

                }
            }
        }

        private static async Task<Document?> ReplaceBinaryOperationWithContains(Document document, SyntaxNode localReferenceOrParameterNode, SyntaxNode treeRoot, IEnumerable<SyntaxNode> argumentsForContainsInvocation, SyntaxNode binaryOperationSyntaxNode, CancellationToken cancellationToken, IVariableDeclarationGroupOperation? variableDeclarationGroupOperation = null, ISymbol? stringComparisonArgumentToContainsInvocation = null)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxEditor syntaxEditor = new SyntaxEditor(treeRoot, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;
            var memberAccessExpression = generator.MemberAccessExpression(localReferenceOrParameterNode, "Contains");
            if (stringComparisonArgumentToContainsInvocation != null)
            {
                var systemIdentifier = generator.IdentifierName("System");
                var stringComparisonIdentifier = generator.MemberAccessExpression(systemIdentifier, "StringComparison");
                var stringComparisonArgument = generator.MemberAccessExpression(stringComparisonIdentifier, stringComparisonArgumentToContainsInvocation.Name);
                argumentsForContainsInvocation = argumentsForContainsInvocation.Concat(stringComparisonArgument);
            }
            var containsInvocation = generator.InvocationExpression(memberAccessExpression, argumentsForContainsInvocation);
            var newIfCondition = generator.LogicalNotExpression(containsInvocation);
            syntaxEditor.ReplaceNode(binaryOperationSyntaxNode, newIfCondition);
            if (variableDeclarationGroupOperation != null)
            {
                syntaxEditor.RemoveNode(variableDeclarationGroupOperation.Syntax);
            }
            var newRoot = syntaxEditor.GetChangedRoot();
            return document.WithSyntaxRoot(newRoot);
        }
    }
}

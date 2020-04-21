// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Operations;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class PreferConstCharOverConstUnitStringFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferConstCharOverConstUnitStringAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document doc = context.Document;
            CancellationToken cancellationToken = context.CancellationToken;
            SyntaxNode root = await doc.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root.FindNode(context.Span) is SyntaxNode expression)
            {
                SemanticModel semanticModel = await doc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                var operation = semanticModel.GetOperation(expression, cancellationToken);
                if (operation is IArgumentOperation argumentOperation)
                {
                    IOperation? isLocalReferenceOperation = argumentOperation.Value as ILocalReferenceOperation;
                    IOperation? isLiteralOperation = argumentOperation.Value as ILiteralOperation;
                    if (isLocalReferenceOperation == null && isLiteralOperation == null)
                    {
                        return;
                    }

                    IVariableDeclaratorOperation? cachedVariableDeclaratorOperation = default;
                    if (isLocalReferenceOperation != null)
                    {
                        ILocalReferenceOperation localReferenceOperation = (ILocalReferenceOperation)argumentOperation.Value;
                        ILocalSymbol localArgumentDeclaration = localReferenceOperation.Local;
                        SyntaxReference declaringSyntaxReference = localArgumentDeclaration.DeclaringSyntaxReferences.FirstOrDefault();
                        if (declaringSyntaxReference is null)
                        {
                            return;
                        }
                        var variableDeclarator = semanticModel.GetOperationWalkingUpParentChain(declaringSyntaxReference.GetSyntax(cancellationToken), cancellationToken);

                        if (variableDeclarator is IVariableDeclaratorOperation variableDeclaratorOperation)
                        {
                            cachedVariableDeclaratorOperation = variableDeclaratorOperation;
                            IVariableDeclarationOperation variableDeclarationOperation = (IVariableDeclarationOperation)variableDeclaratorOperation.Parent;
                            if (variableDeclarationOperation == null)
                            {
                                return;
                            }

                            IVariableDeclarationGroupOperation variableGroupDeclarationOperation = (IVariableDeclarationGroupOperation)variableDeclarationOperation.Parent;
                            if (variableGroupDeclarationOperation.Declarations.Length != 1)
                            {
                                return;
                            }

                            if (variableDeclarationOperation.Declarators.Length != 1)
                            {
                                return;
                            }
                        }
                    }

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderTitle,
                            createChangedDocument: async c =>
                            {
                                if (argumentOperation.Value is ILiteralOperation argumentLiteral)
                                {
                                    return await HandleStringLiteral(argumentLiteral, doc, root, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    return await HandleVariableDeclarator(cachedVariableDeclaratorOperation!, doc, root, cancellationToken).ConfigureAwait(false);
                                }
                            },
                            equivalenceKey: MicrosoftNetCoreAnalyzersResources.PreferConstCharOverConstUnitStringInStringBuilderMessage),
                        context.Diagnostics);
                }
            }
        }

        private static async Task<Document?> HandleStringLiteral(ILiteralOperation argumentLiteral, Document doc, SyntaxNode root, CancellationToken cancellationToken)
        {
            var unitString = (string)argumentLiteral.ConstantValue.Value;
            DocumentEditor editor = await DocumentEditor.CreateAsync(doc, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            char charValue = unitString[0];
            SyntaxNode charLiteralExpressionNode = generator.LiteralExpression(charValue);
            var newRoot = generator.ReplaceNode(root, argumentLiteral.Syntax, charLiteralExpressionNode);
            return doc.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document?> HandleVariableDeclarator(IVariableDeclaratorOperation variableDeclaratorOperation, Document doc, SyntaxNode root, CancellationToken cancellationToken)
        {
            IVariableDeclarationOperation variableDeclarationOperation = (IVariableDeclarationOperation)variableDeclaratorOperation.Parent;
            IVariableDeclarationGroupOperation variableGroupDeclarationOperation = (IVariableDeclarationGroupOperation)variableDeclarationOperation.Parent;

            DocumentEditor editor = await DocumentEditor.CreateAsync(doc, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            ILocalSymbol currentSymbol = variableDeclaratorOperation.Symbol;
            var variableInitializerOperation = OperationExtensions.GetVariableInitializer(variableDeclaratorOperation);
            if (variableInitializerOperation == null)
            {
                return null;
            }

            string unitString = (string)variableInitializerOperation.Value.ConstantValue.Value;
            char charValue = unitString[0];
            SyntaxNode charLiteralExpressionNode = generator.LiteralExpression(charValue);
            var charTypeNode = generator.TypeExpression(SpecialType.System_Char);
            var charSyntaxNode = generator.LocalDeclarationStatement(charTypeNode, currentSymbol.Name, charLiteralExpressionNode, isConst: true);
            charSyntaxNode = charSyntaxNode.WithTriviaFrom(variableGroupDeclarationOperation.Syntax);
            var newRoot = generator.ReplaceNode(root, variableGroupDeclarationOperation.Syntax, charSyntaxNode);
            return doc.WithSyntaxRoot(newRoot);
        }
    }
}

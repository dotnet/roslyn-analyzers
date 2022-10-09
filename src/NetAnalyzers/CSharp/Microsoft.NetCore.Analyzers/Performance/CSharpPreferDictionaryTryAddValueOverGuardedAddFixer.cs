// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    public sealed class CSharpPreferDictionaryTryAddValueOverGuardedAddFixer : PreferDictionaryTryAddValueOverGuardedAddFixer
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            var dictionaryAddLocation = diagnostic?.AdditionalLocations[0];
            if (dictionaryAddLocation is null)
            {
                return;
            }

            Document document = context.Document;
            SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var dictionaryAdd = root.FindNode(dictionaryAddLocation.SourceSpan, getInnermostNodeForTie: true);
            if (dictionaryAdd is not InvocationExpressionSyntax dictionaryAddInvocation
                || root.FindNode(context.Span) is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax containsKeyAccess } containsKeyInvocation)
            {
                return;
            }

            var action = CodeAction.Create(CodeFixTitle, async ct =>
            {
                var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
                var generator = editor.Generator;

                var tryAddValueAccess = generator.MemberAccessExpression(containsKeyAccess.Expression, TryAdd);
                var dictionaryAddArguments = dictionaryAddInvocation.ArgumentList.Arguments;
                var tryAddInvocation = generator.InvocationExpression(tryAddValueAccess, dictionaryAddArguments[0], dictionaryAddArguments[1]);

                var ifStatement = containsKeyInvocation.FirstAncestorOrSelf<IfStatementSyntax>();
                if (ifStatement is null)
                {
                    return editor.OriginalDocument;
                }

                if (ifStatement.Condition is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.LogicalNotExpression))
                {
                    if (ifStatement.Statement is BlockSyntax { Statements.Count: 1 } or ExpressionStatementSyntax)
                    {
                        if (ifStatement.Else is null)
                        {
                            // d.Add() is the only statement in the if and is guarded with a !d.ContainsKey().
                            // Since there is no else-branch, we can replace the entire if-statement with a d.TryAdd() call.
                            var invocationWithTrivia = tryAddInvocation.WithTriviaFrom(ifStatement);
                            editor.ReplaceNode(ifStatement, generator.ExpressionStatement(invocationWithTrivia));
                        }
                        else
                        {
                            // d.Add() is the only statement in the if and is guarded with a !d.ContainsKey().
                            // In this case, we switch out the !d.ContainsKey() call with a !d.TryAdd() call and move the else-branch into the if.
                            editor.ReplaceNode(containsKeyInvocation, tryAddInvocation);
                            editor.ReplaceNode(ifStatement.Statement, ifStatement.Else.Statement);
                            editor.RemoveNode(ifStatement.Else, SyntaxRemoveOptions.KeepNoTrivia);
                        }
                    }
                    else
                    {
                        // d.Add() is one of many statements in the if and is guarded with a !d.ContainsKey().
                        // In this case, we switch out the !d.ContainsKey() call for a d.TryAdd() call.
                        editor.RemoveNode(dictionaryAddInvocation.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                        editor.ReplaceNode(unary, tryAddInvocation);
                    }
                }
                else if (ifStatement.Condition.IsKind(SyntaxKind.InvocationExpression) && ifStatement.Else is not null)
                {
                    var negatedTryAddInvocation = generator.LogicalNotExpression(tryAddInvocation);
                    editor.ReplaceNode(containsKeyInvocation, negatedTryAddInvocation);
                    if (ifStatement.Else.Statement is BlockSyntax { Statements.Count: 1 } or ExpressionStatementSyntax)
                    {
                        // d.Add() is the only statement the else-branch and guarded by a d.ContainsKey() call in the if.
                        // In this case we replace the d.ContainsKey() call with a !d.TryAdd() call and remove the entire else-branch.
                        editor.RemoveNode(ifStatement.Else);
                    }
                    else
                    {
                        // d.Add() is one of many statements in the else-branch and guarded by a d.ContainsKey() call in the if.
                        // In this case we replace the d.ContainsKey() call with a !d.TryAdd() call and remove the d.Add() call in the else-branch.
                        editor.RemoveNode(dictionaryAddInvocation.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }

                return editor.GetChangedDocument();
            }, CodeFixTitle);

            context.RegisterCodeFix(action, context.Diagnostics);
        }
    }
}
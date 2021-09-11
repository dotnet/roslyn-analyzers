// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpPreferHashDataOverComputeHashFixer : PreferHashDataOverComputeHashFixer
    {
        private static readonly CSharpPreferHashDataOverComputeHashFixAllProvider s_fixAllProvider = new();
        public sealed override FixAllProvider GetFixAllProvider() => s_fixAllProvider;

        protected override bool TryGetCodeAction(
            Document document,
            SyntaxNode computeHashNode,
            SyntaxNode hashDataNode,
            SyntaxNode createHashNode,
            SyntaxNode[] disposeNodes,
            [NotNullWhen(true)] out HashDataCodeAction? codeAction)
        {
            switch (createHashNode.Parent)
            {
                case { Parent: UsingStatementSyntax usingStatement } when usingStatement.Declaration.Variables.Count == 1:
                    {
                        codeAction = new CSharpRemoveUsingStatementHashDataCodeAction(document,
                            computeHashNode,
                            hashDataNode,
                            usingStatement);
                        return true;
                    }
                case { Parent: UsingStatementSyntax }:
                    {
                        codeAction = new RemoveNodeHashDataCodeAction(document,
                            computeHashNode,
                            hashDataNode,
                            createHashNode,
                            disposeNodes);
                        return true;
                    }
                case { Parent: LocalDeclarationStatementSyntax localDeclarationStatementSyntax }:
                    {
                        codeAction = new RemoveNodeHashDataCodeAction(document,
                            computeHashNode,
                            hashDataNode,
                            localDeclarationStatementSyntax,
                            disposeNodes);
                        return true;
                    }
                case VariableDeclaratorSyntax variableDeclaratorSyntax:
                    {
                        codeAction = new RemoveNodeHashDataCodeAction(document,
                            computeHashNode,
                            hashDataNode,
                            variableDeclaratorSyntax,
                            disposeNodes);
                        return true;
                    }
                default:
                    {
                        codeAction = null;
                        return false;
                    }
            }
        }

        protected override SyntaxNode? GetHashDataSyntaxNode(PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, string hashTypeName, SyntaxNode computeHashNode)
        {
            return GetHashDataSyntaxNode(computeType, hashTypeName, ((InvocationExpressionSyntax)computeHashNode).ArgumentList);
        }

        private sealed class CSharpRemoveUsingStatementHashDataCodeAction : HashDataCodeAction
        {
            public CSharpRemoveUsingStatementHashDataCodeAction(Document document,
                SyntaxNode computeHashNode,
                SyntaxNode hashDataNode,
                UsingStatementSyntax usingStatementToRemove) : base(document, computeHashNode, hashDataNode)
            {
                UsingStatementToRemove = usingStatementToRemove;
            }

            public UsingStatementSyntax UsingStatementToRemove { get; }

            protected override void EditNodes(DocumentEditor documentEditor)
            {
                var statements = UsingStatementToRemove.Statement.ReplaceNode(ComputeHashNode, HashDataNode)
                    .ChildNodes()
                    .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation));

                documentEditor.InsertBefore(UsingStatementToRemove, statements);
                documentEditor.RemoveNode(UsingStatementToRemove);
            }
        }

        private static SyntaxNode? GetHashDataSyntaxNode(PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, string hashTypeName, ArgumentListSyntax argumentList)
        {
            switch (computeType)
            {
                // hashTypeName.HashData(buffer)
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHash:
                    {
                        var hashData = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(hashTypeName),
                            SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName));
                        return SyntaxFactory.InvocationExpression(hashData, argumentList);
                    }
                // hashTypeName.HashData(buffer.AsSpan(start, end))
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHashSection:
                    {
                        var asSpan = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            argumentList.Arguments[0].Expression,
                            SyntaxFactory.IdentifierName("AsSpan"));
                        var spanArgs = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(SyntaxFactory.List(argumentList.Arguments.Skip(1))));
                        var asSpanInvoked = SyntaxFactory.InvocationExpression(asSpan, spanArgs);
                        var hashData = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(hashTypeName),
                            SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName));
                        var args = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(asSpanInvoked)));
                        return SyntaxFactory.InvocationExpression(hashData, args);
                    }
                // hashTypeName.TryHashData(rosSpan, span, write)
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.TryComputeHash:
                    {
                        var hashData = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(hashTypeName),
                            SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.TryHashDataMethodName));
                        return SyntaxFactory.InvocationExpression(hashData, argumentList);
                    }
                default:
                    Debug.Fail("there is only 3 type of ComputeHash");
                    return null;
            }
        }

        private sealed class CSharpPreferHashDataOverComputeHashFixAllCodeAction : PreferHashDataOverComputeHashFixAllCodeAction
        {
            public CSharpPreferHashDataOverComputeHashFixAllCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix) : base(title, solution, diagnosticsToFix)
            {
            }

            internal override SyntaxNode FixDocumentRoot(SyntaxNode root, HashInstanceTarget[] hashInstanceTargets)
            {
                foreach (var target in hashInstanceTargets)
                {
                    foreach (var c in target.ComputeHashNodes)
                    {
                        var tracked = root.GetCurrentNode(c.ComputeHashNode);
                        var a = GetHashDataSyntaxNode(c.ComputeType, c.HashTypeName, ((InvocationExpressionSyntax)tracked).ArgumentList);
                        root = root.ReplaceNode(tracked, a);
                    }

                    if (target.CreateNode is null)
                    {
                        continue;
                    }

                    var currentCreateNode = root.GetCurrentNode(target.CreateNode);
                    switch (currentCreateNode.Parent)
                    {
                        case { Parent: UsingStatementSyntax usingStatement } when usingStatement.Declaration.Variables.Count == 1:
                            {
                                var statements = usingStatement.Statement
                                    .ChildNodes()
                                    .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation));
                                root = root.TrackNodes(usingStatement);
                                root = root.InsertNodesBefore(root.GetCurrentNode(usingStatement), statements);
                                root = root.RemoveNode(root.GetCurrentNode(usingStatement), SyntaxRemoveOptions.KeepNoTrivia);
                                break;
                            }
                        case { Parent: UsingStatementSyntax usingStatement }:
                            {
                                root = root.RemoveNode(currentCreateNode, SyntaxRemoveOptions.KeepNoTrivia);
                                break;
                            }
                        case { Parent: LocalDeclarationStatementSyntax localDeclarationStatementSyntax }:
                            {
                                root = root.RemoveNode(localDeclarationStatementSyntax, SyntaxRemoveOptions.KeepNoTrivia);
                                break;
                            }
                        case VariableDeclaratorSyntax variableDeclaratorSyntax:
                            {
                                root = root.RemoveNode(variableDeclaratorSyntax, SyntaxRemoveOptions.KeepNoTrivia);
                                break;
                            }
                    }

                    if (target.DisposeNodes is null)
                    {
                        continue;
                    }

                    foreach (var disposeNode in target.DisposeNodes)
                    {
                        var trackedDisposeNode = root.GetCurrentNode(disposeNode);
                        root = root.RemoveNode(trackedDisposeNode, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }
                return root;
            }
        }

        private sealed class CSharpPreferHashDataOverComputeHashFixAllProvider : PreferHashDataOverComputeHashFixAllProvider
        {
            protected override PreferHashDataOverComputeHashFixAllCodeAction GetCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                return new CSharpPreferHashDataOverComputeHashFixAllCodeAction(title, solution, diagnosticsToFix);
            }
        }
    }
}

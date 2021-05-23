﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpPreferHashDataOverComputeHashFixer : PreferHashDataOverComputeHashFixer
    {
        protected override bool TryGetCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode, SyntaxNode nodeToRemove, [NotNullWhen(true)] out HashDataCodeAction? codeAction)
        {
            switch (nodeToRemove)
            {
                case LocalDeclarationStatementSyntax or VariableDeclaratorSyntax:
                    {
                        codeAction = new RemoveNodeHashDataCodeAction(document,
                            hashTypeName,
                            bufferArgNode,
                            computeHashNode,
                            nodeToRemove);
                        return true;
                    }
                case { Parent: UsingStatementSyntax usingStatement } when usingStatement.Declaration.Variables.Count == 1:
                    {
                        codeAction = new CSharpRemoveUsingStatementHashDataCodeAction(document,
                            hashTypeName,
                            bufferArgNode,
                            computeHashNode,
                            usingStatement);
                        return true;
                    }
                default:
                    {
                        codeAction = null;
                        return false;
                    }
            }
        }

        private sealed class CSharpRemoveUsingStatementHashDataCodeAction : HashDataCodeAction
        {
            public CSharpRemoveUsingStatementHashDataCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode, UsingStatementSyntax usingStatementToRemove) : base(document, hashTypeName, bufferArgNode, computeHashNode)
            {
                UsingStatementToRemove = usingStatementToRemove;
            }

            public UsingStatementSyntax UsingStatementToRemove { get; }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                var statements = UsingStatementToRemove.Statement.ReplaceNode(ComputeHashNode, hashDataInvoked)
                    .ChildNodes()
                    .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation));

                documentEditor.InsertBefore(UsingStatementToRemove, statements);
                documentEditor.RemoveNode(UsingStatementToRemove);
            }
        }
    }
}

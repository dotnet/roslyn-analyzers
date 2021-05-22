// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpPreferHashDataOverComputeHashFixer : PreferHashDataOverComputeHashFixer
    {
        protected override bool TryGetCodeFixer(SyntaxNode computeHashNode, SyntaxNode bufferArgNode, SyntaxNode nodeToRemove, [NotNullWhen(true)] out ApplyCodeFixAction? codeFixer)
        {
            switch (nodeToRemove)
            {
                case LocalDeclarationStatementSyntax or VariableDeclaratorSyntax:
                    {
                        codeFixer = (editor, hashDataInvoked) =>
                        {
                            editor.ReplaceNode(computeHashNode, hashDataInvoked);
                            editor.RemoveNode(nodeToRemove);
                        };
                        return true;
                    }
                case { Parent: UsingStatementSyntax usingStatement } when usingStatement.Declaration.Variables.Count == 1:
                    {
                        codeFixer = (editor, hashDataInvoked) =>
                        {
                            var statements = usingStatement.Statement.ReplaceNode(computeHashNode, hashDataInvoked)
                                .ChildNodes()
                                .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation));

                            editor.InsertBefore(usingStatement, statements);
                            editor.RemoveNode(usingStatement);
                        };
                        return true;
                    }
                default:
                    {
                        codeFixer = null;
                        return false;
                    }
            }
        }
    }
}

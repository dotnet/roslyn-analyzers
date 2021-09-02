// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpUseNullSuppressionCorrectlyFixer : UseNullSuppressionCorrectlyFixer
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharpUseNullSuppressionCorrectlyAnalyzer.RuleId);

        public override Task<Document> RemoveNullSuppression(Document document, SyntaxNode root, SyntaxNode node, CancellationToken cancellationToken)
        {
            SyntaxNode newNode = node.ReplaceToken(node.ChildTokens().First(x => x.IsKind(SyntaxKind.ExclamationToken)), Array.Empty<SyntaxToken>());
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
        }
    }
}
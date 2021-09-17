// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpUseNullSuppressionCorrectlyFixer : UseNullSuppressionCorrectlyFixer
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharpUseNullSuppressionCorrectlyAnalyzer.RuleId);

        public override Task<Document> RemoveNullSuppressionAsync(Document document, SyntaxNode root, SyntaxNode node, CancellationToken cancellationToken)
        {
            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(node, node.ChildNodes().First())));
        }
    }
}
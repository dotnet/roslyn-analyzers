// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpAvoidZeroLengthArrayAllocationsFixer : AvoidZeroLengthArrayAllocationsFixer
    {
        protected override T AddElasticMarker<T>(T syntaxNode)
            => syntaxNode.WithTrailingTrivia(syntaxNode.GetTrailingTrivia().Add(SyntaxFactory.ElasticMarker));
    }
}

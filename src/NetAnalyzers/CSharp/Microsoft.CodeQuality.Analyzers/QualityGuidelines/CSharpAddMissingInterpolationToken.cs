// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAddMissingInterpolationToken : AbstractAddMissingInterpolationTokenAnalyzer
    {
        private protected override bool AreAllInterpolationsBindable(SyntaxNode node, SemanticModel model)
        {
            var interpolations = ((InterpolatedStringExpressionSyntax)node).Contents.OfType<InterpolationSyntax>();
            bool hasNonConstantInterpolation = false;
            foreach (var interpolation in interpolations)
            {
                if (interpolation.Expression is LiteralExpressionSyntax)
                {
                    continue;
                }

                if (model.GetSymbolInfo(interpolation.Expression).Symbol is null)
                {
                    return false;
                }

                hasNonConstantInterpolation = true;
            }

            return hasNonConstantInterpolation;
        }

        private protected override SyntaxNode? ParseStringLiteralAsInterpolatedString(ILiteralOperation operation)
            => SyntaxFactory.ParseExpression("$" + operation.Syntax.ToString(), options: operation.Syntax.SyntaxTree.Options) as InterpolatedStringExpressionSyntax;

        private protected override bool TryGetSpeculativeSemanticModel(ILiteralOperation operation, SyntaxNode dummyNode, out SemanticModel model)
            => operation.SemanticModel.TryGetSpeculativeSemanticModel(operation.Syntax.SpanStart, dummyNode.FirstAncestorOrSelf<StatementSyntax>(), out model);
    }
}

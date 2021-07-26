// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        private protected override bool ShouldReport(ILiteralOperation operation)
        {
            var annotation = new SyntaxAnnotation();
            if (SyntaxFactory.ParseExpression("$" + operation.Syntax.ToString(), options: operation.Syntax.SyntaxTree.Options).WithAdditionalAnnotations(annotation)
                is not InterpolatedStringExpressionSyntax dummyNode)
            {
                return false;
            }

            var root = operation.Syntax.SyntaxTree.GetRoot();
            root = root.ReplaceNode(operation.Syntax, dummyNode);
            dummyNode = (InterpolatedStringExpressionSyntax)root.GetAnnotatedNodes(annotation).Single();
            if (!operation.SemanticModel.TryGetSpeculativeSemanticModel(operation.Syntax.SpanStart, dummyNode.FirstAncestorOrSelf<StatementSyntax>(), out var model))
            {
                return false;
            }

            var interpolations = dummyNode.Contents.OfType<InterpolationSyntax>();
            bool hasNonConstantInterpolation = false;
            foreach (var interpolation in interpolations)
            {
                if (interpolation.Expression is LiteralExpressionSyntax)
                {
                    continue;
                }

                hasNonConstantInterpolation = true;
                var info = model.GetSymbolInfo(interpolation.Expression);
                if (info.Symbol is null)
                {
                    return false;
                }
            }

            return hasNonConstantInterpolation;
        }
    }
}

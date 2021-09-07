// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseNullSuppressionCorrectlyAnalyzer : UseNullSuppressionCorrectly<SyntaxKind>
    {
        protected override ImmutableArray<SyntaxKind> SyntaxKinds => ImmutableArray.Create(SyntaxKind.SuppressNullableWarningExpression);

        protected override void AnalyzeNullSuppressedLiterals(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PostfixUnaryExpressionSyntax postfixUnaryExpression ||
                postfixUnaryExpression.Operand is not LiteralExpressionSyntax literalExpression)
            {
                return;
            }

            DiagnosticDescriptor rule = NeverNullLiteralsRule; // default, as a plain literal expression is not null

            SyntaxKind expressionKind = literalExpression.Token.Kind();
            if (expressionKind == SyntaxKind.NullKeyword)
            {
                rule = LiteralAlwaysNullRule;
            }
            else if (expressionKind == SyntaxKind.DefaultKeyword)
            {
                rule = context.SemanticModel.GetTypeInfo(literalExpression).ConvertedType.IsNonNullableValueType()
                    ? NeverNullLiteralsRule
                    : LiteralAlwaysNullRule;
            }

            context.ReportDiagnostic(postfixUnaryExpression.CreateDiagnostic(rule));
        }
    }
}
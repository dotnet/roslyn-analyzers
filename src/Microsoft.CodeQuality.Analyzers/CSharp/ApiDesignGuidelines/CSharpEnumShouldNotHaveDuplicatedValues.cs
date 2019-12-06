// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpEnumShouldNotHaveDuplicatedValues : EnumShouldNotHaveDuplicatedValues
    {
        protected override void AnalyzeEnumMemberValues(Dictionary<object, List<SyntaxNode>> membersByValue, SymbolAnalysisContext context)
        {
            foreach (var kvp in membersByValue)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var enumMember = (EnumMemberDeclarationSyntax)kvp.Value[i];

                    switch (enumMember.EqualsValue?.Value)
                    {
                        case IdentifierNameSyntax identifier:
                            // It is allowed to reference another enum value through its identifier
                            break;

                        case BinaryExpressionSyntax binaryExpression:
                            AnalyzeBinaryExpression(binaryExpression, context, i > 0);
                            break;

                        default:
                            if (i > 0)
                            {
                                context.ReportDiagnostic(enumMember.CreateDiagnostic(RuleDuplicatedValue));
                            }
                            break;
                    }
                }
            }
        }

        private static void AnalyzeBinaryExpression(BinaryExpressionSyntax binaryExpression, SymbolAnalysisContext context, bool isDuplicatedValue)
        {
            var hasBitwiseIssue = false;
            var seen = new HashSet<object>();

            foreach (var identifier in binaryExpression.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (identifier.Identifier.Value == null)
                {
                    continue;
                }

                if (seen.Contains(identifier.Identifier.Value))
                {
                    hasBitwiseIssue = true;
                    context.ReportDiagnostic(identifier.CreateDiagnostic(RuleDuplicatedBitwiseValuePart));
                }
                else
                {
                    seen.Add(identifier.Identifier.Value);
                }
            }

            // This enum value doesn't have any duplicated bitwise part but duplicates another enum value so let's report
            if (!hasBitwiseIssue && isDuplicatedValue)
            {
                context.ReportDiagnostic(binaryExpression.Parent.Parent.CreateDiagnostic(RuleDuplicatedValue));
            }
        }
    }
}

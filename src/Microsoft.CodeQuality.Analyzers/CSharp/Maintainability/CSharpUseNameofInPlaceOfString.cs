// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class CSharpUseNameofInPlaceOfString : UseNameofInPlaceOfStringAnalyzer<SyntaxKind>
    {
        protected override SyntaxKind ArgumentSyntaxKind => SyntaxKind.Argument;

        internal override SyntaxNode GetArgumentExpression(SyntaxNode argumentList)
            => (ExpressionSyntax)argumentList.Parent;

        internal override SyntaxNode GetArgumentListSyntax(SyntaxNode argument)
            => (ArgumentListSyntax)((ArgumentSyntax)argument).Parent;

        internal override int GetIndexOfArgument(SyntaxNode argumentList, SyntaxNode argumentSyntaxNode)
            => ((ArgumentListSyntax)argumentList).Arguments.IndexOf((ArgumentSyntax)argumentSyntaxNode);

        internal override bool IsValidIdentifier(string stringLiteral)
        {
            return SyntaxFacts.IsValidIdentifier(stringLiteral);
        }

        internal override bool TryGetNamedArgument(SyntaxNode argumentSyntaxNode, out string argumentName)
        {
            var argument = (ArgumentSyntax)argumentSyntaxNode;
            if (argument.NameColon == null)
            {
                argumentName = null;
                return false;
            }

            argumentName = argument.NameColon.Name.Identifier.ValueText;
            return true;
        }

        internal override bool TryGetStringLiteralOfExpression(SyntaxNode argument, out SyntaxNode stringLiteral, out string stringText)
        {
            var expression = ((ArgumentSyntax)argument).Expression;
            if (expression == null || !expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                stringLiteral = null;
                stringText = "";
                return false;
            }

            stringLiteral = expression;
            stringText = ((LiteralExpressionSyntax)expression).Token.ValueText;
            return true;
        }

        internal override IEnumerable<string> GetParametersInScope(SyntaxNode node)
        {
            {
                foreach (var ancestor in node.AncestorsAndSelf())
                {
                    if (ancestor.IsKind(SyntaxKind.SimpleLambdaExpression))
                    {
                        yield return ((SimpleLambdaExpressionSyntax)ancestor).Parameter.Identifier.ValueText;
                    }
                    else
                    {
                        var parameterList = GetParameterList(ancestor);
                        if (parameterList != null)
                        {
                            foreach (var parameter in parameterList.Parameters)
                            {
                                yield return parameter.Identifier.ValueText;
                            }
                        }
                    }
                }
            }
        }

        internal override IEnumerable<string> GetPropertiesInScope(SyntaxNode argument)
        {
            // and struct
            var ancestors = ((ArgumentSyntax)argument).FirstAncestorOrSelf<SyntaxNode>(a => a.IsKind(SyntaxKind.ClassDeclaration))
                .ChildNodes()
                .Where(t => t.IsKind(SyntaxKind.PropertyDeclaration))
                .Select(t => ((PropertyDeclarationSyntax)t).Identifier.ValueText);

            return ancestors.Cast<string>();
        }

        internal static BaseParameterListSyntax GetParameterList(SyntaxNode ancestor)
        {
            switch (ancestor?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)ancestor).ParameterList;
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((ParenthesizedLambdaExpressionSyntax)ancestor).ParameterList;
                case SyntaxKind.AnonymousMethodExpression:
                    return ((AnonymousMethodExpressionSyntax)ancestor).ParameterList;
                default:
                    return null;
            }
        }
    }
}

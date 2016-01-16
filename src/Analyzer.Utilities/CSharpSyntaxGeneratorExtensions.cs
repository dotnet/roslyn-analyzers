using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer.Utilities
{
    internal static class CSharpSyntaxGeneratorExtensions
    {
        internal static SyntaxNode CSharpOperatorDeclaration(
            this SyntaxGenerator generator,
            SyntaxNode returnType,
            string operatorName,
            SyntaxNode[] parameters,
            SyntaxNode statement)
        {
            Debug.Assert(returnType is TypeSyntax);
            Debug.Assert(statement is StatementSyntax);

            SyntaxToken operatorToken;
            switch (operatorName)
            {
                case WellKnownMemberNames.EqualityOperatorName:
                    operatorToken = SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken);
                    break;
                case WellKnownMemberNames.InequalityOperatorName:
                    operatorToken = SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken);
                    break;
                case WellKnownMemberNames.LessThanOperatorName:
                    operatorToken = SyntaxFactory.Token(SyntaxKind.LessThanToken);
                    break;
                case WellKnownMemberNames.GreaterThanOperatorName:
                    operatorToken = SyntaxFactory.Token(SyntaxKind.GreaterThanToken);
                    break;
                default:
                    return null;
            }

            return SyntaxFactory.OperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword) }),
                (TypeSyntax)returnType,
                SyntaxFactory.Token(SyntaxKind.OperatorKeyword),
                operatorToken,
                SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters.Cast<ParameterSyntax>())),
                SyntaxFactory.Block((StatementSyntax)statement),
                default(SyntaxToken));
        }
    }
}

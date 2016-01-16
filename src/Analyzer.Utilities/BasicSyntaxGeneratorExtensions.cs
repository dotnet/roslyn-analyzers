using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Analyzer.Utilities
{
    internal static class BasicSyntaxGeneratorExtensions
    {
        internal static SyntaxNode BasicOperatorDeclaration(
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
                    operatorToken = SyntaxFactory.Token(SyntaxKind.EqualsToken);
                    break;
                case WellKnownMemberNames.InequalityOperatorName:
                    operatorToken = SyntaxFactory.Token(SyntaxKind.LessThanGreaterThanToken);
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

            OperatorStatementSyntax operatorStatement = SyntaxFactory.OperatorStatement(
                default(SyntaxList<AttributeListSyntax>),
                SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SharedKeyword) }),
                SyntaxFactory.Token(SyntaxKind.OperatorKeyword),
                operatorToken,
                SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters.Cast<ParameterSyntax>())),
                SyntaxFactory.SimpleAsClause((TypeSyntax)returnType));

            return SyntaxFactory.OperatorBlock(
                operatorStatement,
                SyntaxFactory.SingletonList(statement));
        }
    }
}

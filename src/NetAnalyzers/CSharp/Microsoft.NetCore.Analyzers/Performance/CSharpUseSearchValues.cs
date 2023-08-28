// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <inheritdoc/>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseSearchValuesAnalyzer : UseSearchValuesAnalyzer
    {
        // char[] myField = new char[] { 'a', 'b', 'c' };
        // char[] myField = new[] { 'a', 'b', 'c' };
        // byte[] myField = new[] { (byte)'a', (byte)'b', (byte)'c' };
        protected override bool IsConstantByteOrCharArrayVariableDeclaratorSyntax(SyntaxNode syntax, out int length)
        {
            length = 0;

            return
                syntax is VariableDeclaratorSyntax variableDeclarator &&
                variableDeclarator.Initializer?.Value is { } initializer &&
                IsConstantByteOrCharArrayCreationExpression(initializer, out length);
        }

        // ReadOnlySpan<char> myProperty => new char[] { 'a', 'b', 'c' };
        // ReadOnlySpan<char> myProperty => new[] { 'a', 'b', 'c' };
        // ReadOnlySpan<byte> myProperty => new[] { (byte)'a', (byte)'b', (byte)'c' };
        // ReadOnlySpan<byte> myProperty => "abc"u8;
        protected override bool IsConstantByteOrCharReadOnlySpanPropertyDeclarationSyntax(SyntaxNode syntax, out int length)
        {
            length = 0;

            return
                syntax is PropertyDeclarationSyntax propertyDeclaration &&
                propertyDeclaration.ExpressionBody?.Expression is { } expression &&
                (IsConstantByteOrCharArrayCreationExpression(expression, out length) || IsUtf8StringLiteralExpression(expression, out length));
        }

        // new char[] { 'a', 'b', 'c' };
        // new[] { 'a', 'b', 'c' };
        // new[] { (byte)'a', (byte)'b', (byte)'c' };
        private static bool IsConstantByteOrCharArrayCreationExpression(ExpressionSyntax expression, out int length)
        {
            length = 0;

            InitializerExpressionSyntax? arrayInitializer = null;

            if (expression is ArrayCreationExpressionSyntax arrayCreation)
            {
                arrayInitializer = arrayCreation.Initializer;
            }
            else if (expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
            {
                arrayInitializer = implicitArrayCreation.Initializer;
            }

            if (arrayInitializer?.Expressions is { } values)
            {
                foreach (var value in values)
                {
                    if (!IsByteOrCharLiteral(value))
                    {
                        return false;
                    }
                }

                length = values.Count;
                return true;
            }

            return false;

            static bool IsByteOrCharLiteral(ExpressionSyntax? value)
            {
                if (value is null)
                {
                    return false;
                }

                if (value.IsKind(SyntaxKind.CharacterLiteralExpression))
                {
                    return true;
                }

                return
                    value is CastExpressionSyntax cast &&
                    cast.Type is PredefinedTypeSyntax predefinedType &&
                    predefinedType.Keyword.IsKind(SyntaxKind.ByteKeyword) &&
                    cast.Expression.IsKind(SyntaxKind.CharacterLiteralExpression);
            }
        }

        private static bool IsUtf8StringLiteralExpression(ExpressionSyntax expression, out int length)
        {
            if (expression.IsKind(SyntaxKind.Utf8StringLiteralExpression) &&
                expression is LiteralExpressionSyntax literal &&
                literal.Token.IsKind(SyntaxKind.Utf8StringLiteralToken))
            {
                length = literal.Token.ValueText.Length;
                return true;
            }

            length = 0;
            return false;
        }

        protected override bool ArrayFieldUsesAreLikelyReadOnly(SyntaxNode syntax)
        {
            if (syntax is not VariableDeclaratorSyntax variableDeclarator ||
                syntax.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration)
            {
                return false;
            }

            string fieldName = variableDeclarator.Identifier.ValueText;

            // An optimistic implementation that only looks for simple assignments to the field or its array elements.
            foreach (var member in typeDeclaration.Members)
            {
                bool isCtor = member.IsKind(SyntaxKind.ConstructorDeclaration);

                foreach (var node in member.DescendantNodes())
                {
                    if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                        node is AssignmentExpressionSyntax assignment)
                    {
                        if (assignment.Left.IsKind(SyntaxKind.ElementAccessExpression))
                        {
                            if (assignment.Left is ElementAccessExpressionSyntax elementAccess &&
                                IsFieldReference(elementAccess.Expression, fieldName))
                            {
                                // s_array[42] = foo;
                                return false;
                            }
                        }
                        else if (isCtor)
                        {
                            if (IsFieldReference(assignment.Left, fieldName))
                            {
                                // s_array = foo;
                                return false;
                            }
                        }
                    }
                }
            }

            return true;

            static bool IsFieldReference(ExpressionSyntax expression, string fieldName) =>
                expression.IsKind(SyntaxKind.IdentifierName) &&
                expression is IdentifierNameSyntax identifierName &&
                identifierName.Identifier.ValueText == fieldName;
        }
    }
}
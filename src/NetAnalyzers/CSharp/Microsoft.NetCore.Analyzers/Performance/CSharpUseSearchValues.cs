// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
                IsConstantByteOrCharArrayCreationExpression(initializer, values: null, out length);
        }

        // ReadOnlySpan<char> myProperty => new char[] { 'a', 'b', 'c' };
        // ReadOnlySpan<char> myProperty => new[] { 'a', 'b', 'c' };
        // ReadOnlySpan<byte> myProperty => new[] { (byte)'a', (byte)'b', (byte)'c' };
        // ReadOnlySpan<char> myProperty { get => new[] { 'a', 'b', 'c' }; }
        // ReadOnlySpan<char> myProperty { get { return new[] { 'a', 'b', 'c' }; } }
        protected override bool IsConstantByteOrCharReadOnlySpanPropertyDeclarationSyntax(SyntaxNode syntax, out int length)
        {
            if (syntax is PropertyDeclarationSyntax propertyDeclaration &&
                TryGetPropertyGetterExpression(propertyDeclaration) is { } expression &&
                IsConstantByteOrCharArrayCreationExpression(expression, values: null, out length))
            {
                return true;
            }

            length = 0;
            return false;
        }

        internal static ExpressionSyntax? TryGetPropertyGetterExpression(PropertyDeclarationSyntax propertyDeclaration)
        {
            var expression = propertyDeclaration.ExpressionBody?.Expression;

            if (expression is null &&
                propertyDeclaration.AccessorList?.Accessors is [var accessor] &&
                accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
            {
                expression = accessor.ExpressionBody?.Expression;

                if (expression is null &&
                    accessor.Body?.Statements is [var statement] &&
                    statement is ReturnStatementSyntax returnStatement)
                {
                    expression = returnStatement.Expression;
                }
            }

            return expression;
        }

        // new char[] { 'a', 'b', 'c' };
        // new[] { 'a', 'b', 'c' };
        // new[] { (byte)'a', (byte)'b', (byte)'c' };
        internal static bool IsConstantByteOrCharArrayCreationExpression(ExpressionSyntax expression, List<char>? values, out int length)
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

            if (arrayInitializer?.Expressions is { } valueExpressions)
            {
                foreach (var valueExpression in valueExpressions)
                {
                    if (!TryGetByteOrCharLiteral(valueExpression, out char value))
                    {
                        return false;
                    }

                    values?.Add(value);
                }

                length = valueExpressions.Count;
                return true;
            }

            return false;

            // 'a' or (byte)'a'
            static bool TryGetByteOrCharLiteral(ExpressionSyntax? expression, out char value)
            {
                if (expression is not null)
                {
                    if (expression is CastExpressionSyntax cast &&
                        cast.Type is PredefinedTypeSyntax predefinedType &&
                        predefinedType.Keyword.IsKind(SyntaxKind.ByteKeyword))
                    {
                        expression = cast.Expression;
                    }

                    if (expression.IsKind(SyntaxKind.CharacterLiteralExpression) &&
                        expression is LiteralExpressionSyntax characterLiteral &&
                        characterLiteral.Token.Value is char charValue)
                    {
                        value = charValue;
                        return true;
                    }
                }

                value = default;
                return false;
            }
        }

        protected override bool ArrayFieldUsesAreLikelyReadOnly(SyntaxNode syntax)
        {
            if (syntax is not VariableDeclaratorSyntax variableDeclarator ||
                variableDeclarator.Identifier.Value is not string fieldName ||
                syntax.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration)
            {
                return false;
            }

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
                identifierName.Identifier.Value is string value &&
                value == fieldName;
        }
    }
}
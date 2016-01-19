// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer.Utilities
{
    public static class SyntaxGeneratorExtensions
    {
        private static readonly OperatorKind[] s_comparisonOperators =
        {
            OperatorKind.Equality,
            OperatorKind.Inequality,
            OperatorKind.GreaterThan,
            OperatorKind.GreaterThanOrEqual,
            OperatorKind.LessThan,
            OperatorKind.LessThanOrEqual
        };

        /// <summary>
        /// Creates a declaration for a comparison operator overload.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <param name="operatorKind">
        /// A value specifying which operator overload is to be declared. Must be one of
        /// Equality, Inequality, GreaterThan, GreaterThanOrEqual, LessThan, or LessThanOrEqual.
        /// </param>
        /// <param name="containingType">
        /// A symbol specifying the type of the operands of the comparison operator.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        /// <remarks>
        /// A comparison operator is a public, static (Shared in VB) method with two operands,
        /// each of the containing type, and a return type of bool (Boolean in VB).
        /// </remarks>
        public static SyntaxNode ComparisonOperatorDeclaration(this SyntaxGenerator generator, OperatorKind operatorKind, INamedTypeSymbol containingType)
        {
            if (!s_comparisonOperators.Contains(operatorKind))
            {
                throw new ArgumentException($"{operatorKind} is not a comparison operator", nameof(operatorKind));
            }

            return generator.OperatorDeclaration(
                operatorKind,
                new SyntaxNode[]
                {
                        generator.ParameterDeclaration("left", generator.TypeExpression(containingType)),
                        generator.ParameterDeclaration("right", generator.TypeExpression(containingType)),
                },
                generator.TypeExpression(SpecialType.System_Boolean),
                Accessibility.Public,
                DeclarationModifiers.Static,
                generator.DefaultMethodBody());
        }

        /// <summary>
        /// Creates a declaration for an override of <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode EqualsOverrideDeclaration(this SyntaxGenerator generator)
        {
            return generator.MethodDeclaration(
                WellKnownMemberNames.ObjectEquals,
                new SyntaxNode[]
                {
                    generator.ParameterDeclaration("obj", generator.TypeExpression(SpecialType.System_Object))
                },
                returnType: generator.TypeExpression(SpecialType.System_Boolean),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Override,
                statements: generator.DefaultMethodBody());
        }

        /// <summary>
        /// Creates a declaration for an override of <see cref="object.GetHashCode()"/>.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the declaration.
        /// </param>
        /// <returns>
        /// A <see cref="SyntaxNode"/> representing the declaration.
        /// </returns>
        public static SyntaxNode GetHashCodeOverrideDeclaration(this SyntaxGenerator generator)
        {
            return generator.MethodDeclaration(
                WellKnownMemberNames.ObjectGetHashCode,
                returnType: generator.TypeExpression(SpecialType.System_Int32),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Override,
                statements: generator.DefaultMethodBody());
        }

        /// <summary>
        /// Creates a default set of statements to place within a generated method body.
        /// </summary>
        /// <param name="generator">
        /// The <see cref="SyntaxGenerator"/> used to create the statements.
        /// </param>
        /// <returns>
        /// An sequence containing a single statement that throws <see cref="System.NotImplementedException"/>.
        /// </returns>
        private static IEnumerable<SyntaxNode> DefaultMethodBody(this SyntaxGenerator generator)
        {
            return new SyntaxNode[]
            {
                generator.ThrowStatement(generator.ObjectCreationExpression(generator.DottedName("System.NotImplementedException")))
            };
        }
    }
}

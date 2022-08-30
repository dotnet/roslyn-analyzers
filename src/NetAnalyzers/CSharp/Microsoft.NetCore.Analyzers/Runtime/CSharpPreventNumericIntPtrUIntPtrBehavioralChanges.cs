// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpPreventNumericIntPtrUIntPtrBehavioralChanges : PreventNumericIntPtrUIntPtrBehavioralChanges
    {
        private const string IntPtr = nameof(IntPtr);
        private const string UIntPtr = nameof(UIntPtr);

        protected override bool IsWithinCheckedContext(IOperation operation)
        {
            var parent = operation.Parent.Syntax;
            while (parent != null)
            {
                switch (parent)
                {
                    case CheckedExpressionSyntax expression:
                        return expression.Kind() == SyntaxKind.CheckedExpression;
                    case CheckedStatementSyntax statement:
                        return statement.Kind() == SyntaxKind.CheckedStatement;
                    case MethodDeclarationSyntax:
                        return false;
                }

                parent = parent.Parent;
            }

            return false;
        }

        protected override bool NotAlias(ImmutableArray<SyntaxReference> syntaxReferences)
        {
            foreach (SyntaxReference? reference in syntaxReferences)
            {
                SyntaxNode definition = reference.GetSyntax();

                while (definition is VariableDeclaratorSyntax)
                {
                    definition = definition.Parent;
                }

                var type = GetType(definition);

                if (IdentifierNameIsIntPtrOrUIntPtr(type))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IdentifierNameIsIntPtrOrUIntPtr(ExpressionSyntax? syntax) =>
            syntax is IdentifierNameSyntax identifierName &&
            identifierName.Identifier.Text is IntPtr or UIntPtr;

        protected override bool NotAlias(SyntaxNode syntax)
        {
            if (syntax is CastExpressionSyntax castSyntax)
            {
                if (IdentifierNameIsIntPtrOrUIntPtr(castSyntax.Expression) ||
                    IdentifierNameIsIntPtrOrUIntPtr(castSyntax.Type))
                {
                    return true;
                }
            }

            return false;
        }

        private static TypeSyntax? GetType(SyntaxNode syntax) =>
            syntax switch
            {
                VariableDeclarationSyntax fieldDeclaration => fieldDeclaration.Type,
                ParameterSyntax parameter => parameter.Type,
                CastExpressionSyntax cast => cast.Type,
                _ => null,
            };

    }
}

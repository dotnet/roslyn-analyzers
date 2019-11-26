// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.Analyzers.LeapYear
{
    public class DateKindSemanticModel
    {
        private readonly SemanticModel semanticModel;

        public DateKindSemanticModel(SemanticModel model)
        {
            this.semanticModel = model;
        }

        public IMethodSymbol GetConstructorSymbolInfo(ObjectCreationExpressionSyntax node)
        {
            SymbolInfo symbolInfo = this.semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol != null && symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol;
            }

            return null;
        }

        public TypeInfo GetNodeTypeInfo(SyntaxNode node)
        {
            return this.semanticModel.GetTypeInfo(node);
        }

        public Optional<object> GetConstantValue(SyntaxNode node)
        {
            return this.semanticModel.GetConstantValue(node);
        }

        public void FindIntegerVariableWithLastAssignedBinaryExpression(IdentifierNameSyntax node, DateKindContext context)
        {
            string typeString = this.semanticModel.GetTypeInfo(node).Type?.ToString();
            if (typeString == DateKindConstants.IntQualifiedName)
            {
                // We have detected a variable of type int being used in a date object for the year constructor argument.
                SymbolInfo symbolInfo = this.semanticModel.GetSymbolInfo(node);

                if (symbolInfo.Symbol != null && symbolInfo.Symbol.Kind == SymbolKind.Local)
                {
                    // For now we are retrieving the first declaration, possible issues with partial classes
                    // but we are currently only consuming a file at a time.
                    if (symbolInfo.Symbol.DeclaringSyntaxReferences[0].GetSyntax() is VariableDeclaratorSyntax variableDeclaration)
                    {
                        SyntaxNode containingCodeBlock = variableDeclaration.Ancestors().FirstOrDefault(x => x is BlockSyntax);

                        // The variable has at least been declared prior to the Date object constructor in the current context.
                        // We also check here for the latest assignment expression, if one exists.
                        AssignmentExpressionSyntax assignmentExpression = containingCodeBlock.DescendantNodes()
                            .OfType<AssignmentExpressionSyntax>()
                            .LastOrDefault(d =>
                            {
                                if (d.Left is IdentifierNameSyntax identifierName
                                    && identifierName.Identifier.ValueText == symbolInfo.Symbol.Name
                                    && d.GetLocation().GetLineSpan().EndLinePosition.Line < context.ObjectCreationExpression.GetLocation().GetLineSpan().StartLinePosition.Line)
                                {
                                    SymbolInfo assignmentIdentifierSymbolInfo = this.semanticModel.GetSymbolInfo(identifierName);

                                    return assignmentIdentifierSymbolInfo.Symbol.Equals(symbolInfo.Symbol);
                                }

                                return false;
                            });

                        // Check if the most recent expression includes a binary expression.
                        var variableBinaryExpression = assignmentExpression == null
                            ? variableDeclaration.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault()
                            : assignmentExpression.Right.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault();

                        if (variableBinaryExpression != null)
                        {
                            context.YearArgumentIdentifier = node;
                            context.YearArgumentIdentifierBinaryExpression = variableBinaryExpression;
                        }
                    }
                }
            }
        }
    }
}

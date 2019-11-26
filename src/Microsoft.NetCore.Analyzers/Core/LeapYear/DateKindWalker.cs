// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.LeapYear
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class DateKindWalker : CSharpSyntaxWalker
    {
        private readonly DateKindSemanticModel semanticModel;
        private readonly Stack<DateKindContext> currentContexts;
        private readonly List<DateKindContext> resolvedContexts;

        public DateKindWalker(DateKindSemanticModel model)
        {
            this.semanticModel = model;
            this.currentContexts = new Stack<DateKindContext>();
            this.resolvedContexts = new List<DateKindContext>();
        }

        public IEnumerable<DateKindContext> Dates => this.resolvedContexts;

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            // We are entering an object creation expression (i.e. `new Xyz(arg1, arg2, ...)`).
            DateKindContext context = this.CreateDateContext(node);
            this.currentContexts.Push(context);

            base.VisitObjectCreationExpression(node);

            this.currentContexts.Pop();
            if (context != null)
            {
                this.resolvedContexts.Add(context);
            }
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && context.ObjectCreationExpression.IsEquivalentTo(node.Parent))
            {
                // We are entering the argument list for the DateKind constructor.
                context.CurrentArgumentIdentifier = string.Empty;
            }

            base.VisitArgumentList(node);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && context.ObjectCreationExpression.ArgumentList.IsEquivalentTo(node.Parent)
                && context.CurrentArgumentIdentifier != null)
            {
                // We are visiting a direct argument for the DateKind constructor.
                if (node.NameColon != null)
                {
                    // This argument is named, store the identifier.
                    context.CurrentArgumentIdentifier = node.NameColon.Name.ToString();
                }
                else
                {
                    // This argument isn't named, so we can count on the index of it to be correct.
                    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments
                    // Named arguments, when used with positional arguments, are valid as long as
                    // - they're not followed by any positional arguments, or
                    // - starting with C# 7.2, they're used in the correct position.
                    // It's also hard to mix named and non named arguments for DateKind objects since
                    // the first 7 arguments are all the same type.
                    int indexOfCurrentArgument = context.ObjectCreationExpression.ArgumentList.Arguments.IndexOf(node);
                    if (indexOfCurrentArgument <= 2)
                    {
                        context.CurrentArgumentIdentifier = DateKindConstants.DateKindArgumentIdentifiers[indexOfCurrentArgument];
                    }
                    else
                    {
                        context.CurrentArgumentIdentifier = string.Empty;
                    }
                }
            }

            base.VisitArgument(node);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && this.IsSyntaxNodeAnArgumentForCurrentDateKindContext(node))
            {
                context.StoreArgumentExpression(node);
            }

            base.VisitMemberAccessExpression(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && this.IsSyntaxNodeAnArgumentForCurrentDateKindContext(node))
            {
                context.StoreArgumentExpression(node);

                if (context.CurrentArgumentIdentifier == DateKindConstants.YearParameterIdentifer)
                {
                    this.semanticModel.FindIntegerVariableWithLastAssignedBinaryExpression(node, context);
                }
            }

            base.VisitIdentifierName(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && this.IsSyntaxNodeAnArgumentForCurrentDateKindContext(node))
            {
                if (context.CurrentArgumentIdentifier == DateKindConstants.YearParameterIdentifer)
                {
                    // We have detected a triggering binary expression within the year constructor argument.
                    context.YearArgumentBinaryExpression = node;
                }

                if ((context.CurrentArgumentIdentifier == DateKindConstants.YearParameterIdentifer
                    || context.CurrentArgumentIdentifier == DateKindConstants.MonthParameterIdentifier
                    || context.CurrentArgumentIdentifier == DateKindConstants.DayParameterIndentifier)
                    && node.IsKind(SyntaxKind.ModuloExpression)
                    && !context.IgnoreDiagnostic)
                {
                    context.ShouldNotRaiseDiagnostic("Ignoring Year Increment issue due to detected modulo expression.");
                }
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            // TODO #513386 See if we can this for variables' last set value also.
            DateKindContext context = this.Peek();
            if (context != null
                && this.IsSyntaxNodeAnArgumentForCurrentDateKindContext(node))
            {
                if (node.IsKind(SyntaxKind.NumericLiteralExpression)
                    && (context.CurrentArgumentIdentifier == DateKindConstants.MonthParameterIdentifier
                        || context.CurrentArgumentIdentifier == DateKindConstants.DayParameterIndentifier))
                {
                    // Try to determine a value for the month or day arguments.
                    Optional<object> constantValue = this.semanticModel.GetConstantValue(node);
                    if (constantValue.HasValue
                        && constantValue.Value is int constantIntValue)
                    {
                        if (context.CurrentArgumentIdentifier == DateKindConstants.MonthParameterIdentifier)
                        {
                            context.MonthIntValue = constantIntValue;
                        }
                        else if (context.CurrentArgumentIdentifier == DateKindConstants.DayParameterIndentifier)
                        {
                            context.DayIntValue = constantIntValue;
                        }
                    }
                }
            }

            base.VisitLiteralExpression(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null
                && this.IsSyntaxNodeAnArgumentForCurrentDateKindContext(node))
            {
                if (context.CurrentArgumentIdentifier == DateKindConstants.YearParameterIdentifer)
                {
                    if (node.WhenTrue is BinaryExpressionSyntax trueBinaryExpression)
                    {
                        context.YearArgumentBinaryExpression = trueBinaryExpression;
                    }
                    else if (node.WhenFalse is BinaryExpressionSyntax falseBinaryExpression)
                    {
                        context.YearArgumentBinaryExpression = falseBinaryExpression;
                    }
                }

                if (node.WhenTrue.IsKind(SyntaxKind.SimpleMemberAccessExpression) || node.WhenTrue.IsKind(SyntaxKind.IdentifierName))
                {
                    context.StoreArgumentExpression(node.WhenTrue);
                }

                if (node.WhenFalse.IsKind(SyntaxKind.SimpleMemberAccessExpression) || node.WhenFalse.IsKind(SyntaxKind.IdentifierName))
                {
                    context.StoreArgumentExpression(node.WhenFalse);
                }
            }

            base.VisitConditionalExpression(node);
        }

        private DateKindContext CreateDateContext(ObjectCreationExpressionSyntax node)
        {
            DateKindContext context = null;
            IMethodSymbol methodSymbol = this.semanticModel.GetConstructorSymbolInfo(node);

            TypeInfo type = this.semanticModel.GetNodeTypeInfo(node);
            switch (type.Type?.ToString())
            {
                case DateKindConstants.DateTimeQualifiedName:
                    // All DateTime constructor overloads with 3 or more args start with the parameter signature
                    // `int year, int month, int day, ...`; these are the ones we are interested in.
                    if (node.ArgumentList.Arguments.Count >= 3)
                    {
                        context = new DateKindContext(node, methodSymbol);
                    }

                    break;
                case DateKindConstants.DateTimeOffsetQualifiedName:
                    // All DateTimeOffset constructor overloads with 7 or more args start with the parameter signature
                    // `int year, int month, int day, ...`; these are the ones we are interested in.
                    if (node.ArgumentList.Arguments.Count >= 7)
                    {
                        context = new DateKindContext(node, methodSymbol);
                    }

                    break;
            }

            return context;
        }

        private DateKindContext Peek()
        {
            if (this.currentContexts.Count == 0)
            {
                return null;
            }

            return this.currentContexts.Peek();
        }

        /// <summary>
        /// Returns true if the current context is an active DateKind constructor, the node
        /// is derived from an argument syntax, and it belongs to the same DateKind
        /// constructor as the current context.
        /// </summary>
        private bool IsSyntaxNodeAnArgumentForCurrentDateKindContext(ExpressionSyntax node)
        {
            DateKindContext context = this.Peek();
            if (context != null)
            {
                return context.CurrentArgumentIdentifier != null
                    && node.Parent.IsKind(SyntaxKind.Argument)
                    && context.ObjectCreationExpression.ArgumentList.IsEquivalentTo(node.Parent.Parent);
            }

            return false;
        }
    }
}

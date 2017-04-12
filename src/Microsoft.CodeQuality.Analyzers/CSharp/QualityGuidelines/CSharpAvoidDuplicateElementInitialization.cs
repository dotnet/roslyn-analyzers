// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpAvoidDuplicateElementInitialization : AvoidDuplicateElementInitialization
    {
        private static readonly SyntaxGenerator s_generator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            analysisContext.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectInitializerExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var objectInitializer = (InitializerExpressionSyntax)context.Node;
            var initializedElementIndexes = new HashSet<object[]>(ConstantArgumentEqualityComparer.Instance);

            foreach (var intializationExpression in objectInitializer.Expressions.OfType<AssignmentExpressionSyntax>())
            {
                if (intializationExpression.Left is ImplicitElementAccessSyntax elementAccess)
                {
                    var argumentList = elementAccess.ArgumentList;
                    var values = TryGetResolvedArgumentValues(argumentList, context);
                    if (values != null && !initializedElementIndexes.Add(values))
                    {
                        var indexesText = string.Join(", ", values.Select(value => s_generator.LiteralExpression(value)));
                        context.ReportDiagnostic(Diagnostic.Create(Rule, argumentList.GetLocation(), indexesText));
                    }
                }

                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private static int GetParameterIndex(ImmutableArray<IParameterSymbol> parameters, string parameterName)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name.Equals(parameterName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static int GetParameterIndex(ImmutableArray<IParameterSymbol> parameters, ArgumentSyntax argument, int argumentIndex)
        {
            if (argument.NameColon != null)
            {
                return GetParameterIndex(parameters, GetParameterName(argument));
            }

            if (argumentIndex < parameters.Length)
            {
                return argumentIndex;
            }

            return -1;
        }

        private static string GetParameterName(ArgumentSyntax argument)
        {
            var name = argument.NameColon.Name.ToString();
            if (name.Length != 0 && name[0] == '@')
            {
                return name.Substring(1);
            }
            return name;
        }

        private static object[] TryGetResolvedArgumentValues(BracketedArgumentListSyntax node, SyntaxNodeAnalysisContext context)
        {
            var propertySymbol = context.SemanticModel.GetSymbolInfo(node.Parent, context.CancellationToken).Symbol as IPropertySymbol;
            if (propertySymbol == null)
            {
                return null;
            }

            var parameters = propertySymbol.Parameters;
            var usedParameterIndexes = new HashSet<int>();
            var result = new object[parameters.Length];
            for (int argumentIndex = 0; argumentIndex < node.Arguments.Count; argumentIndex++)
            {
                var argument = node.Arguments[argumentIndex];
                var constant = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken);
                if (!constant.HasValue)
                {
                    return null;
                }

                int parameterIndex = GetParameterIndex(parameters, argument, argumentIndex);
                if (parameterIndex < 0 || !usedParameterIndexes.Add(parameterIndex))
                {
                    return null;
                }
                
                result[parameterIndex] = constant.Value;
            }

            if (usedParameterIndexes.Count < parameters.Length)
            {
                for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    if (usedParameterIndexes.Add(parameterIndex))
                    {
                        var parameter = parameters[parameterIndex];
                        if (!parameter.HasExplicitDefaultValue)
                        {
                            return null;
                        }
                        result[parameterIndex] = parameter.ExplicitDefaultValue;
                    }
                }
            }

            return result;
        }

        private sealed class ConstantArgumentEqualityComparer : IEqualityComparer<object[]>
        {
            public static readonly ConstantArgumentEqualityComparer Instance = new ConstantArgumentEqualityComparer();

            private readonly EqualityComparer<object> _objectComparer = EqualityComparer<object>.Default;

            private ConstantArgumentEqualityComparer() { }

            bool IEqualityComparer<object[]>.Equals(object[] x, object[] y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x == null || y == null || x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (!_objectComparer.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            int IEqualityComparer<object[]>.GetHashCode(object[] obj)
            {
                int hash = 0;
                foreach (var item in obj)
                {
                    hash = unchecked((hash * (int)0xA5555529) + _objectComparer.GetHashCode(item));
                }
                return hash;
            }
        }
    }
}
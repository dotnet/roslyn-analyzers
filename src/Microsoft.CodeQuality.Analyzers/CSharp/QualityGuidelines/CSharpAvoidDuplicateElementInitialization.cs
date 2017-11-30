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
using Microsoft.CodeAnalysis.Operations;

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

            analysisContext.RegisterOperationAction(AnalyzeOperation, OperationKind.ObjectCreation);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            var objectInitializer = (IObjectCreationOperation)context.Operation;
            var semanticModel = context.Compilation.GetSemanticModel(objectInitializer.Syntax.SyntaxTree);
            var initializedElementIndexes = new HashSet<object[]>(ConstantArgumentEqualityComparer.Instance);

            foreach (var initializer in objectInitializer.Initializer.Initializers)
            {
                if (initializer is ISimpleAssignmentOperation assignment &&
                    assignment.Target is IPropertyReferenceOperation propertyReference &&
                    propertyReference.Arguments.Length != 0 &&
                    ((AssignmentExpressionSyntax)assignment.Syntax).Left is ImplicitElementAccessSyntax elementAccess)
                {
                    var values = GetConstantArgumentValues(propertyReference.Arguments, semanticModel, context);
                    if (values != null && !initializedElementIndexes.Add(values))
                    {
                        var indexesText = string.Join(", ", values.Select(value => s_generator.LiteralExpression(value)));
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, elementAccess.ArgumentList.GetLocation(), indexesText));
                    }
                }

                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the argument values in parameter order, filling in defaults if necessary, if all
        /// arguments are constants. Otherwise, returns null.
        /// </summary>
        private static object[] GetConstantArgumentValues(
            ImmutableArray<IArgumentOperation> arguments,
            SemanticModel semanticModel,
            OperationAnalysisContext context)
        {
            var result = new object[arguments.Length];
            foreach(var argument in arguments)
            {
                var parameter = argument.Parameter;
                if (parameter == null ||
                    parameter.Ordinal >= result.Length ||
                    !TryGetConstantValue(argument, out result[parameter.Ordinal]))
                {
                    return null;
                }
            }
            return result;

            bool TryGetConstantValue(IArgumentOperation argument, out object value)
            {
                var parameter = argument.Parameter;
                switch (argument.ArgumentKind)
                {
                    case ArgumentKind.DefaultValue:
                        value = parameter.ExplicitDefaultValue;
                        return parameter.HasExplicitDefaultValue;
                    case ArgumentKind.Explicit:
                        var constantValue = semanticModel.GetConstantValue(
                            ((ArgumentSyntax)argument.Syntax).Expression,
                            context.CancellationToken);
                        value = constantValue.Value;
                        return constantValue.HasValue;
                    default:
                        value = null;
                        return false;
                }
            }
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
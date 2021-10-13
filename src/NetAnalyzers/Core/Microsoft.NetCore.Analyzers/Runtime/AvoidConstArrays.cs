// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA1850: Avoid constant arrays as arguments. Replace with static readonly arrays.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidConstArraysAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1850";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            CreateLocalizableResourceString(AvoidConstArraysTitle),
            CreateLocalizableResourceString(AvoidConstArraysMessage),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(AvoidConstArraysDescription),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol? readonlySpanType = compilationContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1);
                INamedTypeSymbol? functionType = compilationContext.Compilation.GetOrCreateTypeByMetadataName("System.Func`2");
                if (readonlySpanType is null || functionType is null)
                {
                    return;
                }

                // Analyzes an argument operation
                compilationContext.RegisterOperationAction(operationContext =>
                {
                    bool isDirectlyInsideLambda = false;
                    IArgumentOperation? argumentOperation;

                    if (operationContext.Operation is IArrayCreationOperation arrayCreationOperation) // For arrays passed as arguments
                    {
                        argumentOperation = arrayCreationOperation.GetAncestor<IArgumentOperation>(OperationKind.Argument);
                        if (argumentOperation is null)
                        {
                            return;
                        }
                    }
                    else if (operationContext.Operation is IInvocationOperation invocationOperation) // For arrays passed in extension methods, like in LINQ
                    {
                        if (invocationOperation.Descendants().Any(x => x is IArrayCreationOperation)
                            && invocationOperation.Descendants().Any(x => x is IArgumentOperation))
                        {
                            // This is an invocation that contains an array as an argument
                            // This will get caught by the first case in another cycle
                            return;
                        }

                        argumentOperation = invocationOperation.Arguments.FirstOrDefault();
                        if (argumentOperation is not null)
                        {
                            if (argumentOperation.Children.First() is not IConversionOperation conversionOperation
                                || conversionOperation.Operand is not IArrayCreationOperation arrayCreation)
                            {
                                return;
                            }
                            arrayCreationOperation = arrayCreation;
                        }
                        else // An invocation, extension or regular, has an argument, unless it's a VB extension method call
                        {
                            // For VB extension method invocations, find a matching child
                            arrayCreationOperation = (IArrayCreationOperation)invocationOperation.Descendants()
                                .FirstOrDefault(x => x is IArrayCreationOperation);
                            if (arrayCreationOperation is null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (argumentOperation is not null)
                    {
                        ITypeSymbol originalDefinition = argumentOperation.Parameter.Type.OriginalDefinition;

                        // Can't be a ReadOnlySpan, as those are already optimized
                        if (originalDefinition.Equals(readonlySpanType))
                        {
                            return;
                        }

                        // Check if the parameter is a function so the name can be set to null
                        // Otherwise, the parameter name doesn't reflect the array creation as well
                        isDirectlyInsideLambda = originalDefinition.Equals(functionType);
                    }

                    // Must be literal array
                    if (arrayCreationOperation.Initializer.ElementValues.Any(x => x is not ILiteralOperation))
                    {
                        return;
                    }

                    Dictionary<string, string?> properties = new()
                    {
                        { "paramName", isDirectlyInsideLambda ? null : argumentOperation?.Parameter?.Name }
                    };

                    operationContext.ReportDiagnostic(arrayCreationOperation.CreateDiagnostic(Rule, properties.ToImmutableDictionary()));
                },
                OperationKind.ArrayCreation,
                OperationKind.Invocation);
            });
        }
    }
}
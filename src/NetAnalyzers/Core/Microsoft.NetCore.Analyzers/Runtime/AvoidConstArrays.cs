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
    /// CA1854: Avoid constant arrays as arguments. Replace with static readonly arrays.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidConstArraysAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1854";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            CreateLocalizableResourceString(nameof(AvoidConstArraysTitle)),
            CreateLocalizableResourceString(nameof(AvoidConstArraysMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(AvoidConstArraysDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                INamedTypeSymbol? readonlySpanType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1);
                INamedTypeSymbol? functionType = context.Compilation.GetOrCreateTypeByMetadataName("System.Func`2");

                // Analyzes an argument operation
                context.RegisterOperationAction(context =>
                {
                    bool isDirectlyInsideLambda = false;
                    IArgumentOperation? argumentOperation;

                    if (context.Operation is IArrayCreationOperation arrayCreationOperation) // For arrays passed as arguments
                    {
                        argumentOperation = arrayCreationOperation.GetAncestor<IArgumentOperation>(OperationKind.Argument);
                        if (argumentOperation is null)
                        {
                            return;
                        }
                    }
                    else if (context.Operation is IInvocationOperation invocationOperation) // For arrays passed in extension methods, like in LINQ
                    {
                        IEnumerable<IOperation> invocationDescendants = invocationOperation.Descendants();
                        if (invocationDescendants.Any(x => x is IArrayCreationOperation)
                            && invocationDescendants.Any(x => x is IArgumentOperation))
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
                        if (readonlySpanType is not null && originalDefinition.Equals(readonlySpanType))
                        {
                            return;
                        }

                        // Check if the parameter is a function so the name can be set to null
                        // Otherwise, the parameter name doesn't reflect the array creation as well
                        if (functionType is not null)
                        {
                            isDirectlyInsideLambda = originalDefinition.Equals(functionType);
                        }
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

                    context.ReportDiagnostic(arrayCreationOperation.CreateDiagnostic(Rule, properties.ToImmutableDictionary()));
                },
                OperationKind.ArrayCreation,
                OperationKind.Invocation);
            });
        }
    }
}

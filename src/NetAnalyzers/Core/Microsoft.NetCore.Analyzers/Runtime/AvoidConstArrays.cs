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
    /// <summary>
    /// CA1850: Avoid constant arrays as arguments. Replace with static readonly arrays.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidConstArraysAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1850";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Analyzes an argument operation
            context.RegisterOperationAction(operationContext =>
            {
                IArgumentOperation? argumentOperation;
                INamedTypeSymbol readonlySpanType = WellKnownTypeProvider.GetOrCreate(operationContext.Compilation)
                    .GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1)!;

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

                // Can't be a ReadOnlySpan, as those are already optimized
                if (argumentOperation is not null && argumentOperation.Parameter.Type.OriginalDefinition.Equals(readonlySpanType))
                {
                    return;
                }

                // Must be literal array
                if (!arrayCreationOperation.Initializer.ElementValues.All(x => x is ILiteralOperation))
                {
                    return;
                }

                Dictionary<string, string?> properties = new()
                {
                    { "paramName", argumentOperation?.Parameter?.Name }
                };

                operationContext.ReportDiagnostic(arrayCreationOperation.CreateDiagnostic(Rule, properties.ToImmutableDictionary()));
            },
            OperationKind.ArrayCreation,
            OperationKind.Invocation);
        }
    }
}
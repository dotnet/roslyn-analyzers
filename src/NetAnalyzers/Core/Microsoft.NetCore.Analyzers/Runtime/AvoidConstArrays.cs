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
    /// CA1839: Avoid const arrays. Replace with static readonly arrays.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidConstArraysAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1839";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.AvoidConstArraysDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableDescription,
            isPortedFxCopRule: true,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            // Analyzes an argument operation
            context.RegisterOperationAction(operationContext =>
            {
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
                    if (invocationOperation.Descendants().Any(x => x is IArrayCreationOperation))
                    {
                        // This is an invocation that contains an array in it
                        // This will get caught by the first case in another cycle
                        return;
                    }

                    argumentOperation = invocationOperation.Arguments.FirstOrDefault();
                    if (argumentOperation is null || argumentOperation.Children.First() is not IConversionOperation conversionOperation
                        || conversionOperation.Operand is not IArrayCreationOperation arrayCreation)
                    {
                        return;
                    }
                    arrayCreationOperation = arrayCreation;
                }
                else
                {
                    return;
                }

                // Must be literal array
                if (!arrayCreationOperation.Children.First(x => x is IArrayInitializerOperation).Children.All(x => x is ILiteralOperation))
                {
                    return;
                }

                Dictionary<string, string?> properties = new()
                {
                    { "paramName", argumentOperation.Parameter.Name }
                };

                operationContext.ReportDiagnostic(arrayCreationOperation.CreateDiagnostic(Rule, properties.ToImmutableDictionary()));
            },
            OperationKind.ArrayCreation,
            OperationKind.Invocation);
        }
    }
}
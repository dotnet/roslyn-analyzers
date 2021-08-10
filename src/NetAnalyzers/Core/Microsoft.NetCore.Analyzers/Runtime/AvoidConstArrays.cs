// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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

        // Note that this analyzer doesn't analyze local variables that are only referenced once ever, when passed as arguments,
        // as cleaning up useless allocations is not in the scope of this analyzer
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            // Analyzes an argument operation
            context.RegisterOperationAction(operationContext =>
            {
                if (operationContext.Operation is IArrayCreationOperation arrayCreationOperation
                    && arrayCreationOperation.GetAncestor<IArgumentOperation>(OperationKind.Argument) != null)
                {
                    var test = "test";
                }

                if (operationContext.Operation is IArgumentOperation argumentOperation)
                {
                    if (argumentOperation.Value.Type.TypeKind != TypeKind.Array // Check that argument is an array
                        || argumentOperation.Value.Kind != OperationKind.Literal // Must be literal array
                        || argumentOperation.ArgumentKind != ArgumentKind.Explicit) // Must be explicitly declared
                    {
                        return;
                    }

                    ImmutableDictionary<string, string?> properties = ImmutableDictionary.Create<string, string?>();
                    properties.Add("matchingParameter", argumentOperation.Parameter.Name);

                    // Report diagnostic from argument context rather than argument.Value context
                    operationContext.ReportDiagnostic(argumentOperation.CreateDiagnostic(Rule, properties));
                }
                // else if (operationContext.Operation is IArrayCreationOperation arrayCreationOperation)
                // {
                //     if (arrayCreationOperation.Kind != OperationKind.Literal) // Must be literal array
                //     {
                //         return;
                //     }

                //     operationContext.ReportDiagnostic(arrayCreationOperation.CreateDiagnostic(Rule));
                // }
                // else if (operationContext.Operation is IArrayInitializerOperation arrayInitializerOperation)
                // {
                //     if (arrayInitializerOperation.Kind != OperationKind.Literal) // Must be literal array
                //     {
                //         return;
                //     }

                //     operationContext.ReportDiagnostic(arrayInitializerOperation.CreateDiagnostic(Rule));
                // }
                // else if (operationContext.Operation is ILiteralOperation literalOperation)
                // {
                //     if (literalOperation.Type.TypeKind != TypeKind.Array) // Must be literal array
                //     {
                //         return;
                //     }

                //     operationContext.ReportDiagnostic(literalOperation.CreateDiagnostic(Rule));
                // }
            },
            OperationKind.Argument,
            OperationKind.ArrayCreation,
            OperationKind.ArrayInitializer,
            OperationKind.Literal,
            OperationKind.Invocation,
            OperationKind.ExpressionStatement, // Hopefully not necessary
            OperationKind.Block, // Hopefully not necessary
            OperationKind.MethodBody); // Hopefully not necessary
        }
    }
}
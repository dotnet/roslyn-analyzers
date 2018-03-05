// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.ParameterValidationAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ValidateArgumentsOfPublicMethods : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1062";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftDesignAnalyzersResources.ValidateArgumentsOfPublicMethodsTitle), MicrosoftDesignAnalyzersResources.ResourceManager, typeof(MicrosoftDesignAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftDesignAnalyzersResources.ValidateArgumentsOfPublicMethodsMessage), MicrosoftDesignAnalyzersResources.ResourceManager, typeof(MicrosoftDesignAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftDesignAnalyzersResources.ValidateArgumentsOfPublicMethodsDescription), MicrosoftDesignAnalyzersResources.ResourceManager, typeof(MicrosoftDesignAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1062-validate-arguments-of-public-methods",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    // Analyze externally visible methods with reference type parameters.
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod) ||
                        !containingMethod.IsExternallyVisible() ||
                        !containingMethod.Parameters.Any(p => p.Type.IsReferenceType))
                    {
                        return;
                    }

                    // Bail out early if we have no parameter references in the method body. 
                    if (!operationBlockContext.OperationBlocks.HasAnyOperationDescendant<IParameterReferenceOperation>())
                    {
                        return;
                    }

                    // Perform analysis of all direct/indirect parameter usages in the method to get all non-validated usages that can cause a null dereference.
                    ImmutableDictionary<IParameterSymbol, SyntaxNode> hazardousParameterUsages = null;
                    foreach (var operationBlock in operationBlockContext.OperationBlocks)
                    {
                        if (operationBlock is IBlockOperation topmostBlock)
                        {
                            hazardousParameterUsages = ParameterValidationAnalysis.GetOrComputeHazardousParameterUsages(topmostBlock, operationBlockContext.Compilation, containingMethod);
                            break;
                        }
                    }

                    if (hazardousParameterUsages != null)
                    {
                        foreach (var kvp in hazardousParameterUsages)
                        {
                            IParameterSymbol parameter = kvp.Key;
                            SyntaxNode node = kvp.Value;

                            // In externally visible method '{0}', validate parameter '{1}' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
                            var arg1 = containingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            var arg2 = parameter.Name;
                            var diagnostic = node.CreateDiagnostic(Rule, arg1, arg2);
                            operationBlockContext.ReportDiagnostic(diagnostic);
                        }
                    }
                });

            });
        }
    }
}

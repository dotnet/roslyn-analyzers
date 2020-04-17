// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Prefer string.Contains over string.IndexOf when the result is compared to -1
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferStringContainsOverIndexOfAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1834";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
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
            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Check that the object is a string
                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemString, out INamedTypeSymbol? stringType))
                {
                    return;
                }
                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemChar, out INamedTypeSymbol? charType))
                {
                    return;
                }
                if (!compilationContext.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
                {
                    return;
                }

                // First get all the string.IndexOf methods that we are interested in tagging
                var stringIndexOfMethods = stringType
                    .GetMembers("IndexOf")
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(s =>
                        s.Parameters.Length <= 2);

                var stringArgumentIndexOfMethod = stringIndexOfMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(stringType));
                var charArgumentIndexOfMethod = stringIndexOfMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(charType));
                var stringAndComparisonTypeArgumentIndexOfMethod = stringIndexOfMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(stringType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));
                var charAndComparisonTypeArgumentIndexOfMethod = stringIndexOfMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(charType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    IBinaryOperation blockOperation = (IBinaryOperation)operationContext.Operation;
                    // Check that the right hand side is a -1
                    var rightOperand = blockOperation.RightOperand;
                    if (rightOperand is IUnaryOperation unaryOperation && rightOperand.ConstantValue.HasValue && rightOperand.ConstantValue.Value is int intValue && intValue == -1)
                    {
                        var leftOperand = blockOperation.LeftOperand;
                        var blockLocation = blockOperation.Syntax.GetLocation();
                        if (leftOperand is ILocalReferenceOperation localReferenceOperation)
                        {
                            var variableName = localReferenceOperation.Local;
                            SyntaxReference declaration = variableName.DeclaringSyntaxReferences.FirstOrDefault();
                            if (declaration is null)
                            {
                                return;
                            }

                            var semanticModel = operationContext.Operation.SemanticModel;
                            SyntaxNode declarationNode = declaration.GetSyntax(operationContext.CancellationToken);
                            var operation = semanticModel.GetOperation(declarationNode, operationContext.CancellationToken);
                            if (operation is IVariableDeclaratorOperation variableDeclaratorOperation)
                            {
                                if (variableDeclaratorOperation.Parent is IVariableDeclarationOperation variableDeclarationOperation)
                                {
                                    if (variableDeclarationOperation.Parent is IVariableDeclarationGroupOperation declarationGroupOperation)
                                    {
                                        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(declarationGroupOperation.Syntax);
                                        if (dataFlowAnalysis.WrittenOutside.Contains(variableName))
                                        {
                                            return;
                                        }

                                        // Check that the variable is initialized to the result of string.IndexOf
                                        IVariableInitializerOperation variableInitializer = variableDeclaratorOperation.GetVariableInitializer();
                                        if (variableInitializer is null)
                                        {
                                            return;
                                        }
                                        var declarationGroupLocation = declarationGroupOperation.Syntax.GetLocation();
                                        var locations = ImmutableArray.Create(blockLocation, declarationGroupLocation);

                                        if (variableInitializer.Value is IInvocationOperation invocationOperation)
                                        {
                                            HandleInvocationOperation(invocationOperation, operationContext, locations);
                                        }
                                    }
                                }
                            }
                        }
                        else if (leftOperand is IInvocationOperation leftInvocationOperation)
                        {
                            var locations = ImmutableArray.Create(blockLocation);
                            HandleInvocationOperation(leftInvocationOperation, operationContext, locations);
                        }
                    }
                },
                OperationKind.Binary);

                void HandleInvocationOperation(IInvocationOperation invocationOperation, OperationAnalysisContext operationContext, ImmutableArray<Location> locations)
                {
                    var targetMethod = invocationOperation.TargetMethod;
                    if (targetMethod.Equals(stringArgumentIndexOfMethod) || targetMethod.Equals(charArgumentIndexOfMethod) || targetMethod.Equals(stringAndComparisonTypeArgumentIndexOfMethod) || targetMethod.Equals(charAndComparisonTypeArgumentIndexOfMethod))
                    {
                        operationContext.ReportDiagnostic(locations.CreateDiagnostic(Rule));
                    }
                }
            });
        }

    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
    /// Test for single character strings passed in to String.Append
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

                var overloadMapBuilder = ImmutableDictionary.CreateBuilder<IMethodSymbol, IMethodSymbol>();
                compilationContext.RegisterOperationAction(operationContext =>
                {
                    IInvocationOperation invocationOperation = (IInvocationOperation)operationContext.Operation;
                    var targetMethod = invocationOperation.TargetMethod;
                    //if (targetMethod.Equals(stringArgumentIndexOfMethod) || targetMethod.Equals(charArgumentIndexOfMethod) || targetMethod.Equals(stringAndComparisonTypeArgumentIndexOfMethod) || targetMethod.Equals(charAndComparisonTypeArgumentIndexOfMethod))
                    //{
                    //    operationContext.ReportDiagnostic(invocationOperation.CreateDiagnostic(Rule));
                    //}

                    // Get the variable declarator from the invocation.
                    if (invocationOperation.Parent is IVariableInitializerOperation variableInitializerOperation)
                    {
                        if (variableInitializerOperation.Parent is IVariableDeclaratorOperation variableDeclaratorOperation)
                        {
                            var variableName = variableDeclaratorOperation.Symbol;
                        }
                    }
                },
                OperationKind.Invocation);

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    IBinaryOperation blockOperation = (IBinaryOperation)operationContext.Operation;
                    // Check that the right hand side is a -1
                    var rightOperand = blockOperation.RightOperand;
                    if (rightOperand is IUnaryOperation unaryOperation && rightOperand.ConstantValue.HasValue && rightOperand.ConstantValue.Value is int intValue && intValue == -1)
                    {
                        var leftOperand = blockOperation.LeftOperand;
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

                                        operationContext.ReportDiagnostic(blockOperation.CreateDiagnostic(Rule));
                                    }
                                }
                            }
                        }
                    }
                },
                OperationKind.Binary);
            });
        }
    }
}

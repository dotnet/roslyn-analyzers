// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Prefer string.Contains over string.IndexOf when the result is compared to 1 or -1
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferStringContainsOverIndexOfAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2248";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferStringContainsOverIndexOfDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Usage,
                                                                                      RuleLevel.IdeSuggestion,
                                                                                      s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemString, out INamedTypeSymbol? stringType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemChar, out INamedTypeSymbol? charType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
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

                // Check that the contains methods that take 2 parameters exist
                // string.Contains(char) is also .NETStandard2.1+
                var stringContainsMethods = stringType
                    .GetMembers("Contains")
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(s =>
                        s.Parameters.Length <= 2);
                var stringAndComparisonTypeArgumentContainsMethod = stringContainsMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(stringType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));
                var charAndComparisonTypeArgumentContainsMethod = stringContainsMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(charType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));
                var charArgumentContainsMethod = stringContainsMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(charType));
                if (stringAndComparisonTypeArgumentContainsMethod == null ||
                    charAndComparisonTypeArgumentContainsMethod == null ||
                    charArgumentContainsMethod == null)
                {
                    return;
                }

                // Roslyn doesn't yet support "FindAllReferences" at a file/block level. So instead, find references to local int variables in this block.
                context.RegisterOperationBlockStartAction(OnOperationBlockStart);

                void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
                {
                    if (!(context.OwningSymbol is IMethodSymbol method))
                    {
                        return;
                    }

                    PooledConcurrentSet<ISymbol> localsToBailOut = PooledConcurrentSet<ISymbol>.GetInstance();
                    PooledConcurrentDictionary<ISymbol, IInvocationOperation> variableNameToOperationsMap = PooledConcurrentDictionary<ISymbol, IInvocationOperation>.GetInstance();

                    context.RegisterOperationAction(PopulateLocalReferencesSet, OperationKind.LocalReference);

                    context.RegisterOperationAction(AnalyzeInvocationOperation, OperationKind.Invocation);

                    context.RegisterOperationBlockEndAction(OnOperationBlockEnd);

                    // Local Functions
                    void PopulateLocalReferencesSet(OperationAnalysisContext context)
                    {
                        ILocalReferenceOperation localReference = (ILocalReferenceOperation)context.Operation;
                        if (localReference.Local.Type.SpecialType != SpecialType.System_Int32)
                        {
                            return;
                        }

                        var parent = localReference.Parent;
                        if (!(parent is IBinaryOperation binaryOperation))
                        {
                            localsToBailOut.Add(localReference.Local);
                        }
                    }

                    void AnalyzeInvocationOperation(OperationAnalysisContext context)
                    {
                        var invocationOperation = (IInvocationOperation)context.Operation;
                        if (!IsDesiredTargetMethod(invocationOperation.TargetMethod))
                        {
                            return;
                        }

                        var parent = invocationOperation.Parent;
                        if (parent is IBinaryOperation blockOperation)
                        {
                            var operatorKind = blockOperation.OperatorKind;
                            if (operatorKind != BinaryOperatorKind.Equals && operatorKind != BinaryOperatorKind.GreaterThanOrEqual)
                            {
                                return;
                            }

                            var rightOperand = blockOperation.RightOperand;
                            if (rightOperand.ConstantValue.HasValue && rightOperand.ConstantValue.Value is int intValue && (intValue == -1 || intValue == 0))
                            {
                                context.ReportDiagnostic(blockOperation.CreateDiagnostic(Rule));
                            }
                        }
                        else if (parent is IVariableInitializerOperation variableInitializer)
                        {
                            if (variableInitializer.Parent is IVariableDeclaratorOperation variableDeclaratorOperation)
                            {
                                variableNameToOperationsMap.TryAdd(variableDeclaratorOperation.Symbol, invocationOperation);
                            }
                            else if (variableInitializer.Parent is IVariableDeclarationOperation variableDeclarationOperation)
                            {
                                variableNameToOperationsMap.TryAdd(variableDeclarationOperation.Declarators[0].Symbol, invocationOperation);
                            }
                        }
                    }

                    void OnOperationBlockEnd(OperationBlockAnalysisContext context)
                    {
                        if (variableNameToOperationsMap.Count > 0)
                        {
                            foreach (KeyValuePair<ISymbol, IInvocationOperation> variableNameAndLocation in variableNameToOperationsMap)
                            {
                                ISymbol variable = variableNameAndLocation.Key;
                                if (!localsToBailOut.Contains(variable))
                                {
                                    context.ReportDiagnostic(variableNameAndLocation.Value.CreateDiagnostic(Rule));
                                }
                            }
                        }
                        variableNameToOperationsMap.Free();
                    }

                    bool IsDesiredTargetMethod(IMethodSymbol targetMethod) =>
                         targetMethod.Equals(stringArgumentIndexOfMethod)
                         || targetMethod.Equals(charArgumentIndexOfMethod)
                         || targetMethod.Equals(stringAndComparisonTypeArgumentIndexOfMethod)
                         || targetMethod.Equals(charAndComparisonTypeArgumentIndexOfMethod);
                }

            });
        }
    }
}

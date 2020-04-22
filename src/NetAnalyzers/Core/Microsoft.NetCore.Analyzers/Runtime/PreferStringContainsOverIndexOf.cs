// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
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
            context.RegisterCompilationStartAction(compilation =>
            {
                if (!compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemString, out INamedTypeSymbol? stringType) ||
                    !compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemChar, out INamedTypeSymbol? charType) ||
                    !compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
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
                var stringContainsMethods = stringType
                    .GetMembers("Contains")
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(s =>
                        s.Parameters.Length == 2);
                var stringAndComparisonTypeArgumentContainsMethod = stringContainsMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(stringType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));
                var charAndComparisonTypeArgumentContainsMethod = stringContainsMethods.GetFirstOrDefaultMemberWithParameterInfos(
                        ParameterInfo.GetParameterInfo(charType),
                        ParameterInfo.GetParameterInfo(stringComparisonType));
                if (stringAndComparisonTypeArgumentContainsMethod == null ||
                    charAndComparisonTypeArgumentContainsMethod == null)
                {
                    return;
                }

                // Roslyn doesn't yet support "FindAllReferences" at a file/block level. So instead, find references to local variables in this block.
                compilation.RegisterOperationBlockStartAction(context =>
                {
                    ISymbol symbol = context.OwningSymbol;
                    if (!(context.OwningSymbol is IMethodSymbol method))
                    {
                        return;
                    }

                    List<Location>? leftOperandInvocationLocations = null;
                    PooledConcurrentDictionary<string, int> variableNameToNumberOfReferences = PooledConcurrentDictionary<string, int>.GetInstance();
                    PooledConcurrentDictionary<string, ImmutableArray<Location>> variableNameToLocationsMap = PooledConcurrentDictionary<string, ImmutableArray<Location>>.GetInstance();

                    context.RegisterOperationAction(context =>
                    {
                        ILocalReferenceOperation localReference = (ILocalReferenceOperation)context.Operation;
                        ILocalSymbol symbol = localReference.Local;
                        variableNameToNumberOfReferences.AddOrUpdate(symbol.Name, 1, (name, addValue) =>
                        {
                            return addValue + 1;
                        });
                    },
                    OperationKind.LocalReference);

                    context.RegisterOperationAction(context =>
                    {
                        IBinaryOperation blockOperation = (IBinaryOperation)context.Operation;
                        if (blockOperation.OperatorKind != BinaryOperatorKind.Equals)
                        {
                            return;
                        }
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

                                var semanticModel = context.Operation.SemanticModel;
                                SyntaxNode declarationNode = declaration.GetSyntax(context.CancellationToken);
                                var operation = semanticModel.GetOperation(declarationNode, context.CancellationToken);
                                if (operation is IVariableDeclaratorOperation variableDeclaratorOperation)
                                {
                                    if (variableDeclaratorOperation.Parent is IVariableDeclarationOperation variableDeclarationOperation)
                                    {
                                        if (variableDeclarationOperation.Parent is IVariableDeclarationGroupOperation declarationGroupOperation)
                                        {
                                            DataFlowAnalysis dataFlowAnalysis = semanticModel.AnalyzeDataFlow(declarationGroupOperation.Syntax);
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
                                            if (variableInitializer.Value is IInvocationOperation invocationOperation)
                                            {
                                                if (InvocationOperationHasTargetMethod(invocationOperation))
                                                {
                                                    var locations = ImmutableArray.Create(blockLocation, declarationGroupLocation);
                                                    variableNameToLocationsMap.TryAdd(variableDeclaratorOperation.Symbol.Name, locations);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (leftOperand is IInvocationOperation leftInvocationOperation)
                            {
                                var blockLocationList = new List<Location> { blockLocation };
                                if (InvocationOperationHasTargetMethod(leftInvocationOperation))
                                {
                                    leftOperandInvocationLocations = blockLocationList;
                                }
                            }
                        }
                    },
                    OperationKind.Binary);

                    context.RegisterOperationBlockEndAction(context =>
                    {
                        if (leftOperandInvocationLocations != null)
                        {
                            context.ReportDiagnostic(leftOperandInvocationLocations.CreateDiagnostic(Rule));
                        }
                        if (variableNameToLocationsMap.Count > 0)
                        {
                            List<Location> locations = new List<Location>();
                            foreach (KeyValuePair<string, ImmutableArray<Location>> variableNameAndLocation in variableNameToLocationsMap)
                            {
                                string variableName = variableNameAndLocation.Key;
                                if (!variableNameToNumberOfReferences.TryGetValue(variableName, out int numberOfReferences))
                                {
                                    continue;
                                }

                                if (numberOfReferences > 1)
                                {
                                    continue;
                                }

                                context.ReportDiagnostic(variableNameAndLocation.Value.CreateDiagnostic(Rule));
                            }
                        }
                    });
                });

                bool InvocationOperationHasTargetMethod(IInvocationOperation invocationOperation)
                {
                    var targetMethod = invocationOperation.TargetMethod;
                    return targetMethod.Equals(stringArgumentIndexOfMethod) || targetMethod.Equals(charArgumentIndexOfMethod) || targetMethod.Equals(stringAndComparisonTypeArgumentIndexOfMethod) || targetMethod.Equals(charAndComparisonTypeArgumentIndexOfMethod);
                }
            });
        }
    }
}

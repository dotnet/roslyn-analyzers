// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
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

        private ImmutableArray<Location> _locations;

        private readonly Dictionary<string, int> localSymbolsToNumberOfReferences = new Dictionary<string, int>();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilation =>
            {
                if (!compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemString, out INamedTypeSymbol? stringType))
                {
                    return;
                }
                if (!compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemChar, out INamedTypeSymbol? charType))
                {
                    return;
                }
                if (!compilation.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison, out INamedTypeSymbol? stringComparisonType))
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

                compilation.RegisterOperationBlockStartAction(context =>
                {
                    ISymbol symbol = context.OwningSymbol;
                    if (!(context.OwningSymbol is IMethodSymbol method))
                    {
                        return;
                    }
                    context.RegisterOperationAction(context =>
                    {
                        ILocalReferenceOperation localReference = (ILocalReferenceOperation)context.Operation;
                        ILocalSymbol symbol = localReference.Local;
                        if (localSymbolsToNumberOfReferences.TryGetValue(symbol.Name, out int numberOfReferences))
                        {
                            localSymbolsToNumberOfReferences[symbol.Name] = ++numberOfReferences;
                        }
                        else
                        {
                            localSymbolsToNumberOfReferences.Add(symbol.Name, 1);
                        }
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

                                            if (localSymbolsToNumberOfReferences.TryGetValue(variableName.Name, out int numberOfReferences))
                                            {
                                                if (numberOfReferences > 2)
                                                {
                                                    // The variable is read in more than 1 place
                                                    return;
                                                }
                                            }
                                            else
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
                                                if (InvocationOperationIsTargetMethod(invocationOperation, context))
                                                {
                                                    _locations = locations;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (leftOperand is IInvocationOperation leftInvocationOperation)
                            {
                                var locations = ImmutableArray.Create(blockLocation);
                                if (InvocationOperationIsTargetMethod(leftInvocationOperation, context))
                                {
                                    _locations = locations;
                                }
                            }
                        }
                    },
                    OperationKind.Binary);

                    context.RegisterOperationBlockEndAction(context =>
                    {
                        if (_locations != null)
                        {
                            context.ReportDiagnostic(_locations.CreateDiagnostic(Rule));
                        }
                    });

                });

                bool InvocationOperationIsTargetMethod(IInvocationOperation invocationOperation, OperationAnalysisContext operationContext)
                {
                    var targetMethod = invocationOperation.TargetMethod;
                    return targetMethod.Equals(stringArgumentIndexOfMethod) || targetMethod.Equals(charArgumentIndexOfMethod) || targetMethod.Equals(stringAndComparisonTypeArgumentIndexOfMethod) || targetMethod.Equals(charAndComparisonTypeArgumentIndexOfMethod);
                }
            });
        }
    }
}

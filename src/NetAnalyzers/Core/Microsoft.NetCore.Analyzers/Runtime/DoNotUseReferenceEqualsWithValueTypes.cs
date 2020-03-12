// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1307: Specify StringComparison
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseReferenceEqualsWithValueTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2013";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseReferenceEqualsWithValueTypesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseReferenceEqualsWithValueTypesMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseReferenceEqualsWithValueTypesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            RuleLevel.BuildWarning,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationStartContext =>
            {
                var objectType = compilationStartContext.Compilation.GetSpecialType(SpecialType.System_Object);

                // Without these symbols the rule cannot run
                if (objectType == null)
                {
                    return;
                }

                var referenceEqualsMethodGroup = objectType.GetMembers("ReferenceEquals").OfType<IMethodSymbol>();
                var referenceEqualsMethod = referenceEqualsMethodGroup.GetFirstOrDefaultMemberWithParameterInfos(
                    GetParameterInfo(objectType),
                    GetParameterInfo(objectType));

                if (referenceEqualsMethod == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationAction(operationContext =>
                {
                    var invocationExpression = (IInvocationOperation)operationContext.Operation;
                    var targetMethod = invocationExpression.TargetMethod;

                    if (!referenceEqualsMethod.Equals(targetMethod))
                    {
                        return;
                    }

                    foreach (var argument in invocationExpression.Arguments)
                    {
                        var val = argument.Value;

                        if (val is IConversionOperation conversion)
                        {
                            val = conversion.Operand;
                        }

                        if (val.Type.IsValueType)
                        {
                            operationContext.ReportDiagnostic(
                                val.CreateDiagnostic(
                                    Rule,
                                    argument.Parameter.Name,
                                    val.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                        }
                    }
                },
                OperationKind.Invocation);
            });
        }

        private static ParameterInfo GetParameterInfo(INamedTypeSymbol type, bool isArray = false, int arrayRank = 0, bool isParams = false)
        {
            return ParameterInfo.GetParameterInfo(type, isArray, arrayRank, isParams);
        }
    }
}
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
    /// CA2013: Do not use ReferenceEquals with value types.
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
            DiagnosticCategory.Reliability,
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
                    ParameterInfo.GetParameterInfo(objectType),
                    ParameterInfo.GetParameterInfo(objectType));

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

                        // Only check through one level of conversion,
                        // which will be either the boxing conversion to object,
                        // or a reference type implicit conversion to object.
                        if (val is IConversionOperation conversion)
                        {
                            val = conversion.Operand;
                        }

                        if (val.Type?.IsValueType == true)
                        {
                            operationContext.ReportDiagnostic(
                                val.CreateDiagnostic(
                                    Rule,
                                    val.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                        }
                    }
                },
                OperationKind.Invocation);
            });
        }
    }
}
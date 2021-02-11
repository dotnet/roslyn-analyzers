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
    /// CA1307: Specify StringComparison
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyStringComparisonAnalyzer : DiagnosticAnalyzer
    {
        private const string RuleId_CA1307 = "CA1307";
        private const string RuleId_CA1310 = "CA1310";

        private static readonly ImmutableArray<string> s_CA1310MethodNamesWithFirstStringParameter =
            ImmutableArray.Create("Compare", "StartsWith", "EndsWith", "IndexOf", "LastIndexOf");

        private static readonly LocalizableString s_localizableCA1307Title = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1307Title), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCA1307Message = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1307Message), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCA1307Description = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1307Description), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule_CA1307 = DiagnosticDescriptorHelper.Create(RuleId_CA1307,
                                                                             s_localizableCA1307Title,
                                                                             s_localizableCA1307Message,
                                                                             DiagnosticCategory.Globalization,
                                                                             RuleLevel.Disabled,
                                                                             description: s_localizableCA1307Description,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        private static readonly LocalizableString s_localizableCA1310Title = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1310Title), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCA1310Message = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1310Message), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCA1310Description = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyStringComparisonCA1310Description), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule_CA1310 = DiagnosticDescriptorHelper.Create(RuleId_CA1310,
                                                                             s_localizableCA1310Title,
                                                                             s_localizableCA1310Message,
                                                                             DiagnosticCategory.Globalization,
                                                                             RuleLevel.IdeHidden_BulkConfigurable,
                                                                             description: s_localizableCA1310Description,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule_CA1307, Rule_CA1310);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(csaContext =>
            {
                var stringComparisonType = csaContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringComparison);
                var stringType = csaContext.Compilation.GetSpecialType(SpecialType.System_String);

                // Without these symbols the rule cannot run
                if (stringComparisonType == null || stringType == null)
                {
                    return;
                }

                var overloadMap = GetWellKnownStringOverloads(csaContext.Compilation, stringType, stringComparisonType);

                var linqExpressionType = csaContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqExpressionsExpression1);

                csaContext.RegisterOperationAction(oaContext =>
                {
                    var invocationExpression = (IInvocationOperation)oaContext.Operation;
                    var targetMethod = invocationExpression.TargetMethod;

                    if (targetMethod.IsGenericMethod ||
                        targetMethod.ContainingType == null ||
                        targetMethod.ContainingType.IsErrorType())
                    {
                        return;
                    }

                    // Check if we are in a Expression<Func<T...>> context, in which case it is possible
                    // that the underlying call doesn't have the comparison option so we want to bail-out.
                    if (invocationExpression.IsWithinExpressionTree(linqExpressionType))
                    {
                        return;
                    }

                    // Report correctness issue CA1310 for known string comparison methods that default to culture specific string comparison:
                    // https://docs.microsoft.com/dotnet/standard/base-types/best-practices-strings#string-comparisons-that-use-the-current-culture
                    if (targetMethod.ContainingType.SpecialType == SpecialType.System_String &&
                        !overloadMap.IsEmpty &&
                        overloadMap.ContainsKey(targetMethod))
                    {
                        ReportDiagnostic(
                            Rule_CA1310,
                            oaContext,
                            invocationExpression,
                            targetMethod,
                            overloadMap[targetMethod]);

                        return;
                    }

                    // Report maintainability issue CA1307 for any method that has an additional overload with the exact same parameter list,
                    // plus as additional StringComparison parameter. Default StringComparison may or may not match user's intent,
                    // but it is recommended to explicitly specify it for clarity and readability:
                    // https://docs.microsoft.com/dotnet/standard/base-types/best-practices-strings#recommendations-for-string-usage
                    IEnumerable<IMethodSymbol> methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>();
                    if (methodsWithSameNameAsTargetMethod.HasMoreThan(1))
                    {
                        var correctOverload = methodsWithSameNameAsTargetMethod
                                                .GetMethodOverloadsWithDesiredParameterAtTrailing(targetMethod, stringComparisonType)
                                                .FirstOrDefault();

                        if (correctOverload != null)
                        {
                            ReportDiagnostic(
                                Rule_CA1307,
                                oaContext,
                                invocationExpression,
                                targetMethod,
                                correctOverload);
                        }
                    }
                }, OperationKind.Invocation);
            });

            static ImmutableDictionary<IMethodSymbol, IMethodSymbol> GetWellKnownStringOverloads(
                Compilation compilation,
                INamedTypeSymbol stringType,
                INamedTypeSymbol stringComparisonType)
            {
                var objectType = compilation.GetSpecialType(SpecialType.System_Object);
                var booleanType = compilation.GetSpecialType(SpecialType.System_Boolean);
                var integerType = compilation.GetSpecialType(SpecialType.System_Int32);
                var stringCompareToNamedMethods = stringType.GetMembers("CompareTo").OfType<IMethodSymbol>();
                var stringCompareToParameterString = stringCompareToNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                         GetParameterInfo(stringType));
                var stringCompareToParameterObject = stringCompareToNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                         GetParameterInfo(objectType));

                var stringCompareNamedMethods = stringType.GetMembers("Compare").OfType<IMethodSymbol>();
                var stringCompareParameterStringStringBool = stringCompareNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                                 GetParameterInfo(stringType),
                                                                 GetParameterInfo(stringType),
                                                                 GetParameterInfo(booleanType));
                var stringCompareParameterStringStringStringComparison = stringCompareNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                                             GetParameterInfo(stringType),
                                                                             GetParameterInfo(stringType),
                                                                             GetParameterInfo(stringComparisonType));
                var stringCompareParameterStringIntStringIntIntBool = stringCompareNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                                          GetParameterInfo(stringType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(stringType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(booleanType));
                var stringCompareParameterStringIntStringIntIntComparison = stringCompareNamedMethods.GetFirstOrDefaultMemberWithParameterInfos(
                                                                                GetParameterInfo(stringType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(stringType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(stringComparisonType));

                var overloadMapBuilder = ImmutableDictionary.CreateBuilder<IMethodSymbol, IMethodSymbol>();
                overloadMapBuilder.AddKeyValueIfNotNull(stringCompareToParameterString, stringCompareParameterStringStringStringComparison);
                overloadMapBuilder.AddKeyValueIfNotNull(stringCompareToParameterObject, stringCompareParameterStringStringStringComparison);
                overloadMapBuilder.AddKeyValueIfNotNull(stringCompareParameterStringStringBool, stringCompareParameterStringStringStringComparison);
                overloadMapBuilder.AddKeyValueIfNotNull(stringCompareParameterStringIntStringIntIntBool, stringCompareParameterStringIntStringIntIntComparison);

                foreach (var methodName in s_CA1310MethodNamesWithFirstStringParameter)
                {
                    var methodsWithMethodName = stringType.GetMembers(methodName).OfType<IMethodSymbol>();
                    foreach (var method in methodsWithMethodName)
                    {
                        if (!method.Parameters.IsEmpty &&
                            method.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                            !method.Parameters[^1].Type.Equals(stringComparisonType))
                        {
                            var recommendedMethod = methodsWithMethodName
                                                    .GetMethodOverloadsWithDesiredParameterAtTrailing(method, stringComparisonType)
                                                    .FirstOrDefault();
                            if (recommendedMethod != null)
                            {
                                overloadMapBuilder.AddKeyValueIfNotNull(method, recommendedMethod);
                            }
                        }
                    }
                }

                return overloadMapBuilder.ToImmutable();
            }
        }

        private static void ReportDiagnostic(
            DiagnosticDescriptor rule,
            OperationAnalysisContext oaContext,
            IInvocationOperation invocationExpression,
            IMethodSymbol targetMethod,
            IMethodSymbol correctOverload)
        {
            oaContext.ReportDiagnostic(
                invocationExpression.CreateDiagnostic(
                    rule,
                    targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    correctOverload.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }

        private static ParameterInfo GetParameterInfo(INamedTypeSymbol type, bool isArray = false, int arrayRank = 0, bool isParams = false)
        {
            return ParameterInfo.GetParameterInfo(type, isArray, arrayRank, isParams);
        }
    }
}

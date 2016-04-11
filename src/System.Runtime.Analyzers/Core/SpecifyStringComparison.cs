// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1307: Specify StringComparison
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyStringComparisonAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1307";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyStringComparisonTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyStringComparisonMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyStringComparisonDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb386080.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(csaContext =>
            {
                var stringComparisonType = csaContext.Compilation.GetTypeByMetadataName("System.StringComparison");
                var stringType = csaContext.Compilation.GetSpecialType(SpecialType.System_String);

                // Without these symbols the rule cannot run
                if (stringComparisonType == null || stringType == null)
                {
                    return;
                }

                var objectType = csaContext.Compilation.GetSpecialType(SpecialType.System_Object);
                var booleanType = csaContext.Compilation.GetSpecialType(SpecialType.System_Boolean);
                var integerType = csaContext.Compilation.GetSpecialType(SpecialType.System_Int32);
                var stringCompareToNamedMethods = stringType.GetMembers("CompareTo").OfType<IMethodSymbol>();
                var stringCompareToParameterString = stringCompareToNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                         GetParameterInfo(stringType));
                var stringCompareToParameterObject = stringCompareToNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                         GetParameterInfo(objectType));

                var stringCompareNamedMethods = stringType.GetMembers("Compare").OfType<IMethodSymbol>();
                var stringCompareParameterStringStringBool = stringCompareNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                                 GetParameterInfo(stringType),
                                                                 GetParameterInfo(stringType),
                                                                 GetParameterInfo(booleanType));
                var stringCompareParameterStringStringStringComparison = stringCompareNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                                             GetParameterInfo(stringType),
                                                                             GetParameterInfo(stringType),
                                                                             GetParameterInfo(stringComparisonType));
                var stringCompareParameterStringIntStringIntIntBool = stringCompareNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                                          GetParameterInfo(stringType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(stringType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(integerType),
                                                                          GetParameterInfo(booleanType));
                var stringCompareParameterStringIntStringIntIntComparison = stringCompareNamedMethods.GetSingleOrDefaultMemberWithParameterInfos(
                                                                                GetParameterInfo(stringType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(stringType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(integerType),
                                                                                GetParameterInfo(stringComparisonType));

                IDictionary<IMethodSymbol, IMethodSymbol> overloadMap = new Dictionary<IMethodSymbol, IMethodSymbol>();
                overloadMap.AddKeyValueIfNotNull(stringCompareToParameterString, stringCompareParameterStringStringStringComparison);
                overloadMap.AddKeyValueIfNotNull(stringCompareToParameterObject, stringCompareParameterStringStringStringComparison);
                overloadMap.AddKeyValueIfNotNull(stringCompareParameterStringStringBool, stringCompareParameterStringStringStringComparison);
                overloadMap.AddKeyValueIfNotNull(stringCompareParameterStringIntStringIntIntBool, stringCompareParameterStringIntStringIntIntComparison);

                csaContext.RegisterOperationAction(oaContext =>
                {
                    var invocationExpression = (IInvocationExpression)oaContext.Operation;
                    var targetMethod = invocationExpression.TargetMethod;

                    if (targetMethod.IsGenericMethod ||
                        targetMethod.ContainingType == null)
                    {
                        return;
                    }

                    if (overloadMap.Count != 0 && overloadMap.ContainsKey(targetMethod))
                    {
                        ReportDiagnostic(
                            oaContext,
                            invocationExpression,
                            targetMethod,
                            overloadMap[targetMethod]);

                        return;
                    }

                    IEnumerable<IMethodSymbol> methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>();
                    if (methodsWithSameNameAsTargetMethod.Count() > 1)
                    {
                        var correctOverload = methodsWithSameNameAsTargetMethod
                                                .GetMethodOverloadsWithDesiredParameterAtTrailing(targetMethod, stringComparisonType)
                                                .FirstOrDefault();

                        if (correctOverload != null)
                        {
                            ReportDiagnostic(
                                oaContext,
                                invocationExpression,
                                targetMethod,
                                correctOverload);
                        }
                    }
                }, OperationKind.InvocationExpression);
            });
        }

        private void ReportDiagnostic(
            OperationAnalysisContext oaContext,
            IInvocationExpression invocationExpression,
            IMethodSymbol targetMethod,
            IMethodSymbol correctOverload)
        {
            oaContext.ReportDiagnostic(
                invocationExpression.Syntax.CreateDiagnostic(
                    Rule,
                    targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    correctOverload.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }

        private IMethodSymbol GetSingleOrDefaultMemberWithName(IEnumerable<IMethodSymbol> stringFormatMembers, string displayName)
        {
            return stringFormatMembers?.Where(member => string.Equals(member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), displayName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
        }


        private ParameterInfo GetParameterInfo(INamedTypeSymbol type, bool isArray = false, int arrayRank = 0, bool isParams = false)
        {
            return ParameterInfo.GetParameterInfo(type, isArray, arrayRank, isParams);
        }
    }
}
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
                var currentCulture = stringComparisonType.GetMembers("CurrentCulture").SingleOrDefault();
                var currentCultureIgnoreCase = stringComparisonType.GetMembers("CurrentCultureIgnoreCase").SingleOrDefault();
                var ordinalIgnoreCase = stringComparisonType.GetMembers("OrdinalIgnoreCase").SingleOrDefault();
                var ordinal = stringComparisonType.GetMembers("Ordinal").SingleOrDefault();

                // Without these symbols the rule cannot run
                if (stringComparisonType == null || stringType == null || currentCulture == null ||
                    currentCultureIgnoreCase == null || ordinalIgnoreCase == null || ordinal == null)
                {
                    return;
                }

                var stringCompareToNamedMethods = stringType.GetMembers("CompareTo").OfType<IMethodSymbol>();
                var stringCompareToParameterString = stringCompareToNamedMethods.GetSingleOrDefaultMemberWithName("string.CompareTo(string)");
                var stringCompareToParameterObject = stringCompareToNamedMethods.GetSingleOrDefaultMemberWithName("string.CompareTo(object)");

                var boolString = csaContext.Compilation.Language == LanguageNames.CSharp ? "bool" : "Boolean";
                var intString = csaContext.Compilation.Language == LanguageNames.CSharp ? "int" : "Integer";
                var stringCompareNamedMethods = stringType.GetMembers("Compare").OfType<IMethodSymbol>();
                var stringCompareParameterStringStringBool = stringCompareNamedMethods.GetSingleOrDefaultMemberWithName($"string.Compare(string, string, {boolString})");
                var stringCompareParameterStringStringComparison = stringCompareNamedMethods.GetSingleOrDefaultMemberWithName("string.Compare(string, string, System.StringComparison)");
                var stringCompareParameterStringIntStringIntIntBool = stringCompareNamedMethods.GetSingleOrDefaultMemberWithName($"string.Compare(string, {intString}, string, {intString}, {intString}, {boolString})");
                var stringCompareParameterStringIntStringIntIntComparison = stringCompareNamedMethods.GetSingleOrDefaultMemberWithName($"string.Compare(string, {intString}, string, {intString}, {intString}, System.StringComparison)");

                IDictionary<IMethodSymbol, IMethodSymbol> overloadMap = new Dictionary<IMethodSymbol, IMethodSymbol>();
                overloadMap.AddKeyValueIfNotNull(stringCompareToParameterString, stringCompareParameterStringStringComparison);
                overloadMap.AddKeyValueIfNotNull(stringCompareToParameterObject, stringCompareParameterStringStringComparison);
                overloadMap.AddKeyValueIfNotNull(stringCompareParameterStringStringBool, stringCompareParameterStringStringComparison);
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
                            overloadMap[targetMethod],
                            currentCulture,
                            currentCultureIgnoreCase,
                            ordinalIgnoreCase,
                            ordinal);
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
                                correctOverload,
                                currentCulture,
                                currentCultureIgnoreCase,
                                ordinalIgnoreCase,
                                ordinal);
                        }
                    }
                }, OperationKind.InvocationExpression);
            });
        }

        private void ReportDiagnostic(
            OperationAnalysisContext oaContext,
            IInvocationExpression invocationExpression,
            IMethodSymbol targetMethod,
            IMethodSymbol correctOverload,
            ISymbol currentCulture,
            ISymbol currentCultureIgnoreCase,
            ISymbol ordinalIgnoreCase,
            ISymbol ordinal)
        {
            oaContext.ReportDiagnostic(
                invocationExpression.Syntax.CreateDiagnostic(
                    Rule,
                    targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    correctOverload.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    currentCulture.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    currentCultureIgnoreCase.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    ordinalIgnoreCase.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    ordinal.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
        }
    }
}
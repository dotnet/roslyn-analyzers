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
    /// CA1305: Specify IFormatProvider
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyIFormatProviderAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1305";
        internal const string Uri = "https://docs.microsoft.com/visualstudio/code-quality/ca1305-specify-iformatprovider";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageIFormatProviderAlternateString = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderMessageIFormatProviderAlternateString), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIFormatProviderAlternate = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderMessageIFormatProviderAlternate), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUICultureString = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderMessageUICultureString), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUICulture = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderMessageUICulture), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.SpecifyIFormatProviderDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor IFormatProviderAlternateStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIFormatProviderAlternateString,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor IFormatProviderAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIFormatProviderAlternate,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor UICultureStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageUICultureString,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor UICultureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageUICulture,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IFormatProviderAlternateStringRule, IFormatProviderAlternateRule, UICultureStringRule, UICultureRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(csaContext =>
            {
                #region "Get All the WellKnown Types and Members"
                var iformatProviderType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemIFormatProvider);
                var cultureInfoType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemGlobalizationCultureInfo);
                if (iformatProviderType == null || cultureInfoType == null)
                {
                    return;
                }

                var objectType = csaContext.Compilation.GetSpecialType(SpecialType.System_Object);
                var stringType = csaContext.Compilation.GetSpecialType(SpecialType.System_String);
                var stringFormatMembers = stringType?.GetMembers("Format").OfType<IMethodSymbol>();

                var stringFormatMemberWithStringAndObjectParameter = stringFormatMembers.GetFirstOrDefaultMemberWithParameterInfos(
                                                                         GetParameterInfo(stringType),
                                                                         GetParameterInfo(objectType));
                var stringFormatMemberWithStringObjectAndObjectParameter = stringFormatMembers.GetFirstOrDefaultMemberWithParameterInfos(
                                                                               GetParameterInfo(stringType),
                                                                               GetParameterInfo(objectType),
                                                                               GetParameterInfo(objectType));
                var stringFormatMemberWithStringObjectObjectAndObjectParameter = stringFormatMembers.GetFirstOrDefaultMemberWithParameterInfos(
                                                                                     GetParameterInfo(stringType),
                                                                                     GetParameterInfo(objectType),
                                                                                     GetParameterInfo(objectType),
                                                                                     GetParameterInfo(objectType));
                var stringFormatMemberWithStringAndParamsObjectParameter = stringFormatMembers.GetFirstOrDefaultMemberWithParameterInfos(
                                                                               GetParameterInfo(stringType),
                                                                               GetParameterInfo(objectType, isArray: true, arrayRank: 1, isParams: true));
                var stringFormatMemberWithIFormatProviderStringAndParamsObjectParameter = stringFormatMembers.GetFirstOrDefaultMemberWithParameterInfos(
                                                                                              GetParameterInfo(iformatProviderType),
                                                                                              GetParameterInfo(stringType),
                                                                                              GetParameterInfo(objectType, isArray: true, arrayRank: 1, isParams: true));

                var currentCultureProperty = cultureInfoType?.GetMembers("CurrentCulture").OfType<IPropertySymbol>().FirstOrDefault();
                var invariantCultureProperty = cultureInfoType?.GetMembers("InvariantCulture").OfType<IPropertySymbol>().FirstOrDefault();
                var currentUICultureProperty = cultureInfoType?.GetMembers("CurrentUICulture").OfType<IPropertySymbol>().FirstOrDefault();
                var installedUICultureProperty = cultureInfoType?.GetMembers("InstalledUICulture").OfType<IPropertySymbol>().FirstOrDefault();

                var threadType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemThreadingThread);
                var currentThreadCurrentUICultureProperty = threadType?.GetMembers("CurrentUICulture").OfType<IPropertySymbol>().FirstOrDefault();

                var activatorType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemActivator);
                var resourceManagerType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemResourcesResourceManager);

                var computerInfoType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualBasicDevicesComputerInfo);
                var installedUICulturePropertyOfComputerInfoType = computerInfoType?.GetMembers("InstalledUICulture").OfType<IPropertySymbol>().FirstOrDefault();

                var obsoleteAttributeType = csaContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemObsoleteAttribute);
                #endregion

                csaContext.RegisterOperationAction(oaContext =>
                {
                    var invocationExpression = (IInvocationOperation)oaContext.Operation;
                    var targetMethod = invocationExpression.TargetMethod;

                    #region "Exceptions"
                    if (targetMethod.IsGenericMethod || targetMethod.ContainingType == null || targetMethod.ContainingType.IsErrorType() ||
                        (targetMethod.ContainingType != null &&
                         (activatorType != null && activatorType.Equals(targetMethod.ContainingType)) ||
                         (resourceManagerType != null && resourceManagerType.Equals(targetMethod.ContainingType))))
                    {
                        return;
                    }
                    #endregion

                    #region "IFormatProviderAlternateStringRule Only"
                    if (stringType != null && cultureInfoType != null &&
                        (targetMethod.Equals(stringFormatMemberWithStringAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringObjectAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringObjectObjectAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringAndParamsObjectParameter)))
                    {
                        // Sample message for IFormatProviderAlternateStringRule: Because the behavior of string.Format(string, object) could vary based on the current user's locale settings,
                        // replace this call in IFormatProviderStringTest.M() with a call to string.Format(IFormatProvider, string, params object[]).
                        oaContext.ReportDiagnostic(
                            invocationExpression.Syntax.CreateDiagnostic(
                                IFormatProviderAlternateStringRule,
                                targetMethod.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                stringFormatMemberWithIFormatProviderStringAndParamsObjectParameter.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));

                        return;
                    }
                    #endregion

                    #region "IFormatProviderAlternateStringRule & IFormatProviderAlternateRule"

                    IEnumerable<IMethodSymbol> methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>().WhereMethodDoesNotContainAttribute(obsoleteAttributeType).ToList();
                    if (methodsWithSameNameAsTargetMethod.HasMoreThan(1))
                    {
                        var correctOverloads = methodsWithSameNameAsTargetMethod.GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(targetMethod, iformatProviderType).ToList();

                        // If there are two matching overloads, one with CultureInfo as the first parameter and one with CultureInfo as the last parameter,
                        // report the diagnostic on the overload with CultureInfo as the last parameter, to match the behavior of FxCop.
                        var correctOverload = correctOverloads.FirstOrDefault(overload => overload.Parameters.Last().Type.Equals(iformatProviderType)) ?? correctOverloads.FirstOrDefault();

                        // Sample message for IFormatProviderAlternateRule: Because the behavior of Convert.ToInt64(string) could vary based on the current user's locale settings,
                        // replace this call in IFormatProviderStringTest.TestMethod() with a call to Convert.ToInt64(string, IFormatProvider).
                        if (correctOverload != null)
                        {
                            oaContext.ReportDiagnostic(
                                invocationExpression.Syntax.CreateDiagnostic(
                                    targetMethod.ReturnType.Equals(stringType) ?
                                     IFormatProviderAlternateStringRule :
                                     IFormatProviderAlternateRule,
                                    targetMethod.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    correctOverload.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                        }
                    }
                    #endregion

                    #region "UICultureStringRule & UICultureRule"
                    IEnumerable<int> IformatProviderParameterIndices = GetIndexesOfParameterType(targetMethod, iformatProviderType);
                    foreach (var index in IformatProviderParameterIndices)
                    {
                        var argument = invocationExpression.Arguments[index];

                        if (argument != null && currentUICultureProperty != null &&
                            installedUICultureProperty != null && currentThreadCurrentUICultureProperty != null)
                        {
                            var semanticModel = oaContext.Compilation.GetSemanticModel(argument.Syntax.SyntaxTree);

                            var symbol = semanticModel.GetSymbolInfo(argument.Value.Syntax).Symbol;

                            if (symbol != null &&
                                (symbol.Equals(currentUICultureProperty) ||
                                 symbol.Equals(installedUICultureProperty) ||
                                 symbol.Equals(currentThreadCurrentUICultureProperty) ||
                                 (installedUICulturePropertyOfComputerInfoType != null && symbol.Equals(installedUICulturePropertyOfComputerInfoType))))
                            {
                                // Sample message
                                // 1. UICultureStringRule - 'TestClass.TestMethod()' passes 'Thread.CurrentUICulture' as the 'IFormatProvider' parameter to 'TestClass.CalleeMethod(string, IFormatProvider)'.
                                // This property returns a culture that is inappropriate for formatting methods.
                                // 2. UICultureRule -'TestClass.TestMethod()' passes 'CultureInfo.CurrentUICulture' as the 'IFormatProvider' parameter to 'TestClass.Callee(IFormatProvider, string)'.
                                // This property returns a culture that is inappropriate for formatting methods.

                                oaContext.ReportDiagnostic(
                                    invocationExpression.Syntax.CreateDiagnostic(
                                        targetMethod.ReturnType.Equals(stringType) ?
                                            UICultureStringRule :
                                            UICultureRule,
                                        oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                        symbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                        targetMethod.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                            }
                        }
                    }
                    #endregion

                }, OperationKind.Invocation);
            });
        }

        private static IEnumerable<int> GetIndexesOfParameterType(IMethodSymbol targetMethod, INamedTypeSymbol formatProviderType)
        {
            return targetMethod.Parameters
                .Select((Parameter, Index) => new { Parameter, Index })
                .Where(x => x.Parameter.Type.Equals(formatProviderType))
                .Select(x => x.Index);
        }

        private static ParameterInfo GetParameterInfo(INamedTypeSymbol type, bool isArray = false, int arrayRank = 0, bool isParams = false)
        {
            return ParameterInfo.GetParameterInfo(type, isArray, arrayRank, isParams);
        }
    }
}
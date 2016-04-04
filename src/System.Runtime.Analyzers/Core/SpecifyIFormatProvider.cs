// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1305: Specify IFormatProvider
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyIFormatProviderAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1305";
        internal const string Uri = @"https://msdn.microsoft.com/en-us/library/ms182190.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageIFormatProviderAlternateString = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderMessageIFormatProviderAlternateString), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIFormatProviderAlternate = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderMessageIFormatProviderAlternate), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUICultureString = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderMessageUICultureString), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageUICulture = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderMessageUICulture), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyIFormatProviderDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor IFormatProviderAlternateStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIFormatProviderAlternateString,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor IFormatProviderAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIFormatProviderAlternate,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor UICultureStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageUICultureString,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor UICultureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageUICulture,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IFormatProviderAlternateStringRule, IFormatProviderAlternateRule, UICultureStringRule, UICultureRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(csaContext =>
            {
                #region "Get All the WellKnown Types and Members"
                var stringType = csaContext.Compilation.GetSpecialType(SpecialType.System_String);
                var stringFormatMembers = stringType?.GetMembers("Format").OfType<IMethodSymbol>();
                var stringFormatMemberWithStringAndObjectParameter = GetSingleOrDefaultMemberWithName(stringFormatMembers, "string.Format(string, object)");
                var stringFormatMemberWithStringObjectAndObjectParameter = GetSingleOrDefaultMemberWithName(stringFormatMembers, "string.Format(string, object, object)");
                var stringFormatMemberWithStringObjectObjectAndObjectParameter = GetSingleOrDefaultMemberWithName(stringFormatMembers, "string.Format(string, object, object, object)");

                var paramsObjectArraySymbolDisplay = csaContext.Compilation.Language == LanguageNames.CSharp ?
                                                     "params object[]" :
                                                     "ParamArray Object()";
                var stringFormatMemberWithStringAndParamsObjectParameter = GetSingleOrDefaultMemberWithName(stringFormatMembers, $"string.Format(string, {paramsObjectArraySymbolDisplay})");
                var stringFormatMemberWithIFormatProviderStringAndParamsObjectParameter = GetSingleOrDefaultMemberWithName(stringFormatMembers, $"string.Format(System.IFormatProvider, string, {paramsObjectArraySymbolDisplay})");

                var IFormatProviderType = csaContext.Compilation.GetTypeByMetadataName("System.IFormatProvider");

                var CultureInfoType = csaContext.Compilation.GetTypeByMetadataName("System.Globalization.CultureInfo");
                var currentCultureProperty = CultureInfoType?.GetMembers("CurrentCulture").OfType<IPropertySymbol>().SingleOrDefault();
                var invariantCultureProperty = CultureInfoType?.GetMembers("InvariantCulture").OfType<IPropertySymbol>().SingleOrDefault();
                var currentUICultureProperty = CultureInfoType?.GetMembers("CurrentUICulture").OfType<IPropertySymbol>().SingleOrDefault();
                var installedUICultureProperty = CultureInfoType?.GetMembers("InstalledUICulture").OfType<IPropertySymbol>().SingleOrDefault();

                var threadType = csaContext.Compilation.GetTypeByMetadataName("System.Threading.Thread");
                var currentThreadCurrentUICultureProperty = threadType?.GetMembers("CurrentUICulture").OfType<IPropertySymbol>().SingleOrDefault();

                var activatorType = csaContext.Compilation.GetTypeByMetadataName("System.Activator");
                var resourceManagerType = csaContext.Compilation.GetTypeByMetadataName("System.Resources.ResourceManager");

                var computerInfoType = csaContext.Compilation.GetTypeByMetadataName("Microsoft.VisualBasic.Devices.ComputerInfo");
                var installedUICulturePropertyOfComputerInfoType = computerInfoType?.GetMembers("InstalledUICulture").OfType<IPropertySymbol>().SingleOrDefault();
                #endregion

                csaContext.RegisterOperationAction(oaContext =>
                {
                    var invocationExpression = (IInvocationExpression)oaContext.Operation;
                    var targetMethod = invocationExpression.TargetMethod;

                    #region "Exceptions"
                    if (targetMethod.IsGenericMethod ||
                        (targetMethod.ContainingType != null && 
                         (activatorType != null && activatorType.Equals(targetMethod.ContainingType)) ||
                         (resourceManagerType != null && resourceManagerType.Equals(targetMethod.ContainingType))))
                    {
                        return;
                    }
                    #endregion

                    #region "IFormatProviderAlternateStringRule Only"
                    if (stringType != null && CultureInfoType != null &&
                        (targetMethod.Equals(stringFormatMemberWithStringAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringObjectAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringObjectObjectAndObjectParameter) ||
                         targetMethod.Equals(stringFormatMemberWithStringAndParamsObjectParameter)))
                    {
                        // Sample message for IFormatProviderAlternateStringRule: Because the behavior of string.Format(string, object) could vary based on the current user's locale settings,
                        // replace this call in IFormatProviderStringTest.M() with a call to string.Format(System.IFormatProvider, string, params object[]). If the result of 
                        // string.Format(System.IFormatProvider, string, params object[]) will be displayed to the user, specify System.Globalization.CultureInfo.CurrentCulture as the 'IFormatProvider' parameter.
                        // Otherwise, if the result will be stored and accessed by software, such as when it is persisted to disk or to a database, specify System.Globalization.CultureInfo.InvariantCulture.
                        oaContext.ReportDiagnostic(
                            invocationExpression.Syntax.CreateDiagnostic(
                                IFormatProviderAlternateStringRule,
                                targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                stringFormatMemberWithIFormatProviderStringAndParamsObjectParameter.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                currentCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                invariantCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));

                        return;
                    }
                    #endregion

                    #region "IFormatProviderAlternateStringRule & IFormatProviderAlternateRule"
                    IEnumerable<IMethodSymbol> methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>();
                    if (methodsWithSameNameAsTargetMethod.Count() > 1)
                    {
                        var correctOverloads = methodsWithSameNameAsTargetMethod.GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(targetMethod, IFormatProviderType);
                        
                        // If there are two matching overloads, one with CultureInfo as the first parameter and one with CultureInfo as the last parameter,
                        // report the diagnostic on the overload with CultureInfo as the last parameter, to match the behavior of FxCop.
                        var correctOverload = correctOverloads
                                              .Where(overload => overload.Parameters.Last().Type.Equals(IFormatProviderType))
                                              .FirstOrDefault() ?? correctOverloads.FirstOrDefault();

                        // Sample message for IFormatProviderAlternateRule: Because the behavior of System.Convert.ToInt64(string) could vary based on the current user's locale settings,
                        // replace this call in IFormatProviderStringTest.TestMethod() with a call to System.Convert.ToInt64(string, System.IFormatProvider). If the result of 
                        // System.Convert.ToInt64(string, System.IFormatProvider) will be based on input from the user, specify System.Globalization.CultureInfo.CurrentCulture as the 'IFormatProvider' parameter.
                        // Otherwise, if the result will based on input stored and accessed by software, such as when it is loaded from disk or from a database, specify System.Globalization.CultureInfo.InvariantCulture.
                        if (correctOverload != null)
                        {
                            oaContext.ReportDiagnostic(
                                invocationExpression.Syntax.CreateDiagnostic(
                                    targetMethod.ReturnType.Equals(stringType) ? 
                                     IFormatProviderAlternateStringRule :
                                     IFormatProviderAlternateRule,
                                    targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    correctOverload.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    currentCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    invariantCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                        }
                    }
                    #endregion

                    #region "UICultureStringRule & UICultureRule"
                    IEnumerable<int> IformatProviderParameterIndices = GetIndexesOfParameterType(targetMethod, IFormatProviderType);
                    foreach (var index in IformatProviderParameterIndices)
                    {
                        var argument = invocationExpression.ArgumentsInParameterOrder[index];
                        if (argument != null && currentUICultureProperty != null &&
                            installedUICultureProperty != null && currentThreadCurrentUICultureProperty != null)
                        {
                            var semanticModel = oaContext.Compilation.GetSemanticModel(argument.Syntax.SyntaxTree);
                            var symbol = semanticModel.GetSymbolInfo(argument.Syntax).Symbol;

                            if (symbol != null &
                                (symbol.Equals(currentUICultureProperty) ||
                                 symbol.Equals(installedUICultureProperty) ||
                                 symbol.Equals(currentThreadCurrentUICultureProperty) ||
                                 (installedUICulturePropertyOfComputerInfoType != null && symbol.Equals(installedUICulturePropertyOfComputerInfoType))))
                            {
                                // Sample message 
                                // 1. UICultureStringRule - 'TestClass.TestMethod()' passes 'Thread.CurrentUICulture' as the 'IFormatProvider' parameter to 'TestClass.CalleeMethod(string, IFormatProvider)'.
                                // This property returns a culture that is inappropriate for formatting methods. If the result of 'TestClass.CalleeMethod(string, IFormatProvider)' will be displayed to the user,
                                // specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider' parameter. Otherwise, if the result will be stored and accessed by software, such as when it is persisted to disk
                                // or to a database, specify 'CultureInfo.InvariantCulture'.
                                // 2. UICultureRule -'TestClass.TestMethod()' passes 'CultureInfo.CurrentUICulture' as the 'IFormatProvider' parameter to 'TestClass.Callee(IFormatProvider, string)'.
                                // This property returns a culture that is inappropriate for formatting methods. If the result of 'TestClass.Callee(IFormatProvider, string)' will be based on input from the user,
                                // specify 'CultureInfo.CurrentCulture' as the 'IFormatProvider' parameter. Otherwise, if the result will based on input stored and accessed by software, such as when it is loaded
                                // from disk or from a database, specify 'CultureInfo.InvariantCulture'.

                                oaContext.ReportDiagnostic(
                                    invocationExpression.Syntax.CreateDiagnostic(
                                        targetMethod.ReturnType.Equals(stringType) ?
                                            UICultureStringRule :
                                            UICultureRule,
                                        oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                        targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                        currentCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                        invariantCultureProperty.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                            }
                        }
                    }
                    #endregion

                }, OperationKind.InvocationExpression);
            });
        }

        private IEnumerable<int> GetIndexesOfParameterType(IMethodSymbol targetMethod, INamedTypeSymbol formatProviderType)
        {
            return targetMethod.Parameters
                .Select((Parameter, Index) => new { Parameter, Index })
                .Where(x => x.Parameter.Type.Equals(formatProviderType))
                .Select(x => x.Index);
        }

        private static IMethodSymbol GetSingleOrDefaultMemberWithName(IEnumerable<IMethodSymbol> stringFormatMembers, string displayName)
        {
            return stringFormatMembers?.Where(member => string.Equals(member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), displayName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
        }
    }
}
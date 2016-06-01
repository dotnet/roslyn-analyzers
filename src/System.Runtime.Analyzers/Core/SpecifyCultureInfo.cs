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
    /// CA1304: Specify CultureInfo
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SpecifyCultureInfoAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1304";
        private const string Uri = @"https://msdn.microsoft.com/en-us/library/ms182189.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyCultureInfoTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyCultureInfoMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.SpecifyCultureInfoDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(csaContext =>
            {
                var cultureInfoType = csaContext.Compilation.GetTypeByMetadataName("System.Globalization.CultureInfo");
                if (cultureInfoType != null)
                {
                    csaContext.RegisterOperationAction(oaContext =>
                    {
                        var invocationExpression = (IInvocationExpression)oaContext.Operation;
                        var targetMethod = invocationExpression.TargetMethod;
                        if (targetMethod.ContainingType == null || targetMethod.ContainingType.IsErrorType() || targetMethod.IsGenericMethod)
                        {
                            return;
                        }

                        IEnumerable<IMethodSymbol> methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>();
                        if (methodsWithSameNameAsTargetMethod.Count() < 2)
                        {
                            return;
                        }

                        var correctOverloads = methodsWithSameNameAsTargetMethod.GetMethodOverloadsWithDesiredParameterAtLeadingOrTrailing(targetMethod, cultureInfoType);

                        // If there are two matching overloads, one with CultureInfo as the first parameter and one with CultureInfo as the last parameter,
                        // report the diagnostic on the overload with CultureInfo as the last parameter, to match the behavior of FxCop.
                        var correctOverload = correctOverloads
                                              .Where(overload => overload.Parameters.Last().Type.Equals(cultureInfoType))
                                              .FirstOrDefault() ?? correctOverloads.FirstOrDefault();

                        if (correctOverload != null)
                        {
                            oaContext.ReportDiagnostic(
                                invocationExpression.Syntax.CreateDiagnostic(
                                    Rule,
                                    targetMethod.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                                    correctOverload.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                        }
                    }, OperationKind.InvocationExpression);
                }
            });
        }
    }
}
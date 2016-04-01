// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;
using System.Linq;

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
            analysisContext.RegisterCompilationStartAction(csaContext =>
            {
                var cultureInfoType = csaContext.Compilation.GetTypeByMetadataName("System.Globalization.CultureInfo");
                if (cultureInfoType != null)
                {
                    csaContext.RegisterOperationAction(oaContext =>
                    {
                        var invocationExpression = (IInvocationExpression)oaContext.Operation;
                        var targetMethod = invocationExpression.TargetMethod;
                        if (targetMethod.ContainingType == null)
                        {
                            return;
                        }

                        var methodsWithSameNameAsTargetMethod = targetMethod.ContainingType.GetMembers(targetMethod.Name).OfType<IMethodSymbol>();
                        if (methodsWithSameNameAsTargetMethod.Count() < 2)
                        {
                            return;
                        }

                        var correctOverloads = methodsWithSameNameAsTargetMethod.Where(candidateMethod =>
                        {
                            if (targetMethod.Parameters.Count() + 1 != candidateMethod.Parameters.Count())
                            {
                                return false;
                            }

                            // Assume CultureInfo is the last parameter so j, which is the index of the parameter
                            // in m to compare against targetMethod's parameter is set to 0
                            int j = 0;

                            if (candidateMethod.Parameters.First().Type.Equals(cultureInfoType))
                            {
                                // If CultureInfo is the first then the parameters to compare in m against targetMethod
                                // is offset by 1
                                j = 1;
                            }
                            else if (!candidateMethod.Parameters.Last().Type.Equals(cultureInfoType))
                            {
                                return false;
                            }


                            for (int i = 0; i < targetMethod.Parameters.Count(); i++, j++)
                            {
                                if (!targetMethod.Parameters[i].Type.Equals(candidateMethod.Parameters[j].Type))
                                {
                                    return false;
                                }
                            }

                            return true;
                        });

                        // Pick the one with CultureInfo as the last Parameter
                        var correctOverload = correctOverloads
                                              .Where(overload => overload.Parameters.Last().Type.Equals(cultureInfoType))
                                              .FirstOrDefault() ?? correctOverloads.FirstOrDefault();

                        if (correctOverload != null)
                        {
                            oaContext.ReportDiagnostic(
                                invocationExpression.Syntax.CreateDiagnostic(
                                    Rule,
                                    targetMethod.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    oaContext.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                                    correctOverload.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                        }
                    }, OperationKind.InvocationExpression);
                }
            });
        }
    }
}
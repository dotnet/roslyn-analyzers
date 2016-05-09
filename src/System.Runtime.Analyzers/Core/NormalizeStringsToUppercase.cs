// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1308: Normalize strings to uppercase
    /// <para>
    /// Strings should be normalized to uppercase. A small group of characters, when they are converted to lowercase, cannot make a round trip.
    /// To make a round trip means to convert the characters from one locale to another locale that represents character data differently,
    /// and then to accurately retrieve the original characters from the converted characters.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class NormalizeStringsToUppercaseAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1308";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageToUpper = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseMessageToUpper), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor ToUpperRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageToUpper,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: @"https://msdn.microsoft.com/en-us/library/bb386042.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ToUpperRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationStartContext =>
            {
                var stringType = WellKnownTypes.String(compilationStartContext.Compilation);
                if (stringType == null)
                {
                    return;
                }

                var cultureInfo = compilationStartContext.Compilation.GetTypeByMetadataName("System.Globalization.CultureInfo");
                var invariantCulture = cultureInfo?.GetMembers("InvariantCulture").OfType<IPropertySymbol>().FirstOrDefault();

                // We want to flag calls to "ToLowerInvariant" and "ToLower(CultureInfo.InvariantCulture)".
                var toLowerInvariant = stringType.GetMembers("ToLowerInvariant").OfType<IMethodSymbol>().FirstOrDefault();
                var toLowerWithCultureInfo = cultureInfo != null ?
                    stringType.GetMembers("ToLower").OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type == cultureInfo) :
                    null;

                if (toLowerInvariant == null && toLowerWithCultureInfo == null)
                {
                    return;
                }

                // We want to recommend calling "ToUpperInvariant" or "ToUpper(CultureInfo.InvariantCulture)".
                var toUpperInvariant = stringType.GetMembers("ToUpperInvariant").OfType<IMethodSymbol>().FirstOrDefault();
                var toUpperWithCultureInfo = cultureInfo != null ?
                    stringType.GetMembers("ToUpper").OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type == cultureInfo) :
                    null; ;

                if (toUpperInvariant == null && toUpperWithCultureInfo == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    if (operationAnalysisContext.Operation.IsInvalid)
                    {
                        return;
                    }

                    var invocation = (IInvocationExpression)operationAnalysisContext.Operation;
                    var method = invocation.TargetMethod;
                    if (method.Equals(toLowerInvariant) ||
                        (method.Equals(toLowerWithCultureInfo) &&
                         ((IMemberReferenceExpression)invocation.ArgumentsInParameterOrder.FirstOrDefault()?.Value)?.Member == invariantCulture))
                    {
                        var suggestedMethod = toUpperInvariant ?? toUpperWithCultureInfo;

                        // In method {0}, replace the call to {1} with {2}.
                        var diagnostic = Diagnostic.Create(ToUpperRule, invocation.Syntax.GetLocation(), operationAnalysisContext.ContainingSymbol.Name, method.Name, suggestedMethod.Name);
                        operationAnalysisContext.ReportDiagnostic(diagnostic);
                    }

                }, OperationKind.InvocationExpression);
            });
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1068: CancellationToken parameters must come last.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class CancellationTokenParametersMustComeLastAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1068";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.CancellationTokenParametersMustComeLastTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.CancellationTokenParametersMustComeLastMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            RuleLevel.IdeSuggestion,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);
                INamedTypeSymbol? cancellationTokenType = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingCancellationToken);
                INamedTypeSymbol? iprogressType = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIProgress1);
                if (cancellationTokenType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext =>
                {
                    var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                    if (methodSymbol.IsOverride ||
                        methodSymbol.IsImplementationOfAnyInterfaceMember())
                    {
                        return;
                    }

                    int last = methodSymbol.Parameters.Length - 1;
                    if (last >= 0 && methodSymbol.Parameters[last].IsParams)
                    {
                        last--;
                    }

                    // Skip optional parameters, UNLESS one of them is a CancellationToken
                    // AND it's not the last one.
                    if (last >= 0 && methodSymbol.Parameters[last].IsOptional
                        && !methodSymbol.Parameters[last].Type.Equals(cancellationTokenType))
                    {
                        last--;

                        while (last >= 0 && methodSymbol.Parameters[last].IsOptional)
                        {
                            if (methodSymbol.Parameters[last].Type.Equals(cancellationTokenType))
                            {
                                symbolContext.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule, methodSymbol.ToDisplayString()));
                            }

                            last--;
                        }
                    }

                    // Ignore multiple cancellation token parameters at the end of the parameter list.
                    while (last >= 0 && methodSymbol.Parameters[last].Type.Equals(cancellationTokenType))
                    {
                        last--;
                    }

                    // Ignore parameters passed by reference when they appear at the end of the parameter list.
                    while (last >= 0 && methodSymbol.Parameters[last].RefKind != RefKind.None)
                    {
                        last--;
                    }

                    // Ignore IProgress<T> when last
                    if (last >= 0
                        && iprogressType != null
                        && methodSymbol.Parameters[last].Type.OriginalDefinition.Equals(iprogressType))
                    {
                        last--;
                    }

                    // Ignore parameters that hav any of these attributes.
                    // C# reserved attributes: https://docs.microsoft.com/dotnet/csharp/language-reference/attributes/caller-information
                    var callerInformationAttributes = ImmutableArray.Create<INamedTypeSymbol>(
                        compilationContext.Compilation.GetTypeByMetadaName(typeof(CallerFilePathAttribute).FullName),
                        compilationContext.Compilation.GetTypeByMetadaName(typeof(CallerLineNumberAttribute).FullName),
                        compilationContext.Compilation.GetTypeByMetadaName(typeof(CallerMemberNameAttribute).FullName));

                    while (last >= 0 && HasCallerInformationAttribute(methodSymbol.Parameters[last], callerInformationAttributes))
                    {
                        last--;
                    }

                    for (int i = last - 1; i >= 0; i--)
                    {
                        ITypeSymbol parameterType = methodSymbol.Parameters[i].Type;
                        if (!parameterType.Equals(cancellationTokenType))
                        {
                            continue;
                        }

                        // Bail if the CancellationToken is the first parameter of an extension method.
                        if (i == 0 && methodSymbol.IsExtensionMethod)
                        {
                            continue;
                        }

                        symbolContext.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule, methodSymbol.ToDisplayString()));
                        break;
                    }
                },
                SymbolKind.Method);
            });
        }

        private static bool HasCallerInformationAttribute(IParameterSymbol parameter, ImmutableArray<INamedTypeSymbol> callerAttributes)
        {
            var attributes = parameter.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (callerAttributes.Any(callerAttribute => SymbolEqualityComparer.Default.Equals(callerAttribute, attribute.AttributeClass)))
                {
                    return true;
                }
            }
            retrun false;
        }
    }
}

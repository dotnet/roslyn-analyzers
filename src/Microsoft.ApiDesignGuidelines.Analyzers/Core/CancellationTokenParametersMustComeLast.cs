// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1068: CancellationToken parameters must come last.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class CancellationTokenParametersMustComeLastAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1068";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CancellationTokenParametersMustComeLastMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol cancellationTokenType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
                if (cancellationTokenType != null)
                {
                    compilationContext.RegisterSymbolAction(symbolContext =>
                    {
                        var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                        if (methodSymbol.IsOverride
                            || methodSymbol.ExplicitInterfaceImplementations.Any()
                            || ImplementsAnInterfaceMethodImplicitly(methodSymbol))
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
                                    symbolContext.ReportDiagnostic(Diagnostic.Create(
                                        Rule, methodSymbol.Locations.First(), methodSymbol.ToDisplayString()));
                                }

                                last--;
                            }
                        }

                        while (last >= 0 && methodSymbol.Parameters[last].RefKind != RefKind.None)
                        {
                            last--;
                        }

                        for (int i = last; i >= 0; i--)
                        {
                            ITypeSymbol parameterType = methodSymbol.Parameters[i].Type;
                            if (parameterType.Equals(cancellationTokenType)
                                && i != last)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Rule, methodSymbol.Locations.First(), methodSymbol.ToDisplayString()));
                                break;
                            }
                        }
                    },
                    SymbolKind.Method);
                }
            });
        }

        private bool ImplementsAnInterfaceMethodImplicitly(IMethodSymbol methodSymbol)
        {
            // This is an approximation, because another class could derive from this one
            // and rely on methodSymbol implementing one of *it's* interfaces methods, but
            // it's good enough.
            foreach (INamedTypeSymbol interfaceSymbol in methodSymbol.ContainingType.AllInterfaces)
            {
                foreach (ISymbol interfaceMethod in interfaceSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Method))
                {
                    if (methodSymbol.ContainingType.FindImplementationForInterfaceMember(interfaceMethod)?.Equals(methodSymbol) ?? false)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

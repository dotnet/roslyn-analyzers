// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed class ExternalApiAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ExternalApiShouldBeAccessibleTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ExternalApiShouldBeAccessibleMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ExternalApiShouldBeAccessibleDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.ExternalApiShouldBeAccessibleRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var externalApiAttribute = compilationContext.Compilation.GetTypeByMetadataName("Roslyn.Utilities.ExternalApiAttribute");
                var attributeUsageAttribute = WellKnownTypes.AttributeUsageAttribute(compilationContext.Compilation);
                if (externalApiAttribute is null)
                {
                    // We don't need to check assemblies unless they're referencing Roslyn, so we're done
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext =>
                {
                    var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                    var namedTypeAttributes = namedType.GetApplicableAttributes(attributeUsageAttribute);
                    if (!IsExternalApi(namedType, externalApiAttribute))
                    {
                        return;
                    }

                    AnalyzeExternalApi(ref symbolContext, symbolContext.Symbol, externalApiAttribute);
                }, SymbolKind.NamedType);
            });
        }

        private void AnalyzeExternalApi(ref SymbolAnalysisContext context, ISymbol symbol, INamedTypeSymbol externalApiAttribute)
        {
            // Check containing symbols
            if (!IsVisibleToExternalApi(symbol, externalApiAttribute, out var compilerVerified, out var invisibleSymbol))
            {
                if (!compilerVerified)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations.First(), invisibleSymbol.ToDisplayString()));
                }
            }

            // Check types in signature
            foreach (var signatureSymbol in GetTypesInSignature(symbol))
            {
                if (!IsVisibleToExternalApi(signatureSymbol, externalApiAttribute, out compilerVerified, out _))
                {
                    if (!compilerVerified)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations.First(), signatureSymbol.ToDisplayString()));
                    }
                }
            }

            // Check public members
            if (symbol is ITypeSymbol typeSymbol)
            {
                foreach (var memberSymbol in typeSymbol.GetMembers())
                {
                    if (memberSymbol.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    AnalyzeExternalApi(ref context, memberSymbol, externalApiAttribute);
                }
            }
        }

        private static HashSet<ISymbol> GetTypesInSignature(ISymbol symbol)
        {
            var result = new HashSet<ISymbol>();
            AddSignatureTypes(symbol, result);
            return result;

            static void AddSignatureTypes(ISymbol symbol, HashSet<ISymbol> visited)
            {
                if (symbol is null || !visited.Add(symbol))
                {
                    return;
                }

                switch (symbol)
                {
                    case IFieldSymbol fieldSymbol:
                        AddSignatureTypes(fieldSymbol.Type, visited);
                        break;

                    case IEventSymbol eventSymbol:
                        AddSignatureTypes(eventSymbol.Type, visited);
                        break;

                    case IPropertySymbol propertySymbol:
                        AddSignatureTypes(propertySymbol.Type, visited);
                        foreach (var parameter in propertySymbol.Parameters)
                        {
                            AddSignatureTypes(parameter, visited);
                        }

                        break;

                    case IParameterSymbol parameterSymbol:
                        AddSignatureTypes(parameterSymbol.Type, visited);
                        break;

                    case ITypeParameterSymbol typeParameterSymbol:
                        foreach (var constraint in typeParameterSymbol.ConstraintTypes)
                        {
                            AddSignatureTypes(constraint, visited);
                        }

                        break;

                    case IMethodSymbol methodSymbol:
                        AddSignatureTypes(methodSymbol.ReturnType, visited);
                        foreach (var parameter in methodSymbol.Parameters)
                        {
                            AddSignatureTypes(parameter, visited);
                        }

                        foreach (var typeArgument in methodSymbol.TypeArguments)
                        {
                            AddSignatureTypes(typeArgument, visited);
                        }

                        break;

                    case IArrayTypeSymbol arrayTypeSymbol:
                        AddSignatureTypes(arrayTypeSymbol.ElementType, visited);
                        break;

                    case IPointerTypeSymbol pointerTypeSymbol:
                        AddSignatureTypes(pointerTypeSymbol.PointedAtType, visited);
                        break;

                    case INamedTypeSymbol namedTypeSymbol:
                        AddSignatureTypes(namedTypeSymbol.BaseType, visited);
                        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                        {
                            AddSignatureTypes(typeArgument, visited);
                        }

                        AddSignatureTypes(namedTypeSymbol.OriginalDefinition, visited);
                        break;

                    default:
                        break;
                }
            }
        }

        private static bool IsVisibleToExternalApi(ISymbol symbol, INamedTypeSymbol externalApiAttribute, out bool compilerVerified, out ISymbol invisibleSymbol)
        {
            compilerVerified = true;
            for (var currentSymbol = symbol; currentSymbol is object; currentSymbol = currentSymbol.ContainingSymbol)
            {
                if (currentSymbol.Kind == SymbolKind.Assembly
                    || currentSymbol.Kind == SymbolKind.Namespace
                    || currentSymbol.Kind == SymbolKind.NetModule)
                {
                    break;
                }

                if (currentSymbol.DeclaredAccessibility == Accessibility.Public)
                {
                    continue;
                }

                compilerVerified = false;
                if (!IsExternalApi(currentSymbol, externalApiAttribute))
                {
                    invisibleSymbol = currentSymbol;
                    return false;
                }
            }

            invisibleSymbol = null;
            return true;
        }

        private static bool IsExternalApi(ISymbol symbol, INamedTypeSymbol externalApiAttribute)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (Equals(attribute.AttributeClass, externalApiAttribute))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Generic;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1707: Identifiers should not contain underscores
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldNotContainUnderscoresAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1707";
        private const string Uri = "https://msdn.microsoft.com/en-us/library/ms182245.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageAssembly = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageAssembly), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNamespace = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageNamespace), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageType = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageType), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMember = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMember), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeTypeParameter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageTypeTypeParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMethodTypeParameter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMethodTypeParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberParameter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageMemberParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDelegateParameter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresMessageDelegateParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainUnderscoresDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor AssemblyRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageAssembly,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespace,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageType,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMember,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeTypeParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MethodTypeParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMethodTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DelegateParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDelegateParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssemblyRule, NamespaceRule, TypeRule, MemberRule, TypeTypeParameterRule, MethodTypeParameterRule, MemberParameterRule, DelegateParameterRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(symbolAnalysisContext =>
            {
                var symbol = symbolAnalysisContext.Symbol;

                // FxCop compat: only analyze externally visible symbols
                if (!symbol.IsExternallyVisible())
                {
                    return;
                }

                switch (symbol.Kind)
                {
                    case SymbolKind.Namespace:
                        {
                            if (ContainsUnderScore(symbol.Name))
                            {
                                symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(NamespaceRule, symbol.ToDisplayString()));
                            }

                            return;
                        }

                    case SymbolKind.NamedType:
                        {
                            var namedType = symbol as INamedTypeSymbol;
                            AnalyzeTypeParameters(symbolAnalysisContext, namedType.TypeParameters);

                            if (namedType.TypeKind == TypeKind.Delegate &&
                                namedType.DelegateInvokeMethod != null)
                            {
                                AnalyzeParameters(symbolAnalysisContext, namedType.DelegateInvokeMethod.Parameters);
                            }

                            if (!ContainsUnderScore(symbol.Name))
                            {
                                return;
                            }

                            symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(TypeRule, symbol.ToDisplayString()));
                            return;
                        }

                    case SymbolKind.Field:
                        {
                            var fieldSymbol = symbol as IFieldSymbol;
                            if (ContainsUnderScore(symbol.Name) && (fieldSymbol.IsConst || (fieldSymbol.IsStatic && fieldSymbol.IsReadOnly)))
                            {
                                symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                                return;
                            }

                            return;
                        }

                    default:
                        {
                            if (symbol is IMethodSymbol methodSymbol)
                            {
                                if (methodSymbol.IsOperator())
                                {
                                    // Do not flag for operators.
                                    return;
                                }

                                if (methodSymbol.MethodKind == MethodKind.Conversion)
                                {
                                    // Do not flag for conversion methods generated for operators.
                                    return;
                                }

                                AnalyzeParameters(symbolAnalysisContext, methodSymbol.Parameters);
                                AnalyzeTypeParameters(symbolAnalysisContext, methodSymbol.TypeParameters);
                            }

                            if (symbol is IPropertySymbol propertySymbol)
                            {
                                AnalyzeParameters(symbolAnalysisContext, propertySymbol.Parameters);
                            }

                            if (!ContainsUnderScore(symbol.Name) || IsInvalidSymbol(symbol))
                            {
                                return;
                            }

                            symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                            return;
                        }
                }
            },
            SymbolKind.Namespace, // Namespace
            SymbolKind.NamedType, //Type
            SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event // Members
            );

            analysisContext.RegisterCompilationAction(compilationAnalysisContext =>
            {
                var compilation = compilationAnalysisContext.Compilation;
                if (ContainsUnderScore(compilation.AssemblyName))
                {
                    compilationAnalysisContext.ReportDiagnostic(compilation.Assembly.CreateDiagnostic(AssemblyRule, compilation.AssemblyName));
                }
            });
        }

        private static bool IsInvalidSymbol(ISymbol symbol)
        {
            return (!(symbol.IsExternallyVisible() && !symbol.IsOverride)) ||
                symbol.IsAccessorMethod() || symbol.IsImplementationOfAnyInterfaceMember();
        }

        private static void AnalyzeParameters(SymbolAnalysisContext symbolAnalysisContext, IEnumerable<IParameterSymbol> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (ContainsUnderScore(parameter.Name))
                {
                    var containingType = parameter.ContainingType;

                    // Parameter in Delegate
                    if (containingType.TypeKind == TypeKind.Delegate)
                    {
                        if (containingType.IsPublic())
                        {
                            symbolAnalysisContext.ReportDiagnostic(parameter.CreateDiagnostic(DelegateParameterRule, containingType.ToDisplayString(), parameter.Name));
                        }
                    }
                    else if (!IsInvalidSymbol(parameter.ContainingSymbol))
                    {
                        symbolAnalysisContext.ReportDiagnostic(parameter.CreateDiagnostic(MemberParameterRule, parameter.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), parameter.Name));
                    }
                }
            }
        }

        private static void AnalyzeTypeParameters(SymbolAnalysisContext symbolAnalysisContext, IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            foreach (var typeParameter in typeParameters)
            {
                if (ContainsUnderScore(typeParameter.Name))
                {
                    var containingSymbol = typeParameter.ContainingSymbol;
                    if (containingSymbol.Kind == SymbolKind.NamedType)
                    {
                        if (containingSymbol.IsPublic())
                        {
                            symbolAnalysisContext.ReportDiagnostic(typeParameter.CreateDiagnostic(TypeTypeParameterRule, containingSymbol.ToDisplayString(), typeParameter.Name));
                        }
                    }
                    else if (containingSymbol.Kind == SymbolKind.Method && !IsInvalidSymbol(containingSymbol))
                    {
                        symbolAnalysisContext.ReportDiagnostic(typeParameter.CreateDiagnostic(MethodTypeParameterRule, containingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), typeParameter.Name));
                    }
                }
            }
        }

        private static bool ContainsUnderScore(string identifier)
        {
            return identifier.IndexOf('_') != -1;
        }
    }
}
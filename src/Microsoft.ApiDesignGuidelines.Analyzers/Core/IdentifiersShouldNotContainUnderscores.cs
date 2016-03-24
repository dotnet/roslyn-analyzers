// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1707: Identifiers should not contain underscores
    /// </summary>
    public abstract class IdentifiersShouldNotContainUnderscoresAnalyzer<TLanguageSyntaxKind> : DiagnosticAnalyzer
        where TLanguageSyntaxKind : struct
    {
        internal const string RuleId = "CA1707";
        private const string _uri = "https://msdn.microsoft.com/en-us/library/ms182245.aspx";

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
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespace,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageType,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMember,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeTypeParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MethodTypeParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMethodTypeParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DelegateParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDelegateParameter,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: _uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssemblyRule, NamespaceRule, TypeRule, MemberRule, TypeTypeParameterRule, MethodTypeParameterRule, MemberParameterRule, DelegateParameterRule);

        public abstract TLanguageSyntaxKind[] SyntaxKinds { get; }

        private ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ISymbol, List<ISymbol>>> _typeDeclaredMemberOverridingBaseMapping;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                _typeDeclaredMemberOverridingBaseMapping = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ISymbol, List<ISymbol>>>();

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    var symbol = symbolAnalysisContext.Symbol;
                    if (ContainsUnderScore(symbol.Name))
                    {
                        switch (symbol.Kind)
                        {
                            case SymbolKind.Namespace:
                                {
                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(NamespaceRule, symbol.ToDisplayString()));
                                    return;
                                }

                            case SymbolKind.NamedType:
                                {
                                    if (!symbol.IsPublic())
                                    {
                                        return;
                                    }

                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(TypeRule, symbol.ToDisplayString()));
                                    return;
                                }

                            case SymbolKind.Field:
                                {
                                    var fieldSymbol = symbol as IFieldSymbol;
                                    if (symbol.IsPublic() && (fieldSymbol.IsConst || (fieldSymbol.IsStatic && fieldSymbol.IsReadOnly)))
                                    {
                                        symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                                        return;
                                    }

                                    return;
                                }

                            default:
                                {
                                    if (IsInvalidSymbol(symbol))
                                    {
                                        return;
                                    }

                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                                    return;
                                }
                        }
                    }
                },
                SymbolKind.Namespace, // Namespace
                SymbolKind.NamedType, //Type
                SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event // Members
                );

                compilationStartAnalysisContext.RegisterSyntaxNodeAction(syntaxNodeAnalysisContext =>
                {
                    AnalyzeSyntaxNode(syntaxNodeAnalysisContext);
                },SyntaxKinds);
            });

            analysisContext.RegisterCompilationAction(compilationAnalysisContext =>
            {
                var compilation = compilationAnalysisContext.Compilation;
                if (ContainsUnderScore(compilation.AssemblyName))
                {
                    compilationAnalysisContext.ReportDiagnostic(compilation.Assembly.CreateDiagnostic(AssemblyRule, compilation.AssemblyName));
                }
            });
        }

        private bool IsInvalidSymbol(ISymbol symbol)
        {
            return (!(symbol.GetResultantVisibility() == SymbolVisibility.Public && !symbol.IsOverride)) ||
                symbol.IsAccessorMethod() || IsInterfaceImplementation(symbol);
        }

        protected void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var symbol = syntaxNodeAnalysisContext.SemanticModel.GetDeclaredSymbol(syntaxNodeAnalysisContext.Node);
            if (symbol == null || !(symbol.Kind == SymbolKind.Parameter || symbol.Kind == SymbolKind.TypeParameter))
            {
                return;
            }

            if (ContainsUnderScore(symbol.Name))
            {
                if (symbol.Kind == SymbolKind.Parameter)
                {
                    var containingType = symbol.ContainingType;

                    // Parameter in Delegate
                    if (containingType.TypeKind == TypeKind.Delegate)
                    {
                        if (containingType.IsPublic())
                        {
                            syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(DelegateParameterRule, containingType.ToDisplayString(), symbol.Name));
                        }
                    }
                    else if (!IsInvalidSymbol(symbol.ContainingSymbol))
                    {
                        syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberParameterRule, symbol.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), symbol.Name));
                    }
                }
                // symbol is TypeParameter
                else
                {
                    var containingSymbol = symbol.ContainingSymbol;
                    if (containingSymbol.Kind == SymbolKind.NamedType)
                    {
                        if (containingSymbol.IsPublic())
                        {
                            syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(TypeTypeParameterRule, containingSymbol.ToDisplayString(), symbol.Name));
                        }
                    }
                    else if (containingSymbol.Kind == SymbolKind.Method && !IsInvalidSymbol(containingSymbol))
                    {
                        syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MethodTypeParameterRule, containingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), symbol.Name));
                    }
                }
            }
        }

        private bool IsInterfaceImplementation(ISymbol symbol)
        {
            return symbol.IsExplicitInterfaceImplementation() || IsImplicitInterfaceImplementation(symbol);
        }

        private bool IsImplicitInterfaceImplementation(ISymbol symbol)
        {
            INamedTypeSymbol declaringType = symbol.ContainingType;
            if (declaringType.AllInterfaces.Any())
            {
                ConcurrentDictionary<ISymbol, List<ISymbol>> declaredMemberToInterfaceMembers = _typeDeclaredMemberOverridingBaseMapping.GetOrAdd(declaringType, declaringTypeKey =>
                {
                    var declaredMemberSymbolsToImplementedInterfaceMembersMap = new ConcurrentDictionary<ISymbol, List<ISymbol>>();
                    foreach (INamedTypeSymbol implementedInterface in declaringTypeKey.AllInterfaces)
                    {
                        foreach (ISymbol member in implementedInterface.GetMembers())
                        {
                            ISymbol implementedSymbol = declaringTypeKey.FindImplementationForInterfaceMember(member);
                            if (implementedSymbol != null)
                            {
                                List<ISymbol> implementedSymbolImplementingInterfaceMembers = declaredMemberSymbolsToImplementedInterfaceMembersMap.GetOrAdd(implementedSymbol, s => new List<ISymbol>());
                                implementedSymbolImplementingInterfaceMembers.Add(member);
                            }
                        }
                    }

                    return declaredMemberSymbolsToImplementedInterfaceMembersMap;
                });

                List<ISymbol> implementedInterfaceMembers;
                if (declaredMemberToInterfaceMembers.TryGetValue(symbol, out implementedInterfaceMembers))
                {
                    return implementedInterfaceMembers.Any();
                }

                return false;
            }

            return false;
        }

        private static bool ContainsUnderScore(string identifier)
        {
            return identifier.IndexOf('_') != -1;
        }
    }
}
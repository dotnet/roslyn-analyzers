// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1707: Identifiers should not contain underscores
    /// </summary>
    public abstract class IdentifiersShouldNotContainUnderscoresAnalyzer : DiagnosticAnalyzer
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

        private ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ISymbol, List<ISymbol>>> _typeDeclaredMemberMapping;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                _typeDeclaredMemberMapping = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ISymbol, List<ISymbol>>>();

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    var symbol = symbolAnalysisContext.Symbol;
                    if (ContainsUnderScore(symbol.Name))
                    {
                        switch (symbol.Kind)
                        {
                            case SymbolKind.Namespace:
                                {
                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(NamespaceRule, symbol.Name));
                                    return;
                                }

                            case SymbolKind.NamedType:
                                {
                                    if (!symbol.IsPublic())
                                    {
                                        return;
                                    }

                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(TypeRule, symbol.Name));
                                    return;
                                }

                            case SymbolKind.Field:
                                {
                                    var fieldSymbol = symbol as IFieldSymbol;
                                    if (symbol.IsPublic() && (fieldSymbol.IsConst || (fieldSymbol.IsStatic && fieldSymbol.IsReadOnly)))
                                    {
                                        symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.Name));
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

                                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberRule, symbol.Name));
                                    return;
                                }
                        }
                    }
                },
                SymbolKind.Namespace, // Namespace
                SymbolKind.NamedType, //Type
                SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event // Members
                );

                GetSyntaxNodeDiagnostics(compilationStartAnalysisContext);
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
            return (!((symbol.IsPublic() || symbol.IsProtected()) && !symbol.IsOverride)) ||
                symbol.IsAccessorMethod() || IsInterfaceImplementation(symbol);
        }

        internal abstract void GetSyntaxNodeDiagnostics(CompilationStartAnalysisContext compilationStartAnalysisContext);

        protected void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var symbol = syntaxNodeAnalysisContext.SemanticModel.GetDeclaredSymbol(syntaxNodeAnalysisContext.Node);
            if (symbol == null || symbol.Kind != SymbolKind.Parameter)
            {
                return;
            }

            if (ContainsUnderScore(symbol.Name))
            {
                var containingType = symbol.ContainingType;

                // This is parameter in Delegate
                if (containingType.TypeKind == TypeKind.Delegate && containingType.IsPublic())
                {
                    syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(DelegateParameterRule, containingType.Name, symbol.Name));
                }
                //else
                //{
                //    syntaxNodeAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(MemberParameterRule, symbol.ContainingSymbol.Name, symbol.Name));
                //}
            }
        }

        private bool IsInterfaceImplementation(ISymbol symbol)
        {
            return IsExplicitInterfaceImplementation(symbol) || IsImplicitInterfaceImplementation(symbol);
        }

        private bool IsImplicitInterfaceImplementation(ISymbol symbol)
        {
            INamedTypeSymbol declaringType = symbol.ContainingType;
            if (declaringType.AllInterfaces.Any())
            {
                ConcurrentDictionary<ISymbol, List<ISymbol>> declaredMemberToInterfaceMembers = _typeDeclaredMemberMapping.GetOrAdd(declaringType, declaringTypeKey =>
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

        private bool IsExplicitInterfaceImplementation(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol != null && methodSymbol.ExplicitInterfaceImplementations.Any())
            {
                return true;
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null && propertySymbol.ExplicitInterfaceImplementations.Any())
            {
                return true;
            }

            return false;
        }

        private bool ContainsUnderScore(string identifier)
        {
            return identifier.IndexOf('_') != -1;
        }
    }
}
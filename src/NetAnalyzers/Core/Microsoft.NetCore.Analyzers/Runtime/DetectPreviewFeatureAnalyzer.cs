﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// Detect the use of [RequiresPreviewFeatures] in assemblies that have not opted into preview features
    /// </summary>
    public abstract class DetectPreviewFeatureAnalyzer<
        TBaseTypeOrImplementsOrInherits,
        TBaseTypeDeclarationSyntax,
        TTypeConstraintSyntax,
        TTypeArgumentList,
        TParameterSyntax> : DiagnosticAnalyzer
        where TBaseTypeOrImplementsOrInherits : SyntaxNode
        where TBaseTypeDeclarationSyntax : SyntaxNode
        where TTypeConstraintSyntax : SyntaxNode
        where TTypeArgumentList : SyntaxNode
        where TParameterSyntax : SyntaxNode
    {
        internal const string RuleId = "CA2252";
        internal const string DefaultURL = "https://aka.ms/dotnet-warnings/preview-features";
        private static readonly LocalizableString s_localizableTitle = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesTitle));
        private static readonly LocalizableString s_localizableDescription = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDescription));
        private static readonly ImmutableArray<SymbolKind> s_symbols = ImmutableArray.Create(SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event);

        internal static readonly DiagnosticDescriptor GeneralPreviewFeatureAttributeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                    s_localizableTitle,
                                                                                                                    CreateLocalizableResourceString(nameof(DetectPreviewFeaturesMessage)),
                                                                                                                    DiagnosticCategory.Usage,
                                                                                                                    RuleLevel.BuildError,
                                                                                                                    s_localizableDescription,
                                                                                                                    isPortedFxCopRule: false,
                                                                                                                    isDataflowRule: false);
        internal static readonly DiagnosticDescriptor GeneralPreviewFeatureAttributeRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                    s_localizableTitle,
                                                                                                                    CreateLocalizableResourceString(nameof(DetectPreviewFeaturesMessageWithCustomMessagePlaceholder)),
                                                                                                                    DiagnosticCategory.Usage,
                                                                                                                    RuleLevel.BuildError,
                                                                                                                    s_localizableDescription,
                                                                                                                    isPortedFxCopRule: false,
                                                                                                                    isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ImplementsPreviewInterfaceRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(ImplementsPreviewInterfaceMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);
        internal static readonly DiagnosticDescriptor ImplementsPreviewInterfaceRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(ImplementsPreviewInterfaceMessageWithCustomMessagePlaceholder)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ImplementsPreviewMethodRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(ImplementsPreviewMethodMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);
        internal static readonly DiagnosticDescriptor ImplementsPreviewMethodRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(ImplementsPreviewMethodMessageWithCustomMessagePlaceholder)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static readonly DiagnosticDescriptor OverridesPreviewMethodRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(OverridesPreviewMethodMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);
        internal static readonly DiagnosticDescriptor OverridesPreviewMethodRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(OverridesPreviewMethodMessageWithCustomMessagePlaceholder)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static readonly DiagnosticDescriptor DerivesFromPreviewClassRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                             s_localizableTitle,
                                                                                                             CreateLocalizableResourceString(nameof(DerivesFromPreviewClassMessage)),
                                                                                                             DiagnosticCategory.Usage,
                                                                                                             RuleLevel.BuildError,
                                                                                                             s_localizableDescription,
                                                                                                             isPortedFxCopRule: false,
                                                                                                             isDataflowRule: false);
        internal static readonly DiagnosticDescriptor DerivesFromPreviewClassRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                             s_localizableTitle,
                                                                                                             CreateLocalizableResourceString(nameof(DerivesFromPreviewClassMessageWithCustomMessagePlaceholder)),
                                                                                                             DiagnosticCategory.Usage,
                                                                                                             RuleLevel.BuildError,
                                                                                                             s_localizableDescription,
                                                                                                             isPortedFxCopRule: false,
                                                                                                             isDataflowRule: false);

        internal static readonly DiagnosticDescriptor UsesPreviewTypeParameterRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(UsesPreviewTypeParameterMessage)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);
        internal static readonly DiagnosticDescriptor UsesPreviewTypeParameterRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(UsesPreviewTypeParameterMessageWithCustomMessagePlaceholder)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);

        internal static readonly DiagnosticDescriptor MethodReturnsPreviewTypeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(MethodReturnsPreviewTypeMessage)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);
        internal static readonly DiagnosticDescriptor MethodReturnsPreviewTypeRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(MethodReturnsPreviewTypeMessageWithCustomMessagePlaceholder)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);

        internal static readonly DiagnosticDescriptor MethodUsesPreviewTypeAsParameterRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                      s_localizableTitle,
                                                                                                                      CreateLocalizableResourceString(nameof(MethodUsesPreviewTypeAsParameterMessage)),
                                                                                                                      DiagnosticCategory.Usage,
                                                                                                                      RuleLevel.BuildError,
                                                                                                                      s_localizableDescription,
                                                                                                                      isPortedFxCopRule: false,
                                                                                                                      isDataflowRule: false);
        internal static readonly DiagnosticDescriptor MethodUsesPreviewTypeAsParameterRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                      s_localizableTitle,
                                                                                                                      CreateLocalizableResourceString(nameof(MethodUsesPreviewTypeAsParameterMessageWithCustomMessagePlaceholder)),
                                                                                                                      DiagnosticCategory.Usage,
                                                                                                                      RuleLevel.BuildError,
                                                                                                                      s_localizableDescription,
                                                                                                                      isPortedFxCopRule: false,
                                                                                                                      isDataflowRule: false);
        internal static readonly DiagnosticDescriptor FieldOrEventIsPreviewTypeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                        s_localizableTitle,
                                                                                                        CreateLocalizableResourceString(nameof(FieldIsPreviewTypeMessage)),
                                                                                                        DiagnosticCategory.Usage,
                                                                                                        RuleLevel.BuildError,
                                                                                                        s_localizableDescription,
                                                                                                        isPortedFxCopRule: false,
                                                                                                        isDataflowRule: false);
        internal static readonly DiagnosticDescriptor FieldOrEventIsPreviewTypeRuleWithCustomMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                        s_localizableTitle,
                                                                                                        CreateLocalizableResourceString(nameof(FieldIsPreviewTypeMessageWithCustomMessagePlaceholder)),
                                                                                                        DiagnosticCategory.Usage,
                                                                                                        RuleLevel.BuildError,
                                                                                                        s_localizableDescription,
                                                                                                        isPortedFxCopRule: false,
                                                                                                        isDataflowRule: false);

        internal static readonly DiagnosticDescriptor StaticAbstractIsPreviewFeatureRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                    s_localizableTitle,
                                                                                                                    CreateLocalizableResourceString(nameof(StaticAndAbstractRequiresPreviewFeatures)),
                                                                                                                    DiagnosticCategory.Usage,
                                                                                                                    RuleLevel.BuildError,
                                                                                                                    s_localizableDescription,
                                                                                                                    isPortedFxCopRule: false,
                                                                                                                    isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            GeneralPreviewFeatureAttributeRule,
            ImplementsPreviewInterfaceRule,
            ImplementsPreviewMethodRule,
            OverridesPreviewMethodRule,
            DerivesFromPreviewClassRule,
            UsesPreviewTypeParameterRule,
            MethodReturnsPreviewTypeRule,
            MethodUsesPreviewTypeAsParameterRule,
            FieldOrEventIsPreviewTypeRule,
            GeneralPreviewFeatureAttributeRuleWithCustomMessage,
            ImplementsPreviewInterfaceRuleWithCustomMessage,
            ImplementsPreviewMethodRuleWithCustomMessage,
            OverridesPreviewMethodRuleWithCustomMessage,
            DerivesFromPreviewClassRuleWithCustomMessage,
            UsesPreviewTypeParameterRuleWithCustomMessage,
            MethodReturnsPreviewTypeRuleWithCustomMessage,
            MethodUsesPreviewTypeAsParameterRuleWithCustomMessage,
            FieldOrEventIsPreviewTypeRuleWithCustomMessage,
            StaticAbstractIsPreviewFeatureRule);

        protected abstract SyntaxNode? GetConstraintSyntaxNodeForTypeConstrainedByPreviewTypes(ISymbol typeOrMethodSymbol, ISymbol previewInterfaceConstraintSymbol);

        protected abstract SyntaxNode? GetPreviewImplementsClauseSyntaxNodeForMethodOrProperty(ISymbol methodOrPropertySymbol, ISymbol previewSymbol);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeVersioningRequiresPreviewFeaturesAttribute, out var previewFeaturesAttribute))
                {
                    return;
                }

                if (context.Compilation.Assembly.HasAttribute(previewFeaturesAttribute))
                {
                    // This assembly has enabled preview attributes.
                    return;
                }

                ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols = new();

                IFieldSymbol? virtualStaticsInInterfaces = null;
                if (context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesRuntimeFeature, out INamedTypeSymbol? runtimeFeatureType))
                {
                    virtualStaticsInInterfaces = runtimeFeatureType
                        .GetMembers("VirtualStaticsInInterfaces")
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault();

                    if (virtualStaticsInInterfaces != null)
                    {
                        SymbolIsAnnotatedAsPreview(virtualStaticsInInterfaces, requiresPreviewFeaturesSymbols, previewFeaturesAttribute);
                    }
                }

                // Handle symbol operations involving preview features
                context.RegisterOperationAction(context => BuildSymbolInformationFromOperations(context, requiresPreviewFeaturesSymbols, previewFeaturesAttribute),
                    OperationKind.Invocation,
                    OperationKind.ObjectCreation,
                    OperationKind.PropertyReference,
                    OperationKind.FieldReference,
                    OperationKind.DelegateCreation,
                    OperationKind.EventReference,
                    OperationKind.Unary,
                    OperationKind.Binary,
                    OperationKind.ArrayCreation,
                    OperationKind.CatchClause,
                    OperationKind.TypeOf,
                    OperationKind.EventAssignment
                    );

                // Handle preview symbol definitions
                context.RegisterSymbolAction(context => AnalyzeSymbol(context, requiresPreviewFeaturesSymbols, virtualStaticsInInterfaces, previewFeaturesAttribute), s_symbols);
                AnalyzeTypeSyntax(context, requiresPreviewFeaturesSymbols, symbol => SymbolIsAnnotatedAsPreview(symbol, requiresPreviewFeaturesSymbols, previewFeaturesAttribute));
            });
        }

        protected abstract void AnalyzeTypeSyntax(
            CompilationStartAnalysisContext context,
            ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
            Func<ISymbol, bool> symbolIsAnnotatedAsPreview);

        /// <summary>
        /// Returns null if the type arguments are not preview
        /// </summary>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the typeArguments was something like List[List[List[PreviewType]]]], this function will return PreviewType"
        /// </remarks>
        private static ISymbol? GetPreviewSymbolForGenericTypesFromTypeArguments(ImmutableArray<ITypeSymbol> typeArguments,
            ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
            INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            for (int i = 0; i < typeArguments.Length; i++)
            {
                ITypeSymbol typeParameter = typeArguments[i];
                while (typeParameter is IArrayTypeSymbol array)
                {
                    typeParameter = array.ElementType;
                }

                if (typeParameter is INamedTypeSymbol innerNamedType && innerNamedType.Arity > 0)
                {
                    ISymbol? previewSymbol = GetPreviewSymbolForGenericTypesFromTypeArguments(innerNamedType.TypeArguments, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol);
                    if (previewSymbol != null)
                    {
                        return previewSymbol;
                    }
                }
                if (SymbolIsAnnotatedAsPreview(typeParameter, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    return typeParameter;
                }
            }

            return null;
        }

        private void ProcessTypeSymbolAttributes(SymbolAnalysisContext context,
                                                 ITypeSymbol symbol,
                                                 ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                 INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            if (ProcessTypeAttributeForPreviewness(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out SyntaxReference? attributeSyntaxReference, out string? attributeName, out ISymbol? previewAttributeSymbol))
            {
                ReportDiagnosticWithCustomMessageIfItExists(context, attributeSyntaxReference.GetSyntax(context.CancellationToken), previewAttributeSymbol, requiresPreviewFeaturesSymbols, GeneralPreviewFeatureAttributeRule, GeneralPreviewFeatureAttributeRuleWithCustomMessage, attributeName);
            }
        }

        private static bool ProcessTypeAttributeForPreviewness(ISymbol symbol,
                                                               ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                               INamedTypeSymbol previewFeatureAttribute,
                                                               [NotNullWhen(true)] out SyntaxReference? attributeSyntaxReference,
                                                               [NotNullWhen(true)] out string? attributeName,
                                                               [NotNullWhen(true)] out ISymbol? previewSymbol)
        {
            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                AttributeData attribute = attributes[i];
                if (SymbolIsAnnotatedAsPreview(attribute.AttributeClass, requiresPreviewFeaturesSymbols, previewFeatureAttribute))
                {
                    attributeName = attribute.AttributeClass.Name;
                    attributeSyntaxReference = attribute.ApplicationSyntaxReference;
                    previewSymbol = attribute.AttributeClass;
                    return true;
                }

                if (SymbolIsAnnotatedAsPreview(attribute.AttributeConstructor, requiresPreviewFeaturesSymbols, previewFeatureAttribute))
                {
                    attributeName = attribute.AttributeClass.Name;
                    attributeSyntaxReference = attribute.ApplicationSyntaxReference;
                    previewSymbol = attribute.AttributeConstructor;
                    return true;
                }
            }

            attributeName = null;
            attributeSyntaxReference = null;
            previewSymbol = null;
            return false;
        }

        private bool SymbolContainsGenericTypesWithPreviewAttributes(ISymbol symbol,
                                                                     ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                     INamedTypeSymbol previewFeatureAttribute,
                                                                     [NotNullWhen(true)] out ISymbol? previewSymbol,
                                                                     bool checkTypeParametersForPreviewFeatures = true)
        {
            if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.Arity > 0)
            {
                ISymbol? previewTypeArgument = GetPreviewSymbolForGenericTypesFromTypeArguments(typeSymbol.TypeArguments, requiresPreviewFeaturesSymbols, previewFeatureAttribute);
                if (previewTypeArgument != null)
                {
                    previewSymbol = previewTypeArgument;
                    return true;
                }

                if (checkTypeParametersForPreviewFeatures)
                {
                    ImmutableArray<ITypeParameterSymbol> typeParameters = typeSymbol.TypeParameters;
                    if (TypeParametersHavePreviewAttribute(typeSymbol, typeParameters, requiresPreviewFeaturesSymbols, previewFeatureAttribute, out previewSymbol, out _))
                    {
                        return true;
                    }
                }
            }

            if (symbol is IMethodSymbol methodSymbol && methodSymbol.Arity > 0)
            {
                ISymbol? previewTypeArgument = GetPreviewSymbolForGenericTypesFromTypeArguments(methodSymbol.TypeArguments, requiresPreviewFeaturesSymbols, previewFeatureAttribute);
                if (previewTypeArgument != null)
                {
                    previewSymbol = previewTypeArgument;
                    return true;
                }

                if (checkTypeParametersForPreviewFeatures)
                {
                    ImmutableArray<ITypeParameterSymbol> typeParameters = methodSymbol.TypeParameters;
                    if (TypeParametersHavePreviewAttribute(methodSymbol, typeParameters, requiresPreviewFeaturesSymbols, previewFeatureAttribute, out previewSymbol, out _))
                    {
                        return true;
                    }
                }
            }

            previewSymbol = null;
            return false;
        }

        private static bool SymbolIsStaticAndAbstract(ISymbol symbol, ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols, INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            // Static Abstract is only legal on interfaces. Anything else is likely a compile error and we shouldn't tag such cases.
            return symbol.IsStatic && symbol.IsAbstract && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Interface && !SymbolIsAnnotatedAsPreview(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol);
        }

        private void ProcessPropertyOrMethodAttributes(SymbolAnalysisContext context,
                                                       ISymbol propertyOrMethodSymbol,
                                                       ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                       IFieldSymbol? virtualStaticsInInterfaces,
                                                       INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            if (SymbolIsStaticAndAbstract(propertyOrMethodSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
            {
                if (virtualStaticsInInterfaces != null && SymbolIsAnnotatedAsPreview(virtualStaticsInInterfaces, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    context.ReportDiagnostic(propertyOrMethodSymbol.CreateDiagnostic(StaticAbstractIsPreviewFeatureRule));
                }
            }

            if (propertyOrMethodSymbol.IsImplementationOfAnyImplicitInterfaceMember(out ISymbol baseInterfaceMember))
            {
                if (SymbolIsAnnotatedAsPreview(baseInterfaceMember, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    string baseInterfaceMemberName = baseInterfaceMember.ContainingSymbol != null ? baseInterfaceMember.ContainingSymbol.Name + "." + baseInterfaceMember.Name : baseInterfaceMember.Name;
                    SyntaxNode? previewImplementsClause = null;

                    if (propertyOrMethodSymbol.Language is LanguageNames.VisualBasic)
                    {
                        previewImplementsClause = GetPreviewImplementsClauseSyntaxNodeForMethodOrProperty(propertyOrMethodSymbol, baseInterfaceMember);
                    }

                    if (previewImplementsClause != null)
                    {
                        ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, previewImplementsClause, baseInterfaceMember, requiresPreviewFeaturesSymbols,
                            ImplementsPreviewMethodRule, ImplementsPreviewMethodRuleWithCustomMessage, propertyOrMethodSymbol.Name, baseInterfaceMemberName);
                    }
                    else
                    {
                        ReportDiagnosticWithCustomMessageIfItExists(context, baseInterfaceMember, propertyOrMethodSymbol, requiresPreviewFeaturesSymbols,
                            ImplementsPreviewMethodRule, ImplementsPreviewMethodRuleWithCustomMessage, propertyOrMethodSymbol.Name, baseInterfaceMemberName);
                    }
                }
            }

            if (propertyOrMethodSymbol.IsOverride)
            {
                ISymbol overridden = propertyOrMethodSymbol.GetOverriddenMember();
                if (SymbolIsAnnotatedAsPreview(overridden, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    string overriddenName = overridden.ContainingSymbol != null ? overridden.ContainingSymbol.Name + "." + overridden.Name : overridden.Name;
                    ReportDiagnosticWithCustomMessageIfItExists(context, overridden, propertyOrMethodSymbol, requiresPreviewFeaturesSymbols, OverridesPreviewMethodRule, OverridesPreviewMethodRuleWithCustomMessage, propertyOrMethodSymbol.Name, overriddenName);
                }
            }
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context,
                                   ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                   IFieldSymbol? virtualStaticsInInterfaces,
                                   INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            ISymbol symbol = context.Symbol;

            if (SymbolIsAnnotatedAsPreview(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol) ||
                (symbol is IMethodSymbol method && method.AssociatedSymbol != null && SymbolIsAnnotatedAsPreview(method.AssociatedSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol)))
            {
                return;
            }

            if (symbol is ITypeSymbol typeSymbol)
            {
                ProcessTypeSymbolAttributes(context, typeSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol);
            }
            else if (symbol is IMethodSymbol || symbol is IPropertySymbol)
            {
                ProcessPropertyOrMethodAttributes(context, symbol, requiresPreviewFeaturesSymbols, virtualStaticsInInterfaces, previewFeatureAttributeSymbol);
            }
        }

        private void BuildSymbolInformationFromOperations(OperationAnalysisContext context,
                                                          ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                          INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            if (OperationUsesPreviewFeatures(context, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out ISymbol? symbol))
            {
                IOperation operation = context.Operation;
                if (operation is ICatchClauseOperation catchClauseOperation)
                {
                    operation = catchClauseOperation.ExceptionDeclarationOrExpression;
                }

                ReportDiagnosticWithCustomMessageIfItExists(context, operation, symbol, requiresPreviewFeaturesSymbols, GeneralPreviewFeatureAttributeRule, GeneralPreviewFeatureAttributeRuleWithCustomMessage, symbol.Name);
            }
        }

        private bool SymbolIsAnnotatedOrUsesPreviewTypes(ISymbol symbol,
                                                         ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                         INamedTypeSymbol previewFeatureAttributeSymbol,
                                                         [NotNullWhen(true)] out ISymbol? referencedPreviewSymbol)
        {
            if (SymbolIsAnnotatedAsPreview(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
            {
                referencedPreviewSymbol = symbol;
                return true;
            }

            if (SymbolContainsGenericTypesWithPreviewAttributes(symbol,
                                                                requiresPreviewFeaturesSymbols,
                                                                previewFeatureAttributeSymbol,
                                                                out referencedPreviewSymbol,
                                                                checkTypeParametersForPreviewFeatures: false))
            {
                return true;
            }

            referencedPreviewSymbol = null;
            return false;
        }

        private bool OperationUsesPreviewFeatures(OperationAnalysisContext context,
                                                  ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                  INamedTypeSymbol previewFeatureAttributeSymbol,
                                                  [NotNullWhen(true)] out ISymbol? referencedPreviewSymbol)
        {
            IOperation operation = context.Operation;
            ISymbol containingSymbol = context.ContainingSymbol;
            if (SymbolIsAnnotatedAsPreview(containingSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
            {
                referencedPreviewSymbol = null;
                return false;
            }

            ISymbol? symbol = GetOperationSymbol(operation);
            if (symbol != null)
            {
                if (symbol is IPropertySymbol propertySymbol)
                {
                    // bool AProperty => true is different from bool AProperty { get => false }. Handle both here
                    if (SymbolIsAnnotatedOrUsesPreviewTypes(propertySymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out referencedPreviewSymbol))
                    {
                        return true;
                    }

                    ValueUsageInfo usageInfo = operation.GetValueUsageInfo(propertySymbol);
                    if (usageInfo.IsReadFrom())
                    {
                        symbol = propertySymbol.GetMethod;
                    }

                    if (usageInfo.IsWrittenTo() || usageInfo == ValueUsageInfo.ReadWrite)
                    {
                        symbol = propertySymbol.SetMethod;
                    }
                }

                if (operation is IEventAssignmentOperation eventAssignment && symbol is IEventSymbol eventSymbol)
                {
                    symbol = eventAssignment.Adds ? eventSymbol.AddMethod : eventSymbol.RemoveMethod;
                }

                if (symbol == null)
                {
                    referencedPreviewSymbol = null;
                    return false;
                }

                if (symbol is IMethodSymbol methodSymbol && methodSymbol.IsConstructor())
                {
                    if (SymbolIsAnnotatedOrUsesPreviewTypes(methodSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out referencedPreviewSymbol))
                    {
                        // Constructor symbols have the name .ctor. Return the containing type instead so we get meaningful names in the diagnostic message
                        referencedPreviewSymbol = referencedPreviewSymbol.ContainingSymbol;
                        return true;
                    }

                    if (SymbolIsAnnotatedOrUsesPreviewTypes(methodSymbol.ContainingSymbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out referencedPreviewSymbol))
                    {
                        return true;
                    }
                }

                if (SymbolIsAnnotatedOrUsesPreviewTypes(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out referencedPreviewSymbol))
                {
                    return true;
                }
            }

            referencedPreviewSymbol = null;
            return false;
        }

        private static ISymbol? GetOperationSymbol(IOperation operation)
            => operation switch
            {
                IInvocationOperation iOperation => iOperation.TargetMethod,
                IObjectCreationOperation cOperation => cOperation.Constructor,
                IPropertyReferenceOperation pOperation => pOperation.Property,
                IFieldReferenceOperation fOperation => fOperation.Field,
                IDelegateCreationOperation dOperation => dOperation.Type,
                IEventReferenceOperation eOperation => eOperation.Member,
                IUnaryOperation uOperation => uOperation.OperatorMethod,
                IBinaryOperation bOperation => bOperation.OperatorMethod,
                IArrayCreationOperation arrayCreationOperation => SymbolFromArrayCreationOperation(arrayCreationOperation),
                ICatchClauseOperation catchClauseOperation => catchClauseOperation.ExceptionType,
                ITypeOfOperation typeOfOperation => typeOfOperation.TypeOperand,
                IEventAssignmentOperation eventAssignment => GetOperationSymbol(eventAssignment.EventReference),
                _ => null,
            };

        private static ISymbol SymbolFromArrayCreationOperation(IArrayCreationOperation operation)
        {
            ISymbol ret = operation.Type;
            while (ret is IArrayTypeSymbol arrayTypeSymbol)
            {
                ret = arrayTypeSymbol.ElementType;
            }

            return ret;
        }

        private bool TypeParametersHavePreviewAttribute(ISymbol namedTypeSymbolOrMethodSymbol,
                                                        ImmutableArray<ITypeParameterSymbol> typeParameters,
                                                        ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                        INamedTypeSymbol previewFeatureAttribute,
                                                        [NotNullWhen(true)] out ISymbol? previewSymbol,
                                                        out SyntaxNode? previewSyntaxNode)
        {
            foreach (ITypeParameterSymbol typeParameter in typeParameters)
            {
                ImmutableArray<ITypeSymbol> constraintTypes = typeParameter.ConstraintTypes;
                previewSymbol = GetPreviewSymbolForGenericTypesFromTypeArguments(constraintTypes, requiresPreviewFeaturesSymbols, previewFeatureAttribute);
                if (previewSymbol != null)
                {
                    previewSyntaxNode = GetConstraintSyntaxNodeForTypeConstrainedByPreviewTypes(namedTypeSymbolOrMethodSymbol, previewSymbol);
                    return true;
                }
            }

            previewSymbol = null;
            previewSyntaxNode = null;
            return false;
        }

#pragma warning disable CA1054,CA1055 // url should be of type URI
        private static string? GetMessageAndURLFromAttributeConstructor(AttributeData attribute, out string? url)
#pragma warning restore CA1054,CA1055
        {
            string? message = null;
            url = null;
            ImmutableArray<TypedConstant> constructorArguments = attribute.ConstructorArguments;
            if (constructorArguments.Length != 0)
            {
                if (constructorArguments.First().Value is string messageValue)
                {
                    message = messageValue;
                }
            }

            ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = attribute.NamedArguments;
            if (namedArguments.Length != 0)
            {
                foreach (KeyValuePair<string, TypedConstant> namedArgument in namedArguments)
                {
                    if (namedArgument.Key == "Message")
                    {
                        message = (string)namedArgument.Value.Value;
                    }

                    if (namedArgument.Key == "Url")
                    {
                        url = (string)namedArgument.Value.Value;
                    }
                }
            }

            return message;
        }

        private static void ReportDiagnosticWithCustomMessageIfItExists(OperationAnalysisContext context,
                                                                        IOperation operation,
                                                                        ISymbol symbol,
                                                                        ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        DiagnosticDescriptor diagnosticDescriptorWithPlaceholdersForCustomMessage,
                                                                        string diagnosticMessageArgument)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(symbol, out (bool isPreview, string? message, string? url) existing))
            {
                Debug.Fail($"Should never reach this line. This means the symbol {symbol.Name} was not processed in this analyzer");
            }
            else
            {
                string url = existing.url ?? DefaultURL;
                if (existing.message is string customMessage)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(diagnosticDescriptorWithPlaceholdersForCustomMessage, diagnosticMessageArgument, url, customMessage));
                }
                else
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(diagnosticDescriptor, diagnosticMessageArgument, url));
                }
            }
        }

        private static void ReportDiagnosticWithCustomMessageIfItExists(Action<Diagnostic> reportDiagnostic,
                                                                        SyntaxNode node,
                                                                        ISymbol previewSymbol,
                                                                        ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        DiagnosticDescriptor diagnosticDescriptorWithPlaceholdersForCustomMessage,
                                                                        string diagnosticMessageArgument0,
                                                                        string diagnosticMessageArgument1)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(previewSymbol, out (bool isPreview, string? message, string? url) existing))
            {
                Debug.Fail($"Should never reach this line. This means the symbol {previewSymbol.Name} was not processed in this analyzer");
            }
            else
            {
                string url = existing.url ?? DefaultURL;
                if (existing.message is string customMessage)
                {
                    reportDiagnostic(node.CreateDiagnostic(diagnosticDescriptorWithPlaceholdersForCustomMessage, diagnosticMessageArgument0, diagnosticMessageArgument1, url, customMessage));
                }
                else
                {
                    reportDiagnostic(node.CreateDiagnostic(diagnosticDescriptor, diagnosticMessageArgument0, diagnosticMessageArgument1, url));
                }
            }
        }

        private static void ReportDiagnosticWithCustomMessageIfItExists(Action<Diagnostic> reportDiagnostic,
                                                                SyntaxNode node,
                                                                ISymbol previewSymbol,
                                                                ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                DiagnosticDescriptor diagnosticDescriptor,
                                                                DiagnosticDescriptor diagnosticDescriptorWithPlaceholdersForCustomMessage,
                                                                string diagnosticMessageArgument0)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(previewSymbol, out (bool isPreview, string? message, string? url) existing))
            {
                Debug.Fail($"Should never reach this line. This means the symbol {previewSymbol.Name} was not processed in this analyzer");
            }
            else
            {
                string url = existing.url ?? DefaultURL;
                if (existing.message is string customMessage)
                {
                    reportDiagnostic(node.CreateDiagnostic(diagnosticDescriptorWithPlaceholdersForCustomMessage, diagnosticMessageArgument0, url, customMessage));
                }
                else
                {
                    reportDiagnostic(node.CreateDiagnostic(diagnosticDescriptor, diagnosticMessageArgument0, url));
                }
            }
        }

        protected static void ReportDiagnosticWithCustomMessageIfItExists(SymbolAnalysisContext context,
                                                                        SyntaxNode node,
                                                                        ISymbol previewSymbol,
                                                                        ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        DiagnosticDescriptor diagnosticDescriptorWithPlaceholdersForCustomMessage,
                                                                        string diagnosticMessageArgument0)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(previewSymbol, out (bool isPreview, string? message, string? url) existing))
            {
                Debug.Fail($"Should never reach this line. This means the symbol {previewSymbol.Name} was not processed in this analyzer");
            }
            else
            {
                string url = existing.url ?? DefaultURL;
                if (existing.message is string customMessage)
                {
                    context.ReportDiagnostic(node.CreateDiagnostic(diagnosticDescriptorWithPlaceholdersForCustomMessage, diagnosticMessageArgument0, url, customMessage));
                }
                else
                {
                    context.ReportDiagnostic(node.CreateDiagnostic(diagnosticDescriptor, diagnosticMessageArgument0, url));
                }
            }
        }

        private static void ReportDiagnosticWithCustomMessageIfItExists(SymbolAnalysisContext context,
                                                                        ISymbol previewSymbol,
                                                                        ISymbol symbolToRaiseDiagnosticOn,
                                                                        ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        DiagnosticDescriptor diagnosticDescriptorWithPlaceholdersForCustomMessage,
                                                                        string diagnosticMessageArgument0,
                                                                        string diagnosticMessageArgument1)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(previewSymbol, out (bool isPreview, string? message, string? url) existing))
            {
                Debug.Fail($"Should never reach this line. This means the symbol {previewSymbol.Name} was not processed in this analyzer");
            }
            else
            {
                string url = existing.url ?? DefaultURL;
                if (existing.message is string customMessage)
                {
                    context.ReportDiagnostic(symbolToRaiseDiagnosticOn.CreateDiagnostic(diagnosticDescriptorWithPlaceholdersForCustomMessage, diagnosticMessageArgument0, diagnosticMessageArgument1, url, customMessage));
                }
                else
                {
                    context.ReportDiagnostic(symbolToRaiseDiagnosticOn.CreateDiagnostic(diagnosticDescriptor, diagnosticMessageArgument0, diagnosticMessageArgument1, url));
                }
            }
        }

        private static bool SymbolIsAnnotatedAsPreview(ISymbol symbol, ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols, INamedTypeSymbol previewFeatureAttribute)
        {
            if (symbol is null)
            {
                // We are sometimes null, such as for IPropertySymbol.GetOverriddenMember()
                // when the property symbol represents an indexer, so return false as a precaution
                return false;
            }

            if (!requiresPreviewFeaturesSymbols.TryGetValue(symbol, out (bool isPreview, string? message, string? url) existing))
            {
                ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass.Equals(previewFeatureAttribute))
                    {
                        string? message = GetMessageAndURLFromAttributeConstructor(attribute, out string? url);
                        requiresPreviewFeaturesSymbols.GetOrAdd(symbol, new ValueTuple<bool, string?, string?>(true, message, url));
                        return true;
                    }
                }

                ISymbol? parent = symbol.ContainingSymbol;
                while (parent is INamespaceSymbol)
                {
                    parent = parent.ContainingSymbol;
                }

                if (parent != null)
                {
                    if (SymbolIsAnnotatedAsPreview(parent, requiresPreviewFeaturesSymbols, previewFeatureAttribute))
                    {
                        requiresPreviewFeaturesSymbols.GetOrAdd(symbol, new ValueTuple<bool, string?, string?>(true, null, null));
                        return true;
                    }
                }

                requiresPreviewFeaturesSymbols.GetOrAdd(symbol, new ValueTuple<bool, string?, string?>(false, null, null));
                return false;
            }

            return existing.isPreview;
        }

        private ISymbol? GetSymbol(SyntaxNode node, SyntaxNodeAnalysisContext context)
        {
            while (node is not null)
            {
                node = AdjustSyntaxNodeForGetSymbol(node);
                var symbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
                if (symbol is not null && symbol.Kind != SymbolKind.TypeParameter)
                {
                    return symbol;
                }

                node = node.Parent;
            }

            return null;
        }

        private protected abstract SyntaxNode AdjustSyntaxNodeForGetSymbol(SyntaxNode node);

        protected void AnalyzeTypeSyntax(SyntaxNodeAnalysisContext context, SyntaxNode node, ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols, Func<ISymbol, bool> symbolIsAnnotatedAsPreview)
        {
            var type = context.SemanticModel.GetTypeInfo(node, context.CancellationToken).Type;
            if (type is not null && symbolIsAnnotatedAsPreview(type))
            {
                var enclosingSymbol = GetSymbol(node, context);
                if (enclosingSymbol != null && ShouldSkipSymbol(enclosingSymbol, symbolIsAnnotatedAsPreview))
                {
                    // The node we're analyzing is referencing a preview type, but it's contained in a preview type as well.
                    // No diagnostics are needed.
                    return;
                }

                // We're only concerned about types(class/struct/interface) directly implementing preview interfaces. Implemented interfaces(direct/base) will report their diagnostics independently
                if (type.TypeKind is TypeKind.Interface or TypeKind.Class && node.Parent is TBaseTypeOrImplementsOrInherits)
                {
                    var (rule, ruleWithCustomMessage) = type.TypeKind switch
                    {
                        TypeKind.Interface => (ImplementsPreviewInterfaceRule, ImplementsPreviewInterfaceRuleWithCustomMessage),
                        TypeKind.Class => (DerivesFromPreviewClassRule, DerivesFromPreviewClassRuleWithCustomMessage),
                        _ => throw new InvalidOperationException($"Unexpected TypeKind '{type.TypeKind}'.")
                    };
                    ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, rule, ruleWithCustomMessage, enclosingSymbol?.Name ?? string.Empty, type.Name);
                }
                else if (enclosingSymbol?.Kind is SymbolKind.Field or SymbolKind.Event)
                {
                    ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, FieldOrEventIsPreviewTypeRule, FieldOrEventIsPreviewTypeRuleWithCustomMessage, enclosingSymbol?.Name ?? string.Empty, type.Name);
                }
                else if (IsInReturnType(node))
                {
                    ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, MethodReturnsPreviewTypeRule, MethodReturnsPreviewTypeRuleWithCustomMessage, enclosingSymbol?.Name ?? string.Empty, type.Name);
                }
                else if (node.FirstAncestorOrSelf<TTypeConstraintSyntax>() is not null ||
                    node.FirstAncestorOrSelf<TTypeArgumentList>() is not null)
                {
                    // C#: method, types, delegates, local functions
                    // VB: DeclarationStatementSyntax
                    ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, UsesPreviewTypeParameterRule, UsesPreviewTypeParameterRuleWithCustomMessage, enclosingSymbol?.Name ?? string.Empty, type.Name);
                }
                else if (enclosingSymbol?.Kind == SymbolKind.Parameter && enclosingSymbol.ContainingSymbol is IMethodSymbol { MethodKind: not MethodKind.LambdaMethod })
                {
                    ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, MethodUsesPreviewTypeAsParameterRule, MethodUsesPreviewTypeAsParameterRuleWithCustomMessage, enclosingSymbol.ContainingSymbol.Name, type.Name);
                }
                else
                {
                    //ReportDiagnosticWithCustomMessageIfItExists(context.ReportDiagnostic, node, type, requiresPreviewFeaturesSymbols, GeneralPreviewFeatureAttributeRule, GeneralPreviewFeatureAttributeRuleWithCustomMessage, type.Name);
                }
            }
        }

        protected abstract bool IsParameter(SyntaxNode node);
        protected abstract bool IsInReturnType(SyntaxNode node);

        private static bool ShouldSkipSymbol(ISymbol symbol, Func<ISymbol, bool> symbolIsAnnotatedAsPreview)
        {
            if (symbolIsAnnotatedAsPreview(symbol) ||
                (symbol is IMethodSymbol method && method.AssociatedSymbol != null && symbolIsAnnotatedAsPreview(method.AssociatedSymbol)))
            {
                return true;
            }

            return false;
        }
    }
}

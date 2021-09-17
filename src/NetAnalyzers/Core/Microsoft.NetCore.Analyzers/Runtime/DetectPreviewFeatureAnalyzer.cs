﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public abstract class DetectPreviewFeatureAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2252";
        internal const string DefaultURL = "https://aka.ms/dotnet-warnings/preview-features";
        private static readonly LocalizableString s_localizableTitle = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesTitle));
        private static readonly LocalizableString s_localizableDescription = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDescription));
        internal static readonly LocalizableString s_detectPreviewFeaturesMessage = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesMessage));
        internal static readonly LocalizableString s_detectPreviewFeaturesUrl = CreateLocalizableResourceString(nameof(DetectPreviewFeaturesURL));
        internal static readonly LocalizableString s_implementsPreviewInterfaceMessage = CreateLocalizableResourceString(nameof(ImplementsPreviewInterfaceMessage));
        internal static readonly LocalizableString s_implementsPreviewMethodMessage = CreateLocalizableResourceString(nameof(ImplementsPreviewMethodMessage));
        internal static readonly LocalizableString s_overridePreviewMethodMessage = CreateLocalizableResourceString(nameof(OverridesPreviewMethodMessage));
        internal static readonly LocalizableString s_derivesFromPreviewClassMessage = CreateLocalizableResourceString(nameof(DerivesFromPreviewClassMessage));
        internal static readonly LocalizableString s_usesPreviewTypeParameterMessage = CreateLocalizableResourceString(nameof(UsesPreviewTypeParameterMessage));
        internal static readonly LocalizableString s_methodReturnsPreviewTypeMessage = CreateLocalizableResourceString(nameof(MethodReturnsPreviewTypeMessage));
        internal static readonly LocalizableString s_methodUsesPreviewTypeAsParameterMessage = CreateLocalizableResourceString(nameof(MethodUsesPreviewTypeAsParamaterMessage));
        internal static readonly LocalizableString s_fieldOrEventIsPreviewTypeMessage = CreateLocalizableResourceString(nameof(FieldIsPreviewTypeMessage));
        private static readonly ImmutableArray<SymbolKind> s_symbols = ImmutableArray.Create(SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event);

        internal static DiagnosticDescriptor GeneralPreviewFeatureAttributeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                    s_localizableTitle,
                                                                                                                    CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                                    DiagnosticCategory.Usage,
                                                                                                                    RuleLevel.BuildError,
                                                                                                                    s_localizableDescription,
                                                                                                                    isPortedFxCopRule: false,
                                                                                                                    isDataflowRule: false);

        internal static DiagnosticDescriptor ImplementsPreviewInterfaceRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static DiagnosticDescriptor ImplementsPreviewMethodRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static DiagnosticDescriptor OverridesPreviewMethodRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                s_localizableTitle,
                                                                                                                CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                                DiagnosticCategory.Usage,
                                                                                                                RuleLevel.BuildError,
                                                                                                                s_localizableDescription,
                                                                                                                isPortedFxCopRule: false,
                                                                                                                isDataflowRule: false);

        internal static DiagnosticDescriptor DerivesFromPreviewClassRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                             s_localizableTitle,
                                                                                                             CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                             DiagnosticCategory.Usage,
                                                                                                             RuleLevel.BuildError,
                                                                                                             s_localizableDescription,
                                                                                                             isPortedFxCopRule: false,
                                                                                                             isDataflowRule: false);

        internal static DiagnosticDescriptor UsesPreviewTypeParameterRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);

        internal static DiagnosticDescriptor MethodReturnsPreviewTypeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                              s_localizableTitle,
                                                                                                              CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                              DiagnosticCategory.Usage,
                                                                                                              RuleLevel.BuildError,
                                                                                                              s_localizableDescription,
                                                                                                              isPortedFxCopRule: false,
                                                                                                              isDataflowRule: false);

        internal static DiagnosticDescriptor MethodUsesPreviewTypeAsParameterRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                      s_localizableTitle,
                                                                                                                      CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                                      DiagnosticCategory.Usage,
                                                                                                                      RuleLevel.BuildError,
                                                                                                                      s_localizableDescription,
                                                                                                                      isPortedFxCopRule: false,
                                                                                                                      isDataflowRule: false);
        internal static DiagnosticDescriptor FieldOrEventIsPreviewTypeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                        s_localizableTitle,
                                                                                                        CreateLocalizableResourceString(nameof(DetectPreviewFeaturesDiagnosticMessage)),
                                                                                                        DiagnosticCategory.Usage,
                                                                                                        RuleLevel.BuildError,
                                                                                                        s_localizableDescription,
                                                                                                        isPortedFxCopRule: false,
                                                                                                        isDataflowRule: false);

        internal static DiagnosticDescriptor StaticAbstractIsPreviewFeatureRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                                                    s_localizableTitle,
                                                                                                                    CreateLocalizableResourceString(nameof(StaticAndAbstractRequiresPreviewFeatures)),
                                                                                                                    DiagnosticCategory.Usage,
                                                                                                                    RuleLevel.BuildError,
                                                                                                                    s_localizableDescription,
                                                                                                                    isPortedFxCopRule: false,
                                                                                                                    isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            GeneralPreviewFeatureAttributeRule,
            ImplementsPreviewInterfaceRule,
            ImplementsPreviewMethodRule,
            OverridesPreviewMethodRule,
            DerivesFromPreviewClassRule,
            UsesPreviewTypeParameterRule,
            MethodReturnsPreviewTypeRule,
            MethodUsesPreviewTypeAsParameterRule,
            FieldOrEventIsPreviewTypeRule,
            StaticAbstractIsPreviewFeatureRule);

        protected abstract SyntaxNode? GetPreviewInterfaceNodeForTypeImplementingPreviewInterface(ISymbol typeSymbol, ISymbol previewInterfaceSymbol);

        protected abstract SyntaxNode? GetConstraintSyntaxNodeForTypeConstrainedByPreviewTypes(ISymbol typeOrMethodSymbol, ISymbol previewInterfaceConstraintSymbol);

        protected abstract SyntaxNode? GetPreviewReturnTypeSyntaxNodeForMethodOrProperty(ISymbol methodOrPropertySymbol, ISymbol previewReturnTypeSymbol);

        protected abstract SyntaxNode? GetPreviewParameterSyntaxNodeForMethod(IMethodSymbol methodSymbol, ISymbol parameterSymbol);

        protected abstract SyntaxNode? GetPreviewSyntaxNodeForFieldsOrEvents(ISymbol fieldOrEventSymbol, ISymbol previewSymbol);

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

                ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols = new();
                ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolToMessageAndUrl = new();

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
                context.RegisterOperationAction(context => BuildSymbolInformationFromOperations(context, requiresPreviewFeaturesSymbols, previewSymbolToMessageAndUrl, previewFeaturesAttribute),
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
                context.RegisterSymbolAction(context => AnalyzeSymbol(context, requiresPreviewFeaturesSymbols, previewSymbolToMessageAndUrl, virtualStaticsInInterfaces, previewFeaturesAttribute), s_symbols);
            });
        }

        /// <summary>
        /// Returns null if the type arguments are not preview
        /// </summary>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the typeArguments was something like List[List[List[PreviewType]]]], this function will return PreviewType"
        /// </remarks>
        private static ISymbol? GetPreviewSymbolForGenericTypesFromTypeArguments(ImmutableArray<ITypeSymbol> typeArguments,
            ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
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

        private void ProcessFieldSymbolAttributes(SymbolAnalysisContext context,
                                                  IFieldSymbol symbol,
                                                  ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                  ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                  INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            ISymbol symbolType = symbol.Type;
            while (symbolType is IArrayTypeSymbol arrayType)
            {
                symbolType = arrayType.ElementType;
            }

            ProcessFieldOrEventSymbolAttributes(context, symbol, symbolType, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol);
        }

        private void ProcessFieldOrEventSymbolAttributes(SymbolAnalysisContext context,
                                                         ISymbol symbol,
                                                         ISymbol symbolType,
                                                         ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                         ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                         INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            if (SymbolIsAnnotatedAsPreview(symbolType, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
            {
                string message = string.Format(CultureInfo.CurrentCulture, (string)s_fieldOrEventIsPreviewTypeMessage, symbol.Name, symbolType.Name);
                SyntaxNode? node = GetPreviewSyntaxNodeForFieldsOrEvents(symbol, symbolType);
                if (node != null)
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, node, symbolType, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, FieldOrEventIsPreviewTypeRule, message);
                }
                else
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, symbolType, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, FieldOrEventIsPreviewTypeRule, message);
                }
            }

            if (SymbolContainsGenericTypesWithPreviewAttributes(symbolType,
                                                                requiresPreviewFeaturesSymbols,
                                                                previewFeatureAttributeSymbol,
                                                                out ISymbol? previewSymbol,
                                                                out SyntaxNode? syntaxNode,
                                                                methodOrFieldOrEventSymbolForGenericParameterSyntaxNode: symbol))
            {
                string message = string.Format(CultureInfo.CurrentCulture, (string)s_fieldOrEventIsPreviewTypeMessage, symbol.Name, previewSymbol.Name);
                if (syntaxNode != null)
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, syntaxNode, previewSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, FieldOrEventIsPreviewTypeRule, message);
                }
                else
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, previewSymbol, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, FieldOrEventIsPreviewTypeRule, message);
                }
            }
        }

        private void ProcessEventSymbolAttributes(SymbolAnalysisContext context,
                                                  IEventSymbol symbol,
                                                  ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                  ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                  INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            ISymbol symbolType = symbol.Type;
            while (symbolType is IArrayTypeSymbol arrayType)
            {
                symbolType = arrayType.ElementType;
            }

            ProcessFieldOrEventSymbolAttributes(context, symbol, symbolType, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol);
        }

        private void ProcessTypeSymbolAttributes(SymbolAnalysisContext context,
                                                 ITypeSymbol symbol,
                                                 ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                 ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                 INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            // We're only concerned about types(class/struct/interface) directly implementing preview interfaces. Implemented interfaces(direct/base) will report their diagnostics independently
            ImmutableArray<INamedTypeSymbol> interfaces = symbol.Interfaces;
            foreach (INamedTypeSymbol anInterface in interfaces)
            {
                if (SymbolIsAnnotatedAsPreview(anInterface, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    SyntaxNode? interfaceNode = GetPreviewInterfaceNodeForTypeImplementingPreviewInterface(symbol, anInterface);
                    string message = string.Format(CultureInfo.CurrentCulture, (string)s_implementsPreviewInterfaceMessage, symbol.Name, anInterface.Name);
                    if (interfaceNode != null)
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, interfaceNode, anInterface, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, ImplementsPreviewInterfaceRule, message);
                    }
                    else
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, anInterface, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, ImplementsPreviewInterfaceRule, message);
                    }
                }
            }

            if (SymbolContainsGenericTypesWithPreviewAttributes(symbol,
                                                                requiresPreviewFeaturesSymbols,
                                                                previewFeatureAttributeSymbol,
                                                                out ISymbol? previewSymbol,
                                                                out SyntaxNode? syntaxNode))
            {
                string message = string.Format(CultureInfo.CurrentCulture, (string)s_usesPreviewTypeParameterMessage, symbol.Name, previewSymbol.Name);
                if (syntaxNode != null)
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, syntaxNode, previewSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                }
                else
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, previewSymbol, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                }
            }

            if (ProcessTypeAttributeForPreviewness(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out SyntaxReference? attributeSyntaxReference, out string? attributeName, out ISymbol? previewAttributeSymbol))
            {
                string message = string.Format(CultureInfo.CurrentCulture, (string)s_detectPreviewFeaturesMessage, attributeName);
                ReportDiagnosticWithCustomOrGivenDiagnostic(context, attributeSyntaxReference.GetSyntax(context.CancellationToken), previewAttributeSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, GeneralPreviewFeatureAttributeRule, message);
            }

            INamedTypeSymbol? baseType = symbol.BaseType;
            if (baseType != null)
            {
                if (SymbolIsAnnotatedAsPreview(baseType, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, (string)s_derivesFromPreviewClassMessage, symbol.Name, baseType.Name);
                    SyntaxNode? baseTypeNode = GetPreviewInterfaceNodeForTypeImplementingPreviewInterface(symbol, baseType);
                    if (baseTypeNode != null)
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, baseTypeNode, baseType, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                    }
                    else
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, baseType, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                    }
                }
            }
        }

        private static bool ProcessTypeAttributeForPreviewness(ISymbol symbol,
                                                               ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
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

        private SyntaxNode? GetPreviewSyntaxNodeFromSymbols(ISymbol symbol,
                                                            ISymbol previewType)
        {
            switch (symbol)
            {
                case IFieldSymbol:
                case IEventSymbol:
                    return GetPreviewSyntaxNodeForFieldsOrEvents(symbol, previewType);
                case IMethodSymbol methodSymbol:
                    return GetPreviewParameterSyntaxNodeForMethod(methodSymbol, previewType);
                default:
                    return null;
            }
        }

        private bool SymbolContainsGenericTypesWithPreviewAttributes(ISymbol symbol,
                                                                     ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                                     INamedTypeSymbol previewFeatureAttribute,
                                                                     [NotNullWhen(true)] out ISymbol? previewSymbol,
                                                                     out SyntaxNode? previewSyntaxNode,
                                                                     bool checkTypeParametersForPreviewFeatures = true,
                                                                     ISymbol? methodOrFieldOrEventSymbolForGenericParameterSyntaxNode = null)
        {
            if (symbol is INamedTypeSymbol typeSymbol && typeSymbol.Arity > 0)
            {
                ISymbol? previewTypeArgument = GetPreviewSymbolForGenericTypesFromTypeArguments(typeSymbol.TypeArguments, requiresPreviewFeaturesSymbols, previewFeatureAttribute);
                if (previewTypeArgument != null)
                {
                    if (methodOrFieldOrEventSymbolForGenericParameterSyntaxNode != null)
                    {
                        previewSyntaxNode = GetPreviewSyntaxNodeFromSymbols(methodOrFieldOrEventSymbolForGenericParameterSyntaxNode, previewTypeArgument);
                    }
                    else
                    {
                        previewSyntaxNode = null;
                    }

                    previewSymbol = previewTypeArgument;
                    return true;
                }

                if (checkTypeParametersForPreviewFeatures)
                {
                    ImmutableArray<ITypeParameterSymbol> typeParameters = typeSymbol.TypeParameters;
                    if (TypeParametersHavePreviewAttribute(typeSymbol, typeParameters, requiresPreviewFeaturesSymbols, previewFeatureAttribute, out previewSymbol, out previewSyntaxNode))
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
                    previewSyntaxNode = null;
                    previewSymbol = previewTypeArgument;
                    return true;
                }

                if (checkTypeParametersForPreviewFeatures)
                {
                    ImmutableArray<ITypeParameterSymbol> typeParameters = methodSymbol.TypeParameters;
                    if (TypeParametersHavePreviewAttribute(methodSymbol, typeParameters, requiresPreviewFeaturesSymbols, previewFeatureAttribute, out previewSymbol, out previewSyntaxNode))
                    {
                        return true;
                    }
                }
            }

            previewSymbol = null;
            previewSyntaxNode = null;
            return false;
        }

        private static bool SymbolIsStaticAndAbstract(ISymbol symbol, ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols, INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            // Static Abstract is only legal on interfaces. Anything else is likely a compile error and we shouldn't tag such cases.
            return symbol.IsStatic && symbol.IsAbstract && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Interface && !SymbolIsAnnotatedAsPreview(symbol, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol);
        }

        private void ProcessPropertyOrMethodAttributes(SymbolAnalysisContext context,
                                                       ISymbol propertyOrMethodSymbol,
                                                       ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                       ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
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
                    string message = string.Format(CultureInfo.CurrentCulture, (string)s_implementsPreviewMethodMessage, propertyOrMethodSymbol.Name, baseInterfaceMemberName);
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, baseInterfaceMember, propertyOrMethodSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, ImplementsPreviewMethodRule, message);
                }
            }

            if (propertyOrMethodSymbol.IsOverride)
            {
                ISymbol overridden = propertyOrMethodSymbol.GetOverriddenMember();
                if (SymbolIsAnnotatedAsPreview(overridden, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    string overriddenName = overridden.ContainingSymbol != null ? overridden.ContainingSymbol.Name + "." + overridden.Name : overridden.Name;
                    string message = string.Format(CultureInfo.CurrentCulture, (string)s_overridePreviewMethodMessage, propertyOrMethodSymbol.Name, overriddenName);
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, overridden, propertyOrMethodSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, OverridesPreviewMethodRule, message);
                }
            }

            if (propertyOrMethodSymbol is IMethodSymbol method)
            {
                ITypeSymbol methodReturnType = method.ReturnType;
                while (methodReturnType is IArrayTypeSymbol array)
                {
                    methodReturnType = array.ElementType;
                }

                if (SymbolIsAnnotatedAsPreview(methodReturnType, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, (string)s_methodReturnsPreviewTypeMessage, propertyOrMethodSymbol.Name, methodReturnType.Name);
                    SyntaxNode? returnTypeNode = GetPreviewReturnTypeSyntaxNodeForMethodOrProperty(method.IsPropertyGetter() ? method.AssociatedSymbol : method, methodReturnType);
                    if (returnTypeNode != null)
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, returnTypeNode, methodReturnType, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodReturnsPreviewTypeRule, message);
                    }
                    else
                    {
                        ReportDiagnosticWithCustomOrGivenDiagnostic(context, methodReturnType, method, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodReturnsPreviewTypeRule, message);
                    }
                }

                if (methodReturnType is INamedTypeSymbol typeSymbol && typeSymbol.Arity > 0)
                {
                    ISymbol? innerPreviewSymbol = GetPreviewSymbolForGenericTypesFromTypeArguments(typeSymbol.TypeArguments, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol);
                    if (innerPreviewSymbol != null)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, (string)s_methodReturnsPreviewTypeMessage, propertyOrMethodSymbol.Name, innerPreviewSymbol.Name);
                        SyntaxNode? returnTypeNode = GetPreviewReturnTypeSyntaxNodeForMethodOrProperty(method.IsPropertyGetter() ? method.AssociatedSymbol : method, innerPreviewSymbol);
                        if (returnTypeNode != null)
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, returnTypeNode, innerPreviewSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodReturnsPreviewTypeRule, message);
                        }
                        else
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, innerPreviewSymbol, method, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodReturnsPreviewTypeRule, message);
                        }
                    }
                }

                ImmutableArray<IParameterSymbol> parameters = method.Parameters;
                foreach (IParameterSymbol parameter in parameters)
                {
                    var parameterType = parameter.Type;
                    while (parameterType is IArrayTypeSymbol array)
                    {
                        parameterType = array.ElementType;
                    }

                    if (SymbolIsAnnotatedAsPreview(parameterType, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol))
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, (string)s_methodUsesPreviewTypeAsParameterMessage, propertyOrMethodSymbol.Name, parameterType.Name);
                        SyntaxNode? previewParameterNode = GetPreviewParameterSyntaxNodeForMethod(method, parameterType);
                        if (previewParameterNode != null)
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, previewParameterNode, parameterType, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodUsesPreviewTypeAsParameterRule, message);
                        }
                        else
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, parameterType, parameter, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, MethodUsesPreviewTypeAsParameterRule, message);
                        }
                    }

                    if (SymbolContainsGenericTypesWithPreviewAttributes(parameterType,
                                                                        requiresPreviewFeaturesSymbols,
                                                                        previewFeatureAttributeSymbol,
                                                                        out ISymbol? referencedPreviewSymbol,
                                                                        out SyntaxNode? syntaxNode,
                                                                        methodOrFieldOrEventSymbolForGenericParameterSyntaxNode: method))
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, (string)s_usesPreviewTypeParameterMessage, propertyOrMethodSymbol.Name, referencedPreviewSymbol.Name);
                        if (syntaxNode != null)
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, syntaxNode, referencedPreviewSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                        }
                        else
                        {
                            ReportDiagnosticWithCustomOrGivenDiagnostic(context, referencedPreviewSymbol, parameter, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                        }
                    }
                }
            }

            if (SymbolContainsGenericTypesWithPreviewAttributes(propertyOrMethodSymbol,
                                                                requiresPreviewFeaturesSymbols,
                                                                previewFeatureAttributeSymbol,
                                                                out ISymbol? previewSymbol,
                                                                out SyntaxNode? referencedPreviewTypeSyntaxNode))
            {
                string message = string.Format(CultureInfo.CurrentCulture, (string)s_usesPreviewTypeParameterMessage, propertyOrMethodSymbol.Name, previewSymbol.Name);
                if (referencedPreviewTypeSyntaxNode != null)
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, referencedPreviewTypeSyntaxNode, previewSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                }
                else
                {
                    ReportDiagnosticWithCustomOrGivenDiagnostic(context, previewSymbol, propertyOrMethodSymbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, UsesPreviewTypeParameterRule, message);
                }
            }
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context,
                                   ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                   ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
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
                ProcessTypeSymbolAttributes(context, typeSymbol, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol);
            }
            else if (symbol is IMethodSymbol || symbol is IPropertySymbol)
            {
                ProcessPropertyOrMethodAttributes(context, symbol, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, virtualStaticsInInterfaces, previewFeatureAttributeSymbol);
            }
            else if (symbol is IFieldSymbol fieldSymbol)
            {
                ProcessFieldSymbolAttributes(context, fieldSymbol, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol);
            }
            else if (symbol is IEventSymbol eventSymbol)
            {
                ProcessEventSymbolAttributes(context, eventSymbol, requiresPreviewFeaturesSymbols, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol);
            }
        }

        private void BuildSymbolInformationFromOperations(OperationAnalysisContext context,
                                                          ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
                                                          ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                          INamedTypeSymbol previewFeatureAttributeSymbol)
        {
            if (OperationUsesPreviewFeatures(context, requiresPreviewFeaturesSymbols, previewFeatureAttributeSymbol, out ISymbol? symbol))
            {
                IOperation operation = context.Operation;
                if (operation is ICatchClauseOperation catchClauseOperation)
                {
                    operation = catchClauseOperation.ExceptionDeclarationOrExpression;
                }

                string message = string.Format(CultureInfo.CurrentCulture, (string)s_detectPreviewFeaturesMessage, symbol.Name);
                ReportDiagnosticWithCustomOrGivenDiagnostic(context, operation, symbol, previewSymbolsToMessageAndUrl, previewFeatureAttributeSymbol, GeneralPreviewFeatureAttributeRule, message);
            }
        }

        private bool SymbolIsAnnotatedOrUsesPreviewTypes(ISymbol symbol,
                                                         ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
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
                                                                out SyntaxNode? _,
                                                                checkTypeParametersForPreviewFeatures: false))
            {
                return true;
            }

            referencedPreviewSymbol = null;
            return false;
        }

        private bool OperationUsesPreviewFeatures(OperationAnalysisContext context,
                                                  ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
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
                                                        ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols,
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

            ImmutableArray<System.Collections.Generic.KeyValuePair<string, TypedConstant>> namedArguments = attribute.NamedArguments;
            if (namedArguments.Length != 0)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, TypedConstant> namedArgument in namedArguments)
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

#pragma warning disable CA1054,CA1055 // url should be of type URI
        private static string? GetMessageAndURLForSymbol(ISymbol symbol,
            INamedTypeSymbol previewFeatureAttribute,
            ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
            out string? url)
#pragma warning restore CA1054,CA1055
        {
            string? message = null;
            url = null;
            if (!previewSymbolsToMessageAndUrl.TryGetValue(symbol, out (string? existingMessage, string? existingUrl) value))
            {
                ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass.Equals(previewFeatureAttribute))
                    {
                        string? customMessage = GetMessageAndURLFromAttributeConstructor(attribute, out string? customUrl);

                        if (customMessage != null)
                        {
                            message = customMessage;
                        }

                        if (customUrl != null)
                        {
                            url = customUrl;
                        }
                    }
                }
            }
            else
            {
                if (value.existingMessage != null)
                {
                    message = value.existingMessage;
                }

                if (value.existingUrl != null)
                {
                    url = value.existingUrl;
                }
            }

            return message;
        }

        private static void ReportDiagnosticWithCustomOrGivenDiagnostic(OperationAnalysisContext context,
                                                                        IOperation operation,
                                                                        ISymbol symbol,
                                                                        ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                                        INamedTypeSymbol previewFeatureAttribute,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        string message)
        {
            string url = DefaultURL;
            if (GetMessageAndURLForSymbol(symbol, previewFeatureAttribute, previewSymbolsToMessageAndUrl, out string? customUrl) is string customMessage)
            {
                message = customMessage;
            }

            url = string.Format(CultureInfo.CurrentCulture, (string)s_detectPreviewFeaturesUrl, customUrl ?? url);
            context.ReportDiagnostic(operation.CreateDiagnostic(diagnosticDescriptor, message, url));
        }

        private static void ReportDiagnosticWithCustomOrGivenDiagnostic(SymbolAnalysisContext context,
                                                                        SyntaxNode node,
                                                                        ISymbol previewSymbol,
                                                                        ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                                        INamedTypeSymbol previewFeatureAttribute,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        string message)
        {
            string url = DefaultURL;
            if (GetMessageAndURLForSymbol(previewSymbol, previewFeatureAttribute, previewSymbolsToMessageAndUrl, out string? customUrl) is string customMessage)
            {
                message = customMessage;
            }

            url = string.Format(CultureInfo.CurrentCulture, (string)s_detectPreviewFeaturesUrl, customUrl ?? url);
            context.ReportDiagnostic(node.CreateDiagnostic(diagnosticDescriptor, message, url));
        }

        private static void ReportDiagnosticWithCustomOrGivenDiagnostic(SymbolAnalysisContext context,
                                                                        ISymbol previewSymbol,
                                                                        ISymbol symbolToRaiseDiagnosticOn,
                                                                        ConcurrentDictionary<ISymbol, ValueTuple<string?, string?>> previewSymbolsToMessageAndUrl,
                                                                        INamedTypeSymbol previewFeatureAttribute,
                                                                        DiagnosticDescriptor diagnosticDescriptor,
                                                                        string message)
        {
            string url = DefaultURL;
            if (GetMessageAndURLForSymbol(previewSymbol, previewFeatureAttribute, previewSymbolsToMessageAndUrl, out string? customUrl) is string customMessage)
            {
                message = customMessage;
            }

            url = string.Format(CultureInfo.CurrentCulture, (string)s_detectPreviewFeaturesUrl, customUrl ?? url);
            context.ReportDiagnostic(symbolToRaiseDiagnosticOn.CreateDiagnostic(diagnosticDescriptor, message, url));
        }

        private static bool SymbolIsAnnotatedAsPreview(ISymbol symbol, ConcurrentDictionary<ISymbol, bool> requiresPreviewFeaturesSymbols, INamedTypeSymbol previewFeatureAttribute)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(symbol, out bool existing))
            {
                if (symbol.HasAttribute(previewFeatureAttribute))
                {
                    requiresPreviewFeaturesSymbols.GetOrAdd(symbol, true);
                    return true;
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
                        requiresPreviewFeaturesSymbols.GetOrAdd(symbol, true);
                        return true;
                    }
                }

                requiresPreviewFeaturesSymbols.GetOrAdd(symbol, false);
                return false;
            }

            return existing;
        }
    }
}

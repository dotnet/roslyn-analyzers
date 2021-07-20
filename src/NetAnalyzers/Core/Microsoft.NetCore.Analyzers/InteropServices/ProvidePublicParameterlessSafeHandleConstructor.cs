﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ProvidePublicParameterlessSafeHandleConstructorAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1419";

        internal const string DiagnosticPropertyConstructorExists = "ConstructorExists";
        internal const string DiagnosticPropertyBaseConstructorAccessible = "BaseConstructorAccessible";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvidePublicParameterlessSafeHandleConstructorTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvidePublicParameterlessSafeHandleConstructorMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ProvidePublicParameterlessSafeHandleConstructorDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
                                                        RuleId,
                                                        s_localizableTitle,
                                                        s_localizableMessage,
                                                        DiagnosticCategory.Interoperability,
                                                        RuleLevel.IdeSuggestion,
                                                        description: s_localizableDescription,
                                                        isPortedFxCopRule: false,
                                                        isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(
                context =>
                {
                    if (context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesSafeHandle, out var safeHandleType))
                    {
                        context.RegisterSymbolAction(context => AnalyzeSymbol(context, safeHandleType), SymbolKind.NamedType);
                    }
                });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol safeHandleType)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

            if (type.TypeKind != TypeKind.Class)
            {
                // SafeHandle-derived types can only be classes.
                return;
            }

            if (type.IsAbstract || !type.Inherits(safeHandleType))
            {
                // We only want to put the diagnostic on concrete SafeHandle-derived types.
                return;
            }

            ImmutableDictionary<string, string?> diagnosticPropertyBag = ImmutableDictionary<string, string?>.Empty;
            ISymbol targetWarningSymbol = type;
            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.Parameters.Length == 0)
                {
                    if (constructor.DeclaredAccessibility == Accessibility.Public)
                    {
                        // The parameterless constructor is public, so there is no diagnostic to emit.
                        return;
                    }

                    // If a parameterless constructor has been defined, emit the diagnostic on the constructor instead of on the type.
                    targetWarningSymbol = constructor;
                    diagnosticPropertyBag = diagnosticPropertyBag.Add(DiagnosticPropertyConstructorExists, string.Empty);
                    break;
                }
            }

            foreach (var constructor in type.BaseType.InstanceConstructors)
            {
                if (constructor.Parameters.Length == 0 &&
                    context.Compilation.IsSymbolAccessibleWithin(constructor, type))
                {
                    diagnosticPropertyBag = diagnosticPropertyBag.Add(DiagnosticPropertyBaseConstructorAccessible, string.Empty);
                    break;
                }
            }

            context.ReportDiagnostic(targetWarningSymbol.CreateDiagnostic(Rule, diagnosticPropertyBag, type.Name));
        }
    }
}

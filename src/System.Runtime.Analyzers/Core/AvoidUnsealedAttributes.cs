﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1813: Seal attribute types for improved performance. Sealing attribute types speeds up performance during reflection on custom attributes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidUnsealedAttributesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1813";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.AvoidUnsealedAttributesTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.AvoidUnsealedAttributesMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.AvoidUnsealedAttributesDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Performance,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: false,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182267.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol attributeType = WellKnownTypes.Attribute(compilationContext.Compilation);
                if (attributeType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(context =>
                {
                    AnalyzeSymbol((INamedTypeSymbol)context.Symbol, attributeType, context.ReportDiagnostic);
                },
                SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(INamedTypeSymbol namedType, INamedTypeSymbol attributeType, Action<Diagnostic> addDiagnostic)
        {
            if (namedType.IsAbstract || namedType.IsSealed || !namedType.GetBaseTypesAndThis().Contains(attributeType))
            {
                return;
            }

            // Non-sealed non-abstract attribute type.
            addDiagnostic(namedType.CreateDiagnostic(Rule));
        }
    }
}

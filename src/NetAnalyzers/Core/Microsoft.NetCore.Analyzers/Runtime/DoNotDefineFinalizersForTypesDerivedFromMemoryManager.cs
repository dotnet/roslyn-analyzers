﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA2015: Do not define finalizers for types derived from MemoryManager&lt;T&gt;.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDefineFinalizersForTypesDerivedFromMemoryManager : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2015";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotDefineFinalizersForTypesDerivedFromMemoryManagerTitle)),
            CreateLocalizableResourceString(nameof(DoNotDefineFinalizersForTypesDerivedFromMemoryManagerMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(DoNotDefineFinalizersForTypesDerivedFromMemoryManagerDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationStartAction(context =>
            {
                var memoryManager1 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffersMemoryManager1);
                if (memoryManager1 == null)
                {
                    return;
                }

                context.RegisterSymbolAction(context =>
                {
                    AnalyzeSymbol((INamedTypeSymbol)context.Symbol, memoryManager1, context.ReportDiagnostic);
                }
                , SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol memoryManager, Action<Diagnostic> addDiagnostic)
        {
            if (namedTypeSymbol.DerivesFromOrImplementsAnyConstructionOf(memoryManager))
            {
                foreach (ISymbol symbol in namedTypeSymbol.GetMembers())
                {
                    if (symbol is IMethodSymbol method && method.IsFinalizer())
                    {
                        addDiagnostic(method.CreateDiagnostic(Rule, method.Name));
                        break;
                    }
                }
            }
        }
    }
}

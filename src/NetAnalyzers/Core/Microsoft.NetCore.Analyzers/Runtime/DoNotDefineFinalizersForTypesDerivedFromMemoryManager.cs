// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2015: Do not define finalizers for types derived from MemoryManager&lt;T&gt;.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDefineFinalizersForTypesDerivedFromMemoryManager : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2015";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotDefineFinalizersForTypesDerivedFromMemoryManagerTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotDefineFinalizersForTypesDerivedFromMemoryManagerMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotDefineFinalizersForTypesDerivedFromMemoryManagerDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol? attributeType = compilationContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemAttribute);
                if (attributeType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var memoryManager1 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBuffersMemoryManager1);

            if (memoryManager1 != null && namedTypeSymbol.DerivesFromOrImplementsAnyConstructionOf(memoryManager1))
            {
                namedTypeSymbol.GetMembers().FirstOrDefault(m => CheckMethod(m, context));
            }
        }

        private static bool CheckMethod(ISymbol m, SymbolAnalysisContext context)
        {
            if (m is IMethodSymbol method && method.IsFinalizer())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
                return true;
            }
            return false;
        }
    }
}

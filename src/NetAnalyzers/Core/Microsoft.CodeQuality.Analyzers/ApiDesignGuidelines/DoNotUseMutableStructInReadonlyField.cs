// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseMutableStructInReadonlyField : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1070";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseMutableStructInReadonlyFieldTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseMutableStructInReadonlyFieldMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotUseMutableStructInReadonlyFieldDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule =
            DiagnosticDescriptorHelper.Create(
                RuleId,
                s_localizableTitle,
                s_localizableMessage,
                DiagnosticCategory.Design,
                RuleLevel.BuildWarningCandidate,
                s_localizableDescription,
                isPortedFxCopRule: true,
                isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(context =>
            {
                var spinLockType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingSpinLock);
                var gcHandleType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesGCHandle);

                context.RegisterSymbolAction(context =>
                {
                    var field = (IFieldSymbol)context.Symbol;

                    if (!field.IsReadOnly ||
                        field.Type?.TypeKind != TypeKind.Struct)
                    {
                        return;
                    }

                    if (field.Type.Equals(spinLockType) ||
                        field.Type.Equals(gcHandleType))
                    {
                        context.ReportDiagnostic(field.CreateDiagnostic(Rule));
                    }
                }, SymbolKind.Field);
            });
        }
    }
}

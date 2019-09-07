// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle =
            new LocalizableResourceString(
                nameof(MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsTitle),
                MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        protected static readonly ImmutableList<string> MutableValueTypesOfInterest = new List<string>
        {
            "System.Threading.SpinLock", "System.Runtime.InteropServices.GCHandle"
        }.ToImmutableList();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            RegisterDiagnosticAction(context);
        }

        protected abstract void RegisterDiagnosticAction(AnalysisContext context);

        protected static void ReportDiagnostic(SyntaxNodeAnalysisContext context, Location readonlyLocation, string fieldName, string fieldTypeName)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, readonlyLocation,
                string.Format(s_localizableMessage.ToString(), fieldName, fieldTypeName)));
        }

        internal const string RuleId = "CA1829";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            "{0}",
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null);    // TODO: add MSDN url

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}

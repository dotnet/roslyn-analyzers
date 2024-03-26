// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;

    public abstract class MakeTypesInternal : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1515";

        protected static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(MakeTypesInternalTitle)),
            CreateLocalizableResourceString(nameof(MakeTypesInternalMessage)),
            DiagnosticCategory.Maintainability,
            RuleLevel.Disabled,
            description: CreateLocalizableResourceString(nameof(MakeTypesInternalDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly ImmutableHashSet<OutputKind> DefaultOutputKinds =
            ImmutableHashSet.Create(OutputKind.ConsoleApplication, OutputKind.WindowsApplication, OutputKind.WindowsRuntimeApplication);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(context =>
            {
                var compilation = context.Compilation;
                if (context.Compilation.SyntaxTrees.FirstOrDefault() is not { } firstSyntaxTree
                    || !context.Options.GetOutputKindsOption(Rule, firstSyntaxTree, compilation, DefaultOutputKinds).Contains(compilation.Options.OutputKind))
                {
                    return;
                }

                var typeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                INamedTypeSymbol? exceptionType = typeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemException);
                if (exceptionType is null)
                {
                    return;
                }

                context.RegisterSymbolAction(ctx => AnalyzeType(ctx, exceptionType), SymbolKind.NamedType);
            });
        }

        // Don't warn for exception types, since that may conflict with CA1064.
        /// <see cref="Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ExceptionsShouldBePublicAnalyzer"/>
        private void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol exceptionType)
        {
            INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (namedTypeSymbol.IsPublic()
                && !namedTypeSymbol.GetBaseTypes().Any(t => SymbolEqualityComparer.Default.Equals(t, exceptionType))
                && GetIdentifier(namedTypeSymbol.DeclaringSyntaxReferences[0].GetSyntax()) is SyntaxToken identifier)
            {
                context.ReportDiagnostic(identifier.CreateDiagnostic(Rule));
            }
        }

        protected abstract SyntaxToken? GetIdentifier(SyntaxNode type);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    }
}
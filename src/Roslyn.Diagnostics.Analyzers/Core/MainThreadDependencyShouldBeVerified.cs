// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MainThreadDependencyShouldBeVerified : AbstractThreadDependencyAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.MainThreadDependencyShouldBeVerifiedTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.MainThreadDependencyShouldBeVerifiedMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.MainThreadDependencyShouldBeVerifiedDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.MainThreadDependencyShouldBeVerifiedRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void HandleCompilationStart(CompilationStartAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, INamedTypeSymbol threadDependencyAttribute)
        {
            context.RegisterSymbolAction(context => HandleMethod(context, wellKnownTypeProvider), SymbolKind.Method);
            context.RegisterSymbolAction(HandleProperty, SymbolKind.Property);
            context.RegisterSymbolAction(HandleEvent, SymbolKind.Event);
            context.RegisterSymbolAction(HandleField, SymbolKind.Field);
            context.RegisterSymbolAction(HandleParameter, SymbolKind.Parameter);
        }

        private void HandleMethod(SymbolAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider)
        {
            var method = (IMethodSymbol)context.Symbol;
            var threadDependencyInfo = GetThreadDependencyInfo(method);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, TryGetThreadDependencyInfoLocation(method, context.CancellationToken) ?? GetLocation(method)));
            }

            threadDependencyInfo = GetThreadDependencyInfoForReturn(wellKnownTypeProvider, method);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {
                // GetThreadDependencyInfoForReturn checks both [NoMainThreadDependency] and
                // [return: NoMainThreadDependency], but TryGetThreadDependencyInfoLocationForReturn only checks the
                // latter. We only want to report a diagnostic specific to the return if we have an attribute specific
                // to the return.
                var returnLocation = TryGetThreadDependencyInfoLocationForReturn(method, context.CancellationToken);
                if (returnLocation is object)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, returnLocation ?? GetLocation(method)));
                }
            }
        }

        private void HandleProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;
            var threadDependencyInfo = GetThreadDependencyInfo(property);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {

                context.ReportDiagnostic(Diagnostic.Create(Rule, TryGetThreadDependencyInfoLocation(property, context.CancellationToken) ?? GetLocation(property)));
            }
        }

        private void HandleEvent(SymbolAnalysisContext context)
        {
            var @event = (IEventSymbol)context.Symbol;
            var threadDependencyInfo = GetThreadDependencyInfo(@event);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, TryGetThreadDependencyInfoLocation(@event, context.CancellationToken) ?? GetLocation(@event)));
            }
        }

        private void HandleField(SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;
            var threadDependencyInfo = GetThreadDependencyInfo(field);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, TryGetThreadDependencyInfoLocation(field, context.CancellationToken) ?? GetLocation(field)));
            }
        }

        private void HandleParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;
            var threadDependencyInfo = GetThreadDependencyInfo(parameter);
            if (threadDependencyInfo.IsExplicit && !threadDependencyInfo.Verified)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, TryGetThreadDependencyInfoLocation(parameter, context.CancellationToken) ?? GetLocation(parameter)));
            }
        }

        private static Location GetLocation(ISymbol symbol)
        {
            var location = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (location is null)
                return Location.None;

            return Location.Create(location.SyntaxTree, location.Span);
        }
    }
}

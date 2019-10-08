// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ImplementWithNoMainThreadDependency : AbstractThreadDependencyAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImplementWithNoMainThreadDependencyTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImplementWithNoMainThreadDependencyMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImplementWithNoMainThreadDependencyDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.ImplementWithNoMainThreadDependencyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        private enum VarianceBehavior
        {
            NoSubsetRelation,
            TrueIsMoreRestrictive,
            FalseIsMoreRestrictive,
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void HandleCompilationStart(CompilationStartAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, INamedTypeSymbol threadDependencyAttribute)
        {
            context.RegisterSymbolAction(context => HandleMethod(context, wellKnownTypeProvider), SymbolKind.Method);
            context.RegisterSymbolAction(context => HandleProperty(context, wellKnownTypeProvider), SymbolKind.Property);
            context.RegisterSymbolAction(HandleEvent, SymbolKind.Event);
        }

        private void HandleMethod(SymbolAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            if (methodSymbol.OverriddenMethod != null)
            {
                CheckMethod(ref context, wellKnownTypeProvider, methodSymbol, methodSymbol.OverriddenMethod);
            }

            if (methodSymbol.ContainingType != null)
            {
                foreach (var interfaceSymbol in methodSymbol.ContainingType.AllInterfaces)
                {
                    foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (methodSymbol.IsImplementationOfInterfaceMember(interfaceMember))
                        {
                            CheckMethod(ref context, wellKnownTypeProvider, methodSymbol, interfaceMember);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleProperty(SymbolAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;
            if (propertySymbol.OverriddenProperty != null)
            {
                CheckProperty(ref context, wellKnownTypeProvider, propertySymbol, propertySymbol.OverriddenProperty);
            }

            if (propertySymbol.ContainingType != null)
            {
                foreach (var interfaceSymbol in propertySymbol.ContainingType.AllInterfaces)
                {
                    foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (propertySymbol.IsImplementationOfInterfaceMember(interfaceMember))
                        {
                            CheckProperty(ref context, wellKnownTypeProvider, propertySymbol, interfaceMember);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleEvent(SymbolAnalysisContext context)
        {
            var eventSymbol = (IEventSymbol)context.Symbol;
            if (eventSymbol.OverriddenEvent != null)
            {
                CheckEvent(ref context, eventSymbol, eventSymbol.OverriddenEvent);
            }

            if (eventSymbol.ContainingType != null)
            {
                foreach (var interfaceSymbol in eventSymbol.ContainingType.AllInterfaces)
                {
                    foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<IEventSymbol>())
                    {
                        if (eventSymbol.IsImplementationOfInterfaceMember(interfaceMember))
                        {
                            CheckEvent(ref context, eventSymbol, interfaceMember);
                            break;
                        }
                    }
                }
            }
        }

        private void CheckMethod(ref SymbolAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, IMethodSymbol implementation, IMethodSymbol definition)
        {
            CheckSymbol(ref context, implementation, RefKind.None, RefKind.In, GetThreadDependencyInfo(implementation), GetThreadDependencyInfo(definition));
            CheckSymbol(ref context, implementation, implementation.RefKind, RefKind.Out, GetThreadDependencyInfoForReturn(wellKnownTypeProvider, implementation), GetThreadDependencyInfoForReturn(wellKnownTypeProvider, definition));
            for (int i = 0; i < implementation.Parameters.Length; i++)
            {
                var dataDirection = implementation.Parameters[i].RefKind;
                if (dataDirection == RefKind.None)
                    dataDirection = RefKind.In;

                CheckSymbol(ref context, location: implementation.Parameters[i], implementation.Parameters[i].RefKind, dataDirection, GetThreadDependencyInfo(implementation.Parameters[i]), GetThreadDependencyInfo(definition.Parameters[i]));
            }
        }

        private void CheckProperty(ref SymbolAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, IPropertySymbol implementation, IPropertySymbol definition)
        {
            RefKind dataDirection;
            if (definition.GetMethod != null)
            {
                dataDirection = definition.SetMethod != null ? RefKind.Ref : RefKind.Out;
            }
            else
            {
                dataDirection = RefKind.In;
            }

            CheckSymbol(ref context, implementation, implementation.RefKind, dataDirection, GetThreadDependencyInfo(implementation), GetThreadDependencyInfo(definition));
            if (definition.GetMethod != null)
            {
                CheckMethod(ref context, wellKnownTypeProvider, implementation.GetMethod, definition.GetMethod);
            }

            if (definition.SetMethod != null)
            {
                CheckMethod(ref context, wellKnownTypeProvider, implementation.SetMethod, definition.SetMethod);
            }
        }

        private void CheckEvent(ref SymbolAnalysisContext context, IEventSymbol implementation, IEventSymbol definition)
        {
            CheckSymbol(ref context, implementation, RefKind.None, RefKind.Ref, GetThreadDependencyInfo(implementation), GetThreadDependencyInfo(definition));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="location"></param>
        /// <param name="refKind"></param>
        /// <param name="dataDirection">
        /// <para><see cref="RefKind.Out"/> if the implementation may be more restricted than the definition</para>
        /// <para><see cref="RefKind.In"/> if the definition may be more restricted than the implementation</para>
        /// <para><see cref="RefKind.Ref"/> if the implementation must match the definition</para>
        /// </param>
        /// <param name="implementation"></param>
        /// <param name="definition"></param>
        private static void CheckSymbol(ref SymbolAnalysisContext context, ISymbol location, RefKind refKind, RefKind dataDirection, in ThreadDependencyInfo implementation, in ThreadDependencyInfo definition)
        {
            if (!implementation.IsExplicit && !definition.IsExplicit)
            {
                return;
            }

            if (definition.IsExplicit && !implementation.IsExplicit)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }

            if (definition.Verified && !implementation.Verified)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }

            if (!definition.PerInstance && implementation.PerInstance)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }

            if (!implementation.MayHaveMainThreadDependency)
            {
                return;
            }

            if (!IsSupportedVariance(definition.AlwaysCompleted, implementation.AlwaysCompleted, refKind, dataDirection, VarianceBehavior.TrueIsMoreRestrictive))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }

            if (!IsSupportedVariance(definition.CapturesContext, implementation.CapturesContext, refKind, dataDirection, VarianceBehavior.FalseIsMoreRestrictive))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }

            if (!IsSupportedVariance(definition.MayDirectlyRequireMainThread, implementation.MayDirectlyRequireMainThread, refKind, dataDirection, VarianceBehavior.FalseIsMoreRestrictive))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(location)));
                return;
            }
        }

        private static bool IsSupportedVariance(bool referenceValue, bool testValue, RefKind refKind, RefKind dataDirection, VarianceBehavior varianceBehavior)
        {
            if (dataDirection == RefKind.Ref
                || refKind == RefKind.Ref
                || dataDirection == RefKind.In && refKind == RefKind.Out
                || dataDirection == RefKind.Out && refKind == RefKind.In)
            {
                refKind = RefKind.Ref;
                varianceBehavior = VarianceBehavior.NoSubsetRelation;
            }
            else if (refKind == RefKind.None)
            {
                refKind = dataDirection;
            }

            if (varianceBehavior == VarianceBehavior.NoSubsetRelation)
            {
                return referenceValue == testValue;
            }

            if (refKind == RefKind.In)
            {
                // referenceValue is allowed to be more restrictive than testValue
                if (varianceBehavior == VarianceBehavior.TrueIsMoreRestrictive)
                {
                    return referenceValue || !testValue;
                }
                else
                {
                    Debug.Assert(varianceBehavior == VarianceBehavior.FalseIsMoreRestrictive);
                    return !referenceValue || testValue;
                }
            }
            else
            {
                // testValue is allowed to be more restrictive than referenceValue
                Debug.Assert(refKind == RefKind.Out);
                if (varianceBehavior == VarianceBehavior.TrueIsMoreRestrictive)
                {
                    return testValue || !referenceValue;
                }
                else
                {
                    Debug.Assert(varianceBehavior == VarianceBehavior.FalseIsMoreRestrictive);
                    return !testValue || referenceValue;
                }
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

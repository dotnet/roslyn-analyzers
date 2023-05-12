﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.FixAnalyzers
{
    using static CodeAnalysisDiagnosticsResources;

    /// <summary>
    /// RS1010: <inheritdoc cref="CreateCodeActionWithEquivalenceKeyTitle"/>
    /// RS1011: <inheritdoc cref="OverrideCodeActionEquivalenceKeyTitle"/>
    /// RS1016: <inheritdoc cref="OverrideGetFixAllProviderTitle"/>
    /// A <see cref="CodeFixProvider"/> that intends to support fix all occurrences must classify the registered code actions into equivalence classes by assigning it an explicit, non-null equivalence key which is unique across all registered code actions by this fixer.
    /// This enables the <see cref="FixAllProvider"/> to fix all diagnostics in the required scope by applying code actions from this fixer that are in the equivalence class of the trigger code action.
    /// This analyzer catches violations of this requirement in the code actions registered by a <see cref="CodeFixProvider"/> that supports <see cref="FixAllProvider"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class FixerWithFixAllAnalyzer : DiagnosticAnalyzer
    {
        private const string CodeActionMetadataName = "Microsoft.CodeAnalysis.CodeActions.CodeAction";
        private const string CreateMethodName = "Create";
        private const string EquivalenceKeyPropertyName = "EquivalenceKey";
        private const string EquivalenceKeyParameterName = "equivalenceKey";
        internal const string CodeFixProviderMetadataName = "Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider";
        internal const string GetFixAllProviderMethodName = "GetFixAllProvider";

        private static readonly LocalizableString s_localizableCodeActionNeedsEquivalenceKeyDescription = CreateLocalizableResourceString(nameof(CodeActionNeedsEquivalenceKeyDescription));

        internal static readonly DiagnosticDescriptor CreateCodeActionEquivalenceKeyRule = new(
            DiagnosticIds.CreateCodeActionWithEquivalenceKeyRuleId,
            CreateLocalizableResourceString(nameof(CreateCodeActionWithEquivalenceKeyTitle)),
            CreateLocalizableResourceString(nameof(CreateCodeActionWithEquivalenceKeyMessage)),
            "Correctness",
            DiagnosticSeverity.Warning,
            description: s_localizableCodeActionNeedsEquivalenceKeyDescription,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        internal static readonly DiagnosticDescriptor OverrideCodeActionEquivalenceKeyRule = new(
            DiagnosticIds.OverrideCodeActionEquivalenceKeyRuleId,
            CreateLocalizableResourceString(nameof(OverrideCodeActionEquivalenceKeyTitle)),
            CreateLocalizableResourceString(nameof(OverrideCodeActionEquivalenceKeyMessage)),
            "Correctness",
            DiagnosticSeverity.Warning,
            description: s_localizableCodeActionNeedsEquivalenceKeyDescription,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        internal static readonly DiagnosticDescriptor OverrideGetFixAllProviderRule = new(
            DiagnosticIds.OverrideGetFixAllProviderRuleId,
            CreateLocalizableResourceString(nameof(OverrideGetFixAllProviderTitle)),
            CreateLocalizableResourceString(nameof(OverrideGetFixAllProviderMessage)),
            "Correctness",
            DiagnosticSeverity.Warning,
            description: CreateLocalizableResourceString(nameof(OverrideGetFixAllProviderDescription)),
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(CreateCodeActionEquivalenceKeyRule, OverrideCodeActionEquivalenceKeyRule, OverrideGetFixAllProviderRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // We need to analyze generated code, but don't intend to report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            context.RegisterCompilationStartAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationStartAnalysisContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            INamedTypeSymbol? codeFixProviderSymbol = context.Compilation.GetOrCreateTypeByMetadataName(CodeFixProviderMetadataName);
            if (codeFixProviderSymbol == null)
            {
                return;
            }

            IMethodSymbol? getFixAllProviderMethod = codeFixProviderSymbol.GetMembers(GetFixAllProviderMethodName).OfType<IMethodSymbol>().FirstOrDefault();
            if (getFixAllProviderMethod == null)
            {
                return;
            }

            INamedTypeSymbol? codeActionSymbol = context.Compilation.GetOrCreateTypeByMetadataName(CodeActionMetadataName);
            if (codeActionSymbol == null)
            {
                return;
            }

            IPropertySymbol? equivalenceKeyProperty = codeActionSymbol.GetMembers(EquivalenceKeyPropertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (equivalenceKeyProperty == null)
            {
                return;
            }

            var createMethods = codeActionSymbol.GetMembers(CreateMethodName).OfType<IMethodSymbol>().ToImmutableHashSet();

            var analysisTypes = new AnalysisTypes(
                CodeFixProviderType: codeFixProviderSymbol,
                CodeActionType: codeActionSymbol,
                GetFixAllProviderMethod: getFixAllProviderMethod,
                EquivalenceKeyProperty: equivalenceKeyProperty,
                CreateMethods: createMethods);

            context.RegisterSymbolStartAction(context => AnalyzeNamedType(context, analysisTypes), SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolStartAnalysisContext context, AnalysisTypes analysisTypes)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;
            if (!symbol.DerivesFrom(analysisTypes.CodeFixProviderType))
                return;

            var namedTypeAnalyzer = new NamedTypeAnalyzer(analysisTypes);

            context.RegisterOperationBlockStartAction(namedTypeAnalyzer.OperationBlockStart);
            context.RegisterSymbolEndAction(namedTypeAnalyzer.SymbolEnd);
        }

        private record AnalysisTypes(
            INamedTypeSymbol CodeFixProviderType,
            INamedTypeSymbol CodeActionType,
            IMethodSymbol GetFixAllProviderMethod,
            IPropertySymbol EquivalenceKeyProperty,
            ImmutableHashSet<IMethodSymbol> CreateMethods);

        private sealed class NamedTypeAnalyzer
        {
            private readonly AnalysisTypes _analysisTypes;

            /// <summary>
            /// Map of invocations from code fix providers to invocations that create a code action using the static "Create" methods on <see cref="CodeAction"/>.
            /// </summary>
            private readonly Dictionary<INamedTypeSymbol, HashSet<IInvocationOperation>> _codeActionCreateInvocations = new();

            /// <summary>
            /// Map of invocations from code fix providers to object creations that create a code action using sub-types of <see cref="CodeAction"/>.
            /// </summary>
            private readonly Dictionary<INamedTypeSymbol, HashSet<IObjectCreationOperation>> _codeActionObjectCreations = new();

            public NamedTypeAnalyzer(AnalysisTypes analysisTypes)
            {
                _analysisTypes = analysisTypes;
            }

            internal void OperationBlockStart(OperationBlockStartAnalysisContext context)
            {
                if (context.OwningSymbol is not IMethodSymbol method)
                {
                    return;
                }

                INamedTypeSymbol namedType = method.ContainingType;
                if (!namedType.DerivesFrom(_analysisTypes.CodeFixProviderType))
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;
                    if (invocation.TargetMethod is IMethodSymbol invocationSym && _analysisTypes.CreateMethods.Contains(invocationSym))
                    {
                        AddOperation(namedType, invocation, _codeActionCreateInvocations);
                    }
                },
                OperationKind.Invocation);

                context.RegisterOperationAction(context =>
                {
                    var objectCreation = (IObjectCreationOperation)context.Operation;
                    IMethodSymbol constructor = objectCreation.Constructor;
                    if (constructor != null && constructor.ContainingType.DerivesFrom(_analysisTypes.CodeActionType))
                    {
                        AddOperation(namedType, objectCreation, _codeActionObjectCreations);
                    }
                },
                OperationKind.ObjectCreation);
            }

            private static void AddOperation<T>(INamedTypeSymbol namedType, T operation, Dictionary<INamedTypeSymbol, HashSet<T>> map)
                where T : IOperation
            {
                lock (map)
                {
                    if (!map.TryGetValue(namedType, out HashSet<T> value))
                    {
                        value = new HashSet<T>();
                        map[namedType] = value;
                    }

                    value.Add(operation);
                }
            }

            internal void SymbolEnd(SymbolAnalysisContext context)
            {
                if (_codeActionCreateInvocations.Count == 0 && _codeActionObjectCreations.Count == 0)
                {
                    // No registered fixes.
                    return;
                }

                // Analyze the fixer if it has FixAll support.
                // Otherwise, report RS1016 (OverrideGetFixAllProviderRule) to recommend adding FixAll support.
                var fixer = (INamedTypeSymbol)context.Symbol;
                if (OverridesGetFixAllProvider(fixer))
                {
                    AnalyzeFixerWithFixAll(fixer, context);
                }
                else if (SymbolEqualityComparer.Default.Equals(fixer.BaseType, _analysisTypes.CodeFixProviderType))
                {
                    Diagnostic diagnostic = fixer.CreateDiagnostic(OverrideGetFixAllProviderRule, fixer.Name);
                    context.ReportDiagnostic(diagnostic);
                }

                return;

                // Local functions
                bool OverridesGetFixAllProvider(INamedTypeSymbol fixer)
                {
                    foreach (INamedTypeSymbol type in fixer.GetBaseTypesAndThis())
                    {
                        if (!SymbolEqualityComparer.Default.Equals(type, _analysisTypes.CodeFixProviderType))
                        {
                            IMethodSymbol getFixAllProviderMethod = type.GetMembers(GetFixAllProviderMethodName).OfType<IMethodSymbol>().FirstOrDefault();
                            if (getFixAllProviderMethod != null && getFixAllProviderMethod.IsOverride)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                static bool IsViolatingCodeActionCreateInvocation(IInvocationOperation invocation)
                {
                    IParameterSymbol param = invocation.TargetMethod.Parameters.FirstOrDefault(p => p.Name == EquivalenceKeyParameterName);
                    if (param == null)
                    {
                        // User is calling an overload without the equivalenceKey parameter
                        return false;
                    }

                    foreach (var argument in invocation.Arguments)
                    {
                        if (SymbolEqualityComparer.Default.Equals(argument.Parameter, param))
                        {
                            return argument.Value.ConstantValue.HasValue && argument.Value.ConstantValue.Value == null;
                        }
                    }

                    return true;
                }

                void AnalyzeFixerWithFixAll(INamedTypeSymbol fixer, SymbolAnalysisContext context)
                {
                    if (_codeActionCreateInvocations != null
                        && _codeActionCreateInvocations.TryGetValue(fixer, out HashSet<IInvocationOperation> invocations))
                    {
                        foreach (IInvocationOperation invocation in invocations)
                        {
                            if (IsViolatingCodeActionCreateInvocation(invocation))
                            {
                                Diagnostic diagnostic = invocation.CreateDiagnostic(CreateCodeActionEquivalenceKeyRule, EquivalenceKeyParameterName);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }

                    if (_codeActionObjectCreations != null
                        && _codeActionObjectCreations.TryGetValue(fixer, out HashSet<IObjectCreationOperation> objectCreations))
                    {
                        foreach (IObjectCreationOperation objectCreation in objectCreations)
                        {
                            if (IsViolatingCodeActionObjectCreation(objectCreation))
                            {
                                Diagnostic diagnostic = objectCreation.CreateDiagnostic(OverrideCodeActionEquivalenceKeyRule, objectCreation.Constructor.ContainingType, EquivalenceKeyPropertyName);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }

                bool IsViolatingCodeActionObjectCreation(IObjectCreationOperation objectCreation)
                {
                    return objectCreation.Constructor.ContainingType.GetBaseTypesAndThis().All(namedType => !IsCodeActionWithOverriddenEquivalenceKey(namedType));

                    // Local functions
                    bool IsCodeActionWithOverriddenEquivalenceKey(INamedTypeSymbol namedType)
                    {
                        if (SymbolEqualityComparer.Default.Equals(namedType, _analysisTypes.CodeActionType))
                        {
                            return false;
                        }

                        // For types in different compilation, perform the check.
                        return IsCodeActionWithOverriddenEquivlanceKeyCore(namedType);
                    }
                }
            }

            private bool IsCodeActionWithOverriddenEquivlanceKeyCore(INamedTypeSymbol namedType)
            {
                if (!namedType.DerivesFrom(_analysisTypes.CodeActionType))
                {
                    // Not a CodeAction.
                    return false;
                }

                IPropertySymbol equivalenceKeyProperty = namedType.GetMembers(EquivalenceKeyPropertyName).OfType<IPropertySymbol>().FirstOrDefault();
                return equivalenceKeyProperty != null && equivalenceKeyProperty.IsOverride;
            }
        }
    }
}

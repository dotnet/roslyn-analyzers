// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1816: Dispose methods should call SuppressFinalize
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class CallGCSuppressFinalizeCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1816";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNotCalledWithFinalizer = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyMessageNotCalledWithFinalizer), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNotCalled = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyMessageNotCalled), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNotPassedThis = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyMessageNotPassedThis), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOutsideDispose = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyMessageOutsideDispose), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.CallGCSuppressFinalizeCorrectlyDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor NotCalledWithFinalizerRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNotCalledWithFinalizer,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-US/library/ms182269.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor NotCalledRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNotCalled,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-US/library/ms182269.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor NotPassedThisRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNotPassedThis,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-US/library/ms182269.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor OutsideDisposeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOutsideDispose,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-US/library/ms182269.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NotCalledWithFinalizerRule, NotCalledRule, NotPassedThisRule, OutsideDisposeRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Make analyzer thread safe.
            //analysisContext.EnableConcurrentExecution();

            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                var gcSuppressFinalizeMethodSymbol = compilationContext.Compilation
                                                        .GetTypeByMetadataName("System.GC")
                                                        ?.GetMembers("SuppressFinalize")
                                                        .OfType<IMethodSymbol>()
                                                        .SingleOrDefault();

                if (gcSuppressFinalizeMethodSymbol == null)
                {
                    return;
                }

                compilationContext.RegisterOperationBlockStartActionInternal(operationBlockContext =>
                {
                    if (operationBlockContext.OwningSymbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    var methodSymbol = (IMethodSymbol)operationBlockContext.OwningSymbol;
                    if (methodSymbol.IsExtern || methodSymbol.IsAbstract)
                    {
                        return;
                    }

                    var analyzer = new SuppressFinalizeAnalyzer(methodSymbol, gcSuppressFinalizeMethodSymbol, compilationContext.Compilation);

                    operationBlockContext.RegisterOperationActionInternal(analyzer.Analyze, OperationKind.InvocationExpression);
                    operationBlockContext.RegisterOperationBlockEndAction(analyzer.OperationBlockEndAction);
                });
            });

        }

        private class SuppressFinalizeAnalyzer
        {
            private enum SuppressFinalizeUsage
            {
                CanCall,
                MustCall,
                MustNotCall
            }

            private readonly Compilation _compilation;
            private readonly IMethodSymbol _containingMethodSymbol;
            private readonly IMethodSymbol _gcSuppressFinalizeMethodSymbol;
            private readonly SuppressFinalizeUsage _expectedUsage;

            private bool _suppressFinalizeCalled;
            private SemanticModel _semanticModel;

            public SuppressFinalizeAnalyzer(IMethodSymbol methodSymbol, IMethodSymbol gcSuppressFinalizeMethodSymbol, Compilation compilation)
            {
                this._compilation = compilation;
                this._containingMethodSymbol = methodSymbol;
                this._gcSuppressFinalizeMethodSymbol = gcSuppressFinalizeMethodSymbol;

                this._expectedUsage = GetAllowedSuppressFinalizeUsage(_containingMethodSymbol);
            }

            public void Analyze(OperationAnalysisContext analysisContext)
            {
                var invocationExpression = (IInvocationExpression)analysisContext.Operation;
                if (invocationExpression.TargetMethod.OriginalDefinition.Equals(_gcSuppressFinalizeMethodSymbol))
                {
                    _suppressFinalizeCalled = true;

                    if (_semanticModel == null)
                    {
                        _semanticModel = analysisContext.Compilation.GetSemanticModel(analysisContext.Operation.Syntax.SyntaxTree);
                    }

                    // Check for GC.SuppressFinalize outside of IDisposable.Dispose()
                    if (_expectedUsage == SuppressFinalizeUsage.MustNotCall)
                    {
                        analysisContext.ReportDiagnostic(invocationExpression.Syntax.CreateDiagnostic(
                            OutsideDisposeRule,
                            _containingMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                            _gcSuppressFinalizeMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                    }

                    // Checks for GC.SuppressFinalize(this)
                    if (invocationExpression.ArgumentsInEvaluationOrder.Count() != 1)
                    {
                        return;
                    }

                    var parameterSymbol = _semanticModel.GetSymbolInfo(invocationExpression.ArgumentsInEvaluationOrder.Single().Syntax).Symbol as IParameterSymbol;
                    if (parameterSymbol == null || !parameterSymbol.IsThis)
                    {
                        analysisContext.ReportDiagnostic(invocationExpression.Syntax.CreateDiagnostic(
                            NotPassedThisRule,
                            _containingMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                            _gcSuppressFinalizeMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                    }
                }
            }

            public void OperationBlockEndAction(OperationBlockAnalysisContext context)
            {
                // Check for absence of GC.SuppressFinalize
                if (!_suppressFinalizeCalled && _expectedUsage == SuppressFinalizeUsage.MustCall)
                {
                    var descriptor = _containingMethodSymbol.ContainingType.HasFinalizer() ? NotCalledWithFinalizerRule : NotCalledRule;
                    context.ReportDiagnostic(_containingMethodSymbol.CreateDiagnostic(
                        descriptor,
                        _containingMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat),
                        _gcSuppressFinalizeMethodSymbol.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                }
            }

            private SuppressFinalizeUsage GetAllowedSuppressFinalizeUsage(IMethodSymbol method)
            {
                // We allow constructors in sealed types to call GC.SuppressFinalize.
                // This allows types that derive from Component (such SqlConnection)
                // to prevent the finalizer they inherit from Component from ever 
                // being called.
                if (method.ContainingType.IsSealed && method.IsConstructor() && !method.IsStatic)
                {
                    return SuppressFinalizeUsage.CanCall;
                }

                if (!method.IsDisposeImplementation(_compilation))
                {
                    return SuppressFinalizeUsage.MustNotCall;
                }

                // If the Dispose method is declared in a sealed type, we do
                // not require that the method calls GC.SuppressFinalize
                var hasFinalizer = method.ContainingType.HasFinalizer();
                if (method.ContainingType.IsSealed && !hasFinalizer)
                {
                    return SuppressFinalizeUsage.CanCall;
                }

                // We don't require that non-public types call GC.SuppressFinalize 
                // if they don't have a finalizer as the owner of the assembly can 
                // control whether any finalizable types derive from them.
                if (method.ContainingType.DeclaredAccessibility != Accessibility.Public && !hasFinalizer)
                {
                    return SuppressFinalizeUsage.CanCall;
                }

                // Even if the Dispose method is declared on a type without a
                // finalizer, we still require it to call GC.SuppressFinalize to
                // prevent derived finalizable types from having to reimplement
                // IDisposable.Dispose just to call it.
                return SuppressFinalizeUsage.MustCall;
            }
        }
    }
}
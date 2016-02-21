// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1063: Implement IDisposable Correctly
    /// </summary>
    public abstract class ImplementIDisposableCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1063";

        private const string HelpLinkUri = "https://msdn.microsoft.com/library/ms244737.aspx";
        private const string DisposeMethodName = "Dispose";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageIDisposableReimplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageIDisposableReimplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFinalizeOverride = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageFinalizeOverride), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeOverride = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeOverride), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeSignature = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeSignature), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageRenameDispose = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageRenameDispose), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeBoolSignature = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeBoolSignature), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDisposeImplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageDisposeImplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageFinalizeImplementation = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageFinalizeImplementation), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageProvideDisposeBool = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyMessageProvideDisposeBool), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIDisposableCorrectlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor IDisposableReimplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIDisposableReimplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FinalizeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinalizeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeSignatureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeSignature,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor RenameDisposeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageRenameDispose,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeBoolSignatureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeBoolSignature,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeImplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeImplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FinalizeImplementationRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinalizeImplementation,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ProvideDisposeBoolRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageProvideDisposeBool,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(IDisposableReimplementationRule, FinalizeOverrideRule, DisposeOverrideRule, DisposeSignatureRule, RenameDisposeRule, DisposeBoolSignatureRule, DisposeImplementationRule, FinalizeImplementationRule, ProvideDisposeBoolRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                context =>
                {
                    var disposableType = WellKnownTypes.IDisposable(context.Compilation);
                    if (disposableType == null)
                    {
                        return;
                    }

                    var disposeInterfaceMethod = disposableType.GetMembers(DisposeMethodName).Single() as IMethodSymbol;
                    if (disposeInterfaceMethod == null)
                    {
                        return;
                    }

                    var analyzer = new Analyzer(context.Compilation, disposableType, disposeInterfaceMethod);
                    analyzer.Initialize(context);
                });
        }

        /// <summary>
        /// Analyzes single instance of compilation.
        /// </summary>
        private class Analyzer
        {
            private Compilation compilation;
            private INamedTypeSymbol disposableType;
            private IMethodSymbol disposeInterfaceMethod;

            public Analyzer(Compilation compilation, INamedTypeSymbol disposableType, IMethodSymbol disposeInterfaceMethod)
            {
                this.compilation = compilation;
                this.disposableType = disposableType;
                this.disposeInterfaceMethod = disposeInterfaceMethod;
            }

            public void Initialize(CompilationStartAnalysisContext context)
            {
                context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            }

            private void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                var type = context.Symbol as INamedTypeSymbol;
                if (type != null && type.IsType && type.TypeKind == TypeKind.Class)
                {
                    var implementsDisposableInBaseType = ImplementsDisposableInBaseType(type);

                    if (ImplementsDisposableDirectly(type))
                    {
                        var disposeMethod = FindDisposeMethod(type);
                        if (disposeMethod != null)
                        {
                            // This is difference from FxCop implementation
                            // IDisposable Reimplementation Rule is violated only if type re-implements Dispose method, not just interface
                            // For example see unit tests:
                            // CSharp_CA1063_IDisposableReimplementation_NoDiagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation
                            // Basic_CA1063_IDisposableReimplementation_Diagnostic_ImplementingInheritedInterfaceWithNoDisposeReimplementation
                            CheckIDisposableReimplementationRule(type, context, implementsDisposableInBaseType);

                            CheckDisposeSignatureRule(disposeMethod, type, context);
                            CheckRenameDisposeRule(disposeMethod, type, context);

                            if (!type.IsSealed)
                            {
                                var disposeBoolMethod = FindDisposeBoolMethod(type);
                                if (disposeBoolMethod != null)
                                {
                                    CheckDisposeBoolSignatureRule(disposeBoolMethod, type, context);
                                }
                                else
                                {
                                    CheckProvideDisposeBoolRule(type, context);
                                }
                            }
                        }
                    }

                    if (implementsDisposableInBaseType)
                    {
                        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                        {
                            CheckDisposeOverrideRule(method, type, context);
                        }

                        CheckFinalizeOverrideRule(type, context);
                    }
                }
            }

            /// <summary>
            /// Check rule: Remove IDisposable from the list of interfaces implemented by {0} and override the base class Dispose implementation instead.
            /// </summary>
            private static void CheckIDisposableReimplementationRule(INamedTypeSymbol type, SymbolAnalysisContext context, bool implementsDisposableInBaseType)
            {
                if (implementsDisposableInBaseType)
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(IDisposableReimplementationRule, type.Name));
                }
            }

            /// <summary>
            /// Checks rule: Ensure that {0} is declared as public and sealed.
            /// </summary>
            private static void CheckDisposeSignatureRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (!method.IsPublic() ||
                    method.IsAbstract || method.IsVirtual || (method.IsOverride && !method.IsSealed))
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeSignatureRule, $"{type.Name}.{method.Name}"));
                }
            }

            /// <summary>
            /// Checks rule: Rename {0} to 'Dispose' and ensure that it is declared as public and sealed.
            /// </summary>
            private static void CheckRenameDisposeRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (method.Name != DisposeMethodName)
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(RenameDisposeRule, $"{type.Name}.{method.Name}"));
                }
            }

            /// <summary>
            /// Checks rule: Remove {0}, override Dispose(bool disposing), and put the dispose logic in the code path where 'disposing' is true.
            /// </summary>
            private void CheckDisposeOverrideRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (method.MethodKind == MethodKind.Ordinary && method.IsOverride && method.ReturnsVoid && method.Parameters.Length == 0)
                {
                    var isDisposeOverride = false;
                    for (var m = method.OverriddenMethod; m != null; m = m.OverriddenMethod)
                    {
                        if (m == FindDisposeMethod(m.ContainingType))
                        {
                            isDisposeOverride = true;
                            break;
                        }
                    }

                    if (isDisposeOverride)
                    {
                        context.ReportDiagnostic(method.CreateDiagnostic(DisposeOverrideRule, $"{type.Name}.{method.Name}"));
                    }
                }
            }

            /// <summary>
            /// Checks rule: Remove the finalizer from type {0}, override Dispose(bool disposing), and put the finalization logic in the code path where 'disposing' is false.
            /// </summary>
            private static void CheckFinalizeOverrideRule(INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (type.HasFinalizer())
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(FinalizeOverrideRule, type.Name));
                }
            }

            /// <summary>
            /// Checks rule: Provide an overridable implementation of Dispose(bool) on {0} or mark the type as sealed. A call to Dispose(false) should only clean up native resources. A call to Dispose(true) should clean up both managed and native resources.
            /// </summary>
            private static void CheckProvideDisposeBoolRule(INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                context.ReportDiagnostic(type.CreateDiagnostic(ProvideDisposeBoolRule, type.Name));
            }

            /// <summary>
            /// Checks rule: Ensure that {0} is declared as protected, virtual, and unsealed.
            /// </summary>
            private static void CheckDisposeBoolSignatureRule(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (method.DeclaredAccessibility != Accessibility.Protected ||
                    !(method.IsVirtual || method.IsAbstract || method.IsOverride) || method.IsSealed)
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeBoolSignatureRule, $"{type.Name}.{method.Name}"));
                }
            }

            /// <summary>
            /// Checks if type implements IDisposable interface or an interface inherited from IDisposable.
            /// Only direct implementation is taken into account, implementation in base type is ignored.
            /// </summary>
            private bool ImplementsDisposableDirectly(ITypeSymbol type)
            {
                return type.Interfaces.Any(i => i.Inherits(disposableType));
            }

            /// <summary>
            /// Checks if base type implements IDisposable interface directly or indirectly.
            /// </summary>
            private bool ImplementsDisposableInBaseType(ITypeSymbol type)
            {
                return type.BaseType != null && type.BaseType.AllInterfaces.Contains(disposableType);
            }

            /// <summary>
            /// Returns method that implements IDisposable.Dispose operation.
            /// Only direct implementation is taken into account, implementation in base type is ignored.
            /// </summary>
            private IMethodSymbol FindDisposeMethod(INamedTypeSymbol type)
            {
                var disposeMethod = type.FindImplementationForInterfaceMember(disposeInterfaceMethod) as IMethodSymbol;
                if (disposeMethod != null && disposeMethod.ContainingType == type)
                {
                    return disposeMethod;
                }

                return null;
            }

            /// <summary>
            /// Returns method: void Dispose(bool)
            /// </summary>
            private static IMethodSymbol FindDisposeBoolMethod(INamedTypeSymbol type)
            {
                foreach (var method in type.GetMembers(DisposeMethodName).OfType<IMethodSymbol>())
                {
                    if (method.MethodKind == MethodKind.Ordinary && method.ReturnsVoid && method.Parameters.Length == 1)
                    {
                        var parameter = method.Parameters[0];
                        if (parameter.Type != null && parameter.Type.SpecialType == SpecialType.System_Boolean && parameter.RefKind == RefKind.None)
                        {
                            return method;
                        }
                    }
                }

                return null;
            }
        }
    }
}
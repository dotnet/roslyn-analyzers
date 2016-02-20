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
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor FinalizeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageFinalizeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeOverrideRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeOverride,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
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
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor DisposeBoolSignatureRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDisposeBoolSignature,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
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
                                                                             helpLinkUri: null,     // TODO: add MSDN url
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

                    var analyzer = new Analyzer(context.Compilation, disposableType);
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

            public Analyzer(Compilation compilation, INamedTypeSymbol disposableType)
            {
                this.compilation = compilation;
                this.disposableType = disposableType;
            }

            public void Initialize(CompilationStartAnalysisContext context)
            {
                context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            }

            private void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                var type = context.Symbol as INamedTypeSymbol;
                if (type != null && type.IsType)
                {
                    if (ImplementsDisposableDirectly(type))
                    {
                        var disposeMethod = FindDisposeMethod(type);
                        if (disposeMethod != null)
                        {
                            CheckDisposeSignature(disposeMethod, type, context);
                        }
                    }
                }
            }

            /// <summary>
            /// Checks rule: Ensure that {0} is declared as public and sealed.
            /// </summary>
            private static void CheckDisposeSignature(IMethodSymbol method, INamedTypeSymbol type, SymbolAnalysisContext context)
            {
                if (!method.IsPublic() ||
                    method.IsAbstract || method.IsVirtual || (method.IsOverride && !method.IsSealed))
                {
                    context.ReportDiagnostic(method.CreateDiagnostic(DisposeSignatureRule, $"{type.Name}.{method.Name}"));
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
            /// Returns method that implements IDisposable.Dispose operation.
            /// </summary>
            private IMethodSymbol FindDisposeMethod(INamedTypeSymbol type)
            {
                return type.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.IsDisposeImplementation(compilation));
            }
        }
    }
}
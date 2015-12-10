// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1033: Interface methods should be callable by child types
    /// <para>
    /// Consider a base type that explicitly implements a public interface method.
    /// A type that derives from the base type can access the inherited interface method only through a reference to the current instance ('this' in C#) that is cast to the interface.
    /// If the derived type re-implements (explicitly) the inherited interface method, the base implementation can no longer be accessed.
    /// The call through the current instance reference will invoke the derived implementation; this causes recursion and an eventual stack overflow.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This rule does not report a violation for an explicit implementation of IDisposable.Dispose when an externally visible Close() or System.IDisposable.Dispose(Boolean) method is provided.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class InterfaceMethodsShouldBeCallableByChildTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1033";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.InterfaceMethodsShouldBeCallableByChildTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.InterfaceMethodsShouldBeCallableByChildTypesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.InterfaceMethodsShouldBeCallableByChildTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                          s_localizableTitle,
                                                                          s_localizableMessage,
                                                                          DiagnosticCategory.Design,
                                                                          DiagnosticSeverity.Warning,
                                                                          isEnabledByDefault: false,
                                                                          description: s_localizableDescription,
                                                                          helpLinkUri: "https://msdn.microsoft.com/library/ms182153.aspx",
                                                                          customTags: WellKnownDiagnosticTags.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext analysisContext) =>
            analysisContext.RegisterOperationBlockAction(AnalyzeOperationBlock);

        private bool ShouldExcludeOperationBlock(ImmutableArray<IOperation> operationBlocks)
        {
            if (operationBlocks != null)
            {
                if (operationBlocks.Length == 0 ||
                    (operationBlocks.Length == 1 && operationBlocks[0].Kind == OperationKind.ThrowStatement))
                {
                    // Empty body OR body that just throws.
                    return true;
                }
            }

            return false;
        }

        private void AnalyzeOperationBlock(OperationBlockAnalysisContext context)
        {
            if (context.OwningSymbol.Kind != SymbolKind.Method)
            {
                return;
            }

            var method = (IMethodSymbol)context.OwningSymbol;

            // We are only interested in private explicit interface implementations within a public non-sealed type.
            if (method.ExplicitInterfaceImplementations.Length == 0 ||
                method.GetResultantVisibility() != SymbolVisibility.Private ||
                method.ContainingType.IsSealed ||
                method.ContainingType.GetResultantVisibility() != SymbolVisibility.Public)
            {
                return;
            }

            // Avoid false reports from simple explicit implementations where the deriving type is not expected to access the base implementation.
            if (ShouldExcludeOperationBlock(context.OperationBlocks))
            {
                return;
            }

            var hasPublicInterfaceImplementation = false;
            foreach (var interfaceMethod in method.ExplicitInterfaceImplementations)
            {
                // If any one of the explicitly implemented interface methods has a visible alternate, then effectively, they all do.
                if (HasVisibleAlternate(method.ContainingType, interfaceMethod))
                {
                    return;
                }

                hasPublicInterfaceImplementation = hasPublicInterfaceImplementation ||
                    interfaceMethod.ContainingType.GetResultantVisibility() == SymbolVisibility.Public;
            }

            // Even if none of the interface methods have alternates, there's only an issue if at least one of the interfaces is public.
            if (hasPublicInterfaceImplementation)
            {
                ReportDiagnostic(context, method.ContainingType.Name, method.Name);
            }
        }

        private static bool HasVisibleAlternate(INamedTypeSymbol namedType, IMethodSymbol interfaceMethod)
        {
            foreach (var type in namedType.GetBaseTypesAndThis())
            {
                foreach (var method in type.GetMembers(interfaceMethod.Name).OfType<IMethodSymbol>())
                {
                    if (method.GetResultantVisibility() == SymbolVisibility.Public)
                    {
                        return true;
                    }
                }
            }

            // This rule does not report a violation for an explicit implementation of IDisposable.Dispose when an externally visible Close() or System.IDisposable.Dispose(Boolean) method is provided.
            return interfaceMethod.Name.Equals("Dispose") &&
                interfaceMethod.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Equals("System.IDisposable") &&
                namedType.GetBaseTypesAndThis().Any(t =>
                    t.GetMembers("Close").OfType<IMethodSymbol>().Any(m =>
                        m.GetResultantVisibility() == SymbolVisibility.Public));
        }

        private static void ReportDiagnostic(OperationBlockAnalysisContext context, params object[] messageArgs)
        {
            var diagnostic = context.OwningSymbol.CreateDiagnostic(Rule, messageArgs);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

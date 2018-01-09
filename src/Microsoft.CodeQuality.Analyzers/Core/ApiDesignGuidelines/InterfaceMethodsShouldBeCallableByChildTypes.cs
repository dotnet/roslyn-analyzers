// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
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
                                                                          DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                          isEnabledByDefault: false,
                                                                          description: s_localizableDescription,
                                                                          helpLinkUri: "https://msdn.microsoft.com/library/ms182153.aspx",
                                                                          customTags: WellKnownDiagnosticTags.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX ? ImmutableArray.Create(Rule) : ImmutableArray<DiagnosticDescriptor>.Empty;

        public sealed override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol iDisposableTypeSymbol = WellKnownTypes.IDisposable(compilationContext.Compilation);
                if (iDisposableTypeSymbol == null)
                {
                    return;
                }

                compilationContext.RegisterOperationBlockAction(operationBlockContext => AnalyzeOperationBlock(operationBlockContext, iDisposableTypeSymbol));
            });
        }

        private static bool ShouldExcludeOperationBlock(ImmutableArray<IOperation> operationBlocks)
        {
            if (operationBlocks != null && operationBlocks.Length == 1)
            {
                IBlockOperation block = operationBlocks[0] as IBlockOperation;

                // Analyze IBlockOperation blocks.
                if (block == null)
                {
                    return true;
                }

                var operations = block.Operations.GetOperations();

                if (operations.Length == 0 ||
                    (operations.Length == 1 &&
                     operations[0].Kind == OperationKind.Throw))
                {
                    // Empty body OR body that just throws.
                    return true;
                }

                // Expression-bodied can be an implicit return and conversion on top of the throw operation
                if (operations.Length == 1 &&
                    operations[0] is IReturnOperation returnOp &&
                    returnOp.IsImplicit &&
                    returnOp.ReturnedValue is IConversionOperation conversionOp &&
                    conversionOp.IsImplicit && conversionOp.Operand.Kind == OperationKind.Throw)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AnalyzeOperationBlock(OperationBlockAnalysisContext context, INamedTypeSymbol iDisposableTypeSymbol)
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
                !method.ContainingType.IsExternallyVisible())
            {
                return;
            }

            // Avoid false reports from simple explicit implementations where the deriving type is not expected to access the base implementation.
            if (ShouldExcludeOperationBlock(context.OperationBlocks))
            {
                return;
            }

            var hasPublicInterfaceImplementation = false;
            foreach (IMethodSymbol interfaceMethod in method.ExplicitInterfaceImplementations)
            {
                // If any one of the explicitly implemented interface methods has a visible alternate, then effectively, they all do.
                if (HasVisibleAlternate(method.ContainingType, interfaceMethod, iDisposableTypeSymbol))
                {
                    return;
                }

                hasPublicInterfaceImplementation = hasPublicInterfaceImplementation ||
                    interfaceMethod.ContainingType.IsExternallyVisible();
            }

            // Even if none of the interface methods have alternates, there's only an issue if at least one of the interfaces is public.
            if (hasPublicInterfaceImplementation)
            {
                ReportDiagnostic(context, method.ContainingType.Name, method.Name);
            }
        }

        private static bool HasVisibleAlternate(INamedTypeSymbol namedType, IMethodSymbol interfaceMethod, INamedTypeSymbol iDisposableTypeSymbol)
        {
            foreach (INamedTypeSymbol type in namedType.GetBaseTypesAndThis())
            {
                foreach (IMethodSymbol method in type.GetMembers(interfaceMethod.Name).OfType<IMethodSymbol>())
                {
                    if (method.IsExternallyVisible())
                    {
                        return true;
                    }
                }
            }

            // This rule does not report a violation for an explicit implementation of IDisposable.Dispose when an externally visible Close() or System.IDisposable.Dispose(Boolean) method is provided.
            return interfaceMethod.Equals("Dispose") &&
                interfaceMethod.ContainingType.Equals(iDisposableTypeSymbol) &&
                namedType.GetBaseTypesAndThis().Any(t =>
                    t.GetMembers("Close").OfType<IMethodSymbol>().Any(m =>
                        m.IsExternallyVisible()));
        }

        private static void ReportDiagnostic(OperationBlockAnalysisContext context, params object[] messageArgs)
        {
            Diagnostic diagnostic = context.OwningSymbol.CreateDiagnostic(Rule, messageArgs);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

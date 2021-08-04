// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CS2255: ModuleInitializer attributes should not be used in libraries.
    /// </summary>
    /// <remarks>
    /// ModuleInitializer methods must:
    /// - Be parameterless
    /// - Be void or async void
    /// - Not be generic or contained in a generic type
    /// - Be accessible in the module using public or internal
    /// </remarks>
#pragma warning disable RS1004 // The [ModuleInitializer] feature only applies to C#
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004
    public sealed class ModuleInitializerAttributeShouldNotBeUsedInLibraries : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2255";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ModuleInitializerAttributeShouldNotBeUsedInLibrariesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ModuleInitializerAttributeShouldNotBeUsedInLibrariesMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.ModuleInitializerAttributeShouldNotBeUsedInLibrariesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                    s_localizableTitle,
                                                                    s_localizableMessage,
                                                                    DiagnosticCategory.Usage,
                                                                    RuleLevel.BuildWarning,
                                                                    s_localizableDescription,
                                                                    isPortedFxCopRule: true,
                                                                    isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol? moduleInitializerAttributeType = compilationContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesModuleInitializerAttribute);

                if (moduleInitializerAttributeType is null) return;

                compilationContext.RegisterSymbolAction(context =>
                {
                    if (context.Symbol is IMethodSymbol method)
                    {
                        if (method.IsPrivate() ||
                            method.Parameters.Length > 0 ||
                            method.IsGenericMethod ||
                            !method.IsStatic ||
                            !method.ReturnsVoid
                        )
                        {
                            return;
                        }

                        AttributeData? initializerAttribute = context.Symbol.GetAttributes(moduleInitializerAttributeType).FirstOrDefault();

                        if (initializerAttribute is not null)
                        {
                            context.ReportDiagnostic(initializerAttribute.ApplicationSyntaxReference.GetSyntax().CreateDiagnostic(Rule));
                        }
                    }
                },
                SymbolKind.Method);
            });
        }
    }
}

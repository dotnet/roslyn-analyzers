// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using Analyzer.Utilities.Extensions;
using System.Linq;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// Async002: Async Method Names Should End in Async
    /// </summary>
    public abstract class AsyncMethodNamesShouldEndInAsyncAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "Async002";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AsyncMethodNamesShouldEndInAsyncTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AsyncMethodNamesShouldEndInAsyncMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AsyncMethodNamesShouldEndInAsyncDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (NeedsAsyncSuffix(method, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations.First(), method.Name));
            }
        }

        private static bool NeedsAsyncSuffix(IMethodSymbol method, Compilation compilation)
        {
            if (!method.ReturnType.IsAwaitableType() || method.Name.EndsWith("Async", StringComparison.Ordinal))
            {
                return false;
            }

            // If the method is marked override, renaming it will cause errors. We will rename such a method when
            // and only when the virtual or abstract method it overrides is also renamed.
            if (method.IsOverride)
            {
                return false;
            }

            // If the method's definition is in another assembly, its name is beyond our control. Renaming references
            // to it will not help.
            var containingAssembly = method.OriginalDefinition.ContainingAssembly;
            var currentAssembly = compilation.Assembly;
            if (containingAssembly != currentAssembly)
            {
                return false;
            }

            // TODO: Do not fire the analyzer for unit test methods.
            // TODO: Do not fire the analyzer when it could introduce a name conflict or overload resolution issues.
            return true;
        }
    }
}

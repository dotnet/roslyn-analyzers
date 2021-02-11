﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class RelaxTestNamingSuppressor : DiagnosticSuppressor
    {
        private const string Id = RoslynDiagnosticIds.RelaxTestNamingSuppressionRuleId;

        // VSTHRD200: Use Async suffix for async methods
        // https://github.com/microsoft/vs-threading/blob/master/doc/analyzers/VSTHRD200.md
        private const string SuppressedDiagnosticId = "VSTHRD200";

        private static readonly LocalizableString s_localizableJustification = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.RelaxTestNamingSuppressorJustification), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static readonly SuppressionDescriptor Rule =
            new(Id, SuppressedDiagnosticId, s_localizableJustification);

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Rule);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            if (context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitFactAttribute) is not { } factAttribute)
            {
                return;
            }

            var knownTestAttributes = new ConcurrentDictionary<INamedTypeSymbol, bool>();

            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                // The diagnostic is reported on the test method
                if (diagnostic.Location.SourceTree is not { } tree)
                {
                    continue;
                }

                var root = tree.GetRoot(context.CancellationToken);
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

                var semanticModel = context.GetSemanticModel(tree);
                var declaredSymbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken);
                if (declaredSymbol is IMethodSymbol method
                    && method.IsXUnitTestMethod(knownTestAttributes, factAttribute))
                {
                    context.ReportSuppression(Suppression.Create(Rule, diagnostic));
                }
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DefineResourceEntryCorrectly : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.DefineResourceEntryCorrectlyTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.DefineResourceEntryCorrectlyMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.DefineResourceEntryCorrectlyDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.DefineResourceEntryCorrectlyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslynDiagnosticsDesign,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly Regex s_dataNameRegex = new Regex("<data.*?name=\"(.*?)\"", RegexOptions.Compiled);
        private static readonly Regex s_valueRegex = new Regex("<value>(.*?)</value>", RegexOptions.Compiled);
        private const StringComparison s_ResxContentStringComparison = StringComparison.Ordinal;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationAction(context =>
            {
                foreach (var file in context.Options.AdditionalFiles)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var fileExtension = Path.GetExtension(file.Path);
                    if (!fileExtension.Equals(".resx", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var isDescription = false;
                    var sourceText = file.GetText(context.CancellationToken);
                    foreach (var line in sourceText.Lines)
                    {
                        var text = line.ToString().Trim();

                        var match = s_dataNameRegex.Match(text);
                        if (match.Success)
                        {
                            isDescription = match.Groups[1].Value.EndsWith("Description", s_ResxContentStringComparison);
                            continue;
                        }

                        match = s_valueRegex.Match(text);
                        if (match.Success)
                        {
                            var endsWithPeriod = match.Groups[1].Value.EndsWith(".", s_ResxContentStringComparison);

                            if (endsWithPeriod && !isDescription)
                            {
                                var linePositionSpan = sourceText.Lines.GetLinePositionSpan(line.Span);
                                var location = Location.Create(file.Path, line.Span, linePositionSpan);
                                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
                            }
                            else if (isDescription && !endsWithPeriod)
                            {
                                var linePositionSpan = sourceText.Lines.GetLinePositionSpan(line.Span);
                                var location = Location.Create(file.Path, line.Span, linePositionSpan);
                                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
                            }
                            else
                            {
                                // do nothing
                            }
                        }
                    }
                }
            });
        }
    }
}

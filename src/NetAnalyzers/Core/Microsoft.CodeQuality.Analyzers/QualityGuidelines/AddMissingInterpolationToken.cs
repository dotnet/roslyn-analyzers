// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AddMissingInterpolationTokenAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2251";

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning disable RS1032 // Define diagnostic message correctly - the analyzer wants a period after the existing question mark.
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning restore RS1032 // Define diagnostic message correctly
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(context =>
            {
                var literalOperation = (ILiteralOperation)context.Operation;
                if (!literalOperation.ConstantValue.HasValue ||
                    literalOperation.ConstantValue.Value is not string stringText)
                {
                    return;
                }

                var parts = GetInterpolationParts(stringText);
                if (parts.All(s => IsValidInterpolationPart(s, literalOperation.SemanticModel, literalOperation.Syntax.SpanStart)))
                {
                    context.ReportDiagnostic(literalOperation.CreateDiagnostic(Rule));
                }
            }, OperationKind.Literal);
        }

        private static bool IsValidInterpolationPart(string s, SemanticModel semanticModel, int position)
        {
            if (uint.TryParse(s, out _))
            {
                // Numerical literals are valid interpolation from language perspective.
                // But we don't want the analyzer to flag for them. So,
                // they're invalid from analyzer perspective.
                return false;
            }
            return !semanticModel.LookupSymbols(position, name: s).IsDefaultOrEmpty;
        }

        private static IEnumerable<string> GetInterpolationParts(string s)
        {
            var index = 0;
            var isInsideInterpolation = false;
            var currentPart = string.Empty;
            var list = new List<string>();
            while (index < s.Length)
            {
                if (ContainsEscapedBraces(index, s) && !isInsideInterpolation)
                {
                    // Escaped brace - either {{ or }}.
                    index += 2;
                    continue;
                }

                if (s[index] == '{')
                {
                    if (isInsideInterpolation)
                    {
                        // The analyzer doesn't flag for nested interpolation.
                        return Enumerable.Empty<string>();
                    }
                    isInsideInterpolation = true;
                }
                else if (s[index] == '}')
                {
                    isInsideInterpolation = false;
                    list.Add(currentPart);
                    currentPart = string.Empty;
                }
                else if (isInsideInterpolation)
                {
                    currentPart += s[index];
                }
                index++;
            }
            return list;
        }

        private static bool ContainsEscapedBraces(int index, string s)
            => index != s.Length - 1 && s[index] == s[index + 1] && s[index] is '{' or '}';
    }
}

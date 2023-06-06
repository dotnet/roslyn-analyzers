// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA1041: <inheritdoc cref="ProvideObsoleteAttributeMessageTitle"/>
    /// </summary>
    public abstract class ProvideObsoleteAttributeMessageAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1041";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(ProvideObsoleteAttributeMessageTitle)),
            CreateLocalizableResourceString(nameof(ProvideObsoleteAttributeMessageMessage)),
            DiagnosticCategory.Design,
            RuleLevel.IdeSuggestion,
            description: CreateLocalizableResourceString(nameof(ProvideObsoleteAttributeMessageDescription)),
            isPortedFxCopRule: true,
            isDataflowRule: false);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private protected static bool IsObsoleteAttributeName(string attributeName) => attributeName is "Obsolete" or "ObsoleteAttribute";
    }
}
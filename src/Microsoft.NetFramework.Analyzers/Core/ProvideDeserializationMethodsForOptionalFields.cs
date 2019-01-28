// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetFramework.Analyzers
{
    /// <summary>
    /// CA2239: Provide deserialization methods for optional fields
    /// </summary>
    public abstract class ProvideDeserializationMethodsForOptionalFieldsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2239";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsTitle), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageOnDeserialized = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsMessageOnDeserialized), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOnDeserializing = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsMessageOnDeserializing), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetFrameworkAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsDescription), MicrosoftNetFrameworkAnalyzersResources.ResourceManager, typeof(MicrosoftNetFrameworkAnalyzersResources));

        internal static DiagnosticDescriptor OnDeserializedRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOnDeserialized,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2239-provide-deserialization-methods-for-optional-fields",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor OnDeserializingRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOnDeserializing,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2239-provide-deserialization-methods-for-optional-fields",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>  ImmutableArray<DiagnosticDescriptor>.Empty;
            //ImmutableArray.Create(OnDeserializedRule, OnDeserializingRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Enable concurrent execution of analyzer actions.
            //analysisContext.EnableConcurrentExecution();

            // TODO: Configure generated code analysis.
            //analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}
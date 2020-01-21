// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseXmlReaderForDeserialize : UseXmlReaderBase
    {
        internal const string DiagnosticId = "CA5369";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseXmlReaderForDeserialize),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor RealRule = DiagnosticDescriptorHelper.Create(
                DiagnosticId,
                s_Title,
                Message,
                DiagnosticCategory.Security,
                RuleLevel.BuildWarning,
                description: Description,
                isPortedFxCopRule: false,
                isDataflowRule: false);

        protected override string TypeMetadataName => WellKnownTypeNames.SystemXmlSerializationXmlSerializer;

        protected override string MethodMetadataName => "Deserialize";

        protected override DiagnosticDescriptor Rule => RealRule;
    }
}

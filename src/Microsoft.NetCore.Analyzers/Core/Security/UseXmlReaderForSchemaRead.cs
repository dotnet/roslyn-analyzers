// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseXmlReaderForSchemaRead : UseXmlReaderBase
    {
        internal const string DiagnosticId = "CA5371";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseXmlReaderForSchemaRead),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor RealRule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);

        protected override string TypeMetadataName => WellKnownTypeNames.SystemXmlSchemaXmlSchema;

        protected override string MethodMetadataName => "Read";

        protected override DiagnosticDescriptor Rule => RealRule;
    }
}

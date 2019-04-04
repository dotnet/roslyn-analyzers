// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseXmlReaderForDeserialize : UseXmlReaderBase
    {
        internal static readonly DiagnosticDescriptor RealRule =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA5369",
                nameof(SystemSecurityCryptographyResources.UseXmlReaderForDeserialize),
                nameof(SystemSecurityCryptographyResources.UseXmlReaderMessage),
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                helpLinkUri: null,
                nameof(SystemSecurityCryptographyResources.UseXmlReaderDescription));

        protected override string TypeMetadataName => WellKnownTypeNames.SystemXmlSerializationXmlSerializer;

        protected override string MethodMetadataName => "Deserialize";

        protected override DiagnosticDescriptor Rule => RealRule;
    }
}

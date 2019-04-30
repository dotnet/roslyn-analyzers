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
        internal static readonly DiagnosticDescriptor RealRule =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA5371",
                nameof(SystemSecurityCryptographyResources.UseXmlReaderForSchemaRead),
                nameof(SystemSecurityCryptographyResources.UseXmlReaderMessage),
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                helpLinkUri: null,
                nameof(SystemSecurityCryptographyResources.UseXmlReaderDescription));

        protected override string TypeMetadataName => WellKnownTypeNames.SystemXmlSchemaXmlSchema;

        protected override string MethodMetadataName => "Read";

        protected override DiagnosticDescriptor Rule => RealRule;
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseXmlReaderForDataSetReadXml : UseXmlReaderBase
    {
        internal static readonly DiagnosticDescriptor RealRule =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA5366",
                nameof(SystemSecurityCryptographyResources.UseXmlReaderForDataSetReadXml),
                nameof(SystemSecurityCryptographyResources.UseXmlReaderForDataSetReadXmlMessage),
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                helpLinkUri: null);

        protected override string TypeMetadataName => WellKnownTypeNames.SystemDataDataSet;

        protected override string MethodMetadataName => "ReadXml";

        protected override DiagnosticDescriptor Rule => RealRule;
    }
}

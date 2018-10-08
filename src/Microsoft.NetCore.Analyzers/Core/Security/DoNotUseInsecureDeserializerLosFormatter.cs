// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with LosFormatter, which can result in remote code execution.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseInsecureDeserializerLosFormatter : DoNotUseInsecureDeserializerMethodsBase
    {
        // TODO paulming: Help link URLs.
        internal static DiagnosticDescriptor RealInvocationDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2310",
                nameof(MicrosoftNetCoreSecurityResources.LosFormatterMethodInvocationTitle),
                nameof(MicrosoftNetCoreSecurityResources.LosFormatterMethodInvocationMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);
        internal static DiagnosticDescriptor RealReferenceDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2311",
                nameof(MicrosoftNetCoreSecurityResources.LosFormatterMethodReferenceTitle),
                nameof(MicrosoftNetCoreSecurityResources.LosFormatterMethodReferenceMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);

        protected override string DeserializerTypeMetadataName => WellKnownTypes.SystemWebUILosFormatter;

        protected override ImmutableHashSet<string> DeserializationMethodNames => 
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize");

        protected override DiagnosticDescriptor InvocationDescriptor => RealInvocationDescriptor;

        protected override DiagnosticDescriptor ReferenceDescriptor => RealReferenceDescriptor;
    }
}

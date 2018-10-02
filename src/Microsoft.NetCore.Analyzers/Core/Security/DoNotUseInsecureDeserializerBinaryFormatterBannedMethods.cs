// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotUseInsecureDeserializerBinaryFormatterBannedMethods : DoNotUseInsecureDeserializerBannedMethodsBase
    {
        // TODO paulming: Help links URLs.
        internal static readonly DiagnosticDescriptor RealBannedMethodDescriptor =
            new DiagnosticDescriptor(
                "CA2300",
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterBannedMethodTitle)),
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterBannedMethodMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false,
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterBannedMethodDescription)));

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypes.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override ImmutableHashSet<string> BannedMethodNames =>
            SecurityConstants.BinaryFormatterDeserializationMethods;

        protected override DiagnosticDescriptor BannedMethodDescriptor => RealBannedMethodDescriptor;
    }
}

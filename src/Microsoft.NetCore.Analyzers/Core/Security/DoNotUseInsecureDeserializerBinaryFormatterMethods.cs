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
    class DoNotUseInsecureDeserializerBinaryFormatterMethods : DoNotUseInsecureDeserializerMethodsBase
    {
        // TODO paulming: Help links URLs.
        internal static readonly DiagnosticDescriptor RealInvocationDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2300",
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodInvocationTitle),
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodInvocationMessage),
                isEnabledByDefault: false,
                helpLinkUri: null,
                descriptionResourceStringName: nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodInvocationDescription));
        internal static readonly DiagnosticDescriptor RealReferenceDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2301",
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodReferenceTitle),
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodReferenceMessage),
                isEnabledByDefault: false,
                helpLinkUri: null,
                descriptionResourceStringName: nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodReferenceDescription));

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypes.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override ImmutableHashSet<string> DeserializationMethodNames =>
            SecurityHelpers.BinaryFormatterDeserializationMethods;

        protected override DiagnosticDescriptor InvocationDescriptor => RealInvocationDescriptor;

        protected override DiagnosticDescriptor ReferenceDescriptor => RealReferenceDescriptor;
    }
}

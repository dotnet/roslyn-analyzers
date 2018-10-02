// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/> when its Binder property is not set.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseInsecureDeserializerBinaryFormatterWithoutBinder : DoNotUseInsecureDeserializerWithoutBinderBase
    {
        // TODO paulming: Help link URLs.
        internal static readonly DiagnosticDescriptor RealBinderDefinitelyNotSetDescriptor =
            new DiagnosticDescriptor(
                "CA2301",
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeWithoutBinderSetTitle)),
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeWithoutBinderSetMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);
        internal static readonly DiagnosticDescriptor RealBinderMaybeNotSetDescriptor =
            new DiagnosticDescriptor(
                "CA2302",
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeMaybeWithoutBinderSetTitle)),
                GetResourceString(nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeMaybeWithoutBinderSetMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);

        protected override string DeserializerTypeMetadataName => 
            WellKnownTypes.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override string SerializationBinderPropertyMetadataName => "Binder";

        protected override ImmutableHashSet<string> DeserializationMethodNames => 
            SecurityConstants.BinaryFormatterDeserializationMethods;

        protected override DiagnosticDescriptor BinderDefinitelyNotSetDescriptor => RealBinderDefinitelyNotSetDescriptor;

        protected override DiagnosticDescriptor BinderMaybeNotSetDescriptor => RealBinderMaybeNotSetDescriptor;
    }
}

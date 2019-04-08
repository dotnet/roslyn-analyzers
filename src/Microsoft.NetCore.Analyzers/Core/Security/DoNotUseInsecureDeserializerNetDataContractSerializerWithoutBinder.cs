// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with <see cref="System.Runtime.Serialization.Formatters.Binary.NetDataContractSerializer"/> when its Binder property is not set.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseInsecureDeserializerNetDataContractSerializerWithoutBinder : DoNotUseInsecureDeserializerWithoutBinderBase
    {
        internal static readonly DiagnosticDescriptor RealBinderDefinitelyNotSetDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2311",
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeWithoutBinderSetTitle),
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeWithoutBinderSetMessage),
                isEnabledByDefault: false,
                helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2301-do-not-call-binaryformatter.deserialize-without-first-setting-binaryformatter.binder");
        internal static readonly DiagnosticDescriptor RealBinderMaybeNotSetDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2312",
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeMaybeWithoutBinderSetTitle),
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeMaybeWithoutBinderSetMessage),
                isEnabledByDefault: false,
                helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2302-ensure-binaryformatter.binder-is-set-before-calling-binaryformatter.deserialize");

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypeNames.SystemRuntimeSerializationNetDataContractSerializer;

        protected override string SerializationBinderPropertyMetadataName => "Binder";

        protected override ImmutableHashSet<string> DeserializationMethodNames =>
            SecurityHelpers.NetDataContractSerializerDeserializationMethods;

        protected override DiagnosticDescriptor BinderDefinitelyNotSetDescriptor => RealBinderDefinitelyNotSetDescriptor;

        protected override DiagnosticDescriptor BinderMaybeNotSetDescriptor => RealBinderMaybeNotSetDescriptor;
    }
}

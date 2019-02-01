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
        // TODO paulming: Help link URLs.
        internal static readonly DiagnosticDescriptor RealBinderDefinitelyNotSetDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2311",
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeWithoutBinderSetTitle),
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeWithoutBinderSetMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);
        internal static readonly DiagnosticDescriptor RealBinderMaybeNotSetDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2312",
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeMaybeWithoutBinderSetTitle),
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerDeserializeMaybeWithoutBinderSetMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypes.SystemRuntimeSerializationNetDataContractSerializer;

        protected override string SerializationBinderPropertyMetadataName => "Binder";

        protected override ImmutableHashSet<string> DeserializationMethodNames =>
            SecurityHelpers.NetDataContractSerializerDeserializationMethods;

        protected override DiagnosticDescriptor BinderDefinitelyNotSetDescriptor => RealBinderDefinitelyNotSetDescriptor;

        protected override DiagnosticDescriptor BinderMaybeNotSetDescriptor => RealBinderMaybeNotSetDescriptor;
    }
}

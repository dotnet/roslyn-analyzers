﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with <see cref="T:System.Runtime.Serialization.Formatters.Binary.NetDataContractSerializer"/>.
    /// </summary>
    [SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "The comment references a type that is not referenced by this compilation.")]
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotUseInsecureDeserializerNetDataContractSerializerMethods : DoNotUseInsecureDeserializerMethodsBase
    {
        // TODO paulming: Help links URLs.
        internal static readonly DiagnosticDescriptor RealMethodUsedDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2310",
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerMethodUsedTitle),
                nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerMethodUsedMessage),
                isEnabledByDefault: false,
                helpLinkUri: null,
                descriptionResourceStringName: nameof(MicrosoftNetCoreSecurityResources.NetDataContractSerializerMethodUsedDescription));

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypeNames.SystemRuntimeSerializationNetDataContractSerializer;

        protected override ImmutableHashSet<string> DeserializationMethodNames =>
            SecurityHelpers.NetDataContractSerializerDeserializationMethods;

        protected override DiagnosticDescriptor MethodUsedDescriptor => RealMethodUsedDescriptor;
    }
}

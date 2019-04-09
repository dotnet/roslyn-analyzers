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
        internal static readonly DiagnosticDescriptor RealMethodUsedDescriptor =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2300",
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodUsedTitle),
                nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodUsedMessage),
                isEnabledByDefault: false,
                helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2300-do-not-use-insecure-deserializer-binaryformatter",
                descriptionResourceStringName: nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterMethodUsedDescription));

        protected override string DeserializerTypeMetadataName =>
            WellKnownTypeNames.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override ImmutableHashSet<string> DeserializationMethodNames =>
            SecurityHelpers.BinaryFormatterDeserializationMethods;

        protected override DiagnosticDescriptor MethodUsedDescriptor => RealMethodUsedDescriptor;
    }
}

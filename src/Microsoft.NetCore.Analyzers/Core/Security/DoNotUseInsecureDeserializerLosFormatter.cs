using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with LosFormatter, which can result in remote code execution.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseInsecureDeserializerLosFormatter : DoNotUseInsecureDeserializerBannedMethodsBase
    {
        internal static DiagnosticDescriptor RealBannedMethodDescriptor = new DiagnosticDescriptor(
            "CA2304",
            GetResourceString(nameof(MicrosoftNetCoreSecurityResources.LosFormatterBannedMethodTitle)),
            GetResourceString(nameof(MicrosoftNetCoreSecurityResources.LosFormatterBannedMethodMessage)),
            DiagnosticCategory.Security,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            false);

        protected override string DeserializerTypeMetadataName => WellKnownTypes.SystemWebUILosFormatter;

        protected override ImmutableHashSet<string> BannedMethodNames => 
            ImmutableHashSet.Create(
                "Deserialize");

        protected override DiagnosticDescriptor BannedMethodDescriptor => RealBannedMethodDescriptor;
    }
}

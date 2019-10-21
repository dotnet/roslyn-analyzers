// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotDecryptWithoutHash : SourceTriggeredTaintedDataAnalyzerBase
    {
        internal static DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5404",
            typeof(MicrosoftNetCoreAnalyzersResources),
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotDecryptWithoutHash),
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotDecryptWithoutHashMessage),
            isEnabledByDefault: false,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.DoNotDecryptWithoutHashDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        protected override SinkKind SinkKind { get { return SinkKind.DecryptWithoutHash; } }

        protected override DiagnosticDescriptor TaintedDataEnteringSinkDescriptor { get { return Rule; } }
    }
}

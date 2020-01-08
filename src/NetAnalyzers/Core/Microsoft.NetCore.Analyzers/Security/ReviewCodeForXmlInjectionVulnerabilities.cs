// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForXmlInjectionVulnerabilities : SourceTriggeredTaintedDataAnalyzerBase
    {
        internal static readonly DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA3009",
            nameof(MicrosoftNetCoreAnalyzersResources.ReviewCodeForXmlInjectionVulnerabilitiesTitle),
            nameof(MicrosoftNetCoreAnalyzersResources.ReviewCodeForXmlInjectionVulnerabilitiesMessage),
            isEnabledByDefault: false,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca3009-review-code-for-xml-injection-vulnerabilities");

        protected override SinkKind SinkKind { get { return SinkKind.Xml; } }

        protected override DiagnosticDescriptor TaintedDataEnteringSinkDescriptor { get { return Rule; } }
    }
}

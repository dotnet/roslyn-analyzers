// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : SourceTriggeredTaintedDataAnalyzerBase
    {
        internal static readonly DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA3001",
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            isEnabledByDefault: false,
            helpLinkUri: null); // TODO paulming: Help link.  https://github.com/dotnet/roslyn-analyzers/issues/1892

        protected override SinkKind SinkKind { get { return SinkKind.Sql; } }

        protected override DiagnosticDescriptor TaintedDataEnteringSinkDescriptor { get { return Rule; } }
    }
}

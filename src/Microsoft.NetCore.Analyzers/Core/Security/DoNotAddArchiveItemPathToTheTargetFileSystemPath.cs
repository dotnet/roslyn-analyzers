// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotAddArchiveItemPathToTheTargetFileSystemPath : SourceTriggeredTaintedDataAnalyzerBase
    {
        internal static DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5389",
            typeof(MicrosoftNetCoreAnalyzersResources),
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotAddArchiveItemPathToTheTargetFileSystemPath),
            nameof(MicrosoftNetCoreAnalyzersResources.DoNotAddArchiveItemPathToTheTargetFileSystemPathMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.DoNotAddArchiveItemPathToTheTargetFileSystemPathDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        protected override SinkKind SinkKind { get { return SinkKind.ZipSlip; } }

        protected override DiagnosticDescriptor TaintedDataEnteringSinkDescriptor { get { return Rule; } }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.BinaryFormatterAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotUseInsecureDeserializerBinaryFormatter : DoNotUseInsecureDeserializerBase
    {
        /// <summary>
        /// When we encounter Deserialize invocations, we want to make sure the Binder property is non-null.
        /// </summary>
        private const string DeserializeMethodName = "Deserialize";

        // TODO paulming: Help link URLs.
        internal static readonly DiagnosticDescriptor RealBannedMethodDescriptor =
            new DiagnosticDescriptor(
                "CA2300",
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterBannedMethodTitle)),
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterBannedMethodMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);
        internal static readonly DiagnosticDescriptor BinderDefinitelyNotSetDescriptor =
            new DiagnosticDescriptor(
                "CA2301",
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeWithoutBinderSetTitle)),
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeWithoutBinderSetMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);
        internal static readonly DiagnosticDescriptor BinderMaybeNotSetDescriptor =
            new DiagnosticDescriptor(
                "CA2302",
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeMaybeWithoutBinderSetTitle)),
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.BinaryFormatterDeserializeMaybeWithoutBinderSetMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create<DiagnosticDescriptor>(
                RealBannedMethodDescriptor,
                BinderDefinitelyNotSetDescriptor,
                BinderMaybeNotSetDescriptor);

        protected override string DeserializerTypeMetadataName => 
            WellKnownTypes.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override ImmutableHashSet<string> BannedMethodNames => ImmutableHashSet.Create<string>(
            "UnsafeDeserialize",
            "UnsafeDeserializeMethodResponse");

        protected override DiagnosticDescriptor BannedMethodDescriptor => RealBannedMethodDescriptor;

        protected override void AdditionalHandleInvocationOperation(
            ISymbol owningSymbol,
            INamedTypeSymbol deserializerTypeSymbol, 
            OperationAnalysisContext operationAnalysisContext,
            IInvocationOperation invocationOperation)
        {
            if (invocationOperation.TargetMethod.ContainingType != deserializerTypeSymbol
                || invocationOperation.TargetMethod.MetadataName != DeserializeMethodName)
            {
                return;
            }

            ControlFlowGraph cfg = invocationOperation.GetEnclosingControlFlowGraph();
            var dfaResult = BinaryFormatterAnalysis.GetOrComputeHazardousParameterUsages(cfg, operationAnalysisContext.Compilation, owningSymbol);
            if (dfaResult.TryGetValue(invocationOperation, out BinaryFormatterAbstractValue abstractValue))
            {
                if (abstractValue == BinaryFormatterAbstractValue.Flagged)
                {
                    operationAnalysisContext.ReportDiagnostic(
                        Diagnostic.Create(
                            BinderDefinitelyNotSetDescriptor,
                            invocationOperation.Syntax.GetLocation()));
                }
                else if (abstractValue == BinaryFormatterAbstractValue.MaybeFlagged)
                {
                    operationAnalysisContext.ReportDiagnostic(
                        Diagnostic.Create(
                            BinderMaybeNotSetDescriptor,
                            invocationOperation.Syntax.GetLocation()));
                }
            }
        }
    }
}

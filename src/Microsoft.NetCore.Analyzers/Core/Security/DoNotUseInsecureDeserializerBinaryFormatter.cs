using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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

        // TODO paulming: Help link URL.
        internal static readonly DiagnosticDescriptor RealBannedMethodDescriptor =
            new DiagnosticDescriptor(
                "CA2300",
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.DoNotUseInsecureDeserializationWithBinaryFormatterTitle)),
                GetResourceString(
                    nameof(MicrosoftNetCoreSecurityResources.DoNotUseInsecureDeserializationWithBinaryFormatterMessage)),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create<DiagnosticDescriptor>(
                RealBannedMethodDescriptor);

        protected override string DeserializerTypeMetadataName => 
            WellKnownTypes.SystemRuntimeSerializationFormattersBinaryBinaryFormatter;

        protected override ImmutableHashSet<string> BannedMethodNames => ImmutableHashSet.Create<string>(
            "UnsafeDeserialize",
            "UnsafeDeserializeMethodResponse");

        protected override DiagnosticDescriptor BannedMethodDescriptor => RealBannedMethodDescriptor;

        protected override void AdditionalHandleInvocationOperation(
            INamedTypeSymbol deserializerTypeSymbol, 
            OperationAnalysisContext operationAnalysisContext,
            IInvocationOperation invocationOperation)
        {
            if (invocationOperation.TargetMethod.ContainingType != deserializerTypeSymbol
                || invocationOperation.TargetMethod.MetadataName != DeserializeMethodName)
            {
                return;
            }

            // TODO paulming: DFA
        }
    }
}

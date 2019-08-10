// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SslProtocolsAnalyzer : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor DeprecatedRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5395",
            nameof(MicrosoftNetCoreAnalyzersResources.DeprecatedSslProtocolsTitle),
            nameof(MicrosoftNetCoreAnalyzersResources.DeprecatedSslProtocolsMessage),
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.DeprecatedSslProtocolsDescription),
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca5395",
            customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor HardcodedRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5396",
            nameof(MicrosoftNetCoreAnalyzersResources.HardcodedSslProtocolsTitle),
            nameof(MicrosoftNetCoreAnalyzersResources.HardcodedSslProtocolsMessage),
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.HardcodedSslProtocolsDescription),
            isEnabledByDefault: false,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca5396",
            customTags: WellKnownDiagnosticTags.Telemetry);

        private readonly ImmutableHashSet<string> HardcodedSslProtocolsMetadataNames = ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "Tls12",
            "Tls13");

        private const int UnsafeBits = 12 | 48 | 192 | 768;    // SslProtocols Ssl2 Ssl3 Tls10 Tls11

        private const int HardcodedBits = 3072 | 12288;    // SslProtocols Tls12 Tls13

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DeprecatedRule, HardcodedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);
                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(
                            WellKnownTypeNames.SystemSecurityAuthenticationSslProtocols,
                            out INamedTypeSymbol sslProtocolsSymbol))
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IFieldReferenceOperation fieldReferenceOperation = (IFieldReferenceOperation)operationAnalysisContext.Operation;
                            if (IsReferencingSslProtocols(
                                    fieldReferenceOperation,
                                    out bool isDeprecatedProtocol,
                                    out bool isHardCodedOkayProtocol))
                            {
                                if (isDeprecatedProtocol)
                                {
                                    operationAnalysisContext.ReportDiagnostic(fieldReferenceOperation.CreateDiagnostic(DeprecatedRule, fieldReferenceOperation.Field.Name));
                                }
                                else if (isHardCodedOkayProtocol)
                                {
                                    operationAnalysisContext.ReportDiagnostic(fieldReferenceOperation.CreateDiagnostic(HardcodedRule, fieldReferenceOperation.Field.Name));
                                }
                            }
                        },
                        OperationKind.FieldReference);

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IAssignmentOperation assignmentOperation = (IAssignmentOperation)operationAnalysisContext.Operation;

                            // Make sure this is an assignment operation for a SslProtocols value.
                            if (!sslProtocolsSymbol.Equals(assignmentOperation.Target.Type))
                            {
                                return;
                            }

                            // Find the topmost operation with a bad bit set, unless we find an operation that would've been
                            // flagged by the FieldReference callback above.
                            IOperation foundDeprecatedOperation = null;
                            bool foundDeprecatedReference = false;
                            IOperation foundHardCodedOperation = null;
                            bool foundHardCodedReference = false;
                            foreach (IOperation childOperation in assignmentOperation.Value.DescendantsAndSelf())
                            {
                                if (childOperation is IFieldReferenceOperation fieldReferenceOperation
                                    && IsReferencingSslProtocols(fieldReferenceOperation, out var isDeprecatedProtocol, out var isHardCodedOkayProtocol))
                                {
                                    if (isDeprecatedProtocol)
                                    {
                                        foundDeprecatedReference = true;
                                    }
                                    else if (isHardCodedOkayProtocol)
                                    {
                                        foundHardCodedReference = true;
                                    }

                                    if (foundDeprecatedReference && foundHardCodedReference)
                                    {
                                        return;
                                    }
                                }

                                if (childOperation.ConstantValue.HasValue
                                    && childOperation.ConstantValue.Value is int integerValue)
                                {
                                    if (foundDeprecatedOperation == null    // Only want the first.
                                        && (integerValue & UnsafeBits) != 0)
                                    {
                                        foundDeprecatedOperation = childOperation;
                                    }

                                    if (foundHardCodedOperation == null    // Only want the first.
                                        && (integerValue & HardcodedBits) != 0)
                                    {
                                        foundHardCodedOperation = childOperation;
                                    }
                                }
                            }

                            if (foundDeprecatedOperation != null && !foundDeprecatedReference)
                            {
                                operationAnalysisContext.ReportDiagnostic(foundDeprecatedOperation.CreateDiagnostic(DeprecatedRule, foundDeprecatedOperation.ConstantValue));
                            }

                            if (foundHardCodedOperation != null && !foundHardCodedReference)
                            {
                                operationAnalysisContext.ReportDiagnostic(foundHardCodedOperation.CreateDiagnostic(HardcodedRule, foundHardCodedOperation.ConstantValue));
                            }
                        },
                        OperationKind.SimpleAssignment,
                        OperationKind.CompoundAssignment);

                    return;

                    // Local function(s).
                    bool IsReferencingSslProtocols(
                        IFieldReferenceOperation fieldReferenceOperation,
                        out bool isDeprecated,
                        out bool isHardcoded)
                    {
                        if (sslProtocolsSymbol.Equals(fieldReferenceOperation.Field.ContainingType))
                        {
                            if (HardcodedSslProtocolsMetadataNames.Contains(fieldReferenceOperation.Field.Name))
                            {
                                isHardcoded = true;
                                isDeprecated = false;
                            }
                            else
                            {
                                isDeprecated = true;
                                isHardcoded = false;
                            }

                            return true;
                        }
                        else
                        {
                            isHardcoded = false;
                            isDeprecated = false;
                            return false;
                        }
                    }
                });
        }
    }
}

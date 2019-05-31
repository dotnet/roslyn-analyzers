// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseDeprecatedSecurityProtocols : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5364";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDeprecatedSecurityProtocols),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDeprecatedSecurityProtocolsMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDeprecatedSecurityProtocolsDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        private readonly ImmutableHashSet<string> SafeProtocolMetadataNames = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "SystemDefault",
                "Tls12");

        private const int UnsafeBits = 48 | 192 | 768;    // SecurityProtocols Ssl3 Tls10 Tls11

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var securityProtocolTypeTypeSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemNetSecurityProtocolType);

                    if (securityProtocolTypeTypeSymbol == null)
                    {
                        return;
                    }

                    bool IsReferencingDeprecatedProtocol(IFieldReferenceOperation fieldReferenceOperation)
                    {
                        return securityProtocolTypeTypeSymbol.Equals(fieldReferenceOperation.Field.ContainingType)
                            && !SafeProtocolMetadataNames.Contains(fieldReferenceOperation.Field.Name);
                    }

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            var fieldReferenceOperation = (IFieldReferenceOperation)operationAnalysisContext.Operation;
                            if (IsReferencingDeprecatedProtocol(fieldReferenceOperation))
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    fieldReferenceOperation.CreateDiagnostic(
                                        Rule,
                                        fieldReferenceOperation.Field.Name));
                            }
                        }, OperationKind.FieldReference);

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            var assignmentOperation = (IAssignmentOperation)operationAnalysisContext.Operation;
                            if (!securityProtocolTypeTypeSymbol.Equals(assignmentOperation.Target.Type))
                            {
                                return;
                            }

                            // Find the topmost operation with a bad bit set, unless we find an operation that would've been
                            // flagged by the FieldReference callback above.
                            IOperation foundOperation = null;
                            foreach (IOperation childOperation in assignmentOperation.Value.DescendantsAndSelf())
                            {
                                if (childOperation is IFieldReferenceOperation fieldReferenceOperation
                                    && IsReferencingDeprecatedProtocol(fieldReferenceOperation))
                                {
                                    // This assignment is handled by the FieldReference callback above.
                                    return;
                                }

                                if (foundOperation == null    // Only want the first.
                                    && childOperation.ConstantValue.HasValue
                                    && childOperation.ConstantValue.Value is int integerValue
                                    && (integerValue & UnsafeBits) != 0)
                                {
                                    foundOperation = childOperation;
                                }
                            }

                            if (foundOperation != null)
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    foundOperation.CreateDiagnostic(
                                        Rule,
                                        foundOperation.ConstantValue));
                            }
                        },
                        OperationKind.SimpleAssignment,
                        OperationKind.CompoundAssignment);
                });
        }
    }
}

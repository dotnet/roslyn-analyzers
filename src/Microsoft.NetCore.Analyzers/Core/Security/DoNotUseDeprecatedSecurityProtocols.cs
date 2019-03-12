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

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            var fieldReferenceOperation = (IFieldReferenceOperation)operationAnalysisContext.Operation;
                            var fieldSymbol = fieldReferenceOperation.Field;

                            if (!fieldSymbol.ContainingType.Equals(securityProtocolTypeTypeSymbol))
                            {
                                return;
                            }

                            var constantValue = fieldSymbol.Name;

                            if (!SafeProtocolMetadataNames.Contains(constantValue))
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    fieldReferenceOperation.CreateDiagnostic(
                                        Rule,
                                        constantValue));
                            }
                        }, OperationKind.FieldReference);
                });
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class ApprovedCipherModeAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5358";
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.ApprovedCipherMode),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.ApprovedCipherModeMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.ApprovedCipherModeDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule =
            CreateDiagnosticDescriptor(DiagnosticId, Title, Message, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal string CipherModeTypeMetadataName =>
            WellKnownTypes.SystemSecurityCryptographyCipherMode;

        internal ImmutableHashSet<string> UnsafeCipherModes = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "ECB",
                "OFB",
                "CFB");

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(string ruleId, LocalizableString title, LocalizableString message, LocalizableString description, string uri = null)
        {
            return new DiagnosticDescriptor(
                ruleId,
                title,
                message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: false,
                description: description,
                helpLinkUri: uri,
                customTags: WellKnownDiagnosticTags.Telemetry);
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    INamedTypeSymbol cipherModeTypeSymbol =
                        compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(CipherModeTypeMetadataName);
                    if (cipherModeTypeSymbol == null)
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationAction(
                        (OperationAnalysisContext operationAnalysisContext) =>
                        {
                            IFieldReferenceOperation fieldReferenceOperation =
                                (IFieldReferenceOperation)operationAnalysisContext.Operation;
                            IFieldSymbol fieldSymbol = fieldReferenceOperation.Field;
                            if (fieldSymbol.ContainingType == cipherModeTypeSymbol
                                && UnsafeCipherModes.Contains(fieldSymbol.MetadataName))
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Rule,
                                        fieldReferenceOperation.Syntax.GetLocation(),
                                        fieldSymbol.MetadataName));
                            }
                        },
                        OperationKind.FieldReference);
                });
        }
    }
}

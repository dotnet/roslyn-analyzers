// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDisableRequestValidation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5363";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableRequestValidation),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableRequestValidationMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotDisableRequestValidationDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

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
                    var validateInputAttributeTypeSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemWebMvcValidateInputAttribute);

                    if (validateInputAttributeTypeSymbol == null)
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            var methodSymbol = (IMethodSymbol)symbolAnalysisContext.Symbol;
                            var attr = methodSymbol.GetAttributes().FirstOrDefault(s => s.AttributeClass.Equals(validateInputAttributeTypeSymbol));

                            if (attr == null)
                            {
                                return;
                            }

                            var constructorArguments = attr.ConstructorArguments;

                            if (constructorArguments != null &&
                                constructorArguments.Length == 1 &&
                                !constructorArguments[0].IsNull &&
                                constructorArguments[0].Value.Equals(false))
                            {
                                symbolAnalysisContext.ReportDiagnostic(
                                    methodSymbol.CreateDiagnostic(
                                        Rule,
                                        methodSymbol.Name));
                            }
                        }, SymbolKind.Method);
                });
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseObsoleteKDFAlgorithm : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5373";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseObsoleteKDFAlgorithm),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseObsoleteKDFAlgorithmMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseObsoleteKDFAlgorithmDescription),
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

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilation = compilationStartAnalysisContext.Compilation;
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                wellKnownTypeProvider.TryGetTypeByMetadataName(
                            WellKnownTypeNames.SystemSecurityCryptographyPasswordDeriveBytes,
                            out INamedTypeSymbol passwordDeriveBytesTypeSymbol);
                wellKnownTypeProvider.TryGetTypeByMetadataName(
                            WellKnownTypeNames.SystemSecurityCryptographyRfc2898DeriveBytes,
                            out INamedTypeSymbol rfc2898DeriveBytesTypeSymbol);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var operation = operationAnalysisContext.Operation;
                    var methodSymbol = operation.Kind == OperationKind.Invocation ? (operation as IInvocationOperation).TargetMethod : (operation as IObjectCreationOperation).Constructor;
                    var typeSymbol = methodSymbol.ContainingType;
                    var methodName = methodSymbol.MethodKind == MethodKind.Constructor ? typeSymbol.Name : methodSymbol.Name;

                    if ((passwordDeriveBytesTypeSymbol != null &&
                         typeSymbol.Equals(passwordDeriveBytesTypeSymbol)) ||
                        (rfc2898DeriveBytesTypeSymbol != null &&
                         typeSymbol.Equals(rfc2898DeriveBytesTypeSymbol) &&
                         methodSymbol.Name == "CryptDeriveKey"))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            operation.CreateDiagnostic(
                                Rule,
                                typeSymbol.Name,
                                methodName));
                    }
                }, OperationKind.Invocation, OperationKind.ObjectCreation);
            });
        }
    }
}

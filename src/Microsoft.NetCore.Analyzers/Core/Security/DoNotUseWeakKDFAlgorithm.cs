﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseWeakKDFAlgorithm : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5379";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakKDFAlgorithm),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakKDFAlgorithmMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseWeakKDFAlgorithmDescription),
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

        private static readonly ImmutableHashSet<string> s_WeakHashAlgorithmNames = ImmutableHashSet.Create("MD5", "SHA1");

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                if (!wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyRfc2898DeriveBytes,
                    out INamedTypeSymbol rfc2898DeriveBytesTypeSymbol))
                {
                    return;
                }

                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyHashAlgorithmName,
                    out INamedTypeSymbol hashAlgorithmNameTypeSymbol);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var objectCreationOperation = (IObjectCreationOperation)operationAnalysisContext.Operation;
                    var typeSymbol = objectCreationOperation.Constructor.ContainingType;
                    var baseTypeSymbol = typeSymbol;

                    while (!rfc2898DeriveBytesTypeSymbol.Equals(baseTypeSymbol))
                    {
                        baseTypeSymbol = baseTypeSymbol.BaseType;

                        if (baseTypeSymbol == null)
                        {
                            return;
                        }
                    }

                    var hashAlgorithmNameArgumentOperation = objectCreationOperation.Arguments.FirstOrDefault(s => s.Parameter.Name == "hashAlgorithm");

                    if (hashAlgorithmNameArgumentOperation != null)
                    {
                        if (hashAlgorithmNameArgumentOperation.Value is IPropertyReferenceOperation propertyReferenceOperation &&
                            !s_WeakHashAlgorithmNames.Contains(propertyReferenceOperation.Property.Name) &&
                            hashAlgorithmNameTypeSymbol != null &&
                            hashAlgorithmNameTypeSymbol.Equals(propertyReferenceOperation.Property.ContainingType))
                        {
                            return;
                        }
                    }

                    operationAnalysisContext.ReportDiagnostic(
                                objectCreationOperation.CreateDiagnostic(
                                    Rule,
                                    typeSymbol.Name));
                }, OperationKind.ObjectCreation);
            });
        }
    }
}

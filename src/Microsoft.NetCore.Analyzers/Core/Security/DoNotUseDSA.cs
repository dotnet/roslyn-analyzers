﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
    public sealed class DoNotUseDSA : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5384";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDSA),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDSAMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotUseDSADescription),
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
                customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        private static readonly ImmutableHashSet<string> s_DSAAlgorithmNames =
            ImmutableHashSet.Create(
                StringComparer.OrdinalIgnoreCase,
                "DSA",
                "System.Security.Cryptography.DSA");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyDSA,
                    out var dsaTypeSymbol);
                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyAsymmetricAlgorithm,
                    out var asymmetricAlgorithmTypeSymbol);
                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyCryptoConfig,
                    out var cryptoConfigTypeSymbol);

                if (dsaTypeSymbol == null &&
                    asymmetricAlgorithmTypeSymbol == null &&
                    cryptoConfigTypeSymbol == null)
                {
                    return;
                }

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var objectCreationOperation = (IObjectCreationOperation)operationAnalysisContext.Operation;
                    var typeSymbol = objectCreationOperation.Constructor.ContainingType;

                    if (typeSymbol == null)
                    {
                        return;
                    }

                    var baseTypesAndThis = typeSymbol.GetBaseTypesAndThis();

                    if (dsaTypeSymbol != null && baseTypesAndThis.Contains(dsaTypeSymbol))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            objectCreationOperation.CreateDiagnostic(
                                Rule,
                                typeSymbol.Name));
                    }
                }, OperationKind.ObjectCreation);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var returnOperation = (IReturnOperation)operationAnalysisContext.Operation;
                    var typeSymbol = returnOperation.ReturnedValue?.Type;

                    if (typeSymbol == null)
                    {
                        return;
                    }

                    var baseTypesAndThis = typeSymbol.GetBaseTypesAndThis();

                    if (dsaTypeSymbol != null && baseTypesAndThis.Contains(dsaTypeSymbol))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            returnOperation.CreateDiagnostic(
                                Rule,
                                typeSymbol.Name));
                    }
                }, OperationKind.Return);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                    var methodSymbol = invocationOperation.TargetMethod;
                    var typeSymbol = methodSymbol.ContainingType;

                    if (typeSymbol == null)
                    {
                        return;
                    }

                    var methodName = methodSymbol.Name;
                    var arguments = invocationOperation.Arguments;

                    if (methodName == "Create" &&
                        typeSymbol.Equals(asymmetricAlgorithmTypeSymbol) &&
                        arguments.Length > 0 &&
                        arguments[0].Parameter.Type.SpecialType == SpecialType.System_String &&
                        arguments[0].Value.ConstantValue.HasValue)
                    {
                        var argValue = arguments[0].Value.ConstantValue.Value;

                        if (s_DSAAlgorithmNames.Contains(argValue.ToString()))
                        {
                            operationAnalysisContext.ReportDiagnostic(
                                invocationOperation.CreateDiagnostic(
                                    Rule,
                                    argValue));
                        }
                    }
                    else if (methodName == "CreateFromName" &&
                            typeSymbol.Equals(cryptoConfigTypeSymbol) &&
                            arguments.Length > 0 &&
                            arguments[0].Parameter.Type.SpecialType == SpecialType.System_String &&
                            arguments[0].Value.ConstantValue.HasValue)
                    {
                        var argValue = arguments[0].Value.ConstantValue.Value;

                        if (s_DSAAlgorithmNames.Contains(argValue.ToString()))
                        {
                            operationAnalysisContext.ReportDiagnostic(
                                invocationOperation.CreateDiagnostic(
                                    Rule,
                                    argValue));
                        }
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}

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
    public sealed class UseRSAWithSufficientKeySize : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5385";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseRSAWithSufficientKeySize),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseRSAWithSufficientKeySizeMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseRSAWithSufficientKeySizeDescription),
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

        private static readonly ImmutableHashSet<string> s_RSAAlgorithmNames =
            ImmutableHashSet.Create(
                StringComparer.OrdinalIgnoreCase,
                "RSA",
                "System.Security.Cryptography.RSA",
                "System.Security.Cryptography.AsymmetricAlgorithm");

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
                    WellKnownTypeNames.SystemSecurityCryptographyRSA,
                    out var rsaTypeSymbol);
                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyAsymmetricAlgorithm,
                    out var asymmetricAlgorithmTypeSymbol);
                wellKnownTypeProvider.TryGetTypeByMetadataName(
                    WellKnownTypeNames.SystemSecurityCryptographyCryptoConfig,
                    out var cryptoConfigTypeSymbol);

                if (rsaTypeSymbol == null &&
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

                    if (rsaTypeSymbol != null && baseTypesAndThis.Contains(rsaTypeSymbol))
                    {
                        var arguments = objectCreationOperation.Arguments;

                        if (arguments.Length == 1 &&
                            arguments[0].Parameter.Type.SpecialType == SpecialType.System_Int32 &&
                            arguments[0].Value.ConstantValue.HasValue &&
                            Convert.ToInt32(arguments[0].Value.ConstantValue.Value) < 2048)
                        {
                            operationAnalysisContext.ReportDiagnostic(
                                objectCreationOperation.CreateDiagnostic(
                                    Rule,
                                    typeSymbol.Name));
                        }
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

                    if (rsaTypeSymbol != null && baseTypesAndThis.Contains(rsaTypeSymbol))
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
                        arguments.Length == 0)
                    {
                        // Use AsymmetricAlgorithm.Create() to create RSA and the default key size is 1024.
                        operationAnalysisContext.ReportDiagnostic(
                                invocationOperation.CreateDiagnostic(
                                    Rule,
                                    "RSA"));
                    }
                    else if (methodName == "Create" &&
                            typeSymbol.Equals(asymmetricAlgorithmTypeSymbol) &&
                            arguments.Length > 0 &&
                            arguments[0].Parameter.Type.SpecialType == SpecialType.System_String &&
                            arguments[0].Value.ConstantValue.HasValue)
                    {
                        var argValue = arguments[0].Value.ConstantValue.Value;

                        if (s_RSAAlgorithmNames.Contains(argValue.ToString()))
                        {
                            // Use AsymmetricAlgorithm.Create(string) to create RSA and the default key size is 1024.
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
                        // Use CryptoConfig.CreateFromName(string, ...).
                        var argValue = arguments[0].Value.ConstantValue.Value;

                        if (s_RSAAlgorithmNames.Contains(argValue.ToString()))
                        {
                            // Create RSA.
                            if (arguments.Length == 1 /* The default key size is 1024 */ ||
                                arguments[1].Value is IArrayCreationOperation arrayCreationOperation /* Use CryptoConfig.CreateFromName(string, object[]) to create RSA */&&
                                arrayCreationOperation.DimensionSizes[0].ConstantValue.Value.Equals(1) &&
                                arrayCreationOperation.Initializer.ElementValues.Any(
                                    s => s is IConversionOperation conversionOperation &&
                                        conversionOperation.Operand.ConstantValue.HasValue &&
                                        Convert.ToInt32(conversionOperation.Operand.ConstantValue.Value) < 2048) /* Specify the key size is smaller than 2048 explicitly */ )
                            {
                                operationAnalysisContext.ReportDiagnostic(
                                invocationOperation.CreateDiagnostic(
                                    Rule,
                                    argValue));
                            }
                        }
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}

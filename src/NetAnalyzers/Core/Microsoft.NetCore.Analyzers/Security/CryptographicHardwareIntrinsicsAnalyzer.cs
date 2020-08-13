// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class CryptographicHardwareIntrinsicsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCryptographicHardwareIntrinsicsTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCryptographicHardwareIntrinsicsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseCryptographicHardwareIntrinsicsDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor s_rule = DiagnosticDescriptorHelper.Create(
            "",
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Security,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    INamedTypeSymbol symbol = compilationStartContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Aes);

                    compilationStartContext.RegisterOperationAction(
                        context => AnalyzeObjectCreation(context, tooGenericExceptionSymbols, reservedExceptionSymbols),
                        OperationKind.ObjectCreation);
                });
        }

        private static void AnalyzeObjectCreation(OperationAnalysisContext context, INamedTypeSymbol symbol)
        {
            var objectCreation = (IObjectCreationOperation)context.Operation;
            var typeSymbol = objectCreation.Constructor.ContainingType;
            // TODO: context.ReportDiagnostic if the object creation is a type derived from "symbol".
        }
}

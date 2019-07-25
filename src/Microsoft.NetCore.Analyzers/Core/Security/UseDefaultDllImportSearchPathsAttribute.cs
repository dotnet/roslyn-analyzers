// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseDefaultDllImportSearchPathsAttribute : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5392";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttribute),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttributeMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttributeDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        private const int UnsafeBits = 2 | 256;
        private const int LegacyBehavior = 0;

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
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);

                if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDllImportAttribute, out INamedTypeSymbol dllImportAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDefaultDllImportSearchPathsAttribute, out INamedTypeSymbol defaultDllImportSearchPathsAttributeTypeSymbol))
                {
                    return;
                }

                var cancellationToken = compilationStartAnalysisContext.CancellationToken;
                var unsafeDllImportSearchPathValues = compilationStartAnalysisContext.Options.GetUnsignedIntegralOptionValue(
                    optionName: EditorConfigOptionNames.UnsafeDllImportSearchPathValues,
                    rule: Rule,
                    defaultValue: UnsafeBits,
                    cancellationToken: cancellationToken);
                var defaultDllImportSearchPathsAttributeOnAssembly = compilation.Assembly.GetAttributes().FirstOrDefault(o => o.AttributeClass.Equals(defaultDllImportSearchPathsAttributeTypeSymbol));
                var dllImportSearchPathOnAssembly = defaultDllImportSearchPathsAttributeOnAssembly == null ? -1 : (int)defaultDllImportSearchPathsAttributeOnAssembly.ConstructorArguments.FirstOrDefault().Value;

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    var symbol = symbolAnalysisContext.Symbol;

                    if (!symbol.IsExtern || !symbol.IsStatic)
                    {
                        return;
                    }

                    var dllImportAttribute = symbol.GetAttributes().FirstOrDefault(s => s.AttributeClass.Equals(dllImportAttributeTypeSymbol));
                    var defaultDllImportSearchPathsAttribute = symbol.GetAttributes().FirstOrDefault(s => s.AttributeClass.Equals(defaultDllImportSearchPathsAttributeTypeSymbol));
                    var dllImportSearchPath = defaultDllImportSearchPathsAttribute == null ? -1 : (int)defaultDllImportSearchPathsAttribute.ConstructorArguments.FirstOrDefault().Value;

                    if (dllImportAttribute != null)
                    {
                        var constructorArguments = dllImportAttribute.ConstructorArguments;

                        if (constructorArguments.Length == 0)
                        {
                            return;
                        }

                        if (Path.IsPathRooted(constructorArguments[0].Value.ToString()))
                        {
                            return;
                        }

                        if (dllImportSearchPath == -1)
                        {
                            if (dllImportSearchPathOnAssembly != -1 &&
                                dllImportSearchPathOnAssembly != LegacyBehavior &&
                                (dllImportSearchPathOnAssembly & unsafeDllImportSearchPathValues) == 0)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (dllImportSearchPath != LegacyBehavior &&
                                (dllImportSearchPath & unsafeDllImportSearchPathValues) == 0)
                            {
                                return;
                            }
                        }

                        symbolAnalysisContext.ReportDiagnostic(
                            symbol.CreateDiagnostic(
                                Rule,
                                symbol.Name));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Publish
{
    /// <summary>
    /// CA3000: Do not use Assembly.Location in single-file publish
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidAssemblyLocationInSingleFile : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.AvoidAssemblyLocationInSingleFileTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.AvoidAssemblyLocationInSingleFileMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.AvoidAssemblyLocationInSingleFileDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Publish,
                                                                             RuleLevel.BuildWarning,
                                                                             s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                var compilation = context.Compilation;
                var isSingleFilePublish = context.Options.GetMSBuildPropertyValue(
                    MSBuildPropertyOptionNames.PublishSingleFile, compilation, context.CancellationToken);
                if (!string.Equals(isSingleFilePublish?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                var includesAllContent = context.Options.GetMSBuildPropertyValue(
                    MSBuildPropertyOptionNames.IncludeAllContentForSelfExtract, compilation, context.CancellationToken);
                if (string.Equals(includesAllContent?.Trim(), "true", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                HashSet<IPropertySymbol> properties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
                HashSet<IMethodSymbol> methods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

                var assemblyType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReflectionAssembly);
                if (assemblyType is not null)
                {
                    // properties
                    addIfNotNull(properties, tryGetSingleSymbol<IPropertySymbol>(assemblyType.GetMembers("Location")));
                    addIfNotNull(properties, tryGetSingleSymbol<IPropertySymbol>(assemblyType.GetMembers("CodeBase")));
                    addIfNotNull(properties, tryGetSingleSymbol<IPropertySymbol>(assemblyType.GetMembers("EscapedCodeBase")));

                    // methods
                    methods.UnionWith(assemblyType.GetMembers("GetFile").OfType<IMethodSymbol>());
                    methods.UnionWith(assemblyType.GetMembers("GetFiles").OfType<IMethodSymbol>());
                }

                var assemblyNameType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReflectionAssemblyName);
                if (assemblyNameType is not null)
                {
                    addIfNotNull(properties, tryGetSingleSymbol<IPropertySymbol>(assemblyNameType.GetMembers("CodeBase")));
                    addIfNotNull(properties, tryGetSingleSymbol<IPropertySymbol>(assemblyNameType.GetMembers("EscapedCodeBase")));
                }

                context.RegisterOperationAction(operationContext =>
                {
                    var access = (IPropertyReferenceOperation)operationContext.Operation;
                    var property = access.Property;
                    if (!properties.Contains(property))
                    {
                        return;
                    }

                    operationContext.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        access.Syntax.GetLocation(),
                        property));
                }, OperationKind.PropertyReference);

                context.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;
                    var targetMethod = invocation.TargetMethod;
                    if (!methods.Contains(targetMethod))
                    {
                        return;
                    }

                    operationContext.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        invocation.Syntax.GetLocation(),
                        targetMethod));
                }, OperationKind.Invocation);

                return;

                static TSymbol? tryGetSingleSymbol<TSymbol>(ImmutableArray<ISymbol> members) where TSymbol : class, ISymbol
                {
                    TSymbol? candidate = null;
                    foreach (var m in members)
                    {
                        if (m is TSymbol tsym)
                        {
                            if (candidate is null)
                            {
                                candidate = tsym;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    return candidate;
                }

                static void addIfNotNull<TSymbol>(HashSet<TSymbol> properties, TSymbol? p) where TSymbol : class, ISymbol
                {
                    if (p is not null)
                    {
                        properties.Add(p);
                    }
                }
            });
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallDangerousMethodsInDeserialization : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5360";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotCallDangerousMethodsInDeserialization),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotCallDangerousMethodsInDeserializationMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotCallDangerousMethodsInDeserializationDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        private readonly ImmutableHashSet<string> SystemIOFileMethodMetadataNames = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "WriteAllBytes",
                "WriteAllLines",
                "WriteAllText",
                "Copy",
                "Move",
                "AppendAllLines",
                "AppendAllText",
                "AppendText",
                "Delete");

        private readonly ImmutableHashSet<string> SystemReflectionAssemblyMethodMetadataNames = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "GetLoadedModules",
                "Load",
                "LoadFile",
                "LoadFrom",
                "LoadModule",
                "LoadWithPartialName",
                "ReflectionOnlyLoad",
                "ReflectionOnlyLoadFrom",
                "UnsafeLoadFrom");

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

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var compilation = compilationStartAnalysisContext.Compilation;
                    var serializableAttributeTypeSymbol = WellKnownTypes.SerializableAttribute(compilation);

                    if (serializableAttributeTypeSymbol == null)
                    {
                        return;
                    }

                    var dangerousMethodSymbolsBuilder = ImmutableArray.CreateBuilder<IMethodSymbol>();
                    var systemIOFileTypeSymbol = WellKnownTypes.SystemIOFile(compilation);

                    if (systemIOFileTypeSymbol != null)
                    {
                        foreach (var targetMethodName in SystemIOFileMethodMetadataNames)
                        {
                            dangerousMethodSymbolsBuilder.AddRange(systemIOFileTypeSymbol.GetMembers(targetMethodName).Where(s => s is IMethodSymbol).Cast<IMethodSymbol>());
                        }
                    }

                    var systemReflectionAssemblyTypeSymbol = WellKnownTypes.SystemReflectionAssembly(compilation);

                    if (systemReflectionAssemblyTypeSymbol != null)
                    {
                        foreach (var targetMethodName in SystemReflectionAssemblyMethodMetadataNames)
                        {
                            dangerousMethodSymbolsBuilder.AddRange(systemReflectionAssemblyTypeSymbol.GetMembers(targetMethodName).Where(s => s is IMethodSymbol).Cast<IMethodSymbol>());
                        }
                    }

                    var dangerousMethodSymbols = dangerousMethodSymbolsBuilder.ToImmutable();

                    if (dangerousMethodSymbols.Length == 0)
                    {
                        return;
                    }

                    var attributeTypeSymbolsBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
                    var onDeserializingAttributeTypeSymbol = WellKnownTypes.OnDeserializingAttribute(compilation);

                    if (onDeserializingAttributeTypeSymbol != null)
                    {
                        attributeTypeSymbolsBuilder.Add(onDeserializingAttributeTypeSymbol);
                    }

                    var onDeserializedAttributeTypeSymbol = WellKnownTypes.OnDeserializedAttribute(compilation);

                    if (onDeserializedAttributeTypeSymbol != null)
                    {
                        attributeTypeSymbolsBuilder.Add(onDeserializedAttributeTypeSymbol);
                    }

                    var attributeTypeSymbols = attributeTypeSymbolsBuilder.ToImmutable();
                    var streamingContextTypeSymbol = WellKnownTypes.StreamingContext(compilation);
                    var IDeserializationCallback = WellKnownTypes.IDeserializationCallback(compilation);
                    // A dictionary from method symbol to set of methods invoked by it directly.
                    // The bool value in the sub ConcurrentDictionary is not used, use ConcurrentDictionary rather than HashSet just for the concurrency security.
                    var callGraph = new ConcurrentDictionary<IMethodSymbol, ConcurrentDictionary<IMethodSymbol, bool>>();

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            var owningSymbol = operationBlockStartAnalysisContext.OwningSymbol;

                            if (owningSymbol.Kind != SymbolKind.Method)
                            {
                                return;
                            }

                            var methodSymbol = (IMethodSymbol)owningSymbol;
                            var classSymbol = methodSymbol.ContainingType;

                            if (!classSymbol.GetAttributes().Any(s => s.AttributeClass.Equals(serializableAttributeTypeSymbol)))
                            {
                                return;
                            }

                            var calledMethods = new ConcurrentDictionary<IMethodSymbol, bool>();
                            callGraph.TryAdd(methodSymbol, calledMethods);

                            operationBlockStartAnalysisContext.RegisterOperationAction(operationContext =>
                            {
                                callGraph[methodSymbol].TryAdd((operationContext.Operation as IInvocationOperation).TargetMethod, true);
                            }, OperationKind.Invocation);
                        });

                    compilationStartAnalysisContext.RegisterCompilationEndAction(
                        (CompilationAnalysisContext compilationAnalysisContext) =>
                        {
                            var visited = new HashSet<IMethodSymbol>();
                            var results = new Dictionary<IMethodSymbol, HashSet<IMethodSymbol>>();

                            foreach (var kvp in callGraph)
                            {
                                var methodSymbol = kvp.Key;
                                FindCalledDangerousMethod(methodSymbol, visited, results);

                                // Determine if the method is called automatically when an object is deserialized.
                                // This includes methods with OnDeserializing attribute, method with OnDeserialized attribute, deserialization callbacks as well as cleanup/dispose calls.
                                var parameters = methodSymbol.GetParameters();
                                var flagHasDeserializeAttributes = attributeTypeSymbols.Length != 0
                                    && attributeTypeSymbols.Any(s => methodSymbol.HasAttribute(s))
                                    && parameters.Length == 1
                                    && parameters[0].Type.Equals(streamingContextTypeSymbol);
                                var flagImplementOnDeserializationMethod = methodSymbol.IsOnDeserializationImplementation(IDeserializationCallback);
                                var flagImplementDisposeMethod = methodSymbol.IsDisposeImplementation(compilation);
                                var flagIsFinalizer = methodSymbol.IsFinalizer();

                                if (!flagHasDeserializeAttributes && !flagImplementOnDeserializationMethod && !flagImplementDisposeMethod && !flagIsFinalizer)
                                {
                                    continue;
                                }

                                foreach (var result in results[methodSymbol])
                                {
                                    compilationAnalysisContext.ReportDiagnostic(
                                        methodSymbol.CreateDiagnostic(
                                            Rule,
                                            methodSymbol.ContainingType.MetadataName,
                                            methodSymbol.MetadataName,
                                            result.MetadataName));
                                }
                            }
                        });

                    /// <summary>
                    /// Analyze the method to find all the dangerous method it calls.
                    /// </summary>
                    /// <param name="methodSymbol">The symbol of the method to be analyzed</param>
                    /// <param name="visited">All the method has been analyzed</param>
                    /// <param name="results">The result is organized by &lt;method to be analyzed, dangerous method it calls&gt;</param>
                    void FindCalledDangerousMethod(IMethodSymbol methodSymbol, HashSet<IMethodSymbol> visited, Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> results)
                    {
                        if (visited.Add(methodSymbol))
                        {
                            results.Add(methodSymbol, new HashSet<IMethodSymbol>());

                            foreach (var child in callGraph[methodSymbol].Keys)
                            {
                                if (dangerousMethodSymbols.Contains(child))
                                {
                                    results[methodSymbol].Add(child);
                                }

                                if (child.IsInSource())
                                {
                                    if (results.TryGetValue(child, out var result))
                                    {
                                        results[methodSymbol].UnionWith(result);
                                    }
                                    else
                                    {
                                        FindCalledDangerousMethod(child, visited, results);
                                        results[methodSymbol].UnionWith(results[child]);
                                    }
                                }
                            }
                        }
                    }
                });
        }
    }
}

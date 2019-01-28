// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
                    var attributeTypeSymbols = ImmutableArray.Create(
                        WellKnownTypes.OnDeserializingAttribute(compilation),
                        WellKnownTypes.OnDeserializedAttribute(compilation));
                    var serializableAttributeTypeSymbol = WellKnownTypes.SerializableAttribute(compilation);
                    var streamingContextTypeSymbol = WellKnownTypes.StreamingContext(compilation);

                    if (serializableAttributeTypeSymbol == null || streamingContextTypeSymbol == null)
                    {
                        return;
                    }

                    var builder = ImmutableArray.CreateBuilder<IMethodSymbol>();
                    var systemIOFileTypeSymbol = WellKnownTypes.SystemIOFile(compilation);

                    if (systemIOFileTypeSymbol != null)
                    {
                        foreach (var targetMethodName in SystemIOFileMethodMetadataNames)
                        {
                            builder.AddRange(systemIOFileTypeSymbol.GetMembers(targetMethodName).Cast<IMethodSymbol>());
                        }
                    }

                    var systemReflectionAssemblyTypeSymbol = WellKnownTypes.SystemReflectionAssembly(compilation);

                    if (systemReflectionAssemblyTypeSymbol != null)
                    {
                        foreach (var targetMethodName in SystemReflectionAssemblyMethodMetadataNames)
                        {
                            builder.AddRange(systemReflectionAssemblyTypeSymbol.GetMembers(targetMethodName).Cast<IMethodSymbol>());
                        }
                    }

                    var dangerousMethodSymbols = builder.ToImmutable();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            var classSymbol = symbolAnalysisContext.Symbol as INamedTypeSymbol;

                            if (!classSymbol.GetAttributes().Any(s => s.AttributeClass.Equals(serializableAttributeTypeSymbol)))
                            {
                                return;
                            }
                            
                            foreach (var member in classSymbol.GetMembers())
                            {
                                if (member.Kind == SymbolKind.Method)
                                {
                                    var methodSymbol = member as IMethodSymbol;

                                    if (methodSymbol.GetAttributes().Any(s => attributeTypeSymbols.Contains(s.AttributeClass))
                                        || methodSymbol.MetadataName == "OnDeserialization")
                                    {
                                        var parameters = methodSymbol.GetParameters();

                                        if (parameters == null
                                            || parameters.Length != 1
                                            || !parameters[0].Type.Equals(streamingContextTypeSymbol))
                                        {
                                            return;
                                        }

                                        var visitedMethodSymbols = new List<IMethodSymbol>();
                                        var invocationOperations = FindDangerousMethodInvocatioinOperation(methodSymbol, compilation, dangerousMethodSymbols, ref visitedMethodSymbols);

                                        foreach (var item in invocationOperations)
                                        {
                                            symbolAnalysisContext.ReportDiagnostic(
                                                item.CreateDiagnostic(
                                                    Rule,
                                                    classSymbol.MetadataName,
                                                    methodSymbol.MetadataName,
                                                    item.TargetMethod.MetadataName));
                                        }
                                    }
                                }
                            }
                        },
                        SymbolKind.NamedType);
                });

            /// <summary>
            /// Traverse every invocation in the method to find all the dangerous function it invokes.
            /// </summary>
            /// <param name="methodSymbol">The Symbol of the method which is going to be analyzed</param>
            /// <param name="compilation">Compilation context</param>
            /// <param name="dangerousMethodSymbols">Symbols reprensent dangerous methods</param>
            /// <param name="visitedMethodSymbols">Methods that have been analyzed</param>
            IEnumerable<IInvocationOperation> FindDangerousMethodInvocatioinOperation(
                IMethodSymbol methodSymbol,
                Compilation compilation,
                ImmutableArray<IMethodSymbol> dangerousMethodSymbols,
                ref List<IMethodSymbol> visitedMethodSymbols)
            {
                var result = new List<IInvocationOperation>();

                // Every method in the source just needs to be analyzed once.
                if (visitedMethodSymbols.Find((IMethodSymbol m) => m.Equals(methodSymbol)) == null)
                {
                    var blockOperation = methodSymbol.GetTopmostOperationBlock(compilation);

                    if (blockOperation != null)
                    {
                        foreach (var operation in blockOperation.Descendants())
                        {
                            if (operation.Kind.Equals(OperationKind.Invocation))
                            {
                                var invocationOperation = operation as IInvocationOperation;
                                var invokedMethodSymbol = invocationOperation.TargetMethod;

                                // If it calls a dengerous method, add the invocation operation to the result.
                                if (dangerousMethodSymbols.Contains(invokedMethodSymbol))
                                {
                                    result.Add(invocationOperation);
                                }

                                // If it calls a method which is reachable, analyze that method recursively.
                                if (invokedMethodSymbol.IsInSource())
                                {
                                    result.AddRange(FindDangerousMethodInvocatioinOperation(invokedMethodSymbol, compilation, dangerousMethodSymbols, ref visitedMethodSymbols));
                                }
                            }
                        }
                    }
                }

                visitedMethodSymbols.Add(methodSymbol);
                return result;
            }
        }
    }
}


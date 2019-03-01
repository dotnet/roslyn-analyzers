// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotReferSelfInSerializableClass : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5362";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotReferSelfInSerializableClass),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotReferSelfInSerializableClassMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotReferSelfInSerializableClassDescription),
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

                    var nonSerializedAttribute = WellKnownTypes.NonSerializedAttribute(compilation);

                    if (nonSerializedAttribute == null)
                    {
                        return;
                    }
                    
                    var forwardGraph = new ConcurrentDictionary<ISymbol, HashSet<ISymbol>>();
                    var invertedGraph = new ConcurrentDictionary<ISymbol, HashSet<ISymbol>>();

                    // It keeps the out Degree of every vertex in the invertedGraph, which is corresponding to the in Degree of the vertex in forwardGraph.
                    var inDegree = new ConcurrentDictionary<ISymbol, int>();

                    // It Keeps the out degree of every vertex in the forwardGraph, which is corresponding to the in Degree of the vertex in invertedGraph.
                    var outDegree = new ConcurrentDictionary<ISymbol, int>();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            var classSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;

                            if (!classSymbol.HasAttribute(serializableAttributeTypeSymbol))
                            {
                                return;
                            }

                            var fields = classSymbol.GetMembers().OfType<IFieldSymbol>().Where(s => !s.HasAttribute(nonSerializedAttribute) &&
                                                                                                    !s.IsStatic &&
                                                                                                    s.Type.IsInSource());

                            foreach (var field in fields)
                            {
                                var fieldSymbol = (ISymbol)field;
                                var fieldType = (ISymbol)field.Type;

                                AddLine(classSymbol, fieldSymbol, outDegree, forwardGraph);
                                AddLine(fieldSymbol, fieldType, outDegree, forwardGraph);
                                AddLine(fieldType, fieldSymbol, inDegree, invertedGraph);
                                AddLine(fieldSymbol, classSymbol, inDegree, invertedGraph);
                            }
                        }, SymbolKind.NamedType);

                    compilationStartAnalysisContext.RegisterCompilationEndAction(
                        (CompilationAnalysisContext compilationAnalysisContext) =>
                        {
                            ModifyDegree(inDegree, forwardGraph);
                            ModifyDegree(outDegree, invertedGraph);

                            // If the degree of a vertex is greater than 0 both in the forward graph and inverted graph after topological sorting,
                            // the vertex must belong to a loop.
                            var leftVertices = inDegree.Where(s => s.Value > 0).Select(s => s.Key).ToImmutableHashSet();
                            var invertedLeftVertices = outDegree.Where(s => s.Value > 0).Select(s => s.Key).ToImmutableHashSet();
                            var verticesInLoop = leftVertices.Intersect(invertedLeftVertices);

                            foreach (var vertex in verticesInLoop)
                            {
                                if (vertex is IFieldSymbol)
                                {
                                    compilationAnalysisContext.ReportDiagnostic(
                                        vertex.CreateDiagnostic(Rule, vertex.Name));
                                }
                            }
                        });

                    /// <summary>
                    /// Add a line to the graph.
                    /// </summary>
                    /// <param name="from">The start point of the line</param>
                    /// <param name="to">The end point of the line</param>
                    /// <param name="degree">The out degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    void AddLine(ISymbol from, ISymbol to, ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, HashSet<ISymbol>> graph)
                    {
                        var value = AddPoint(from, degree, graph);

                        if (value.Add(to))
                        {
                            degree[from]++;
                        }

                        AddPoint(to, degree, graph);
                    }

                    /// <summary>
                    /// Add a point to the graph.
                    /// </summary>
                    /// <param name="point">The point to be added</param>
                    /// <param name="degree">The out degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    HashSet<ISymbol> AddPoint(ISymbol point, ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, HashSet<ISymbol>> graph)
                    {
                        if (!graph.TryGetValue(point, out var value))
                        {
                            value = new HashSet<ISymbol>();
                            graph.TryAdd(point, value);
                            degree.TryAdd(point, 0);
                        }

                        return value;
                    }

                    /// <summary>
                    /// According to topological sorting, modify the degree of every vertex in the graph.
                    /// </summary>
                    /// <param name="degree">The in degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    void ModifyDegree(ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, HashSet<ISymbol>> graph)
                    {
                        var stack = new Stack<ISymbol>(degree.Where(s => s.Value == 0).Select(s => s.Key));

                        while (stack.Count != 0)
                        {
                            var start = stack.Pop();
                            degree[start]--;

                            foreach (var vertex in graph[start])
                            {
                                degree[vertex]--;

                                if (degree[vertex] == 0)
                                {
                                    stack.Push(vertex);
                                }
                            }
                        }
                    }
                });
        }
    }
}

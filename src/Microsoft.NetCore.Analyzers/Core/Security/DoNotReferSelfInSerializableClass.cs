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
                isEnabledByDefault: false,
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

                    var forwardGraph = new ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>>();
                    var invertedGraph = new ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>>();

                    // It keeps the out Degree of every vertex in the invertedGraph, which is corresponding to the in Degree of the vertex in forwardGraph.
                    var inDegree = new ConcurrentDictionary<ISymbol, int>();

                    // It Keeps the out degree of every vertex in the forwardGraph, which is corresponding to the in Degree of the vertex in invertedGraph.
                    var outDegree = new ConcurrentDictionary<ISymbol, int>();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            DrawGraph(symbolAnalysisContext.Symbol);
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
                                if (vertex is IFieldSymbol fieldInLoop)
                                {
                                    var associatedSymbol = fieldInLoop.AssociatedSymbol;
                                    compilationAnalysisContext.ReportDiagnostic(
                                        fieldInLoop.CreateDiagnostic(
                                            Rule,
                                            associatedSymbol == null ? vertex.Name : associatedSymbol.Name));
                                }
                            }
                        });

                    /// <summary>
                    /// Traverse from point to its descendants, save the information into a directed graph.
                    /// </summary>
                    /// <param name="point">The initial point</param>
                    void DrawGraph(ISymbol point)
                    {
                        // If the point has been visited, return.
                        if (forwardGraph.Keys.Contains(point))
                        {
                            return;
                        }

                        // Add the point to the graph, mark it as visited.
                        AddPointToBothGraphs(point);

                        if (point is INamedTypeSymbol namedTypePoint)
                        {
                            // 1. When the point is of generic type, its children point can be the type arguments of the generic type.
                            if (namedTypePoint.IsGenericType)
                            {
                                foreach (var arg in namedTypePoint.TypeArguments)
                                {
                                    AddLineToBothGraphs(point, arg);
                                    DrawGraph(arg);
                                }
                            }

                            // 2. When the point is a INamedTypeSymbol, its children point can be the fields of the type that constructs the point.
                            var constructedFrom = namedTypePoint.ConstructedFrom;

                            if (!constructedFrom.HasAttribute(serializableAttributeTypeSymbol))
                            {
                                return;
                            }

                            if (constructedFrom.IsInSource())
                            {
                                var fields = constructedFrom.GetMembers().OfType<IFieldSymbol>().Where(s => !s.HasAttribute(nonSerializedAttribute) &&
                                                                                                            !s.IsStatic);

                                foreach (var field in fields)
                                {
                                    AddLineToBothGraphs(point, field);
                                    DrawGraph(field);
                                }
                            }
                        }

                        if (point is IArrayTypeSymbol arrayTypePoint)
                        {
                            // 3. When the point is a IArrayTypeSymbol, its children point can be element type of the array.
                            var elementType = arrayTypePoint.ElementType;
                            AddLineToBothGraphs(arrayTypePoint, elementType);
                            DrawGraph(elementType);

                            // 4. When the point is a IArrayTypeSymbol, its children point can be the array type itself.
                            var baseType = arrayTypePoint.BaseType;
                            AddLineToBothGraphs(arrayTypePoint, baseType);
                            DrawGraph(baseType);
                        }

                        // 5. When the point is a IFieldSymbol, its children point can be the type of the field.
                        if (point is IFieldSymbol fieldSymbolPoint)
                        {
                            var fieldType = fieldSymbolPoint.Type;
                            AddLineToBothGraphs(point, fieldType);
                            DrawGraph(fieldType);
                        }
                    }

                    /// <summary>
                    /// Add a line to the graph.
                    /// </summary>
                    /// <param name="from">The start point of the line</param>
                    /// <param name="to">The end point of the line</param>
                    /// <param name="degree">The out degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    void AddLine(ISymbol from, ISymbol to, ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>> graph)
                    {
                        graph.AddOrUpdate(from, new ConcurrentDictionary<ISymbol, bool> { [to] = true }, (k, v) => { v[to] = true; return v; });
                        degree.AddOrUpdate(from, 1, (k, v) => v + 1);
                    }

                    /// <summary>
                    /// Add a point to the graph.
                    /// </summary>
                    /// <param name="point">The point to be added</param>
                    /// <param name="degree">The out degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    void AddPoint(ISymbol point, ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>> graph)
                    {
                        graph.TryAdd(point, new ConcurrentDictionary<ISymbol, bool>());
                        degree.TryAdd(point, 0);
                    }

                    /// <summary>
                    /// Add a line to the forward graph and inverted graph unconditionally.
                    /// </summary>
                    /// <param name="from">The start point of the line</param>
                    /// <param name="to">The end point of the line</param>
                    void AddLineToBothGraphs(ISymbol from, ISymbol to)
                    {
                        AddLine(from, to, outDegree, forwardGraph);
                        AddLine(to, from, inDegree, invertedGraph);
                    }

                    /// <summary>
                    /// Add a point to the forward graph and inverted graph unconditionally.
                    /// </summary>
                    /// <param name="point">The point to be added</param>
                    void AddPointToBothGraphs(ISymbol point)
                    {
                        AddPoint(point, outDegree, forwardGraph);
                        AddPoint(point, inDegree, invertedGraph);
                    }

                    /// <summary>
                    /// According to topological sorting, modify the degree of every vertex in the graph.
                    /// </summary>
                    /// <param name="degree">The in degree of all vertices in the graph</param>
                    /// <param name="graph">The graph</param>
                    void ModifyDegree(ConcurrentDictionary<ISymbol, int> degree, ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>> graph)
                    {
                        var stack = new Stack<ISymbol>(degree.Where(s => s.Value == 0).Select(s => s.Key));

                        while (stack.Count != 0)
                        {
                            var start = stack.Pop();
                            degree.AddOrUpdate(start, -1, (k, v) => v - 1);

                            foreach (var vertex in graph[start].Keys)
                            {
                                degree.AddOrUpdate(vertex, -1, (k, v) => v - 1);

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

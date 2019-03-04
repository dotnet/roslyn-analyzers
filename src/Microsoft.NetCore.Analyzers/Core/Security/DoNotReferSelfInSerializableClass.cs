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
                            var classSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;

                            if (!classSymbol.HasAttribute(serializableAttributeTypeSymbol))
                            {
                                return;
                            }

                            var fields = classSymbol.GetMembers().OfType<IFieldSymbol>().Where(s => !s.HasAttribute(nonSerializedAttribute) &&
                                                                                                    !s.IsStatic);

                            foreach (var field in fields)
                            {
                                // Identify if there's a line 'class symbol - field symbol - type associated with field' belongs to the graph.
                                var startpointIsRelated = false;
                                var fieldType = field.Type;

                                // If field is a generic type, add 'field symbol - type parameters' to the graphs.
                                if (fieldType is INamedTypeSymbol namedField)
                                {
                                    fieldType = namedField.ConstructedFrom;

                                    foreach (var arg in namedField.TypeArguments)
                                    {
                                        UpdateAllGraphsConditionally(field, arg, ref startpointIsRelated);
                                    }
                                }

                                // If field is a array, add 'field symbol - element type' to the graphs.
                                if (fieldType is IArrayTypeSymbol arrayField)
                                {
                                    fieldType = arrayField.BaseType;
                                    UpdateAllGraphsConditionally(field, arrayField.ElementType, ref startpointIsRelated);
                                }

                                // Add 'field symbol - field type' to the graphs.
                                UpdateAllGraphsConditionally(field, fieldType, ref startpointIsRelated);

                                if (startpointIsRelated)
                                {
                                    // Add 'class symbol - field symbol' to the graphs.
                                    UpdateAllGraphsUnconditionally(classSymbol, field);
                                }
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
                        graph.TryAdd(to, new ConcurrentDictionary<ISymbol, bool>());
                        degree.TryAdd(to, 0);
                    }

                    /// <summary>
                    /// Add a line to the forward graph and inverted graph unconditionally.
                    /// </summary>
                    /// <param name="from">The start point of the line</param>
                    /// <param name="to">The end point of the line</param>
                    void UpdateAllGraphsUnconditionally(ISymbol from, ISymbol to)
                    {
                        AddLine(from, to, outDegree, forwardGraph);
                        AddLine(to, from, inDegree, invertedGraph);
                    }

                    /// <summary>
                    /// Add a line to the forward graph and inverted graph only if the endpoint is related - that is, the symbol represented by endpoint is defined in source.
                    /// </summary>
                    /// <param name="from">The start point of the line</param>
                    /// <param name="to">The end point of the line</param>
                    /// <param name="startpointIsRelated">If any endpoint associated with startpoint is related with the graph, the startpoint is related with the graph</param>
                    void UpdateAllGraphsConditionally(ISymbol from, ISymbol to, ref bool startpointIsRelated)
                    {
                        var endpointIsRelated = to.IsInSource();
                        startpointIsRelated = startpointIsRelated || endpointIsRelated;

                        if (endpointIsRelated)
                        {
                            UpdateAllGraphsUnconditionally(from, to);
                        }
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

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseAutoValidateAntiforgeryToken : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5391";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseAutoValidateAntiforgeryToken),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseAutoValidateAntiforgeryTokenMessage),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.UseAutoValidateAntiforgeryTokenDescription),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly Regex s_AntiForgeryAttributeRegex = new Regex("^[a-zA-Z]*Validate[a-zA-Z]*Anti[Ff]orgery[a-zA-Z]*Attribute$", RegexOptions.Compiled);
        private static readonly Regex s_AntiForgeryRegex = new Regex("^[a-zA-Z]*Validate[a-zA-Z]*Anti[Ff]orgery[a-zA-Z]*$", RegexOptions.Compiled);

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

        public delegate bool RequirementsOfValidateMethod(IMethodSymbol methodSymbol);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilation = compilationStartAnalysisContext.Compilation;
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcFiltersFilterCollection, out var filterCollectionTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcController, out var controllerTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcControllerBase, out var controllerBaseTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcNonActionAttribute, out var nonActionAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcHttpPostAttribute, out var httpPostAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcHttpPutAttribute, out var httpPutAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcHttpDeleteAttribute, out var httpDeleteAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcHttpPatchAttribute, out var httpPatchAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcFiltersIFilterMetadata, out var iFilterMetadataTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreAntiforgeryIAntiforgery, out var iAntiforgeryTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcFiltersIAsyncAuthorizationFilter, out var iAsyncAuthorizationFilterTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask, out var taskTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.MicrosoftAspNetCoreMvcFiltersAuthorizationFilterContext, out var authorizationFilterContextTypeSymbol))
                {
                    return;
                }

                // A dictionary from method symbol to set of methods calling it directly.
                // The bool value in the sub ConcurrentDictionary is not used, use ConcurrentDictionary rather than HashSet just for the concurrency security.
                var inverseGraph = new ConcurrentDictionary<ISymbol, ConcurrentDictionary<ISymbol, bool>>();

                // Ignore cases where a global anti forgery filter is in use.
                var hasGlobalAntiForgeryFilter = false;

                // Verify that validate anti forgery token attributes are used somewhere within this project,
                // to avoid reporting false positives on projects that use an alternative approach to mitigate CSRF issues.
                var usingValidateAntiForgeryAttribute = false;
                var onAuthorizationAsyncMethodSymbols = new HashSet<IMethodSymbol>();
                var actionMethodSymbols = new HashSet<(IMethodSymbol, string)>();

                // Constructing inverse callGraph.
                // When it comes to delegate function assignment Del handler = DelegateMethod;, inverse call Graph will add:
                // (1) key: method gets called in DelegateMethod, value: handler.
                // When it comes to calling delegate function handler(), inverse callGraph will add:
                // (1) key: delegate function handler, value: callerMethod.
                // (2) key: Invoke(), value: callerMethod.
                compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                    (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                    {
                        if (hasGlobalAntiForgeryFilter)
                        {
                            return;
                        }

                        var owningSymbol = operationBlockStartAnalysisContext.OwningSymbol;
                        inverseGraph.TryAdd(owningSymbol, new ConcurrentDictionary<ISymbol, bool>());

                        operationBlockStartAnalysisContext.RegisterOperationAction(operationContext =>
                        {
                            ISymbol calledSymbol = null;
                            ConcurrentDictionary<ISymbol, bool> callers = null;

                            switch (operationContext.Operation)
                            {
                                case IInvocationOperation invocationOperation:
                                    calledSymbol = invocationOperation.TargetMethod.OriginalDefinition;

                                    break;

                                case IFieldReferenceOperation fieldReferenceOperation:
                                    var fieldSymbol = (IFieldSymbol)fieldReferenceOperation.Field;

                                    if (fieldSymbol.Type.TypeKind == TypeKind.Delegate)
                                    {
                                        calledSymbol = fieldSymbol;

                                        break;
                                    }

                                    return;
                            }

                            if (calledSymbol == null)
                            {
                                return;
                            }

                            callers = inverseGraph.GetOrAdd(calledSymbol, new ConcurrentDictionary<ISymbol, bool>());
                            callers.TryAdd(owningSymbol, true);
                        }, OperationKind.Invocation, OperationKind.FieldReference);
                    });

                // Holds if the project has a global anti forgery filter.
                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    if (hasGlobalAntiForgeryFilter)
                    {
                        return;
                    }

                    var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                    var methodSymbol = invocationOperation.TargetMethod;

                    if (methodSymbol.Name == "Add" &&
                        methodSymbol.ContainingType.GetBaseTypesAndThis().Contains(filterCollectionTypeSymbol))
                    {
                        var potentialAntiForgeryFilters = invocationOperation
                            .Arguments
                            .Where(s => s.Parameter.Name == "filterType")
                            .Select(s => s.Value)
                            .OfType<ITypeOfOperation>()
                            .Select(s => s.TypeOperand)
                            .Union(methodSymbol.TypeArguments);

                        foreach (var potentialAntiForgeryFilter in potentialAntiForgeryFilters)
                        {
                            if (potentialAntiForgeryFilter.AllInterfaces.Contains(iFilterMetadataTypeSymbol) &&
                                s_AntiForgeryRegex.IsMatch(potentialAntiForgeryFilter.Name))
                            {
                                hasGlobalAntiForgeryFilter = true;

                                return;
                            }
                            else if (potentialAntiForgeryFilter.AllInterfaces.Contains(iAsyncAuthorizationFilterTypeSymbol))
                            {
                                onAuthorizationAsyncMethodSymbols.Add(
                                    potentialAntiForgeryFilter
                                    .GetMembers()
                                    .OfType<IMethodSymbol>()
                                    .FirstOrDefault(
                                        s => s.Name == "OnAuthorizationAsync" &&
                                            s.ReturnType.Equals(taskTypeSymbol) &&
                                            s.Parameters.Length == 1 &&
                                            s.Parameters[0].Type.Equals(authorizationFilterContextTypeSymbol)));
                            }
                        }
                    }
                }, OperationKind.Invocation);

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    if (hasGlobalAntiForgeryFilter)
                    {
                        return;
                    }

                    var derivedControllerTypeSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;
                    var baseTypes = derivedControllerTypeSymbol.GetBaseTypes();

                    // An subtype of `Microsoft.AspNetCore.Mvc.Controller` or `Microsoft.AspNetCore.Mvc.ControllerBase`)
                    if (baseTypes.Contains(controllerTypeSymbol) ||
                        baseTypes.Contains(controllerBaseTypeSymbol))
                    {
                        // The controller class is protected by a validate anti forgery token attribute
                        if (derivedControllerTypeSymbol.GetAttributes().Any(s => s_AntiForgeryAttributeRegex.IsMatch(s.AttributeClass.Name)))
                        {
                            usingValidateAntiForgeryAttribute = true;

                            return;
                        }

                        foreach (var actionMethodSymbol in derivedControllerTypeSymbol.GetMembers().OfType<IMethodSymbol>())
                        {
                            // The method is protected by a validate anti forgery token attribute
                            if (actionMethodSymbol.GetAttributes().Any(s => s_AntiForgeryAttributeRegex.IsMatch(s.AttributeClass.Name)))
                            {
                                usingValidateAntiForgeryAttribute = true;

                                return;
                            }

                            if (actionMethodSymbol.IsPublic() &&
                                !actionMethodSymbol.IsStatic &&
                                !actionMethodSymbol.HasAttribute(nonActionAttributeTypeSymbol))
                            {
                                if (actionMethodSymbol.HasAttribute(httpPostAttributeTypeSymbol))
                                {
                                    actionMethodSymbols.Add((actionMethodSymbol, "HttpPost"));
                                }
                                else if (actionMethodSymbol.HasAttribute(httpPutAttributeTypeSymbol))
                                {
                                    actionMethodSymbols.Add((actionMethodSymbol, "HttpPut"));
                                }
                                else if (actionMethodSymbol.HasAttribute(httpDeleteAttributeTypeSymbol))
                                {
                                    actionMethodSymbols.Add((actionMethodSymbol, "HttpDelete"));
                                }
                                else if (actionMethodSymbol.HasAttribute(httpPatchAttributeTypeSymbol))
                                {
                                    actionMethodSymbols.Add((actionMethodSymbol, "HttpPatch"));
                                }
                            }
                        }
                    }
                }, SymbolKind.NamedType);

                compilationStartAnalysisContext.RegisterCompilationEndAction(
                (CompilationAnalysisContext compilationAnalysisContext) =>
                {
                    if (usingValidateAntiForgeryAttribute && !hasGlobalAntiForgeryFilter && actionMethodSymbols.Any())
                    {
                        var visited = new HashSet<ISymbol>();
                        var results = new Dictionary<ISymbol, HashSet<ISymbol>>();

                        if (onAuthorizationAsyncMethodSymbols.Any())
                        {
                            foreach (var calleeMethod in inverseGraph.Keys)
                            {
                                if (calleeMethod.Name == "ValidateRequestAsync" &&
                                    (calleeMethod.ContainingType.AllInterfaces.Contains(iAntiforgeryTypeSymbol) ||
                                    calleeMethod.ContainingType.Equals(iAntiforgeryTypeSymbol)))
                                {
                                    FindAllTheSpecifiedCalleeMethod(calleeMethod, visited, results);

                                    if (results.Values.Any(s => s.Any()))
                                    {
                                        return;
                                    }
                                }
                            }
                        }

                        foreach (var (methodSymbol, attributeName) in actionMethodSymbols)
                        {
                            compilationAnalysisContext.ReportDiagnostic(
                                methodSymbol.CreateDiagnostic(
                                    Rule,
                                    methodSymbol.Name,
                                    attributeName));
                        }
                    }
                });

                // <summary>
                // Analyze the method to find all the specified method it calls, in this case, all the method symbols in onAuthorizationAsyncMethodSymbols.
                // </summary>
                // <param name="methodSymbol">The symbol of the method to be analyzed</param>
                // <param name="visited">All the method has been analyzed</param>
                // <param name="results">The result is organized by &lt;method to be analyzed, dangerous method it calls&gt;</param>
                void FindAllTheSpecifiedCalleeMethod(ISymbol methodSymbol, HashSet<ISymbol> visited, Dictionary<ISymbol, HashSet<ISymbol>> results)
                {
                    if (visited.Add(methodSymbol))
                    {
                        results.Add(methodSymbol, new HashSet<ISymbol>());

                        if (!inverseGraph.TryGetValue(methodSymbol, out var calledMethods))
                        {
                            Debug.Fail(methodSymbol.Name + " was not found in inverseGraph");

                            return;
                        }

                        foreach (var child in calledMethods.Keys)
                        {
                            if (onAuthorizationAsyncMethodSymbols.Contains(child))
                            {
                                results[methodSymbol].Add(child);
                            }

                            FindAllTheSpecifiedCalleeMethod(child, visited, results);

                            if (results.TryGetValue(child, out var result))
                            {
                                results[methodSymbol].UnionWith(result);
                            }
                            else
                            {
                                Debug.Fail(child.Name + " was not found in results");
                            }
                        }
                    }
                }
            });
        }
    }
}

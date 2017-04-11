﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Diagnostics.Analyzers
{
    public partial class DeclarePublicAPIAnalyzer : DiagnosticAnalyzer
    {
        private sealed class ApiLine
        {
            public string Text { get; private set; }
            public TextSpan Span { get; private set; }
            public SourceText SourceText { get; private set; }
            public string Path { get; private set; }
            public bool IsShippedApi { get; private set; }

            internal ApiLine(string text, TextSpan span, SourceText sourceText, string path, bool isShippedApi)
            {
                Text = text;
                Span = span;
                SourceText = sourceText;
                Path = path;
                IsShippedApi = isShippedApi;
            }
        }

        private struct RemovedApiLine
        {
            public string Text { get; private set; }
            public ApiLine ApiLine { get; private set; }

            internal RemovedApiLine(string text, ApiLine apiLine)
            {
                Text = text;
                ApiLine = apiLine;
            }
        }

        private struct ApiData
        {
            public ImmutableArray<ApiLine> ApiList { get; private set; }
            public ImmutableArray<RemovedApiLine> RemovedApiList { get; private set; }

            internal ApiData(ImmutableArray<ApiLine> apiList, ImmutableArray<RemovedApiLine> removedApiList)
            {
                ApiList = apiList;
                RemovedApiList = removedApiList;
            }
        }

        private sealed class Impl
        {
            private static readonly HashSet<MethodKind> s_ignorableMethodKinds = new HashSet<MethodKind>
            {
                MethodKind.EventAdd,
                MethodKind.EventRemove
            };

            private readonly Compilation _compilation;
            private readonly ApiData _unshippedData;
            private readonly Dictionary<ITypeSymbol, bool> _typeCanBeExtendedCache = new Dictionary<ITypeSymbol, bool>();
            private readonly HashSet<string> _visitedApiList = new HashSet<string>(StringComparer.Ordinal);
            private readonly Dictionary<string, ApiLine> _publicApiMap = new Dictionary<string, ApiLine>(StringComparer.Ordinal);

            internal Impl(Compilation compilation, ApiData shippedData, ApiData unshippedData)
            {
                _compilation = compilation;
                _unshippedData = unshippedData;

                foreach (ApiLine cur in shippedData.ApiList)
                {
                    _publicApiMap.Add(cur.Text, cur);
                }

                foreach (ApiLine cur in unshippedData.ApiList)
                {
                    _publicApiMap.Add(cur.Text, cur);
                }
            }

            internal void OnSymbolAction(SymbolAnalysisContext symbolContext)
            {
                OnSymbolActionCore(symbolContext.Symbol, symbolContext.ReportDiagnostic);
            }

            /// <param name="symbol">The symbol to analyze. Will also analyze implicit constructors too.</param>
            /// <param name="reportDiagnostic">Action called to actually report a diagnostic.</param>
            /// <param name="explicitLocation">A location to report the diagnostics for a symbol at. If null, then
            /// the location of the symbol will be used.</param>
            private void OnSymbolActionCore(ISymbol symbol, Action<Diagnostic> reportDiagnostic, Location explicitLocation = null)
            {
                if (!IsPublicAPI(symbol))
                {
                    return;
                }

                Debug.Assert(!symbol.IsImplicitlyDeclared);
                OnSymbolActionCore(symbol, reportDiagnostic, isImplicitlyDeclaredConstructor: false, explicitLocation: explicitLocation);

                // Handle implicitly declared public constructors.
                if (symbol.Kind == SymbolKind.NamedType)
                {
                    var namedType = (INamedTypeSymbol)symbol;
                    if (namedType.InstanceConstructors.Length == 1 &&
                        (namedType.TypeKind == TypeKind.Class || namedType.TypeKind == TypeKind.Struct))
                    {
                        var instanceConstructor = namedType.InstanceConstructors[0];
                        if (instanceConstructor.IsImplicitlyDeclared)
                        {
                            OnSymbolActionCore(instanceConstructor, reportDiagnostic, isImplicitlyDeclaredConstructor: true, explicitLocation: explicitLocation);
                        }
                    }
                }
            }

            /// <param name="symbol">The symbol to analyze.</param>
            /// <param name="reportDiagnostic">Action called to actually report a diagnostic.</param>
            /// <param name="isImplicitlyDeclaredConstructor">If the symbol is an implicitly declared constructor.</param>
            /// <param name="explicitLocation">A location to report the diagnostics for a symbol at. If null, then
            /// the location of the symbol will be used.</param>
            private void OnSymbolActionCore(ISymbol symbol, Action<Diagnostic> reportDiagnostic, bool isImplicitlyDeclaredConstructor, Location explicitLocation = null)
            {
                Debug.Assert(IsPublicAPI(symbol));

                string publicApiName = GetPublicApiName(symbol);
                _visitedApiList.Add(publicApiName);

                List<Location> locationsToReport = new List<Location>();

                if (explicitLocation != null)
                {
                    locationsToReport.Add(explicitLocation);
                }
                else
                {
                    var locations = isImplicitlyDeclaredConstructor ? symbol.ContainingType.Locations : symbol.Locations;
                    locationsToReport.AddRange(locations.Where(l => l.IsInSource));
                }

                void reportDiagnosticAtLocations(DiagnosticDescriptor descriptor, ImmutableDictionary<string, string> propertyBag, params object[] args)
                {
                    foreach (Location location in locationsToReport)
                    {
                        reportDiagnostic(Diagnostic.Create(descriptor, location, propertyBag, args));
                    }
                }

                var hasPublicApiEntry = _publicApiMap.TryGetValue(publicApiName, out ApiLine apiLine);
                if (!hasPublicApiEntry)
                {
                    // Unshipped public API with no entry in public API file - report diagnostic.
                    string errorMessageName = GetErrorMessageName(symbol, isImplicitlyDeclaredConstructor);
                    // Compute public API names for any stale siblings to remove from unshipped text (e.g. during signature change of unshipped public API).
                    var siblingPublicApiNamesToRemove = GetSiblingNamesToRemoveFromUnshippedText(symbol);
                    ImmutableDictionary<string, string> propertyBag = ImmutableDictionary<string, string>.Empty
                        .Add(PublicApiNamePropertyBagKey, publicApiName)
                        .Add(MinimalNamePropertyBagKey, errorMessageName)
                        .Add(PublicApiNamesOfSiblingsToRemovePropertyBagKey, siblingPublicApiNamesToRemove);

                    reportDiagnosticAtLocations(DeclareNewApiRule, propertyBag, errorMessageName);
                }

                if (symbol.Kind == SymbolKind.Method)
                {
                    var method = (IMethodSymbol)symbol;
                    var isMethodShippedApi = hasPublicApiEntry && apiLine.IsShippedApi;

                    // Check if a public API is a constructor that makes this class instantiable, even though the base class
                    // is not instantiable. That API pattern is not allowed, because it causes protected members of
                    // the base class, which are not considered public APIs, to be exposed to subclasses of this class.
                    if (!isMethodShippedApi &&
                        method.MethodKind == MethodKind.Constructor &&
                        method.ContainingType.TypeKind == TypeKind.Class &&
                        !method.ContainingType.IsSealed &&
                        method.ContainingType.BaseType != null &&
                        IsPublicApiCore(method.ContainingType.BaseType) &&
                        !CanTypeBeExtendedPublicly(method.ContainingType.BaseType))
                    {
                        string errorMessageName = GetErrorMessageName(method, isImplicitlyDeclaredConstructor);
                        ImmutableDictionary<string, string> propertyBag = ImmutableDictionary<string, string>.Empty;
                        var locations = isImplicitlyDeclaredConstructor ? method.ContainingType.Locations : method.Locations;
                        reportDiagnostic(Diagnostic.Create(ExposedNoninstantiableType, locations[0], propertyBag, errorMessageName));
                    }

                    // Flag public API with optional parameters that violate backcompat requirements: https://github.com/dotnet/roslyn/blob/master/docs/Adding%20Optional%20Parameters%20in%20Public%20API.md.
                    if (method.HasOptionalParameters())
                    {
                        foreach (var overload in method.GetOverloads())
                        {
                            if (!IsPublicAPI(overload))
                            {
                                continue;
                            }

                            // Don't flag overloads which have identical params (e.g. overloading a generic and non-generic method with same parameter types).
                            if (overload.Parameters.Length == method.Parameters.Length &&
                                overload.Parameters.Select(p => p.Type).SequenceEqual(method.Parameters.Select(p => p.Type)))
                            {
                                continue;
                            }

                            // RS0026: Symbol '{0}' violates the backcompat requirement: 'Do not add multiple overloads with optional parameters'. See '{1}' for details.
                            var overloadHasOptionalParams = overload.HasOptionalParameters();
                            if (overloadHasOptionalParams)
                            {
                                // Flag only if 'method' is a new unshipped API with optional parameters.
                                if (!isMethodShippedApi)
                                {
                                    string errorMessageName = GetErrorMessageName(method, isImplicitlyDeclaredConstructor);
                                    reportDiagnosticAtLocations(AvoidMultipleOverloadsWithOptionalParameters, ImmutableDictionary<string, string>.Empty, errorMessageName, AvoidMultipleOverloadsWithOptionalParameters.HelpLinkUri);
                                    break;
                                }
                            }

                            // RS0027: Symbol '{0}' violates the backcompat requirement: 'Public API with optional parameter(s) should have the most parameters amongst its public overloads'. See '{1}' for details.
                            if (method.Parameters.Length <= overload.Parameters.Length)
                            {
                                // 'method' is unshipped: Flag regardless of whether the overload is shipped/unshipped.
                                // 'method' is shipped:   Flag only if overload is unshipped and has no optional parameters (overload will already be flagged with RS0026)
                                if (!isMethodShippedApi)
                                {
                                    string errorMessageName = GetErrorMessageName(method, isImplicitlyDeclaredConstructor);
                                    reportDiagnosticAtLocations(OverloadWithOptionalParametersShouldHaveMostParameters, ImmutableDictionary<string, string>.Empty, errorMessageName, OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri);
                                    break;
                                }
                                else if (!overloadHasOptionalParams)
                                {
                                    var overloadPublicApiName = GetPublicApiName(overload);
                                    var isOverloadUnshipped = !_publicApiMap.TryGetValue(overloadPublicApiName, out ApiLine overloadPublicApiLine) ||
                                        !overloadPublicApiLine.IsShippedApi;
                                    if (isOverloadUnshipped)
                                    {
                                        string errorMessageName = GetErrorMessageName(method, isImplicitlyDeclaredConstructor);
                                        reportDiagnosticAtLocations(OverloadWithOptionalParametersShouldHaveMostParameters, ImmutableDictionary<string, string>.Empty, errorMessageName, OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static string GetErrorMessageName(ISymbol symbol, bool isImplicitlyDeclaredConstructor)
            {
                return isImplicitlyDeclaredConstructor ?
                    string.Format(RoslynDiagnosticsAnalyzersResources.PublicImplicitConstructorErrorMessageName, symbol.ContainingSymbol.ToDisplayString(ShortSymbolNameFormat)) :
                    symbol.ToDisplayString(ShortSymbolNameFormat);
            }

            private string GetSiblingNamesToRemoveFromUnshippedText(ISymbol symbol)
            {
                // Don't crash the analyzer if we are unable to determine stale entries to remove in public API text.
                try
                {
                    return GetSiblingNamesToRemoveFromUnshippedTextCore(symbol);
                }
                catch(Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                    return string.Empty;
                }
            }

            private string GetSiblingNamesToRemoveFromUnshippedTextCore(ISymbol symbol)
            {
                // Compute all sibling names that must be removed from unshipped text, as they are no longer public or have been changed.
                if (symbol.ContainingSymbol is INamespaceOrTypeSymbol containingSymbol)
                {
                    // First get the lines in the unshipped text for siblings of the symbol:
                    //  (a) Contains Public API name of containing symbol.
                    //  (b) Doesn't contain Public API name of nested types/namespaces of containing symbol.
                    var containingSymbolPublicApiName = GetPublicApiName(containingSymbol);

                    var nestedNamespaceOrTypeMembers = containingSymbol.GetMembers().OfType<INamespaceOrTypeSymbol>().ToImmutableArray();
                    var nestedNamespaceOrTypesPublicApiNames = new List<string>(nestedNamespaceOrTypeMembers.Length);
                    foreach (var nestedNamespaceOrType in nestedNamespaceOrTypeMembers)
                    {
                        var nestedNamespaceOrTypePublicApiName = GetPublicApiName(nestedNamespaceOrType);
                        nestedNamespaceOrTypesPublicApiNames.Add(nestedNamespaceOrTypePublicApiName);
                    }

                    var publicApiLinesForSiblingsOfSymbol = new HashSet<string>();
                    foreach (var apiLine in _unshippedData.ApiList)
                    {
                        var apiLineText = apiLine.Text;
                        if (apiLineText == containingSymbolPublicApiName)
                        {
                            // Not a sibling of symbol.
                            continue;
                        }

                        if (!ContainsPublicApiName(apiLineText, containingSymbolPublicApiName + "."))
                        {
                            // Doesn't contain containingSymbol public API name - not a sibling of symbol.
                            continue;
                        }

                        var containedInNestedMember = false;
                        foreach (var nestedNamespaceOrTypePublicApiName in nestedNamespaceOrTypesPublicApiNames)
                        {
                            if (ContainsPublicApiName(apiLineText, nestedNamespaceOrTypePublicApiName + "."))
                            {
                                // Belongs to a nested type/namespace in containingSymbol - not a sibling of symbol.
                                containedInNestedMember = true;
                                break;
                            }
                        }

                        if (containedInNestedMember)
                        {
                            continue;
                        }

                        publicApiLinesForSiblingsOfSymbol.Add(apiLineText);
                    }

                    // Now remove the lines for siblings which are still public APIs - we don't want to remove those.
                    if (publicApiLinesForSiblingsOfSymbol.Count > 0)
                    {
                        var siblings = containingSymbol.GetMembers();
                        foreach (var sibling in siblings)
                        {
                            if (sibling.IsImplicitlyDeclared)
                            {
                                if (!sibling.IsConstructor())
                                {
                                    continue;
                                }
                            }
                            else if (!IsPublicAPI(sibling))
                            {
                                continue;
                            }

                            var siblingPublicApiName = GetPublicApiName(sibling);
                            publicApiLinesForSiblingsOfSymbol.Remove(siblingPublicApiName);
                        }

                        // Join all the symbols names with a special separator.
                        return string.Join(PublicApiNamesOfSiblingsToRemovePropertyBagValueSeparator, publicApiLinesForSiblingsOfSymbol);
                    }
                }

                return string.Empty;
            }

            private string GetPublicApiName(ISymbol symbol)
            {
                string publicApiName = symbol.ToDisplayString(s_publicApiFormat);

                ITypeSymbol memberType = null;
                if (symbol is IMethodSymbol)
                {
                    memberType = ((IMethodSymbol)symbol).ReturnType;
                }
                else if (symbol is IPropertySymbol)
                {
                    memberType = ((IPropertySymbol)symbol).Type;
                }
                else if (symbol is IEventSymbol)
                {
                    memberType = ((IEventSymbol)symbol).Type;
                }
                else if (symbol is IFieldSymbol)
                {
                    memberType = ((IFieldSymbol)symbol).Type;
                }

                if (memberType != null)
                {
                    publicApiName = publicApiName + " -> " + memberType.ToDisplayString(s_publicApiFormat);
                }

                if (((symbol as INamespaceSymbol)?.IsGlobalNamespace).GetValueOrDefault())
                {
                    return string.Empty;
                }

                if (symbol.ContainingAssembly != null && !symbol.ContainingAssembly.Equals(_compilation.Assembly))
                {
                    publicApiName += $" (forwarded, contained in {symbol.ContainingAssembly.Name})";
                }
    
                return publicApiName;
            }

            private static bool ContainsPublicApiName(string apiLineText, string publicApiNameToSearch)
            {
                // Ensure we don't search in parameter list/return type.
                var indexOfParamsList = apiLineText.IndexOf('(');
                if (indexOfParamsList > 0)
                {
                    apiLineText = apiLineText.Substring(0, indexOfParamsList);
                }
                else
                {
                    var indexOfReturnType = apiLineText.IndexOf("->", StringComparison.Ordinal);
                    if (indexOfReturnType > 0)
                    {
                        apiLineText = apiLineText.Substring(0, indexOfReturnType);
                    }
                }

                // Ensure that we don't have any leading characters in matched substring, apart from whitespace.
                var index = apiLineText.IndexOf(publicApiNameToSearch, StringComparison.Ordinal);
                return index == 0 || (index > 0 && apiLineText[index - 1] == ' ');
            }

            internal void OnCompilationEnd(CompilationAnalysisContext context)
            {
                ProcessTypeForwardedAttributes(context.Compilation, context.ReportDiagnostic, context.CancellationToken);
                List<ApiLine> deletedApiList = GetDeletedApiList();
                foreach (ApiLine cur in deletedApiList)
                {
                    LinePositionSpan linePositionSpan = cur.SourceText.Lines.GetLinePositionSpan(cur.Span);
                    Location location = Location.Create(cur.Path, cur.Span, linePositionSpan);
                    ImmutableDictionary<string, string> propertyBag = ImmutableDictionary<string, string>.Empty.Add(PublicApiNamePropertyBagKey, cur.Text);
                    context.ReportDiagnostic(Diagnostic.Create(RemoveDeletedApiRule, location, propertyBag, cur.Text));
                }
            }

            private void ProcessTypeForwardedAttributes(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
            {
                var typeForwardedToAttribute = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.TypeForwardedToAttribute");

                if (typeForwardedToAttribute != null)
                {
                    foreach (var attribute in compilation.Assembly.GetAttributes())
                    {
                        if (attribute.AttributeClass.Equals(typeForwardedToAttribute))
                        {
                            if (attribute.AttributeConstructor.Parameters.Length == 1 &&
                                attribute.ConstructorArguments.Length == 1)
                            {
                                var forwardedType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

                                if (forwardedType != null)
                                {
                                    VisitForwardedTypeRecursively(forwardedType, reportDiagnostic, attribute.ApplicationSyntaxReference.GetSyntax(cancellationToken).GetLocation(), cancellationToken);
                                }
                            }
                        }
                    }
                }
            }

            private void VisitForwardedTypeRecursively(ISymbol symbol, Action<Diagnostic> reportDiagnostic, Location typeForwardedAttributeLocation, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnSymbolActionCore(symbol, reportDiagnostic, typeForwardedAttributeLocation);

                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    foreach (var nestedType in namedTypeSymbol.GetTypeMembers())
                    {
                        VisitForwardedTypeRecursively(nestedType, reportDiagnostic, typeForwardedAttributeLocation, cancellationToken);
                    }

                    foreach (var member in namedTypeSymbol.GetMembers())
                    {
                        if (!(member.IsImplicitlyDeclared && member.IsDefaultConstructor()))
                        {
                            VisitForwardedTypeRecursively(member, reportDiagnostic, typeForwardedAttributeLocation, cancellationToken);
                        }
                    }
                }
            }

            /// <summary>
            /// Calculated the set of APIs which have been deleted but not yet documented.
            /// </summary>
            /// <returns></returns>
            internal List<ApiLine> GetDeletedApiList()
            {
                var list = new List<ApiLine>();
                foreach (KeyValuePair<string, ApiLine> pair in _publicApiMap)
                {
                    if (_visitedApiList.Contains(pair.Key))
                    {
                        continue;
                    }

                    if (_unshippedData.RemovedApiList.Any(x => x.Text == pair.Key))
                    {
                        continue;
                    }

                    list.Add(pair.Value);
                }

                return list;
            }

            private bool IsPublicAPI(ISymbol symbol)
            {
                if (symbol is IMethodSymbol methodSymbol && s_ignorableMethodKinds.Contains(methodSymbol.MethodKind))
                {
                    return false;
                }

                return IsPublicApiCore(symbol);
            }

            private bool IsPublicApiCore(ISymbol symbol)
            {
                switch (symbol.DeclaredAccessibility)
                {
                    case Accessibility.Public:
                        return symbol.ContainingType == null || IsPublicApiCore(symbol.ContainingType);
                    case Accessibility.Protected:
                    case Accessibility.ProtectedOrInternal:
                        // Protected symbols must have parent types (that is, top-level protected
                        // symbols are not allowed.
                        return
                            symbol.ContainingType != null &&
                            IsPublicApiCore(symbol.ContainingType) &&
                            CanTypeBeExtendedPublicly(symbol.ContainingType);
                    default:
                        return false;
                }
            }

            private bool CanTypeBeExtendedPublicly(ITypeSymbol type)
            {
                if (_typeCanBeExtendedCache.TryGetValue(type, out bool result))
                {
                    return result;
                }

                // a type can be extended publicly if (1) it isn't sealed, and (2) it has some constructor that is
                // not internal, private or protected&internal
                result = !type.IsSealed &&
                    type.GetMembers(WellKnownMemberNames.InstanceConstructorName).Any(
                        m => m.DeclaredAccessibility != Accessibility.Internal && m.DeclaredAccessibility != Accessibility.Private && m.DeclaredAccessibility != Accessibility.ProtectedAndInternal
                    );

                _typeCanBeExtendedCache.Add(type, result);
                return result;
            }
        }
    }
}

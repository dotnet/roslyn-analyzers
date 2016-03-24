// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class ISymbolExtensions
    {
        public static bool IsType(this ISymbol symbol)
        {
            var typeSymbol = symbol as ITypeSymbol;
            return typeSymbol != null && typeSymbol.IsType;
        }

        public static bool IsAccessorMethod(this ISymbol symbol)
        {
            var accessorSymbol = symbol as IMethodSymbol;
            return accessorSymbol != null &&
                (accessorSymbol.MethodKind == MethodKind.PropertySet || accessorSymbol.MethodKind == MethodKind.PropertyGet ||
                accessorSymbol.MethodKind == MethodKind.EventRemove || accessorSymbol.MethodKind == MethodKind.EventAdd);
        }

        public static bool IsPublic(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility == Accessibility.Public;
        }

        public static bool IsErrorType(this ISymbol symbol)
        {
            return
                symbol is ITypeSymbol &&
                ((ITypeSymbol)symbol).TypeKind == TypeKind.Error;
        }

        public static bool IsConstructor(this ISymbol symbol)
        {
            return (symbol as IMethodSymbol)?.MethodKind == MethodKind.Constructor;
        }

        public static bool IsDestructor(this ISymbol symbol)
        {
            return (symbol as IMethodSymbol)?.MethodKind == MethodKind.Destructor;
        }

        public static bool IsIndexer(this ISymbol symbol)
        {
            return (symbol as IPropertySymbol)?.IsIndexer == true;
        }

        public static bool IsUserDefinedOperator(this ISymbol symbol)
        {
            return (symbol as IMethodSymbol)?.MethodKind == MethodKind.UserDefinedOperator;
        }

        public static ImmutableArray<IParameterSymbol> GetParameters(this ISymbol symbol)
        {
            return symbol.TypeSwitch(
                (IMethodSymbol m) => m.Parameters,
                (IPropertySymbol p) => p.Parameters,
                _ => ImmutableArray.Create<IParameterSymbol>());
        }

        public static SymbolVisibility GetResultantVisibility(this ISymbol symbol)
        {
            // Start by assuming it's visible.
            SymbolVisibility visibility = SymbolVisibility.Public;

            switch (symbol.Kind)
            {
                case SymbolKind.Alias:
                    // Aliases are uber private.  They're only visible in the same file that they
                    // were declared in.
                    return SymbolVisibility.Private;

                case SymbolKind.Parameter:
                    // Parameters are only as visible as their containing symbol
                    return GetResultantVisibility(symbol.ContainingSymbol);

                case SymbolKind.TypeParameter:
                    // Type Parameters are private.
                    return SymbolVisibility.Private;
            }

            while (symbol != null && symbol.Kind != SymbolKind.Namespace)
            {
                switch (symbol.DeclaredAccessibility)
                {
                    // If we see anything private, then the symbol is private.
                    case Accessibility.NotApplicable:
                    case Accessibility.Private:
                        return SymbolVisibility.Private;

                    // If we see anything internal, then knock it down from public to
                    // internal.
                    case Accessibility.Internal:
                    case Accessibility.ProtectedAndInternal:
                        visibility = SymbolVisibility.Internal;
                        break;

                        // For anything else (Public, Protected, ProtectedOrInternal), the
                        // symbol stays at the level we've gotten so far.
                }

                symbol = symbol.ContainingSymbol;
            }

            return visibility;
        }

        public static bool MatchMemberDerivedByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.ContainingType.DerivesFrom(type) && member.MetadataName == name;
        }

        public static bool MatchMethodDerivedByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Method && member.MatchMemberDerivedByName(type, name);
        }

        public static bool MatchMethodByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Method && member.MatchMemberByName(type, name);
        }

        public static bool MatchPropertyDerivedByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Property && member.MatchMemberDerivedByName(type, name);
        }

        public static bool MatchFieldDerivedByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Field && member.MatchMemberDerivedByName(type, name);
        }

        public static bool MatchMemberByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.ContainingType == type && member.MetadataName == name;
        }

        public static bool MatchPropertyByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Property && member.MatchMemberByName(type, name);
        }

        public static bool MatchFieldByName(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Field && member.MatchMemberByName(type, name);
        }

        public static bool ContainsParameterWithType(this IEnumerable<IParameterSymbol> parameters, INamedTypeSymbol type)
        {
            var parametersWithType = GetParametersWithType(parameters, type);
            return parametersWithType.Any();
        }

        public static IEnumerable<IParameterSymbol> GetParametersWithType(this IEnumerable<IParameterSymbol> parameters, INamedTypeSymbol type)
        {
            return parameters.Where(p => p.Type?.Equals(type) == true);
        }

        public static bool OverloadWithGivenTypeParameterExist(this IEnumerable<IMethodSymbol> overloads, IMethodSymbol self, INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            foreach (IMethodSymbol overload in overloads)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (self?.Equals(overload) == true)
                {
                    continue;
                }

                if (overload.Parameters.ContainsParameterWithType(type))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<int> GetParameterIndices(this IMethodSymbol method, IEnumerable<IParameterSymbol> parameters, CancellationToken cancellationToken)
        {
            var set = new HashSet<IParameterSymbol>(parameters);
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (set.Contains(method.Parameters[i]))
                {
                    yield return i;
                }
            }
        }

        public static bool ParameterTypesAreSame(this IMethodSymbol method1, IMethodSymbol method2, IEnumerable<int> parameterIndices, CancellationToken cancellationToken)
        {
            foreach (int index in parameterIndices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // this doesnt account for type conversion but FxCop implementation seems doesnt either
                // so this should match FxCop implementation.
                if (method2.Parameters[index].Type?.Equals(method1.Parameters[index].Type) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AllParametersHaveGivenType(this IMethodSymbol method, IEnumerable<int> parameterIndices, INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            foreach (var index in parameterIndices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (method.Parameters[index].Type?.Equals(type) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsFromMscorlib(this ISymbol symbol, Compilation compilation)
        {
            var @object = WellKnownTypes.Object(compilation);
            return symbol.ContainingAssembly?.Equals(@object.ContainingAssembly) == true;
        }

        public static IMethodSymbol GetMatchingOverload(this IMethodSymbol method, IEnumerable<IMethodSymbol> overloads, int parameterIndex, INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            foreach (IMethodSymbol overload in overloads)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // does not account for method with optional parameters
                if (method.Equals(overload) || overload.Parameters.Length != method.Parameters.Length)
                {
                    // either itself, or signature is not same
                    continue;
                }

                if (!method.ParameterTypesAreSame(overload, Enumerable.Range(0, method.Parameters.Length).Where(i => i != parameterIndex), cancellationToken))
                {
                    // check whether remaining parameters match existing types, otherwise, we are not interested
                    continue;
                }

                if (overload.Parameters[parameterIndex].Type?.Equals(type) == true)
                {
                    // we no longer interested in this overload. there can be only 1 match
                    return overload;
                }
            }

            return null;
        }
    }

    public enum SymbolVisibility
    {
        Public,
        Internal,
        Private,
    }
}

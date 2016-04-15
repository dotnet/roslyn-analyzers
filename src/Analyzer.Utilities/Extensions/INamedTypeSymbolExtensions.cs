// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class INamedTypeSymbolExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetBaseTypesAndThis(this INamedTypeSymbol type)
        {
            INamedTypeSymbol current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static bool ImplementsOperator(this INamedTypeSymbol symbol, string op)
        {
            // TODO: should this filter on the right-hand-side operator type?
            return symbol.GetMembers(op).OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.UserDefinedOperator).Any();
        }

        /// <summary>
        /// Returns a value indicating whether the specified type implements both the
        /// equality and inequality operators.
        /// </summary>
        /// <param name="symbol">
        /// A symbols specifying the type to examine.
        /// </param>
        /// <returns>
        /// true if the type specified by <paramref name="symbol"/> implements both the
        /// equality and inequality operators, otherwise false.
        /// </returns>
        public static bool ImplementsEqualityOperators(this INamedTypeSymbol symbol)
        {
            return symbol.ImplementsOperator(WellKnownMemberNames.EqualityOperatorName) &&
                   symbol.ImplementsOperator(WellKnownMemberNames.InequalityOperatorName);
        }

        /// <summary>
        /// Returns a value indicating whether the specified type implements the comparison
        /// operators.
        /// </summary>
        /// <param name="symbol">
        /// A symbols specifying the type to examine.
        /// </param>
        /// <returns>
        /// true if the type specified by <paramref name="symbol"/> implements the comparison
        /// operators (which includes the equality and inequality operators), otherwise false.
        /// </returns>
        public static bool ImplementsComparisonOperators(this INamedTypeSymbol symbol)
        {
            return symbol.ImplementsEqualityOperators() &&
                   symbol.ImplementsOperator(WellKnownMemberNames.LessThanOperatorName) &&
                   symbol.ImplementsOperator(WellKnownMemberNames.GreaterThanOperatorName);
        }

        public static bool OverridesEquals(this INamedTypeSymbol symbol)
        {
            // Does the symbol override Object.Equals?
            return symbol.GetMembers(WellKnownMemberNames.ObjectEquals).OfType<IMethodSymbol>().Where(m => m.IsEqualsOverride()).Any();
        }

        public static bool OverridesGetHashCode(this INamedTypeSymbol symbol)
        {
            // Does the symbol override Object.GetHashCode?
            return symbol.GetMembers(WellKnownMemberNames.ObjectGetHashCode).OfType<IMethodSymbol>().Where(m => m.IsGetHashCodeOverride()).Any();
        }

        public static bool HasFinalizer(this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Any(IMethodSymbolExtensions.IsFinalizer);
        }

        private static bool IsEqualsOverride(IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.ReturnType.SpecialType == SpecialType.System_Boolean &&
                   method.Parameters.Length == 1 &&
                   method.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                   IsObjectMethodOverride(method);
        }

        private static bool IsGetHashCodeOverride(IMethodSymbol method)
        {
            return method.IsOverride &&
                   method.ReturnType.SpecialType == SpecialType.System_Int32 &&
                   method.Parameters.Length == 0 &&
                   IsObjectMethodOverride(method);
        }

        private static bool IsObjectMethodOverride(IMethodSymbol method)
        {
            IMethodSymbol overriddenMethod = method.OverriddenMethod;
            while (overriddenMethod != null)
            {
                if (overriddenMethod.ContainingType.SpecialType == SpecialType.System_Object)
                {
                    return true;
                }

                overriddenMethod = overriddenMethod.OverriddenMethod;
            }

            return false;
        }
    }
}

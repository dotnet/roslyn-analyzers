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
                .Any(m => m.IsFinalizer());
        }

        public static bool IsOkToBeUnused(this INamedTypeSymbol symbol, Compilation compilation)
        {
            if (symbol.TypeKind != TypeKind.Class || symbol.IsStatic || symbol.IsAbstract)
            {
                return true;
            }

            // Attributes are not instantiated in IL but are created by reflection.
            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("System.Attribute");
            if (symbol.Inherits(attributeSymbol))
            {
                return true;
            }

            // The type containing the assembly's entry point is OK.
            if (symbol.ContainsEntryPoint(compilation))
            {
                return true;
            }

            // MEF exported classes are instantiated by MEF, by reflection.
            if (symbol.IsMefExported(compilation))
            {
                return true;
            }

            // Types implementing the (deprecated) IConfigurationSectionHandler interface
            // are OK because they are instantiated by the configuration system.
            INamedTypeSymbol iConfigurationSectionHandlerSymbol = compilation.GetTypeByMetadataName("System.Configuration.IConfigurationSectionHandler");
            if (symbol.Inherits(iConfigurationSectionHandlerSymbol))
            {
                return true;
            }

            // Likewise for types derived from ConfigurationSection.
            INamedTypeSymbol configurationSection = compilation.GetTypeByMetadataName("System.Configuration.ConfigurationSection");
            if (symbol.Inherits(configurationSection))
            {
                return true;
            }

            // SafeHandles can be created from within the type itself by native code.
            INamedTypeSymbol safeHandle = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.SafeHandle");
            if (symbol.Inherits(safeHandle))
            {
                return true;
            }

            INamedTypeSymbol traceListener = compilation.GetTypeByMetadataName("System.Diagnostics.TraceListener");
            if (symbol.Inherits(traceListener))
            {
                return true;
            }

            return false;
        }

        public static bool IsMefExported(this INamedTypeSymbol symbol, Compilation compilation)
        {
            INamedTypeSymbol mef1ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.Composition.ExportAttribute");
            INamedTypeSymbol mef2ExportAttributeSymbol = compilation.GetTypeByMetadataName("System.Composition.ExportAttribute");

            return (mef1ExportAttributeSymbol != null && symbol.HasAttribute(mef1ExportAttributeSymbol))
                || (mef2ExportAttributeSymbol != null && symbol.HasAttribute(mef2ExportAttributeSymbol));
        }

        public static bool HasAttribute(this INamedTypeSymbol symbol, INamedTypeSymbol attribute)
        {
            return symbol.GetAttributes().Any(attr => attr.AttributeClass.Equals(attribute));
        }

        private static bool ContainsEntryPoint(this INamedTypeSymbol symbol, Compilation compilation)
        {
            // If this type doesn't live in an application assembly (.exe), it can't contain
            // the entry point.
            if (compilation.Options.OutputKind != OutputKind.ConsoleApplication &&
                compilation.Options.OutputKind != OutputKind.WindowsApplication &&
                compilation.Options.OutputKind != OutputKind.WindowsRuntimeApplication)
            {
                return false;
            }

            // TODO: Handle the case where Compilation.Options.MainTypeName matches this type.
            // TODO: Test: can't have type parameters.
            // TODO: Main in nested class? If allowed, what name does it have?
            // TODO: Test that parameter is array of int.
            return symbol.GetMembers("Main")
                .Where(m => m is IMethodSymbol)
                .Cast<IMethodSymbol>()
                .Any(IsEntryPoint);
        }

        private static bool IsEntryPoint(IMethodSymbol method)
        {
            if (!method.IsStatic)
            {
                return false;
            }

            if (method.ReturnType.SpecialType != SpecialType.System_Int32 && !method.ReturnsVoid)
            {
                return false;
            }

            if (method.Parameters.Count() == 0)
            {
                return true;
            }

            if (method.Parameters.Count() > 1)
            {
                return false;
            }

            ITypeSymbol parameterType = method.Parameters.Single().Type;

            return true;
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

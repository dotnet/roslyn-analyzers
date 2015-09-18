// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers.Common
{
    public static class DiagnosticHelpers
    {
        public static bool MatchMemberDerived(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.ContainingType.IsDerivedFrom(type) && member.MetadataName == name;
        }

        public static bool MatchMethodDerived(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Method && member.MatchMemberDerived(type, name);
        }

        public static bool MatchPropertyDerived(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Property && member.MatchMemberDerived(type, name);
        }

        public static bool MatchFieldDerived(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Field && member.MatchMemberDerived(type, name);
        }

        public static bool IsDerivedFrom(this ITypeSymbol typeSymbol, ITypeSymbol baseSymbol, bool baseTypesOnly = false)
        {
            if (baseSymbol == null)
            {
                return false;
            }

            if (!baseTypesOnly && typeSymbol.AllInterfaces.Contains(baseSymbol))
            {
                return true;
            }

            for (ITypeSymbol baseType = typeSymbol; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType == baseSymbol)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MatchMember(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.ContainingType == type && member.MetadataName == name;
        }

        public static bool MatchMethod(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Method && member.MatchMember(type, name);
        }

        public static bool MatchProperty(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Property && member.MatchMember(type, name);
        }

        public static bool MatchField(this ISymbol member, INamedTypeSymbol type, string name)
        {
            return member != null && member.Kind == SymbolKind.Field && member.MatchMember(type, name);
        }

        public static ITypeSymbol GetVariableSymbolType(this ISymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }
            SymbolKind kind = symbol.Kind;
            switch (kind)
            {
                case SymbolKind.Field:
                    return ((IFieldSymbol)symbol).Type;
                case SymbolKind.Local:
                    return ((ILocalSymbol)symbol).Type;
                case SymbolKind.Parameter:
                    return ((IParameterSymbol)symbol).Type;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Type;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if a symbol is visible outside of an assembly.
        /// </summary>
        /// <param name="symbol">The symbol whose access shall be checked.</param>
        /// <returns>true if the symbol is visible outside its assembly; otherwise, false.</returns>
        public static bool IsVisibleOutsideAssembly(this ISymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            for (ISymbol containingType = symbol; containingType != null; containingType = containingType.ContainingType)
            {
                if (IsInvisibleOutsideAssemblyAtSymbolLevel(containingType))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsInvisibleOutsideAssemblyAtSymbolLevel(ISymbol symbol)
        {
            return SymbolIsPrivateOrInternal(symbol)
                || SymbolIsProtectedInSealed(symbol);
        }

        private static bool SymbolIsPrivateOrInternal(ISymbol symbol)
        {
            var access = symbol.DeclaredAccessibility;
            return access == Accessibility.Private
                || access == Accessibility.Internal
                || access == Accessibility.ProtectedAndInternal
                || access == Accessibility.NotApplicable;
        }

        private static bool SymbolIsProtectedInSealed(ISymbol symbol)
        {
            var containgType = symbol.ContainingType;
            if (containgType != null && containgType.IsSealed)
            {
                var access = symbol.DeclaredAccessibility;
                return access == Accessibility.Protected
                    || access == Accessibility.ProtectedOrInternal;
            }

            return false;
        }

        /// <summary>
        /// Determine wether a type (given by name) is actually declared in the expected assembly (also given by name)
        /// </summary>               
        /// <remarks>
        /// This can be used to decide wether we are referencing the expected framework for a given type. 
        /// For example, System.String exists in mscorlib for .NET Framework and System.Runtime for other framework (e.g. .NET Core). 
        /// </remarks>
        public static bool? IsTypeDeclaredInExpectedAssembly(Compilation compilation, string typeName, string assemblyName)
        {
            if (compilation == null)
            {
                return null;
            }
            var typeSymbol = compilation.GetTypeByMetadataName(typeName);
            return typeSymbol?.ContainingAssembly.Identity.Name.Equals(assemblyName, StringComparison.Ordinal);
        }

        public static bool IsTypeDeclaredInExpectedAssembly(ITypeSymbol typeSymbol, string assemblyName)
        {                                                               
            return typeSymbol.ContainingAssembly.Identity.Name.Equals(assemblyName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the version of the target .NET framework of the compilation.
        /// </summary>                          
        /// <returns>
        /// Null if the target framenwork is not .NET Framework.
        /// </returns>
        /// <remarks>
        /// This method returns the assembly version of mscorlib for .NET Framework prior version 4.0, 
        /// i.e. for .NET framework 3.5, the returned version would be 2.0.0.0.
        /// For .NET Framework 4.X, this method returns the actual framework version instead of assembly verison of mscorlib,
        /// i.e. for .NET framework 4.5.2, this method return 4.5.2 instead of 4.0.0.0.
        /// </remarks>
        public static Version GetDotNetFrameworkVersion(Compilation compilation)
        {
            if (compilation == null || !IsTypeDeclaredInExpectedAssembly(compilation, "System.String", "mscorlib").GetValueOrDefault())
            {
                return null;
            }

            
            var mscorlibAssembly = compilation.GetTypeByMetadataName("System.String").ContainingAssembly;
            if (mscorlibAssembly.Identity.Version.Major < 4)
            {
                return mscorlibAssembly.Identity.Version;
            }

            if (mscorlibAssembly.GetTypeByMetadataName("System.AppContext") != null)
            {
                return new Version(4, 6);
            }
            INamedTypeSymbol typeSymbol = mscorlibAssembly.GetTypeByMetadataName("System.IO.UnmanagedMemoryStream");
            if (!typeSymbol.GetMembers("FlushAsync").IsEmpty)
            {
                return new Version(4, 5, 2);
            }
            typeSymbol = mscorlibAssembly.GetTypeByMetadataName("System.Diagnostics.Tracing.EventSource");
            if (typeSymbol != null)
            {
                return typeSymbol.GetMembers("CurrentThreadActivityId").IsEmpty ? new Version(4, 5) : new Version(4, 5, 1);
            }
            return new Version(4, 0);
        } 
        public static LocalizableResourceString GetLocalizableResourceString(string resourceName)
        {
            return new LocalizableResourceString(resourceName, DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        }
    }
}

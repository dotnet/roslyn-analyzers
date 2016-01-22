// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class DiagnosticHelpers
    {
        public static bool TryConvertToUInt64(object value, SpecialType specialType, out ulong convertedValue)
        {
            bool success = false;
            convertedValue = 0;
            if (value != null)
            {
                switch (specialType)
                {
                    case SpecialType.System_Int16:
                        convertedValue = unchecked((ulong)((short)value));
                        success = true;
                        break;
                    case SpecialType.System_Int32:
                        convertedValue = unchecked((ulong)((int)value));
                        success = true;
                        break;
                    case SpecialType.System_Int64:
                        convertedValue = unchecked((ulong)((long)value));
                        success = true;
                        break;
                    case SpecialType.System_UInt16:
                        convertedValue = (ushort)value;
                        success = true;
                        break;
                    case SpecialType.System_UInt32:
                        convertedValue = (uint)value;
                        success = true;
                        break;
                    case SpecialType.System_UInt64:
                        convertedValue = (ulong)value;
                        success = true;
                        break;
                    case SpecialType.System_Byte:
                        convertedValue = (byte)value;
                        success = true;
                        break;
                    case SpecialType.System_SByte:
                        convertedValue = unchecked((ulong)((sbyte)value));
                        success = true;
                        break;
                    case SpecialType.System_Char:
                        convertedValue = (char)value;
                        success = true;
                        break;
                }
            }

            return success;
        }

        internal static bool TryGetEnumMemberValues(INamedTypeSymbol enumType, out IList<ulong> values)
        {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.TypeKind == TypeKind.Enum);

            values = new List<ulong>();
            foreach (IFieldSymbol field in enumType.GetMembers().Where(m => m.Kind == SymbolKind.Field && !m.IsImplicitlyDeclared))
            {
                if (!field.HasConstantValue)
                {
                    return false;
                }

                ulong convertedValue;
                if (!TryConvertToUInt64(field.ConstantValue, enumType.EnumUnderlyingType.SpecialType, out convertedValue))
                {
                    return false;
                }

                values.Add(convertedValue);
            }

            return true;
        }

        public static string GetMemberName(ISymbol symbol)
        {
            // For Types
            if (symbol.Kind == SymbolKind.NamedType)
            {
                if ((symbol as INamedTypeSymbol).IsGenericType)
                {
                    return symbol.MetadataName;
                }
            }

            // For other language constructs
            return symbol.Name;
        }
               
        public static bool IsInvisibleOutsideAssemblyAtSymbolLevel(ISymbol symbol)
        {
            return SymbolIsPrivateOrInternal(symbol)
                || SymbolIsProtectedInSealed(symbol);
        }

        public static bool SymbolIsPrivateOrInternal(ISymbol symbol)
        {
            var access = symbol.DeclaredAccessibility;
            return access == Accessibility.Private
                || access == Accessibility.Internal
                || access == Accessibility.ProtectedAndInternal
                || access == Accessibility.NotApplicable;
        }

        public static bool SymbolIsProtectedInSealed(ISymbol symbol)
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
    }
}

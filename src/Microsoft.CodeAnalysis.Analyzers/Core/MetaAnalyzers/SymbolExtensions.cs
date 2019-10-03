// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    internal static class SymbolExtensions
    {
        public static bool IsTypeSymbol(this INamedTypeSymbol namedTypeSymbol, ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.Equals(namedTypeSymbol))
            {
                return true;
            }

            if (typeSymbol.AllInterfaces.Contains(namedTypeSymbol))
            {
                return true;
            }

            return false;
        }

        public static bool IsTypeSymbol(this INamedTypeSymbol namedTypeSymbol, IOperation operation)
        {
            if (operation.Type is object && namedTypeSymbol.IsTypeSymbol(operation.Type))
            {
                return true;
            }

            if (operation is IConversionOperation conversion)
            {
                return namedTypeSymbol.IsTypeSymbol(conversion.Operand);
            }

            if (operation is IArgumentOperation argumentOperation)
            {
                return namedTypeSymbol.IsTypeSymbol(argumentOperation.Parameter.Type);
            }

            return false;
        }
    }
}

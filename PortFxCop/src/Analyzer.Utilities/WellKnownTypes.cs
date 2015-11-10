// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class WellKnownTypes
    {
        public static INamedTypeSymbol GetICollectionType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Collections.ICollection");
        }

        public static INamedTypeSymbol GetArrayType(this Compilation compilation)
        {
            return compilation.GetSpecialType(SpecialType.System_Array);
        }
    }
}
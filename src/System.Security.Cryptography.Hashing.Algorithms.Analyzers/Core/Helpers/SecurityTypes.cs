// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace System.Security.Cryptography.Hashing.Algorithms.Analyzers
{
    public static class SecurityTypes
    {
        public static INamedTypeSymbol MD5(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.MD5");
        }
        public static INamedTypeSymbol SHA1(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.SHA1");
        }
        public static INamedTypeSymbol HMACSHA1(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.HMACSHA1");
        }
    }
}

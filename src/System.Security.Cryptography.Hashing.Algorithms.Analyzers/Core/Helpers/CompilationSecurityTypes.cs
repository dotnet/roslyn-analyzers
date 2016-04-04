// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace System.Security.Cryptography.Hashing.Algorithms.Analyzers
{
    public class CompilationSecurityTypes
    {
        public INamedTypeSymbol MD5 { get; private set; }
        public INamedTypeSymbol SHA1 { get; private set; }
        public INamedTypeSymbol HMACSHA1 { get; private set; }

        public CompilationSecurityTypes(Compilation compilation)
        {
            MD5 = SecurityTypes.MD5(compilation);
            SHA1 = SecurityTypes.SHA1(compilation);
            HMACSHA1 = SecurityTypes.HMACSHA1(compilation);
        }
    }
}

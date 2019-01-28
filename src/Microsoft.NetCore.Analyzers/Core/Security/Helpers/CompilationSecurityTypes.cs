// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.NetCore.Analyzers.Security.Helpers
{
    public class CompilationSecurityTypes
    {
        // Some of these types may only exist in .NET Framework and not in .NET Core, but that's okay, we'll look anyway.

        public INamedTypeSymbol MD5 { get; private set; }
        public INamedTypeSymbol SHA1 { get; private set; }
        public INamedTypeSymbol HMACSHA1 { get; private set; }
        public INamedTypeSymbol DES { get; private set; }
        public INamedTypeSymbol DSA { get; private set; }
        public INamedTypeSymbol DSASignatureFormatter { get; private set; }
        public INamedTypeSymbol HMACMD5 { get; private set; }
        public INamedTypeSymbol RC2 { get; private set; }
        public INamedTypeSymbol TripleDES { get; private set; }
        public INamedTypeSymbol RIPEMD160 { get; private set; }
        public INamedTypeSymbol HMACRIPEMD160 { get; private set; }

        public CompilationSecurityTypes(Compilation compilation)
        {
            MD5 = SecurityTypes.MD5(compilation);
            SHA1 = SecurityTypes.SHA1(compilation);
            HMACSHA1 = SecurityTypes.HMACSHA1(compilation);
            DES = SecurityTypes.DES(compilation);
            DSA = SecurityTypes.DSA(compilation);
            DSASignatureFormatter = SecurityTypes.DSASignatureFormatter(compilation);
            HMACMD5 = SecurityTypes.HMACMD5(compilation);
            RC2 = SecurityTypes.RC2(compilation);
            TripleDES = SecurityTypes.TripleDES(compilation);
            RIPEMD160 = SecurityTypes.RIPEMD160(compilation);
            HMACRIPEMD160 = SecurityTypes.HMACRIPEMD160(compilation);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
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
            MD5 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyMD5);
            SHA1 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographySHA1);
            HMACSHA1 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyHMACSHA1);
            DES = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyDES);
            DSA = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyDSA);
            DSASignatureFormatter = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyDSASignatureFormatter);
            HMACMD5 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyHMACMD5);
            RC2 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyRC2);
            TripleDES = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyTripleDES);
            RIPEMD160 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyRIPEMD160);
            HMACRIPEMD160 = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyHMACRIPEMD160);
        }
    }
}

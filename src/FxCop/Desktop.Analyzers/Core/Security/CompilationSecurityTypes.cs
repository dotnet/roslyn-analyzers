// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers.Common
{
    public class CompilationSecurityTypes
    {
        public INamedTypeSymbol HandleProcessCorruptedStateExceptionsAttribute { get; private set; }
        public INamedTypeSymbol SystemObject { get; private set; }
        public INamedTypeSymbol SystemException { get; private set; }
        public INamedTypeSymbol SystemSystemException { get; private set; }
        public INamedTypeSymbol DES { get; private set; }
        public INamedTypeSymbol DSA { get; private set; }
        public INamedTypeSymbol DSASignatureFormatter { get; private set; } 
        public INamedTypeSymbol HMACMD5 { get; private set; }
        public INamedTypeSymbol RC2 { get; private set; }
        public INamedTypeSymbol Rijndael { get; private set; }  
        public INamedTypeSymbol TripleDES { get; private set; }
        public INamedTypeSymbol RIPEMD160 { get; private set; }
        public INamedTypeSymbol HMACRIPEMD160 { get; private set; } 

        public CompilationSecurityTypes(Compilation compilation)
        {
            HandleProcessCorruptedStateExceptionsAttribute = 
                SecurityTypes.HandleProcessCorruptedStateExceptionsAttribute(compilation);
            SystemObject = SecurityTypes.SystemObject(compilation);
            SystemException = SecurityTypes.SystemException(compilation);
            SystemSystemException = SecurityTypes.SystemSystemException(compilation);
            DES = SecurityTypes.DES(compilation);
            DSA = SecurityTypes.DSA(compilation);
            DSASignatureFormatter = SecurityTypes.DSASignatureFormatter(compilation); 
            HMACMD5 = SecurityTypes.HMACMD5(compilation);
            RC2 = SecurityTypes.RC2(compilation);
            Rijndael = SecurityTypes.Rijndael(compilation);
            TripleDES = SecurityTypes.TripleDES(compilation);
            RIPEMD160 = SecurityTypes.RIPEMD160(compilation);
            HMACRIPEMD160 = SecurityTypes.HMACRIPEMD160(compilation);
        }
    }

    public static class SecurityMemberNames
    {
        public const string CreateSignature = "CreateSignature";
    }
}

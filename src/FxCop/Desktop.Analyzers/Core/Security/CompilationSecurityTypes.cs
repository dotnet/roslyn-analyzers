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
        public INamedTypeSymbol XmlDocument { get; private set; }
        public INamedTypeSymbol XPathDocument { get; private set; }
        public INamedTypeSymbol XmlSchema { get; private set; }
        public INamedTypeSymbol DataSet { get; private set; }
        public INamedTypeSymbol XmlSerializer { get; private set; }
        public INamedTypeSymbol DataTable { get; private set; }
        public INamedTypeSymbol XmlNode { get; private set; }
        public INamedTypeSymbol DataViewManager { get; private set; }
        public INamedTypeSymbol XmlTextReader { get; private set; }
        public INamedTypeSymbol XmlReader { get; private set; }
        public INamedTypeSymbol DtdProcessing { get; private set; }
        public INamedTypeSymbol XmlReaderSettings { get; private set; }
        public INamedTypeSymbol XslCompiledTransform { get; private set; }
        public INamedTypeSymbol XmlResolver { get; private set; }
        public INamedTypeSymbol XmlSecureResolver { get; private set; }
        public INamedTypeSymbol XsltSettings { get; private set; }
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
            this.HandleProcessCorruptedStateExceptionsAttribute = 
                SecurityTypes.HandleProcessCorruptedStateExceptionsAttribute(compilation);
            this.SystemObject = SecurityTypes.SystemObject(compilation);
            this.SystemException = SecurityTypes.SystemException(compilation);
            this.SystemSystemException = SecurityTypes.SystemSystemException(compilation);
        }

    public static class SecurityMemberNames
    {
        public const string CreateSignature = "CreateSignature";
    }
}

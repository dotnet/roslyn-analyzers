// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers.Common
{
    public static class SecurityTypes
    {
        public static INamedTypeSymbol HandleProcessCorruptedStateExceptionsAttribute(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(
                "System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute");
        }

        public static INamedTypeSymbol SystemObject(Compilation compilation)
        {
            return compilation.GetSpecialType(SpecialType.System_Object);
        }

        public static INamedTypeSymbol SystemException(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Exception");
        }

        public static INamedTypeSymbol SystemSystemException(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.SystemException");
        }

        public static INamedTypeSymbol XmlDocument(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlDocument");
        }

        public static INamedTypeSymbol XPathDocument(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XPath.XPathDocument");
        }

        public static INamedTypeSymbol XmlSchema(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.Schema.XmlSchema");
        }

        public static INamedTypeSymbol DataSet(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Data.DataSet");
        }

        public static INamedTypeSymbol XmlSerializer(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.Serialization.XmlSerializer");
        }

        public static INamedTypeSymbol DataTable(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Data.DataTable");
        }

        public static INamedTypeSymbol XmlNode(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlNode");
        }

        public static INamedTypeSymbol DataViewManager(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Data.DataViewManager");
        }

        public static INamedTypeSymbol XmlTextReader(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlTextReader");
        }

        public static INamedTypeSymbol XmlReader(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlReader");
        }

        public static INamedTypeSymbol DtdProcessing(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.DtdProcessing");
        }

        public static INamedTypeSymbol XmlReaderSettings(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlReaderSettings");
        }

        public static INamedTypeSymbol XslCompiledTransform(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.Xsl.XslCompiledTransform");
        }

        public static INamedTypeSymbol XmlResolver(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlResolver");
        }

        public static INamedTypeSymbol XmlSecureResolver(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.XmlSecureResolver");
        }

        public static INamedTypeSymbol XsltSettings(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Xml.Xsl.XsltSettings");
        }
        public static INamedTypeSymbol DES(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.DES");
        }

        public static INamedTypeSymbol DSA(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.DSA");
        }

        public static INamedTypeSymbol DSASignatureFormatter(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.DSASignatureFormatter");
        }          

        public static INamedTypeSymbol HMACMD5(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.HMACMD5");
        }

        public static INamedTypeSymbol RC2(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.RC2");
        }

        public static INamedTypeSymbol TripleDES(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.TripleDES");
        }

        public static INamedTypeSymbol RIPEMD160(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.RIPEMD160");
        }

        public static INamedTypeSymbol HMACRIPEMD160(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Security.Cryptography.HMACRIPEMD160");
        }
    }
}

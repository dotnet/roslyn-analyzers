// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;

namespace Microsoft.NetFramework.Analyzers.Helpers
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


        public CompilationSecurityTypes(Compilation compilation)
        {
            HandleProcessCorruptedStateExceptionsAttribute =
                compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemRuntimeExceptionServicesHandleProcessCorruptedStateExceptionsAttribute);
            SystemObject = compilation.GetSpecialType(SpecialType.System_Object);
            SystemException = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemException);
            SystemSystemException = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemSystemException);
            XmlDocument = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlDocument);
            XPathDocument = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXPathXPathDocument);
            XmlSchema = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlSchemaXmlSchema);
            DataSet = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemDataDataSet);
            XmlSerializer = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlSerializationXmlSerializer);
            DataTable = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemDataDataTable);
            XmlNode = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlNode);
            DataViewManager = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemDataDataViewManager);
            XmlTextReader = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlTextReader);
            XmlReader = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlReader);
            DtdProcessing = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlDtdProcessing);
            XmlReaderSettings = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlReaderSettings);
            XslCompiledTransform = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXslXslCompiledTransform);
            XmlResolver = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlResolver);
            XmlSecureResolver = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXmlSecureResolver);
            XsltSettings = compilation.GetTypeByMetadataName(WellKnownTypeNames.SystemXmlXslXsltSettings);
        }
    }
}

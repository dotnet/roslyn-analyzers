// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers
{
    class SecurityDiagnosticHelpers
    {
        public static bool IsXslCompiledTransformLoad(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null
                && method.MatchMethod(xmlTypes.XslCompiledTransform, SecurityMemberNames.Load);
        }

        public static bool IsXmlDocumentCtorDerived(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null &&
                   method.MatchMethodDerived(xmlTypes.XmlDocument, WellKnownMemberNames.InstanceConstructorName);
        }

        public static bool IsXmlDocumentXmlResolverPropertyDerived(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedPropertyDerived(symbol, xmlTypes.XmlDocument, SecurityMemberNames.XmlResolver);  
        }

        public static bool IsXmlDocumentXmlResolverProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlDocument, SecurityMemberNames.XmlResolver); 
        }

        public static bool IsXmlTextReaderCtor(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null 
                && method.MatchMethod(xmlTypes.XmlTextReader, WellKnownMemberNames.InstanceConstructorName);
        }

        public static bool IsXmlTextReaderCtorDerived(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null
                && method.MatchMethodDerived(xmlTypes.XmlTextReader, WellKnownMemberNames.InstanceConstructorName);
        }

        public static bool IsXmlTextReaderXmlResolverPropertyDerived(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedPropertyDerived(symbol, xmlTypes.XmlTextReader, SecurityMemberNames.XmlResolver);   
        }

        public static bool IsXmlTextReaderDtdProcessingPropertyDerived(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedPropertyDerived(symbol, xmlTypes.XmlTextReader, SecurityMemberNames.DtdProcessing);
        }

        public static bool IsXmlTextReaderXmlResolverProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlTextReader, SecurityMemberNames.XmlResolver);
        }

        public static bool IsXmlTextReaderDtdProcessingProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlTextReader, SecurityMemberNames.DtdProcessing);
        }

        public static bool IsXmlReaderCreate(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null
                && method.MatchMethod(xmlTypes.XmlReader, SecurityMemberNames.Create);
        }

        public static bool IsXmlReaderSettingsCtor(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null 
                && method.MatchMethod(xmlTypes.XmlReaderSettings, WellKnownMemberNames.InstanceConstructorName);
        }

        public static bool IsXmlReaderSettingsXmlResolverProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlReaderSettings, SecurityMemberNames.XmlResolver); 
        }

        public static bool IsXmlReaderSettingsDtdProcessingProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlReaderSettings, SecurityMemberNames.DtdProcessing); 
        }

        public static bool IsXmlReaderSettingsMaxCharactersFromEntitiesProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XmlReaderSettings, SecurityMemberNames.MaxCharactersFromEntities);  
        }

        public static bool IsXsltSettingsCtor(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return method != null 
                && method.MatchMethod(xmlTypes.XsltSettings, WellKnownMemberNames.InstanceConstructorName);
        }

        public static bool IsXsltSettingsTrustedXsltProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XsltSettings, SecurityMemberNames.TrustedXslt); 
        }

        public static bool IsXsltSettingsDefaultProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XsltSettings, SecurityMemberNames.Default);  
        }

        public static bool IsXsltSettingsEnableDocumentFunctionProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XsltSettings, SecurityMemberNames.EnableDocumentFunction);  
        }

        public static bool IsXsltSettingsEnableScriptProperty(ISymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return IsSpecifiedProperty(symbol, xmlTypes.XsltSettings, SecurityMemberNames.EnableScript);  
        }


        public static bool IsXmlResolverType(ITypeSymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return symbol != null
                && symbol.IsDerivedFrom(xmlTypes.XmlResolver, baseTypesOnly: true);
        }

        public static bool IsXmlSecureResolverType(ITypeSymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return symbol != null
                && symbol.IsDerivedFrom(xmlTypes.XmlSecureResolver, baseTypesOnly: true);
        }

        public static bool IsXsltSettingsType(ITypeSymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return symbol == xmlTypes.XsltSettings;
        }

        public static bool IsXmlReaderSettingsType(ITypeSymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return symbol == xmlTypes.XmlReaderSettings;                                
        }

        public static  int HasXmlResolverParameter(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return HasSpecifiedParameter(method, xmlTypes, IsXmlResolverType);
        }

        public static int HasXsltSettingsParameter(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return HasSpecifiedParameter(method, xmlTypes, IsXsltSettingsType);    
        }

        public static int HasXmlReaderSettingsParameter(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return HasSpecifiedParameter(method, xmlTypes, IsXmlReaderSettingsType);
        }

        public static bool IsXmlReaderType(ITypeSymbol symbol, CompilationSecurityTypes xmlTypes)
        {
            return symbol != null
                && symbol.IsDerivedFrom(xmlTypes.XmlReader, baseTypesOnly: true);
        }

        public static int HasXmlReaderParameter(IMethodSymbol method, CompilationSecurityTypes xmlTypes)
        {
            return HasSpecifiedParameter(method, xmlTypes, IsXmlReaderType); 
        }

        private static bool IsSpecifiedProperty(ISymbol symbol, INamedTypeSymbol namedType, string propertyName)
        {
            if (symbol != null && symbol.Kind == SymbolKind.Property)
            {
                IPropertySymbol property = (IPropertySymbol)symbol;
                return property.MatchProperty(namedType, propertyName);
            }

            return false;
        }

        private static bool IsSpecifiedPropertyDerived(ISymbol symbol, INamedTypeSymbol namedType, string propertyName)
        {
            if (symbol != null && symbol.Kind == SymbolKind.Property)
            {
                IPropertySymbol property = (IPropertySymbol)symbol;
                return property.MatchPropertyDerived(namedType, propertyName);
            }

            return false;
        }

        private static int HasSpecifiedParameter(IMethodSymbol method, CompilationSecurityTypes xmlTypes, Func<ITypeSymbol, CompilationSecurityTypes, bool> func)
        {
            int index = -1;
            if (method == null)
            {
                return index;
            }     
            for (int i = 0; i < method.Parameters.Length; i++)
            {
                var parameter = method.Parameters[i].Type;
                if (func(parameter, xmlTypes))
                {
                    index = i;
                    break;
                }
            }       
            return index;
        }

        public static LocalizableResourceString GetLocalizableResourceString(string resourceName)
        {
            return new LocalizableResourceString(resourceName, DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        }

        public static LocalizableResourceString GetLocalizableResourceString(string resourceName, params string[] formatArguments)
        {
            return new LocalizableResourceString(resourceName, DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources), formatArguments);
        }
    }
}

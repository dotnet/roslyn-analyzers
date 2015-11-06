﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2227: Collection properties should be read only
    /// 
    /// Cause:
    /// An externally visible writable property is a type that implements System.Collections.ICollection. 
    /// Arrays, indexers(properties with the name 'Item'), and permission sets are ignored by the rule.
    /// 
    /// Description:
    /// A writable collection property allows a user to replace the collection with a completely different collection. 
    /// A read-only property stops the collection from being replaced but still allows the individual members to be set.
    /// If replacing the collection is a goal, the preferred design pattern is to include a method to remove all the elements 
    /// from the collection and a method to re-populate the collection.See the Clear and AddRange methods of the System.Collections.ArrayList class
    /// for an example of this pattern.
    /// 
    /// Both binary and XML serialization support read-only properties that are collections.
    /// The System.Xml.Serialization.XmlSerializer class has specific requirements for types that implement ICollection and 
    /// System.Collections.IEnumerable in order to be serializable.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class CollectionPropertiesShouldBeReadOnlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2227";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionPropertiesShouldBeReadOnly), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionPropertiesShouldBeReadOnlyMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Usage,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: true,
                                                                         helpLinkUri: "https://msdn.microsoft.com/library/ms182327.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(Rule);
            }
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var iCollectionType = context.Compilation.GetICollectionType();
                    var arrayType = context.Compilation.GetArrayType();

                    if (iCollectionType == null || arrayType == null)
                    {
                        return;
                    }

                    var analyzer = new Analyzer(iCollectionType, arrayType);

                    context.RegisterSymbolAction(analyzer.AnalyzeSymbol, SymbolKind.Property);
                });
        }

        private class Analyzer
        {
            protected readonly INamedTypeSymbol _iCollectionType;
            protected readonly INamedTypeSymbol _arrayType;

            public Analyzer(INamedTypeSymbol iCollectionType, INamedTypeSymbol arrayType)
            {
                _iCollectionType = iCollectionType;
                _arrayType = arrayType;
            }

            public void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                var property = context.Symbol as IPropertySymbol;
                if (property == null)
                {
                    return;
                }

                // check whether it has a public setter
                var setter = property.SetMethod;
                if (setter == null || setter.DeclaredAccessibility != Accessibility.Public)
                {
                    return;
                }

                // make sure this property is NOT indexer, return type is NOT array but implement ICollection
                if (property.IsIndexer || Inherits(property.Type, _arrayType) || !Inherits(property.Type, _iCollectionType))
                {
                    return;
                }

                context.ReportDiagnostic(property.CreateDiagnostic(Rule));
            }

            private bool Inherits(ITypeSymbol symbol, ITypeSymbol baseType)
            {
                return symbol == null ? false : symbol.Inherits(baseType);
            }
        }
    }
}

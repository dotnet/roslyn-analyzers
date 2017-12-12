// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
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

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionPropertiesShouldBeReadOnlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionPropertiesShouldBeReadOnlyMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionPropertiesShouldBeReadOnlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                    s_localizableTitle,
                                                                    s_localizableMessage,
                                                                    DiagnosticCategory.Usage,
                                                                    DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                    isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                    description: s_localizableDescription,
                                                                    helpLinkUri: "https://msdn.microsoft.com/library/ms182327.aspx",
                                                                    customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    INamedTypeSymbol iCollectionType = WellKnownTypes.ICollection(context.Compilation);
                    INamedTypeSymbol arrayType = WellKnownTypes.Array(context.Compilation);
                    INamedTypeSymbol dataMemberAttribute = WellKnownTypes.DataMemberAttribute(context.Compilation);

                    if (iCollectionType == null || arrayType == null)
                    {
                        return;
                    }

                    context.RegisterSymbolAction(c => AnalyzeSymbol(c, iCollectionType, arrayType, dataMemberAttribute), SymbolKind.Property);
                });
        }

        public static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol iCollectionType, INamedTypeSymbol arrayType, INamedTypeSymbol dataMemberAttribute)
        {
            var property = (IPropertySymbol)context.Symbol;

            // check whether it has a public setter
            IMethodSymbol setter = property.SetMethod;
            if (setter == null || !setter.IsExternallyVisible())
            {
                return;
            }

            // make sure this property is NOT indexer, return type is NOT array but implement ICollection
            if (property.IsIndexer || Inherits(property.Type, arrayType) || !Inherits(property.Type, iCollectionType))
            {
                return;
            }

            if (dataMemberAttribute != null)
            {
                // Special case: the DataContractSerializer requires that a public setter exists.
                bool hasDataMemberAttribute = property.GetAttributes().Any(a => a.AttributeClass.Equals(dataMemberAttribute));
                if (hasDataMemberAttribute)
                {
                    return;
                }
            }

            context.ReportDiagnostic(property.CreateDiagnostic(Rule));
        }

        private static bool Inherits(ITypeSymbol symbol, ITypeSymbol baseType)
        {
            return symbol == null ? false : symbol.Inherits(baseType);
        }
    }
}
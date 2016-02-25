// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1010: Collections should implement generic interface
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class CollectionsShouldImplementGenericInterfaceAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1010";

        private static readonly LocalizableString s_localizableTitle =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionsShouldImplementGenericInterfaceTitle),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionsShouldImplementGenericInterfaceMessage),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.CollectionsShouldImplementGenericInterfaceDescription),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                RuleId,
                s_localizableTitle,
                s_localizableMessage,
                DiagnosticCategory.Design,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: s_localizableDescription,
                helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182132.aspx",
                customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
               (context) =>
               {
                   INamedTypeSymbol iCollectionType = WellKnownTypes.ICollection(context.Compilation);
                   INamedTypeSymbol genericICollectionType = WellKnownTypes.GenericICollection(context.Compilation);
                   INamedTypeSymbol iEnumerableType = WellKnownTypes.IEnumerable(context.Compilation);
                   INamedTypeSymbol genericIEnumerableType = WellKnownTypes.GenericIEnumerable(context.Compilation);
                   INamedTypeSymbol iListType = WellKnownTypes.IList(context.Compilation);
                   INamedTypeSymbol genericIListType = WellKnownTypes.GenericIList(context.Compilation);

                   if (iCollectionType == null && genericICollectionType == null &&
                       iEnumerableType == null && genericIEnumerableType == null &&
                       iListType == null && genericIListType == null)
                   {
                       return;
                   }

                   context.RegisterSymbolAction(c => AnalyzeSymbol(c,
                                                iCollectionType, genericICollectionType,
                                                iEnumerableType, genericIEnumerableType,
                                                iListType, genericIListType),
                                                SymbolKind.NamedType);
               });
        }

        public void AnalyzeSymbol(SymbolAnalysisContext context,
                                   INamedTypeSymbol iCollectionType, INamedTypeSymbol gCollectionType,
                                   INamedTypeSymbol iEnumerableType, INamedTypeSymbol gEnumerableType,
                                   INamedTypeSymbol iListType, INamedTypeSymbol gListType)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            System.Collections.Generic.IEnumerable<INamedTypeSymbol> allInterfaces = namedTypeSymbol.AllInterfaces.Select(t => t.OriginalDefinition);

            foreach (INamedTypeSymbol @interface in allInterfaces)
            {
                if ((@interface.Equals(iCollectionType) && !allInterfaces.Contains(gCollectionType)) ||
                     (@interface.Equals(iEnumerableType) && !allInterfaces.Contains(gEnumerableType)) ||
                      (@interface.Equals(iListType) && !allInterfaces.Contains(gListType)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, namedTypeSymbol.Locations.First(), namedTypeSymbol.Name, @interface.Name));
                    break;
                }
            }
        }
    }
}

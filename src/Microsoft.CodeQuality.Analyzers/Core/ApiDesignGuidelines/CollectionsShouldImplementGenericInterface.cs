// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
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

        private static readonly LocalizableString s_localizableStandardMessage =
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
                s_localizableStandardMessage,
                DiagnosticCategory.Design,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_localizableDescription,
                helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182132.aspx",
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

        private static void AnalyzeSymbol(
            SymbolAnalysisContext context,
            INamedTypeSymbol iCollectionType,
            INamedTypeSymbol gCollectionType,
            INamedTypeSymbol iEnumerableType,
            INamedTypeSymbol gEnumerableType,
            INamedTypeSymbol iListType,
            INamedTypeSymbol gListType)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // FxCop compat: only fire on externally visible types.
            if (!namedTypeSymbol.IsExternallyVisible())
            {
                return;
            }

            var allInterfaces = namedTypeSymbol.AllInterfaces.Select(t => t.OriginalDefinition).ToImmutableArray();

            var allInterfacesStatus = default(CollectionsInterfaceStatus);
            foreach (var @interface in allInterfaces)
            {
                if (@interface.Equals(iCollectionType))
                {
                    allInterfacesStatus.ICollectionPresent = true;
                }
                else if (@interface.Equals(iEnumerableType))
                {
                    allInterfacesStatus.IEnumerablePresent = true;
                }
                else if (@interface.Equals(iListType))
                {
                    allInterfacesStatus.IListPresent = true;
                }
                else if (@interface.Equals(gCollectionType))
                {
                    allInterfacesStatus.GCollectionPresent = true;
                }
                else if (@interface.Equals(gEnumerableType))
                {
                    allInterfacesStatus.GEnumerablePresent = true;
                }
                else if (@interface.Equals(gListType))
                {
                    allInterfacesStatus.GListPresent = true;
                }
            }

            INamedTypeSymbol missingInterface;
            INamedTypeSymbol implementedInterface;
            if (allInterfacesStatus.GListPresent)
            {
                // Implemented IList<T>, meaning has all 3 generic interfaces. Nothing can be wrong.
                return;
            }
            else if (allInterfacesStatus.IListPresent)
            {
                // Implemented IList but not IList<T>.
                missingInterface = gListType;
                implementedInterface = iListType;
            }
            else if (allInterfacesStatus.GCollectionPresent)
            {
                // Implemented ICollection<T>, and doesn't have an inherit of IList. Nothing can be wrong
                return;
            }
            else if (allInterfacesStatus.ICollectionPresent)
            {
                // Implemented ICollection but not ICollection<T>
                missingInterface = gCollectionType;
                implementedInterface = iCollectionType;
            }
            else if (allInterfacesStatus.GEnumerablePresent)
            {
                // Implemented IEnumerable<T>, and doesn't have an inherit of ICollection. Nothing can be wrong
                return;
            }
            else if (allInterfacesStatus.IEnumerablePresent)
            {
                // Implemented IEnumerable, but not IEnumerable<T>
                missingInterface = gEnumerableType;
                implementedInterface = iEnumerableType;
            }
            else
            {
                // No collections implementation, nothing can be wrong.
                return;
            }

            Debug.Assert(missingInterface != null && implementedInterface != null);
            context.ReportDiagnostic(Diagnostic.Create(Rule,
                                                       namedTypeSymbol.Locations.First(),
                                                       namedTypeSymbol.Name,
                                                       implementedInterface.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                       missingInterface.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        private struct CollectionsInterfaceStatus
        {
            public bool IListPresent { get; set; }
            public bool GListPresent { get; set; }
            public bool ICollectionPresent { get; set; }
            public bool GCollectionPresent { get; set; }
            public bool IEnumerablePresent { get; set; }
            public bool GEnumerablePresent { get; set; }
        }
    }
}

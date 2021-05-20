// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// Detect the use of [RequiresPreviewFeatures] in assemblies that have not opted into preview features
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DetectPreviewFeatureAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2250";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPreviewFeaturesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPreviewFeaturesMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPreviewFeaturesDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Usage,
                                                                                      RuleLevel.BuildWarning,
                                                                                      s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        private const string RequiresPreviewFeaturesAttribute = nameof(RequiresPreviewFeaturesAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context =>
            {
                IAssemblySymbol? thisAssembly = context.Compilation.Assembly;
                ImmutableArray<AttributeData> attributes = thisAssembly.GetAttributes();

                AttributeData? attribute = attributes.FirstOrDefault(x => x.AttributeClass.ToString() == RequiresPreviewFeaturesAttribute);
                if (!(attribute is null))
                {
                    // This assembly has enabled preview attributes.
                    return;
                }

                var enabledPreviewFeatures = context.Options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.EnablePreviewFeatures, context.Compilation, context.CancellationToken);
                if (string.Equals(enabledPreviewFeatures, "true", System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols = new ConcurrentDictionary<ISymbol, IOperation?>();
                context.RegisterOperationAction(context => BuildSymbolInformation(context, requiresPreviewFeaturesSymbols),
                    OperationKind.Invocation,
                    OperationKind.ObjectCreation,
                    OperationKind.PropertyReference,
                    OperationKind.FieldReference,
                    OperationKind.DelegateCreation,
                    OperationKind.EventReference
                    );
            });
        }

        private void BuildSymbolInformation(OperationAnalysisContext context, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            if (OperationUsesPreviewFeatures(context.Operation, requiresPreviewFeaturesSymbols))
            {
                context.ReportDiagnostic(context.Operation.CreateDiagnostic(Rule));
            }
        }

        private bool OperationUsesPreviewFeatures(IOperation operation, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            ISymbol? symbol = GetOperationSymbol(operation);
            if (symbol == null)
            {
                return false;
            }

            return OperationUsesPreviewFeatures(operation, symbol, requiresPreviewFeaturesSymbols, true);
        }

        private static ISymbol? GetOperationSymbol(IOperation operation)
            => operation switch
            {
                IInvocationOperation iOperation => iOperation.TargetMethod,
                IObjectCreationOperation cOperation => cOperation.Constructor,
                IPropertyReferenceOperation pOperation => pOperation.Property,
                IFieldReferenceOperation fOperation => fOperation.Field,
                IDelegateCreationOperation dOperation => dOperation.Type,
                IEventReferenceOperation eOperation => eOperation.Member,
                _ => null,
            };

        private bool TryGetOrCachePreviewFeaturesAttributesOnSymbol(IOperation operation, ISymbol symbol, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols, bool checkParents)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(symbol, out IOperation? existing))
            {
                if (checkParents)
                {
                    var containingSymbol = symbol.ContainingSymbol;
                    // Namespaces do not have attributes
                    while (containingSymbol is INamespaceSymbol)
                    {
                        containingSymbol = containingSymbol.ContainingSymbol;
                    }

                    if (containingSymbol != null)
                    {
                        if (TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, containingSymbol, requiresPreviewFeaturesSymbols, checkParents))
                        {
                            return true;
                        }
                    }
                }
                if (symbol is ITypeSymbol typeSymbol)
                {
                    ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.AllInterfaces;
                    foreach (INamedTypeSymbol? anInterface in interfaces)
                    {
                        if (TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, anInterface, requiresPreviewFeaturesSymbols, checkParents))
                        {
                            return true;
                        }
                    }

                    INamedTypeSymbol? baseType = typeSymbol.BaseType;
                    if (baseType != null)
                    {
                        if (TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, baseType, requiresPreviewFeaturesSymbols, checkParents))
                        {
                            return true;
                        }
                    }
                }
                if (symbol.IsOverride)
                {
                    ISymbol? overriddenMember = symbol.GetOverriddenMember();
                    if (overriddenMember != null)
                    {
                        if (TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, overriddenMember, requiresPreviewFeaturesSymbols, checkParents))
                        {
                            return true;
                        }
                    }
                }
                return GetOrAddAttributes(symbol, operation, requiresPreviewFeaturesSymbols);
            }

            return existing != null;
        }

        private static bool GetOrAddAttributes(ISymbol symbol, IOperation operation, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            IOperation? value = null;
            bool ret = false;
            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
            foreach (AttributeData attribute in attributes)
            {
                string attributeName = attribute.AttributeClass.Name;
                if (attributeName == RequiresPreviewFeaturesAttribute)
                {
                    ret = true;
                    value = operation;
                    break;
                }
            }
            requiresPreviewFeaturesSymbols.GetOrAdd(symbol, value);
            return ret;
        }

        private bool OperationUsesPreviewFeatures(IOperation operation, ISymbol symbol, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols, bool checkParents)
        {
            return TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, symbol, requiresPreviewFeaturesSymbols, checkParents);
        }
    }
}

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

        private bool thisAssemblyUsesPreviewFeatures;
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
                context.RegisterOperationBlockStartAction(context => BuildSymbolInformation(context, requiresPreviewFeaturesSymbols));
            });
        }

        private void BuildSymbolInformation(OperationBlockStartAnalysisContext context, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            context.RegisterOperationAction(context => AnalyzeOperation(context.Operation, requiresPreviewFeaturesSymbols),
                OperationKind.Invocation,
                OperationKind.ObjectCreation,
                OperationKind.PropertyReference,
                OperationKind.FieldReference,
                OperationKind.DelegateCreation,
                OperationKind.EventReference
                );

            context.RegisterOperationBlockEndAction(context =>
            {
                if (thisAssemblyUsesPreviewFeatures)
                {
                    foreach (var symbolAndOperation in requiresPreviewFeaturesSymbols)
                    {
                        IOperation? value = symbolAndOperation.Value;
                        if (value != null)
                        {
                            context.ReportDiagnostic(value.CreateDiagnostic(Rule));
                        }
                    }
                }
            });
        }

        private void AnalyzeOperation(IOperation operation, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            ISymbol? symbol = GetOperationSymbol(operation);
            if (symbol == null)
            {
                return;
            }

            CheckOperationAttributes(operation, symbol, requiresPreviewFeaturesSymbols, true);
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

        private void TryGetOrCachePreviewFeaturesAttributesOnSymbol(IOperation operation, ISymbol symbol, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols, bool checkParents)
        {
            if (!requiresPreviewFeaturesSymbols.TryGetValue(symbol, out IOperation? _))
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
                        TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, containingSymbol, requiresPreviewFeaturesSymbols, checkParents);
                    }
                }
                if (symbol is ITypeSymbol typeSymbol)
                {
                    ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.AllInterfaces;
                    foreach (INamedTypeSymbol? anInterface in interfaces)
                    {
                        TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, anInterface, requiresPreviewFeaturesSymbols, checkParents);
                    }

                    INamedTypeSymbol? baseType = typeSymbol.BaseType;
                    if (baseType != null)
                    {
                        TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, baseType, requiresPreviewFeaturesSymbols, checkParents);
                    }
                }
                if (symbol.IsOverride)
                {
                    ISymbol? overriddenMember = symbol.GetOverriddenMember();
                    if (overriddenMember != null)
                    {
                        TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, overriddenMember, requiresPreviewFeaturesSymbols, checkParents);
                    }
                }
                GetOrAddAttributes(symbol, operation, requiresPreviewFeaturesSymbols);
            }
        }

        private void GetOrAddAttributes(ISymbol symbol, IOperation operation, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols)
        {
            IOperation? value = null;
            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
            foreach (AttributeData attribute in attributes)
            {
                string attributeName = attribute.AttributeClass.Name;
                if (attributeName == RequiresPreviewFeaturesAttribute)
                {
                    value = operation;
                    thisAssemblyUsesPreviewFeatures = true;
                    break;
                }
            }
            requiresPreviewFeaturesSymbols.GetOrAdd(symbol, value);
        }

        private void CheckOperationAttributes(IOperation operation, ISymbol symbol, ConcurrentDictionary<ISymbol, IOperation?> requiresPreviewFeaturesSymbols, bool checkParents)
        {
            TryGetOrCachePreviewFeaturesAttributesOnSymbol(operation, symbol, requiresPreviewFeaturesSymbols, checkParents);
        }
    }
}

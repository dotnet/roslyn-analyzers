// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferContainsKeyOrContainsValue : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1835";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferContainsKeyOrContainsValueTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferContainsKeyOrContainsValueMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferContainsKeyOrContainsValueDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var idictionaryType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2);
                var ireadOnlyDictionaryType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIReadOnlyDictionary2);

                if (idictionaryType == null && ireadOnlyDictionaryType == null)
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;

                    if (invocation.TargetMethod.Name != "Contains" ||
                        !(GetReceiverOperationOrDefault(invocation) is IPropertyReferenceOperation propertyReference) ||
                        !(propertyReference.Member is IPropertySymbol property))
                    {
                        return;
                    }

                    if (property.Name == "Keys")
                    {
                        // The 'ContainsKey' method is part of the interface so we do not need extra checks
                        if (IsDictionary(property, idictionaryType, ireadOnlyDictionaryType))
                        {
                            context.ReportDiagnostic(invocation.CreateDiagnostic(Rule));
                        }
                    }
                    else if (property.Name == "Values")
                    {
                        // The 'ContainsValue' method only exists in some of the implementations so we need to check
                        // if the method exists before reporting a diagnostic.
                        if (IsDictionary(property, idictionaryType, ireadOnlyDictionaryType) &&
                            property.ContainingType.GetMembers("ContainsValue").OfType<IMethodSymbol>().Any())
                        {
                            context.ReportDiagnostic(invocation.CreateDiagnostic(Rule));
                        }
                    }
                    else
                    {
                        // do nothing
                    }
                }, OperationKind.Invocation);
            });
        }

        private static bool IsDictionary(IPropertySymbol property, INamedTypeSymbol? idictionaryType, INamedTypeSymbol? ireadOnlyDictionaryType)
            => property.ContainingType.OriginalDefinition.Equals(idictionaryType) ||
                property.ContainingType.OriginalDefinition.Equals(ireadOnlyDictionaryType) ||
                property.ContainingType.DerivesFrom(idictionaryType) ||
                property.ContainingType.DerivesFrom(ireadOnlyDictionaryType);

        private static IOperation? GetReceiverOperationOrDefault(IInvocationOperation invocation)
        {
            if (invocation.Instance != null)
            {
                return invocation.Instance;
            }
            else if (invocation.TargetMethod.IsExtensionMethod && invocation.Arguments.Length > 0)
            {
                return invocation.Arguments[0].Value.WalkDownConversion();
            }
            else
            {
                return null;
            }
        }
    }
}

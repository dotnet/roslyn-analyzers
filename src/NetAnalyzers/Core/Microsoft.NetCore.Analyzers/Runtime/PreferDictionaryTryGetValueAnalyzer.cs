// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class PreferDictionaryTryGetValueAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1838";

        private const string ContainsKeyMethodName = nameof(IDictionary<dynamic, dynamic>.ContainsKey);
        private const string IndexerName = "this[]";

        internal static readonly DiagnosticDescriptor ContainsKeyRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            "s_localizableTitle",
            "s_localizableContainsKeyMessage",
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            "s_localizableContainsKeyDescription.",
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ContainsKeyRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext compilationContext)
        {
            var compilation = compilationContext.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericICollection1, out _))
                return;
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2, out var dictionaryType))
                return;
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIEnumerable1, out _))
                return;

            compilationContext.RegisterOperationAction(context => OnOperationAction(context, dictionaryType), OperationKind.PropertyReference);
        }

        private void OnOperationAction(OperationAnalysisContext context, INamedTypeSymbol dictionaryType)
        { 
            var propertyReference = (IPropertyReferenceOperation)context.Operation;
            
            if (!IsDictionaryAccess(propertyReference, dictionaryType) 
                || !TryGetParentConditionalOperation(propertyReference, out var conditionalOperation)
                || !TryGetContainsKeyGuard(conditionalOperation, out var containsKeyInvocation))
            {
                return;
            }

            if (conditionalOperation!.WhenTrue is IBlockOperation blockOperation && DictionaryEntryIsModified(blockOperation))
            {
                return;
            }

            var additionalLocations = ImmutableArray.Create(propertyReference.Syntax.GetLocation());
            context.ReportDiagnostic(Diagnostic.Create(ContainsKeyRule, containsKeyInvocation.Syntax.GetLocation()));
        }

        private static bool TryGetContainsKeyGuard(IConditionalOperation conditionalOperation, [NotNullWhen(true)] out IInvocationOperation? invocationOperation)
        {
            invocationOperation = null;
            
            if (conditionalOperation.Condition is IInvocationOperation i)
            {
                invocationOperation = i;

                return true;
            }
            
            return false;
        }

        private static bool DictionaryEntryIsModified(IBlockOperation blockOperation)
        {
            return false;
        }

        private bool IsDictionaryAccess(IPropertyReferenceOperation propertyReference, INamedTypeSymbol dictionaryType)
        {
            return propertyReference.Property.IsIndexer && IsDictionaryType(propertyReference.Property.ContainingType, dictionaryType) &&
                   propertyReference.Property.OriginalDefinition.Name == IndexerName;
        }

        private static bool TryGetParentConditionalOperation(IOperation derivedOperation, [NotNullWhen(true)] out IConditionalOperation? conditionalOperation)
        {
            conditionalOperation = null;
            do
            {
                if (derivedOperation.Parent is IConditionalOperation c)
                {
                    conditionalOperation = c;
                    
                    return true;
                }

                derivedOperation = derivedOperation.Parent;
            } while (derivedOperation.Parent != null);

            return false;
        }

        private static bool IsDictionaryType(INamedTypeSymbol derived, INamedTypeSymbol dictionaryType)
        {
            var constructedDictionaryType = derived.GetBaseTypesAndThis()
                .WhereAsArray(x => x.OriginalDefinition.Equals(dictionaryType, SymbolEqualityComparer.Default))
                .SingleOrDefault() ?? derived.AllInterfaces
                .WhereAsArray(x => x.OriginalDefinition.Equals(dictionaryType, SymbolEqualityComparer.Default))
                .SingleOrDefault();

            return constructedDictionaryType is not null;
        }
    }
}
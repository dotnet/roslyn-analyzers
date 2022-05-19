// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
    public sealed class PreferDictionaryTryGetValueAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1854";

        private const string ContainsKeyMethodName = nameof(IDictionary<dynamic, dynamic>.ContainsKey);
        private const string IndexerName = "this[]";
        private const string IndexerNameVb = "Item";

        private static readonly LocalizableString s_localizableTitle = CreateResource(nameof(MicrosoftNetCoreAnalyzersResources.PreferDictionaryTryGetValueTitle));
        private static readonly LocalizableString s_localizableTryGetValueMessage = CreateResource(nameof(MicrosoftNetCoreAnalyzersResources.PreferDictionaryTryGetValueMessage));
        private static readonly LocalizableString s_localizableTryGetValueDescription = CreateResource(nameof(MicrosoftNetCoreAnalyzersResources.PreferDictionaryTryGetValueDescription));

        internal static readonly DiagnosticDescriptor ContainsKeyRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableTryGetValueMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            s_localizableTryGetValueDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ContainsKeyRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext compilationContext)
        {
            var compilation = compilationContext.Compilation;
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2, out var dictionaryType))
                return;

            compilationContext.RegisterOperationAction(context => OnOperationAction(context, dictionaryType), OperationKind.PropertyReference);
        }

        private static void OnOperationAction(OperationAnalysisContext context, INamedTypeSymbol dictionaryType)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            if (propertyReference.Parent is IAssignmentOperation
                || !IsDictionaryAccess(propertyReference, dictionaryType)
                || !TryGetParentConditionalOperation(propertyReference, out var conditionalOperation)
                || !TryGetContainsKeyGuard(conditionalOperation, out var containsKeyInvocation))
            {
                return;
            }

            if (conditionalOperation.WhenTrue is IBlockOperation blockOperation && DictionaryEntryIsModified(propertyReference, blockOperation))
            {
                return;
            }

            var additionalLocations = ImmutableArray.Create(propertyReference.Syntax.GetLocation());
            context.ReportDiagnostic(Diagnostic.Create(ContainsKeyRule, containsKeyInvocation.Syntax.GetLocation(), additionalLocations));
        }

        private static bool TryGetContainsKeyGuard(IConditionalOperation conditionalOperation, [NotNullWhen(true)] out IInvocationOperation? containsKeyInvocation)
        {
            containsKeyInvocation = conditionalOperation.Condition as IInvocationOperation ?? FindContainsKeyInvocation(conditionalOperation.Condition);
            if (containsKeyInvocation is not null && containsKeyInvocation.TargetMethod.Name == ContainsKeyMethodName)
            {
                return true;
            }

            return false;
        }

        private static IInvocationOperation? FindContainsKeyInvocation(IOperation baseOperation)
        {
            return baseOperation switch
            {
                IInvocationOperation i when i.TargetMethod.Name == ContainsKeyMethodName => i,
                IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalAnd or BinaryOperatorKind.ConditionalOr } b =>
                    FindContainsKeyInvocation(b.LeftOperand) ?? FindContainsKeyInvocation(b.RightOperand),
                _ => null
            };
        }

        private static bool DictionaryEntryIsModified(IPropertyReferenceOperation dictionaryAccess, IBlockOperation blockOperation)
        {
            return blockOperation.Operations.OfType<IExpressionStatementOperation>().Any(o =>
                o.Operation is IAssignmentOperation { Target: IPropertyReferenceOperation reference } && reference.Property.Equals(dictionaryAccess.Property, SymbolEqualityComparer.Default));
        }

        private static bool IsDictionaryAccess(IPropertyReferenceOperation propertyReference, INamedTypeSymbol dictionaryType)
        {
            return propertyReference.Property.IsIndexer && IsDictionaryType(propertyReference.Property.ContainingType, dictionaryType) &&
                   (propertyReference.Property.OriginalDefinition.Name == IndexerName || propertyReference.Language == LanguageNames.VisualBasic && propertyReference.Property.OriginalDefinition.Name == IndexerNameVb);
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

        private static bool IsDictionaryType(INamedTypeSymbol derived, ISymbol dictionaryType)
        {
            var constructedDictionaryType = derived.GetBaseTypesAndThis()
                .WhereAsArray(x => x.OriginalDefinition.Equals(dictionaryType, SymbolEqualityComparer.Default))
                .SingleOrDefault() ?? derived.AllInterfaces
                .WhereAsArray(x => x.OriginalDefinition.Equals(dictionaryType, SymbolEqualityComparer.Default))
                .SingleOrDefault();

            return constructedDictionaryType is not null;
        }

        private static LocalizableString CreateResource(string resourceName)
        {
            return new LocalizableResourceString(resourceName, MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        }
    }
}
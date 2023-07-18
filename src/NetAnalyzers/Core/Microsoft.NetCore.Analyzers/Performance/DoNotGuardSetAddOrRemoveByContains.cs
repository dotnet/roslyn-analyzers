// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA1865: <inheritdoc cref="@DoNotGuardSetAddOrRemoveByContainsTitle"/>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotGuardSetAddOrRemoveByContains : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1865";

        private const string Contains = nameof(Contains);
        private const string Add = nameof(Add);
        private const string Remove = nameof(Remove);

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotGuardSetAddOrRemoveByContainsTitle)),
            CreateLocalizableResourceString(nameof(DoNotGuardSetAddOrRemoveByContainsMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotGuardSetAddOrRemoveByContainsDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!TryGetRequiredMethods(context.Compilation, out var containsMethod, out var addMethod, out var removeMethod))
            {
                return;
            }

            context.RegisterOperationAction(context => OnConditional(context, containsMethod, addMethod, removeMethod), OperationKind.Conditional);
        }

        private static void OnConditional(OperationAnalysisContext context, IMethodSymbol containsMethod, IMethodSymbol addMethod, IMethodSymbol removeMethod)
        {
            var conditional = (IConditionalOperation)context.Operation;

            if (!TryExtractContainsInvocation(conditional.Condition, containsMethod, out var containsInvocation, out var containsNegated))
            {
                return;
            }

            if (!TryExtractAddOrRemoveInvocation(conditional.WhenTrue.Children, addMethod, removeMethod, containsNegated, out var addOrRemoveInvocation))
            {
                return;
            }

            if (!AreInvocationsOnSameInstance(containsInvocation, addOrRemoveInvocation))
            {
                return;
            }

            using var locations = ArrayBuilder<Location>.GetInstance(2);
            locations.Add(conditional.Syntax.GetLocation());
            locations.Add(addOrRemoveInvocation.Syntax.Parent!.GetLocation());

            context.ReportDiagnostic(containsInvocation.CreateDiagnostic(Rule, additionalLocations: locations.ToImmutable(), null));
        }

        private static bool TryGetRequiredMethods(
            Compilation compilation,
            [NotNullWhen(true)] out IMethodSymbol? containsMethod,
            [NotNullWhen(true)] out IMethodSymbol? addMethod,
            [NotNullWhen(true)] out IMethodSymbol? removeMethod)
        {
            var iSetType = WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericISet1);
            var iCollectionType = WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericICollection1);

            if (iSetType is null || iCollectionType is null)
            {
                containsMethod = null;
                addMethod = null;
                removeMethod = null;

                return false;
            }

            addMethod = iSetType.GetMembers(Add).OfType<IMethodSymbol>().FirstOrDefault();
            containsMethod = iCollectionType.GetMembers(Contains).OfType<IMethodSymbol>().FirstOrDefault();
            removeMethod = iCollectionType.GetMembers(Remove).OfType<IMethodSymbol>().FirstOrDefault();

            return containsMethod is not null && addMethod is not null && removeMethod is not null;
        }

        private static bool TryExtractContainsInvocation(
            IOperation condition,
            IMethodSymbol containsMethod,
            [NotNullWhen(true)] out IInvocationOperation? containsInvocation,
            out bool containsNegated)
        {
            containsNegated = false;
            containsInvocation = null;

            switch (condition.WalkDownParentheses())
            {
                case IInvocationOperation invocation:
                    containsInvocation = invocation;
                    break;
                case IUnaryOperation unaryOperation when unaryOperation.OperatorKind == UnaryOperatorKind.Not && unaryOperation.Operand is IInvocationOperation operand:
                    containsNegated = true;
                    containsInvocation = operand;
                    break;
                default:
                    return false;
            }

            return DoesImplementInterfaceMethod(containsInvocation.TargetMethod, containsMethod);
        }

        private static bool TryExtractAddOrRemoveInvocation(
            IEnumerable<IOperation> operations,
            IMethodSymbol addMethod,
            IMethodSymbol removeMethod,
            bool containsNegated,
            [NotNullWhen(true)] out IInvocationOperation? addOrRemoveInvocation)
        {
            addOrRemoveInvocation = operations
                .FirstOrDefault()
                ?.DescendantsAndSelf()
                        .OfType<IInvocationOperation>()
                        .FirstOrDefault(i => containsNegated ?
                            DoesImplementInterfaceMethod(i.TargetMethod, addMethod) :
                            DoesImplementInterfaceMethod(i.TargetMethod, removeMethod));

            return addOrRemoveInvocation is not null;
        }

        private static bool AreInvocationsOnSameInstance(IInvocationOperation invocation1, IInvocationOperation invocation2)
        {
            return (invocation1.Instance, invocation2.Instance) switch
            {
                (IFieldReferenceOperation fieldRef1, IFieldReferenceOperation fieldRef2) => fieldRef1.Member == fieldRef2.Member,
                (IPropertyReferenceOperation propRef1, IPropertyReferenceOperation propRef2) => propRef1.Member == propRef2.Member,
                (IParameterReferenceOperation paramRef1, IParameterReferenceOperation paramRef2) => paramRef1.Parameter == paramRef2.Parameter,
                (ILocalReferenceOperation localRef1, ILocalReferenceOperation localRef2) => localRef1.Local == localRef2.Local,
                _ => false,
            };
        }

        private static bool DoesImplementInterfaceMethod(IMethodSymbol method, IMethodSymbol interfaceMethod)
        {
            return method.IsImplementationOfInterfaceMethod(method.Parameters[0].Type, interfaceMethod.ContainingType, interfaceMethod.Name);
        }
    }
}

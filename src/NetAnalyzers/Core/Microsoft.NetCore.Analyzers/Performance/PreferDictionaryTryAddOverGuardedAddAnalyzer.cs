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
using static Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferDictionaryTryAddOverGuardedAddAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1863";

        private const string ContainsKey = nameof(IDictionary<dynamic, dynamic>.ContainsKey);
        private const string Add = nameof(IDictionary<dynamic, dynamic>.Add);
        private const string TryAdd = nameof(TryAdd);

        private static readonly DiagnosticDescriptor DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(PreferDictionaryTryAddTitle)),
            CreateLocalizableResourceString(nameof(PreferDictionaryTryAddMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferDictionaryTryAddDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!TryGetDictionaryTypeAndMembers(context.Compilation, out var iDictionaryType, out var containsKeySymbol, out var addSymbol))
            {
                return;
            }

            context.RegisterOperationAction(ctx => OnOperationAnalysis(iDictionaryType, containsKeySymbol, addSymbol, ctx), OperationKind.Conditional);
        }

        private static void OnOperationAnalysis(INamedTypeSymbol iDictionaryType, IMethodSymbol containsKeySymbol, IMethodSymbol addSymbol, OperationAnalysisContext context)
        {
            var conditional = (IConditionalOperation)context.Operation;
            var (invocation, branch) = conditional.Condition switch
            {
                IInvocationOperation i => (i, conditional.WhenFalse),
                IUnaryOperation { OperatorKind: UnaryOperatorKind.Not, Operand: IInvocationOperation i } => (i, conditional.WhenTrue),
                _ => (null, null)
            };
            if (invocation is not null
                && IsValidDictionaryType(invocation.TargetMethod.ContainingType, iDictionaryType, invocation.SemanticModel)
                && DoesSignatureMatch(invocation.TargetMethod, containsKeySymbol)
                && TryGetEligibleDictionaryAddLocation(branch, invocation.Instance, invocation.Arguments[0].Value, addSymbol, out var dictionaryAddLocation))
            {
                var additionalLocations = ImmutableArray.Create(dictionaryAddLocation);
                context.ReportDiagnostic(invocation.CreateDiagnostic(DiagnosticDescriptor, additionalLocations, null));
            }
        }

        private static bool TryGetEligibleDictionaryAddLocation(IOperation? conditionalBody, IOperation dictionaryInstance, IOperation containsKeyArgument, IMethodSymbol addSymbol,
            [NotNullWhen(true)]
            out Location? dictionaryAddLocation)
        {
            dictionaryAddLocation = null;
            if (conditionalBody is null)
            {
                return false;
            }

            if (conditionalBody is IBlockOperation block)
            {
                dictionaryAddLocation = block.Operations.OfType<IExpressionStatementOperation>().FirstOrDefault(e => e.Operation is IInvocationOperation i && IsEligibleDictionaryAddInvocation(i))
                    ?.Operation.Syntax.GetLocation();
            }
            else if (conditionalBody is IExpressionStatementOperation { Operation: IInvocationOperation iOperation } && IsEligibleDictionaryAddInvocation(iOperation))
            {
                dictionaryAddLocation = iOperation.Syntax.GetLocation();
            }

            bool IsEligibleDictionaryAddInvocation(IInvocationOperation invocation)
            {
                return SymbolEqualityComparer.Default.Equals(invocation.Instance.GetSymbolFromReference(), dictionaryInstance.GetSymbolFromReference())
                       && DoesSignatureMatch(invocation.TargetMethod, addSymbol)
                       && invocation.Arguments[0].Value.Syntax.IsEquivalentTo(containsKeyArgument.Syntax)
                       && invocation.Arguments[1].Value.Kind == OperationKind.Literal;
            }

            return dictionaryAddLocation is not null;
        }

        private static bool TryGetDictionaryTypeAndMembers(
            Compilation compilation,
            [NotNullWhen(true)]
            out INamedTypeSymbol? iDictionaryType,
            [NotNullWhen(true)]
            out IMethodSymbol? containsKeySymbol,
            [NotNullWhen(true)]
            out IMethodSymbol? addSymbol)
        {
            iDictionaryType = WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2);
            if (iDictionaryType is null)
            {
                containsKeySymbol = null;
                addSymbol = null;

                return false;
            }

            containsKeySymbol = iDictionaryType.GetMembers(ContainsKey).OfType<IMethodSymbol>().FirstOrDefault();
            addSymbol = iDictionaryType.GetMembers(Add).OfType<IMethodSymbol>().FirstOrDefault();

            return containsKeySymbol is not null && addSymbol is not null;
        }

        private static bool IsValidDictionaryType(INamedTypeSymbol suspectedDictionaryType, ISymbol iDictionaryType, SemanticModel semanticModel)
        {
            // Either the type is the IDictionary or it is a type which (indirectly) implements it.
            // If the type does not have a TryAdd() method (e.g. netstandard), we return false.
            return (suspectedDictionaryType.OriginalDefinition.Equals(iDictionaryType, SymbolEqualityComparer.Default)
                    || suspectedDictionaryType.AllInterfaces.Any((@interface, dictionary) => @interface.OriginalDefinition.Equals(dictionary, SymbolEqualityComparer.Default), iDictionaryType))
                   && semanticModel.LookupSymbols(0, suspectedDictionaryType, TryAdd, true).Length > 0;
        }

        private static bool DoesSignatureMatch(IMethodSymbol suspected, IMethodSymbol comparator)
        {
            return suspected.OriginalDefinition.ReturnType.Name == comparator.ReturnType.Name
                   && suspected.Name == comparator.Name
                   && suspected.Parameters.Length == comparator.Parameters.Length
                   && suspected.Parameters.Zip(comparator.Parameters, (p1, p2) => p1.OriginalDefinition.Type.Name == p2.Type.Name).All(isParameterEqual => isParameterEqual);
        }
    }
}
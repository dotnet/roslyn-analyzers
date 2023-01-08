// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using static Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferDictionaryTryAddValueOverGuardedAddAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1863";

        private const string ContainsKey = nameof(IDictionary<dynamic, dynamic>.ContainsKey);
        private const string Add = nameof(IDictionary<dynamic, dynamic>.Add);

        private static readonly LocalizableString s_localizableTitle = CreateLocalizableResourceString(nameof(PreferDictionaryTryAddValueTitle));
        private static readonly LocalizableString s_localizableTryGetValueMessage = CreateLocalizableResourceString(nameof(PreferDictionaryTryAddValueMessage));
        private static readonly LocalizableString s_localizableTryGetValueDescription = CreateLocalizableResourceString(nameof(PreferDictionaryTryAddValueDescription));

        private static readonly DiagnosticDescriptor DiagnosticDescriptor = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableTryGetValueMessage,
            "Performance",
            RuleLevel.IdeSuggestion,
            s_localizableTryGetValueDescription,
            false,
            false);

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
            if (conditional.Condition is IInvocationOperation invocation
                && IsDictionaryType(invocation.TargetMethod.ContainingType, iDictionaryType)
                && DoesSignatureMatch(invocation.TargetMethod, containsKeySymbol)
                && TryGetEligibleDictionaryAddLocation(conditional.WhenFalse, invocation.Instance, invocation.Arguments[0].Value, addSymbol, out var dictionaryAddLocation))
            {
                var additionalLocations = ImmutableArray.Create(dictionaryAddLocation);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, invocation.Syntax.GetLocation(), additionalLocations));
            }
            else if (conditional.Condition is IUnaryOperation { OperatorKind: UnaryOperatorKind.Not, Operand: IInvocationOperation i }
                     && IsDictionaryType(i.TargetMethod.ContainingType, iDictionaryType)
                     && DoesSignatureMatch(i.TargetMethod, containsKeySymbol)
                     && TryGetEligibleDictionaryAddLocation(conditional.WhenTrue, i.Instance, i.Arguments[0].Value, addSymbol, out dictionaryAddLocation))
            {
                var additionalLocations = ImmutableArray.Create(dictionaryAddLocation);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, i.Syntax.GetLocation(), additionalLocations));
            }
        }

        private static bool TryGetEligibleDictionaryAddLocation(IOperation? conditionalBody, IOperation dictionaryInstance, IOperation containsKeyArgument, IMethodSymbol addSymbol, [NotNullWhen(true)] out Location? dictionaryAddLocation)
        {
            dictionaryAddLocation = null;
            if (conditionalBody is null)
            {
                return false;
            }

            static bool OperationEquals(SemanticModel semanticModel, IOperation left, IOperation right)
            {
                var leftSymbol = semanticModel.GetSymbolInfo(left.Syntax).Symbol;
                var rightSymbol = semanticModel.GetSymbolInfo(right.Syntax).Symbol;

                return SymbolEqualityComparer.Default.Equals(leftSymbol, rightSymbol);
            }

            bool IsEligibleDictionaryAddInvocation(IInvocationOperation invocation)
            {
                return OperationEquals(invocation.SemanticModel, invocation.Instance, dictionaryInstance)
                       && DoesSignatureMatch(invocation.TargetMethod, addSymbol)
                       && invocation.Arguments[0].Value.Syntax.IsEquivalentTo(containsKeyArgument.Syntax)
                       && invocation.Arguments[1].Value.Kind == OperationKind.Literal;
            }

            if (conditionalBody is IBlockOperation block)
            {
                dictionaryAddLocation = block.Operations.OfType<IExpressionStatementOperation>().FirstOrDefault(e => e.Operation is IInvocationOperation i && IsEligibleDictionaryAddInvocation(i))?.Operation.Syntax.GetLocation();
            }
            else if (conditionalBody is IExpressionStatementOperation { Operation: IInvocationOperation iOperation } && IsEligibleDictionaryAddInvocation(iOperation))
            {
                dictionaryAddLocation = iOperation.Syntax.GetLocation();
            }

            return dictionaryAddLocation is not null;
        }

        private static bool TryGetDictionaryTypeAndMembers(
            Compilation compilation,
            [NotNullWhen(true)] out INamedTypeSymbol? iDictionaryType,
            [NotNullWhen(true)] out IMethodSymbol? containsKeySymbol,
            [NotNullWhen(true)] out IMethodSymbol? addSymbol)
        {
            containsKeySymbol = null;
            addSymbol = null;
            iDictionaryType = WellKnownTypeProvider.GetOrCreate(compilation).GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2);
            if (iDictionaryType is null)
            {
                return false;
            }

            containsKeySymbol = iDictionaryType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == ContainsKey);
            addSymbol = iDictionaryType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == Add);

            return containsKeySymbol is not null && addSymbol is not null;
        }

        private static bool IsDictionaryType(INamedTypeSymbol suspectedDictionaryType, ISymbol iDictionaryType)
        {
            // Either the type is the IDictionary or it is a type which (indirectly) implements it.
            return suspectedDictionaryType.OriginalDefinition.Equals(iDictionaryType, SymbolEqualityComparer.Default)
                   || suspectedDictionaryType.AllInterfaces.Any((@interface, dictionary) => @interface.OriginalDefinition.Equals(dictionary, SymbolEqualityComparer.Default), iDictionaryType);
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
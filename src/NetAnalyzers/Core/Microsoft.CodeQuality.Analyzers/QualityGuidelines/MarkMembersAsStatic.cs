// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA1822: Mark members as static
    /// </summary>
    public abstract class MarkMembersAsStaticAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1822";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.MarkMembersAsStaticTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.MarkMembersAsStaticMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.MarkMembersAsStaticDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected abstract bool SupportStaticLocalFunctions(ParseOptions parseOptions);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Don't report in generated code since that's not actionable.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);

                // Get the list of all method' attributes for which the rule shall not be triggered.
                ImmutableArray<INamedTypeSymbol> skippedAttributes = GetSkippedAttributes(wellKnownTypeProvider);

                var isWebProject = compilationContext.Compilation.IsWebProject(compilationContext.Options);

                compilationContext.RegisterSymbolStartAction(
                    symbolStartContext => OnSymbolStart(symbolStartContext, wellKnownTypeProvider, skippedAttributes, isWebProject),
                    SymbolKind.NamedType);
            });

            return;

            void OnSymbolStart(
                SymbolStartAnalysisContext symbolStartContext,
                WellKnownTypeProvider wellKnownTypeProvider,
                ImmutableArray<INamedTypeSymbol> skippedAttributes,
                bool isWebProject)
            {
                // Since property/event accessors cannot be marked static themselves and the associated symbol (property/event)
                // has to be marked static, we want to report the diagnostic on the property/event.
                // So we make a note of the property/event symbols which have at least one accessor with no instance access.
                // At symbol end, we report candidate property/event symbols whose all accessors are candidates to be marked static.
                var propertyOrEventCandidates = PooledConcurrentSet<ISymbol>.GetInstance();
                var accessorCandidates = PooledConcurrentSet<IMethodSymbol>.GetInstance();

                var methodCandidates = PooledConcurrentSet<IMethodSymbol>.GetInstance();

                // Do not flag methods that are used as delegates: https://github.com/dotnet/roslyn-analyzers/issues/1511
                var methodsUsedAsDelegates = PooledConcurrentSet<IMethodSymbol>.GetInstance();

                symbolStartContext.RegisterOperationAction(OnMethodReference, OperationKind.MethodReference);
                symbolStartContext.RegisterOperationBlockStartAction(OnOperationBlockStart);
                symbolStartContext.RegisterSymbolEndAction(OnSymbolEnd);

                return;

                void OnMethodReference(OperationAnalysisContext operationContext)
                {
                    var methodReference = (IMethodReferenceOperation)operationContext.Operation;
                    methodsUsedAsDelegates.Add(methodReference.Method);
                }

                void OnOperationBlockStart(OperationBlockStartAnalysisContext blockStartContext)
                {
                    if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol ||
                        // Don't run any other check for this method if it isn't a valid analysis context
                        !ShouldAnalyze(methodSymbol, wellKnownTypeProvider, skippedAttributes, isWebProject, blockStartContext))
                    {
                        return;
                    }

                    var localVariables = PooledConcurrentSet<ISymbol>.GetInstance();
                    bool isInstanceReferenced = false;

                    blockStartContext.RegisterOperationAction(operationContext =>
                        CheckHasInstanceReference(operationContext.Operation, ref isInstanceReferenced)
                        , OperationKind.InstanceReference);

                    blockStartContext.RegisterOperationAction(operationContext =>
                        CheckHasReferenceInXml(operationContext.Operation, ref isInstanceReferenced)
                        , OperationKind.None);

                    if (SupportStaticLocalFunctions(blockStartContext.OperationBlocks[0].Syntax.SyntaxTree.Options))
                    {
                        // We want to keep all local declarations because they could be captured by local functions.
                        blockStartContext.RegisterOperationAction(context =>
                            localVariables.Add(((IVariableDeclaratorOperation)context.Operation).Symbol)
                            , OperationKind.VariableDeclarator);

                        // The block start analysis context is not called for local functions and there is no start analysis
                        // alternative so we will have to manually check the children operations.
                        blockStartContext.RegisterOperationAction(context =>
                            AnalyzeLocalFunction(context, wellKnownTypeProvider, localVariables, methodCandidates)
                            , OperationKind.LocalFunction);
                    }

                    blockStartContext.RegisterOperationBlockEndAction(blockEndContext =>
                    {
                        if (isInstanceReferenced)
                        {
                            return;
                        }

                        if (methodSymbol.IsAccessorMethod())
                        {
                            accessorCandidates.Add(methodSymbol);
                            propertyOrEventCandidates.Add(methodSymbol.AssociatedSymbol);
                        }
                        else if (methodSymbol.IsExternallyVisible())
                        {
                            if (!IsOnObsoleteMemberChain(methodSymbol, wellKnownTypeProvider))
                            {
                                blockEndContext.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule, methodSymbol.Name));
                            }
                        }
                        else
                        {
                            methodCandidates.Add(methodSymbol);
                        }
                    });
                }

                void OnSymbolEnd(SymbolAnalysisContext symbolEndContext)
                {
                    foreach (var candidate in methodCandidates)
                    {
                        if (methodsUsedAsDelegates.Contains(candidate))
                        {
                            continue;
                        }

                        if (!IsOnObsoleteMemberChain(candidate, wellKnownTypeProvider))
                        {
                            symbolEndContext.ReportDiagnostic(candidate.CreateDiagnostic(Rule, candidate.Name));
                        }
                    }

                    foreach (var candidatePropertyOrEvent in propertyOrEventCandidates)
                    {
                        var allAccessorsAreCandidates = true;
                        foreach (var accessor in candidatePropertyOrEvent.GetAccessors())
                        {
                            if (!accessorCandidates.Contains(accessor) ||
                                IsOnObsoleteMemberChain(accessor, wellKnownTypeProvider))
                            {
                                allAccessorsAreCandidates = false;
                                break;
                            }
                        }

                        if (allAccessorsAreCandidates)
                        {
                            symbolEndContext.ReportDiagnostic(candidatePropertyOrEvent.CreateDiagnostic(Rule, candidatePropertyOrEvent.Name));
                        }
                    }

                    propertyOrEventCandidates.Free(symbolEndContext.CancellationToken);
                    accessorCandidates.Free(symbolEndContext.CancellationToken);
                    methodCandidates.Free(symbolEndContext.CancellationToken);
                    methodsUsedAsDelegates.Free(symbolEndContext.CancellationToken);
                }
            }
        }

        private static bool ShouldAnalyze(
            IMethodSymbol methodSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            ImmutableArray<INamedTypeSymbol> skippedAttributes,
            bool isWebProject,
#pragma warning disable RS1012 // Start action has no registered actions
            OperationBlockStartAnalysisContext blockStartContext)
#pragma warning restore RS1012 // Start action has no registered actions
        {
            // Modifiers that we don't care about
            // 'PartialImplementationPart is not null' means the method is partial, and the
            // symbol we have is the "definition", not the "implementation".
            // We should only analyze the implementation.
            if (methodSymbol.IsStatic || methodSymbol.IsOverride || methodSymbol.IsVirtual ||
                methodSymbol.IsExtern || methodSymbol.IsAbstract || methodSymbol.PartialImplementationPart is not null ||
                methodSymbol.IsImplementationOfAnyInterfaceMember())
            {
                return false;
            }

            // Do not analyze constructors, finalizers, and indexers.
            if (methodSymbol.IsConstructor() || methodSymbol.IsFinalizer() || methodSymbol.AssociatedSymbol.IsIndexer())
            {
                return false;
            }

            // Don't report methods which have a single throw statement
            // with NotImplementedException or NotSupportedException
            if (blockStartContext.IsMethodNotImplementedOrSupported())
            {
                return false;
            }

            if (methodSymbol.IsExternallyVisible())
            {
                // Do not analyze public APIs for web projects
                // See https://github.com/dotnet/roslyn-analyzers/issues/3835 for details.
                if (isWebProject)
                {
                    return false;
                }

                // CA1000 says one shouldn't declare static members on generic types. So don't flag such cases.
                if (methodSymbol.ContainingType.IsGenericType)
                {
                    return false;
                }
            }

            // We consider that auto-property have the intent to always be instance members so we want to workaround this issue.
            if (methodSymbol.IsAutoPropertyAccessor())
            {
                return false;
            }

            // Awaitable-awaiter pattern members should not be marked as static.
            // There is no need to check for INotifyCompletion or ICriticalNotifyCompletion members as they are already excluded.
            if (wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesINotifyCompletion, out var inotifyCompletionType)
                && wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesICriticalNotifyCompletion, out var icriticalNotifyCompletionType))
            {
                if (methodSymbol.IsGetAwaiterFromAwaitablePattern(inotifyCompletionType, icriticalNotifyCompletionType)
                    || methodSymbol.IsGetResultFromAwaiterPattern(inotifyCompletionType, icriticalNotifyCompletionType))
                {
                    return false;
                }

                if (methodSymbol.AssociatedSymbol is IPropertySymbol property
                    && property.IsIsCompletedFromAwaiterPattern(inotifyCompletionType, icriticalNotifyCompletionType))
                {
                    return false;
                }
            }

            var attributes = methodSymbol.GetAttributes();
            if (methodSymbol.AssociatedSymbol != null)
            {
                // For accessors we want to also check the attributes of the associated symbol
                attributes = attributes.AddRange(methodSymbol.AssociatedSymbol.GetAttributes());
            }

            // FxCop doesn't check for the fully qualified name for these attributes - so we'll do the same.
            if (attributes.Any(attribute => skippedAttributes.Any(attr => attribute.AttributeClass.Inherits(attr))))
            {
                return false;
            }

            if (IsEventHandlerLike(methodSymbol, wellKnownTypeProvider) ||
                IsExplicitlyVisibleFromCom(methodSymbol, wellKnownTypeProvider))
            {
                return false;
            }

            var hasCorrectVisibility = blockStartContext.Options.MatchesConfiguredVisibility(Rule, methodSymbol, wellKnownTypeProvider.Compilation,
                defaultRequiredVisibility: SymbolVisibilityGroup.All);
            if (!hasCorrectVisibility)
            {
                return false;
            }

            return true;
        }

        private static bool ShouldAnalyzeLocalFunction(IMethodSymbol methodSymbol, WellKnownTypeProvider wellKnownTypeProvider, OperationAnalysisContext context)
        {
            Debug.Assert(methodSymbol.MethodKind == MethodKind.LocalFunction);

            if (methodSymbol.IsStatic || IsEventHandlerLike(methodSymbol, wellKnownTypeProvider))
            {
                return false;
            }

            var hasCorrectVisibility = context.Options.MatchesConfiguredVisibility(Rule, methodSymbol, wellKnownTypeProvider.Compilation,
                defaultRequiredVisibility: SymbolVisibilityGroup.All);
            if (!hasCorrectVisibility)
            {
                return false;
            }

            return true;
        }

        // If this looks like an event handler don't flag such cases.
        // However, we do want to consider EventRaise accessor as a candidate
        // so we can flag the associated event if none of it's accessors need instance reference.
        private static bool IsEventHandlerLike(IMethodSymbol methodSymbol, WellKnownTypeProvider wellKnownTypeProvider)
            => methodSymbol.HasEventHandlerSignature(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemEventArgs))
                && methodSymbol.MethodKind != MethodKind.EventRaise;

        private static bool IsExplicitlyVisibleFromCom(IMethodSymbol methodSymbol, WellKnownTypeProvider wellKnownTypeProvider)
        {
            if (!methodSymbol.IsExternallyVisible() || methodSymbol.IsGenericMethod)
            {
                return false;
            }

            var comVisibleAttribute = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesComVisibleAttribute);
            if (comVisibleAttribute == null)
            {
                return false;
            }

            if (methodSymbol.HasAttribute(comVisibleAttribute) ||
                methodSymbol.ContainingType.HasAttribute(comVisibleAttribute))
            {
                return true;
            }

            return false;
        }

        private static ImmutableArray<INamedTypeSymbol> GetSkippedAttributes(WellKnownTypeProvider wellKnownTypeProvider)
        {
            ImmutableArray<INamedTypeSymbol>.Builder? builder = null;

            void Add(INamedTypeSymbol? symbol)
            {
                if (symbol != null)
                {
                    builder ??= ImmutableArray.CreateBuilder<INamedTypeSymbol>();
                    builder.Add(symbol);
                }
            }

            // MSTest attributes
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestInitializeAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestMethodAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingDataTestMethodAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingTestCleanupAttribute));

            // XUnit attributes
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.XunitFactAttribute));

            // NUnit Attributes
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkSetUpAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkInterfacesITestBuilder));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkOneTimeSetUpAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkOneTimeTearDownAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkTestAttribute));
            Add(wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.NUnitFrameworkTearDownAttribute));

            return builder?.ToImmutable() ?? ImmutableArray<INamedTypeSymbol>.Empty;
        }

        private static bool IsOnObsoleteMemberChain(ISymbol symbol, WellKnownTypeProvider wellKnownTypeProvider)
        {
            var obsoleteAttributeType = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemObsoleteAttribute);
            if (obsoleteAttributeType is null)
            {
                return false;
            }

            var allAttributes = new List<AttributeData>();

            while (symbol != null)
            {
                allAttributes.AddRange(symbol.GetAttributes());
                symbol = symbol is IMethodSymbol method && method.AssociatedSymbol != null
                    ? method.AssociatedSymbol :
                    symbol.ContainingSymbol;
            }

            return allAttributes.Any(attribute => attribute.AttributeClass.Equals(obsoleteAttributeType));
        }

        private static void CheckHasInstanceReference(IOperation operation, ref bool isInstanceReferenced)
        {
            Debug.Assert(operation.Kind == OperationKind.InstanceReference);

            if (((IInstanceReferenceOperation)operation).ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
            {
                isInstanceReferenced = true;
            }
        }

        private static void CheckHasReferenceInXml(IOperation operation, ref bool isInstanceReferenced)
        {
            // Workaround for https://github.com/dotnet/roslyn/issues/27564
            if (!operation.IsOperationNoneRoot())
            {
                isInstanceReferenced = true;
            }
        }

        private static void AnalyzeLocalFunction(OperationAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider,
            PooledConcurrentSet<ISymbol> realFunctionVariables, PooledConcurrentSet<IMethodSymbol> methodCandidates)
        {
            Debug.Assert(context.Operation.Kind == OperationKind.LocalFunction);

            var workStack = new Queue<ILocalFunctionOperation>();
            workStack.Enqueue((ILocalFunctionOperation)context.Operation);

            while (workStack.Count > 0)
            {
                var localFunction = workStack.Dequeue();

                if (!ShouldAnalyzeLocalFunction(localFunction.Symbol, wellKnownTypeProvider, context))
                {
                    continue;
                }

                var hasLocalOrInstanceReference = false;
                foreach (var childOperation in localFunction.Descendants())
                {
                    switch (childOperation.Kind)
                    {
                        case OperationKind.None:
                            CheckHasReferenceInXml(childOperation, ref hasLocalOrInstanceReference);
                            break;

                        case OperationKind.InstanceReference:
                            CheckHasInstanceReference(childOperation, ref hasLocalOrInstanceReference);
                            break;

                        case OperationKind.LocalReference:
                            if (realFunctionVariables.Contains(((ILocalReferenceOperation)childOperation).Local))
                            {
                                hasLocalOrInstanceReference = true;
                            }
                            break;

                        // Local functions can contain local functions so we need to queue them for analysis
                        case OperationKind.LocalFunction:
                            workStack.Enqueue((ILocalFunctionOperation)childOperation);
                            break;

                        default:
                            // Do nothing
                            break;
                    }
                }

                if (!hasLocalOrInstanceReference)
                {
                    methodCandidates.Add(localFunction.Symbol);
                }
            }
        }
    }
}

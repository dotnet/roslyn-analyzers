// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using UnusedParameterDictionary = IDictionary<IMethodSymbol, ISet<IParameterSymbol>>;

    /// <summary>
    /// CA1801: Review unused parameters
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ReviewUnusedParametersAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1801";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182268.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider making this analyzer thread-safe.
            //context.EnableConcurrentExecution();

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                INamedTypeSymbol eventsArgSymbol = compilationStartContext.Compilation.GetTypeByMetadataName("System.EventArgs");

                // Ignore conditional methods (FxCop compat - One conditional will often call another conditional method as its only use of a parameter)
                INamedTypeSymbol conditionalAttributeSymbol = WellKnownTypes.ConditionalAttribute(compilationStartContext.Compilation);

                // Ignore methods with special serialization attributes (FxCop compat - All serialization methods need to take 'StreamingContext')
                INamedTypeSymbol onDeserializingAttribute = WellKnownTypes.OnDeserializingAttribute(compilationStartContext.Compilation);
                INamedTypeSymbol onDeserializedAttribute = WellKnownTypes.OnDeserializedAttribute(compilationStartContext.Compilation);
                INamedTypeSymbol onSerializingAttribute = WellKnownTypes.OnSerializingAttribute(compilationStartContext.Compilation);
                INamedTypeSymbol onSerializedAttribute = WellKnownTypes.OnSerializedAttribute(compilationStartContext.Compilation);
                INamedTypeSymbol obsoleteAttribute = WellKnownTypes.ObsoleteAttribute(compilationStartContext.Compilation);

                ImmutableHashSet<INamedTypeSymbol> attributeSetForMethodsToIgnore = ImmutableHashSet.Create(
                    conditionalAttributeSymbol,
                    onDeserializedAttribute,
                    onDeserializingAttribute,
                    onSerializedAttribute,
                    onSerializingAttribute,
                    obsoleteAttribute);

                ImmutableHashSet<INamedTypeSymbol> exceptionsToSkip = ImmutableHashSet.Create(
                    WellKnownTypes.NotImplementedException(compilationStartContext.Compilation),
                    WellKnownTypes.NotSupportedException(compilationStartContext.Compilation));

                UnusedParameterDictionary unusedMethodParameters = new ConcurrentDictionary<IMethodSymbol, ISet<IParameterSymbol>>();
                ISet<IMethodSymbol> methodsUsedAsDelegates = new HashSet<IMethodSymbol>();

                // Create a list of functions to exclude from analysis. We assume that any function that is used in an IMethodBindingExpression
                // cannot have its signature changed, and add it to the list of methods to be excluded from analysis.
                compilationStartContext.RegisterOperationAction(operationContext =>
                {
                    var methodBinding = (IMethodReferenceOperation)operationContext.Operation;
                    methodsUsedAsDelegates.Add(methodBinding.Method.OriginalDefinition);
                }, OperationKind.MethodReference);

                compilationStartContext.RegisterOperationBlockStartAction(startOperationBlockContext =>
                {
                    // We only care about methods.
                    if (startOperationBlockContext.OwningSymbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    // We only care about methods with parameters.
                    var method = (IMethodSymbol)startOperationBlockContext.OwningSymbol;
                    if (method.Parameters.IsEmpty)
                    {
                        return;
                    }

                    // Ignore implicitly declared methods, extern methods, abstract methods, virtual methods, interface implementations and finalizers (FxCop compat).
                    if (method.IsImplicitlyDeclared ||
                        method.IsExtern ||
                        method.IsAbstract ||
                        method.IsVirtual ||
                        method.IsOverride ||
                        method.IsImplementationOfAnyInterfaceMember() ||
                        method.IsFinalizer())
                    {
                        return;
                    }

                    // Ignore property accessors.
                    if (method.IsPropertyAccessor())
                    {
                        return;
                    }

                    // Ignore event handler methods "Handler(object, MyEventArgs)"
                    if (eventsArgSymbol != null &&
                        method.Parameters.Length == 2 &&
                        method.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                        method.Parameters[1].Type.Inherits(eventsArgSymbol))
                    {
                        return;
                    }

                    // Ignore methods with any attributes in 'attributeSetForMethodsToIgnore'.
                    if (method.GetAttributes().Any(a => a.AttributeClass != null && attributeSetForMethodsToIgnore.Contains(a.AttributeClass)))
                    {
                        return;
                    }

                    // Ignore methods that were used as delegates
                    if (methodsUsedAsDelegates.Contains(method))
                    {
                        return;
                    }

                    // Initialize local mutable state in the start action.
                    var analyzer = new UnusedParametersAnalyzer(method, unusedMethodParameters, exceptionsToSkip);

                    // Register an intermediate non-end action that accesses and modifies the state.
                    startOperationBlockContext.RegisterOperationAction(analyzer.AnalyzeOperation, OperationKind.ParameterReference);

                    // Register an end action to add unused parameters to the unusedMethodParameters dictionary
                    startOperationBlockContext.RegisterOperationBlockEndAction(analyzer.OperationBlockEndAction);
                });

                // Register a compilation end action to filter all methods used as delegates and report any diagnostics
                compilationStartContext.RegisterCompilationEndAction(compilationAnalysisContext =>
                {
                    // Report diagnostics for unused parameters.
                    var unusedParameters = unusedMethodParameters.Where(kvp => !methodsUsedAsDelegates.Contains(kvp.Key)).SelectMany(kvp => kvp.Value);
                    foreach (var parameter in unusedParameters)
                    {
                        var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, parameter.ContainingSymbol.Name);
                        compilationAnalysisContext.ReportDiagnostic(diagnostic);
                    }

                });
            });
        }

        private class UnusedParametersAnalyzer
        {
            #region Per-CodeBlock mutable state

            private readonly ImmutableHashSet<INamedTypeSymbol> _exceptionsToSkip;
            private readonly HashSet<IParameterSymbol> _unusedParameters;
            private readonly UnusedParameterDictionary _finalUnusedParameters;
            private readonly IMethodSymbol _method;

            #endregion

            #region State intialization

            public UnusedParametersAnalyzer(IMethodSymbol method, UnusedParameterDictionary finalUnusedParameters, ImmutableHashSet<INamedTypeSymbol> exceptionsToSkip)
            {
                // Initialization: Assume all parameters are unused.
                _unusedParameters = new HashSet<IParameterSymbol>(method.Parameters);
                _exceptionsToSkip = exceptionsToSkip;
                _finalUnusedParameters = finalUnusedParameters;
                _method = method;
            }

            #endregion

            #region Intermediate actions

            public void AnalyzeOperation(OperationAnalysisContext context)
            {
                // Check if we have any pending unreferenced parameters.
                if (_unusedParameters.Count == 0)
                {
                    return;
                }

                // Mark this parameter as used.
                IParameterSymbol parameter = ((IParameterReferenceOperation)context.Operation).Parameter;
                _unusedParameters.Remove(parameter);
            }

            #endregion

            #region End action

            public void OperationBlockEndAction(OperationBlockAnalysisContext context)
            {
                // Check to see if the method just throws a NotImplementedException/NotSupportedException. If it does,
                // we shouldn't warn about parameters.
                // Note that VB method bodies with 1 action have 3 operations.
                // The first is the actual operation, the second is a label statement, and the third is a return
                // statement. The last two are implicit in these scenarios.

                // Filter out operation roots with no IOperation API support (OperationKind.None)
                var operationBlocks = context.OperationBlocks;
                if (operationBlocks.Any(operation => operation.IsOperationNoneRoot()))
                {
                    operationBlocks = operationBlocks.Where(operation => !operation.IsOperationNoneRoot()).ToImmutableArray();
                }

                // In the presence of parameter initializers, there will be multiple operation blocks. We assume that there
                // is only one IBlockOperation, and that the rest are something else
                Debug.Assert(!(operationBlocks.Where(op => op.Kind == OperationKind.Block).Count() > 1));

                IBlockOperation methodBlock = null;
                if (operationBlocks.Length == 1 && operationBlocks[0].Kind == OperationKind.Block)
                {
                    methodBlock = (IBlockOperation)operationBlocks[0];
                }
                else if (operationBlocks.Length > 1)
                {
                    foreach (var block in operationBlocks)
                    {
                        if (block.Kind == OperationKind.Block)
                        {
                            methodBlock = (IBlockOperation)block;
                            break;
                        }
                    }
                }

                if (methodBlock != null)
                {
                    bool IsSingleStatementBody(IBlockOperation body)
                    {
                        return body.Operations.Length == 1 ||
                            (body.Operations.Length == 3 && body.Syntax.Language == LanguageNames.VisualBasic &&
                             body.Operations[1] is ILabeledOperation labeledOp && labeledOp.IsImplicit &&
                             body.Operations[2] is IReturnOperation returnOp && returnOp.IsImplicit);
                    }

                    if (IsSingleStatementBody(methodBlock))
                    {
                        var innerOperation = methodBlock.Operations.First();

                        // Because of https://github.com/dotnet/roslyn/issues/23152, there can be an expression-statement
                        // wrapping expression-bodied throw operations. Compensate by unwrapping if necessary.
                        if (innerOperation.Kind == OperationKind.ExpressionStatement &&
                            innerOperation is IExpressionStatementOperation exprStatement)
                        {
                            innerOperation = exprStatement.Operation;
                        }

                        if (innerOperation.Kind == OperationKind.Throw &&
                            innerOperation is IThrowOperation throwOperation &&
                            throwOperation.Exception.Kind == OperationKind.ObjectCreation &&
                            throwOperation.Exception is IObjectCreationOperation createdException)
                        {
                            if (_exceptionsToSkip.Contains(createdException.Type.OriginalDefinition))
                            {
                                return;
                            }
                        }
                    }
                }

                // Do not raise warning for unused 'this' parameter of an extension method.
                if (_method.IsExtensionMethod)
                {
                    var thisParamter = _unusedParameters.Where(p => p.Ordinal == 0).FirstOrDefault();
                    _unusedParameters.Remove(thisParamter);
                }

                _finalUnusedParameters.Add(_method, _unusedParameters);
            }

            #endregion
        }
    }
}
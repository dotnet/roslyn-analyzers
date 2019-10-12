// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class CodeMayHaveMainThreadDependency : AbstractThreadDependencyAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.CodeMayHaveMainThreadDependencyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void HandleCompilationStart(CompilationStartAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, INamedTypeSymbol threadDependencyAttribute)
        {
            context.RegisterOperationBlockStartAction(context => HandleOperationBlockStart(context, wellKnownTypeProvider));
        }

        private void HandleOperationBlockStart(OperationBlockStartAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider)
        {
            var threadDependencyInfo = GetThreadDependencyInfo(context.OwningSymbol);
            if (!threadDependencyInfo.IsExplicit || !threadDependencyInfo.Verified)
            {
                return;
            }

            var threadDependencyInfoForReturn = context.OwningSymbol is IMethodSymbol methodSymbol
                ? GetThreadDependencyInfoForReturn(wellKnownTypeProvider, methodSymbol)
                : threadDependencyInfo;

            context.RegisterOperationAction(context => HandleReturnOperation(context, wellKnownTypeProvider, threadDependencyInfoForReturn), OperationKind.Return);
            context.RegisterOperationAction(ctx => HandleAwaitOperation(ctx, wellKnownTypeProvider, threadDependencyInfoForReturn), OperationKind.Await);

            context.RegisterOperationBlockEndAction(context => HandleOperationBlockEnd(context, threadDependencyInfo, threadDependencyInfoForReturn));
        }

        private void HandleOperationBlockEnd(OperationBlockAnalysisContext context, ThreadDependencyInfo threadDependencyInfo, ThreadDependencyInfo threadDependencyInfoForReturn)
        {
            foreach (var operationBlock in context.OperationBlocks)
            {
                var controlFlowGraph = context.GetControlFlowGraph(operationBlock);
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                var threadDependencyAnalysisResult = ThreadDependencyAnalysis.TryGetOrComputeResult(
                    controlFlowGraph,
                    context.OwningSymbol,
                    context.Options,
                    wellKnownTypeProvider,
                    Rule,
                    context.CancellationToken);
                if (threadDependencyAnalysisResult is null)
                {
                    continue;
                }

                bool isAsync = context.OwningSymbol is IMethodSymbol { IsAsync: true };
                foreach (var operation in controlFlowGraph.DescendantOperations())
                {
                    var operationValue = threadDependencyAnalysisResult[operation];
                    if (operationValue is null || operationValue.YieldKind == ThreadDependencyAnalysis.YieldKind.Unknown)
                    {
                        throw new InvalidOperationException();
                    }

                    if (operation is IReturnOperation returnOperation)
                    {
                        if (isAsync)
                        {
                            // TODO: Validate the returned value against the constraints of the type parameter of the return
                        }
                        else
                        {
                            if (threadDependencyInfoForReturn.AlwaysCompleted && threadDependencyAnalysisResult[returnOperation.ReturnedValue].AlwaysComplete != true)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    Rule,
                                    operation.Syntax.GetLocation(),
                                    GetAdditionalLocations(context.OwningSymbol, returnOperation, context.CancellationToken),
                                    properties: ScenarioProperties.WithAlwaysCompleted(ScenarioProperties.TargetMissingAttribute)));
                            }
                        }
                    }
                    else if (operationValue.YieldKind != ThreadDependencyAnalysis.YieldKind.NotYielded && operation is IAwaitOperation awaitOperation)
                    {
                        if (threadDependencyAnalysisResult[awaitOperation.Operation].YieldKind == ThreadDependencyAnalysis.YieldKind.NotYielded)
                        {
                            // This await operation is the first potential yield in the control flow
                            if (threadDependencyInfoForReturn.AlwaysCompleted)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    Rule,
                                    operation.Syntax.GetLocation(),
                                    GetAdditionalLocations(context.OwningSymbol, awaitOperation, context.CancellationToken),
                                    properties: ScenarioProperties.WithAlwaysCompleted(ScenarioProperties.TargetMissingAttribute)));
                            }
                        }
                    }
                }
            }
        }

        private void HandleReturnOperation(OperationAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, ThreadDependencyInfo threadDependencyInfo)
        {
            var returnOperation = (IReturnOperation)context.Operation;
            if (returnOperation.ReturnedValue is null || returnOperation.IsImplicit)
            {
                return;
            }

            if (context.ContainingSymbol is IMethodSymbol method && method.IsAsync)
            {
                // TODO: Validate the returned value against the constraints of the type parameter of the return
                return;
            }

            var valueThreadDependencyInfo = GetThreadDependencyInfo(wellKnownTypeProvider, returnOperation.ReturnedValue, captureContextUnlessConfigured: false);
            if (valueThreadDependencyInfo.AlwaysCompleted)
            {
                return;
            }

            ImmutableDictionary<string, string> propertiesOverride = null;
            if (!valueThreadDependencyInfo.IsExplicit)
            {
                propertiesOverride = ScenarioProperties.TargetMissingAttribute;
                if (threadDependencyInfo.CapturesContext)
                {
                    propertiesOverride = ScenarioProperties.WithCapturesContext(propertiesOverride);
                }

                if (IsReceiverMarkedPerInstance(wellKnownTypeProvider, returnOperation.ReturnedValue))
                {
                    propertiesOverride = ScenarioProperties.WithPerInstance(propertiesOverride);
                }
            }

            if (!valueThreadDependencyInfo.AlwaysCompleted)
            {
                if (threadDependencyInfo.AlwaysCompleted)
                {
                    //context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: propertiesOverride));
                    return;
                }

                if (valueThreadDependencyInfo.CapturesContext && !threadDependencyInfo.CapturesContext)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: propertiesOverride ?? ScenarioProperties.ContainingMethodShouldCaptureContext));
                    return;
                }
            }

            if (valueThreadDependencyInfo.MayDirectlyRequireMainThread && !threadDependencyInfo.MayDirectlyRequireMainThread)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: propertiesOverride));
                return;
            }

            if (valueThreadDependencyInfo.PerInstance && !threadDependencyInfo.PerInstance)
            {
                var properties = propertiesOverride;
                var locationSyntax = context.Operation.Syntax;
                if (properties is null && !IsReceiverMarkedPerInstance(wellKnownTypeProvider, returnOperation.ReturnedValue))
                {
                    var receiverOperation = GetReceiver(returnOperation.ReturnedValue);
                    if (receiverOperation is object && !HasExplicitThreadDependencyInfo(wellKnownTypeProvider, receiverOperation))
                    {
                        locationSyntax = receiverOperation.Syntax;
                        properties = ScenarioProperties.TargetMissingAttribute;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, locationSyntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: properties));
                return;
            }
        }

        private void HandleAwaitOperation(OperationAnalysisContext context, WellKnownTypeProvider wellKnownTypeProvider, ThreadDependencyInfo threadDependencyInfo)
        {
            var awaitOperation = (IAwaitOperation)context.Operation;
            var valueThreadDependencyInfo = GetThreadDependencyInfo(wellKnownTypeProvider, awaitOperation.Operation, captureContextUnlessConfigured: true);

            if (valueThreadDependencyInfo.AlwaysCompleted)
            {
                return;
            }

            ImmutableDictionary<string, string> propertiesOverride = null;
            if (!valueThreadDependencyInfo.IsExplicit)
            {
                propertiesOverride = ScenarioProperties.TargetMissingAttribute;
                if (threadDependencyInfo.CapturesContext)
                {
                    propertiesOverride = ScenarioProperties.WithCapturesContext(propertiesOverride);
                }

                if (IsReceiverMarkedPerInstance(wellKnownTypeProvider, awaitOperation.Operation))
                {
                    propertiesOverride = ScenarioProperties.WithPerInstance(propertiesOverride);
                }
            }

            if (!valueThreadDependencyInfo.AlwaysCompleted)
            {
                if (threadDependencyInfo.AlwaysCompleted)
                {
                    //context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: propertiesOverride));
                    return;
                }

                if (valueThreadDependencyInfo.CapturesContext && !threadDependencyInfo.CapturesContext)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: propertiesOverride ?? ScenarioProperties.ContainingMethodShouldCaptureContext));
                    return;
                }
            }

            if (valueThreadDependencyInfo.MayDirectlyRequireMainThread && !threadDependencyInfo.MayDirectlyRequireMainThread)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: propertiesOverride));
                return;
            }

            if (valueThreadDependencyInfo.PerInstance)
            {
                var properties = propertiesOverride;
                var locationSyntax = context.Operation.Syntax;
                if (properties is null && !IsReceiverMarkedPerInstance(wellKnownTypeProvider, awaitOperation.Operation))
                {
                    var receiverOperation = GetReceiver(awaitOperation.Operation);
                    if (receiverOperation is object && !HasExplicitThreadDependencyInfo(wellKnownTypeProvider, receiverOperation))
                    {
                        locationSyntax = receiverOperation.Syntax;
                        properties = ScenarioProperties.TargetMissingAttribute;
                        if (threadDependencyInfo.PerInstance)
                        {
                            properties = ScenarioProperties.WithPerInstance(properties);
                        }
                    }
                }

                if (properties is object || !threadDependencyInfo.PerInstance)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, locationSyntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: properties));
                }

                return;
            }
        }

        private ThreadDependencyInfo GetThreadDependencyInfo(WellKnownTypeProvider wellKnownTypeProvider, IOperation operation, bool captureContextUnlessConfigured)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfoForReturn(wellKnownTypeProvider, conversion.OperatorMethod);
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    var instanceDependencyInfo = GetThreadDependencyInfo(wellKnownTypeProvider, invocation.Instance, captureContextUnlessConfigured: false);
                    if (!instanceDependencyInfo.CapturesContext
                        && invocation.Arguments.Length == 1
                        && invocation.Arguments[0].Value.TryGetBoolConstantValue(out var continueOnCapturedContext)
                        && continueOnCapturedContext)
                    {
                        instanceDependencyInfo = instanceDependencyInfo.WithCapturesContext(true);
                    }

                    return instanceDependencyInfo;
                }
                else
                {
                    var targetDependencyInfo = GetThreadDependencyInfoForReturn(wellKnownTypeProvider, invocation.TargetMethod);
                    if (targetDependencyInfo.PerInstance)
                    {
                        var instanceDependencyInfo = GetThreadDependencyInfo(wellKnownTypeProvider, invocation.Instance, captureContextUnlessConfigured: false);
                        if (instanceDependencyInfo.IsExplicit && !instanceDependencyInfo.MayHaveMainThreadDependency)
                        {
                            if (!instanceDependencyInfo.MayHaveMainThreadDependency)
                            {
                                targetDependencyInfo = targetDependencyInfo.WithPerInstance(instanceDependencyInfo.PerInstance);
                            }
                            else
                            {
                                targetDependencyInfo = ThreadDependencyInfo.DefaultAsynchronous;
                            }
                        }
                    }

                    if (captureContextUnlessConfigured)
                    {
                        targetDependencyInfo = targetDependencyInfo.WithCapturesContext(true);
                    }

                    return targetDependencyInfo;
                }
            }

            if (operation is IParameterReferenceOperation parameterReference)
            {
                var parameterDependencyInfo = GetThreadDependencyInfo(parameterReference.Parameter);
                if (captureContextUnlessConfigured)
                {
                    parameterDependencyInfo = parameterDependencyInfo.WithCapturesContext(true);
                }

                return parameterDependencyInfo;
            }

            if (operation is IFieldReferenceOperation fieldReference)
            {
                var fieldDependencyInfo = GetThreadDependencyInfo(fieldReference.Field);
                if (captureContextUnlessConfigured)
                {
                    fieldDependencyInfo = fieldDependencyInfo.WithCapturesContext(true);
                }

                return fieldDependencyInfo;
            }

            if (operation is IPropertyReferenceOperation propertyReference)
            {
                var propertyDependencyInfo = GetThreadDependencyInfo(propertyReference.Property);
                if (captureContextUnlessConfigured)
                {
                    propertyDependencyInfo = propertyDependencyInfo.WithCapturesContext(true);
                }

                return propertyDependencyInfo;
            }

            if (operation is IAwaitOperation awaitOperation)
            {
                var awaitDependencyInfo = GetThreadDependencyInfo(wellKnownTypeProvider, awaitOperation.Operation, captureContextUnlessConfigured: true);
                if (awaitDependencyInfo.AlwaysCompleted)
                {
                    return awaitDependencyInfo;
                }
            }

            return ThreadDependencyInfo.DefaultAsynchronous;
        }

        private IOperation GetReceiver(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetReceiver(conversion.Operand);
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    return GetReceiver(invocation.Instance);
                }
                else
                {
                    return GetReceiver(invocation.Instance);
                }
            }

            if (operation is IParameterReferenceOperation || operation is IFieldReferenceOperation || operation is IPropertyReferenceOperation)
            {
                return operation;
            }

            return null;
        }

        private bool IsReceiverMarkedPerInstance(WellKnownTypeProvider wellKnownTypeProvider, IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfo(wellKnownTypeProvider, operation, captureContextUnlessConfigured: false).PerInstance;
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    return IsReceiverMarkedPerInstance(wellKnownTypeProvider, invocation.Instance);
                }
                else
                {
                    return HasExplicitThreadDependencyInfo(wellKnownTypeProvider, invocation.Instance);
                }
            }

            if (operation is IParameterReferenceOperation || operation is IFieldReferenceOperation || operation is IPropertyReferenceOperation)
            {
                return false;
            }

            return false;
        }

        private bool HasExplicitThreadDependencyInfo(WellKnownTypeProvider wellKnownTypeProvider, IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfo(wellKnownTypeProvider, operation, captureContextUnlessConfigured: false).IsExplicit;
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    return HasExplicitThreadDependencyInfo(wellKnownTypeProvider, invocation.Instance);
                }
                else
                {
                    return GetThreadDependencyInfo(wellKnownTypeProvider, invocation.Instance, captureContextUnlessConfigured: false).IsExplicit;
                }
            }

            return GetThreadDependencyInfo(wellKnownTypeProvider, operation, captureContextUnlessConfigured: false).IsExplicit;
        }

        private static IEnumerable<Location> GetAdditionalLocations(ISymbol containingSymbol, IOperation operation, CancellationToken cancellationToken)
        {
            if (operation is IAwaitOperation || operation is IReturnOperation)
            {
                if (containingSymbol is IMethodSymbol method)
                {
                    var returnLocation = TryGetThreadDependencyInfoLocationForReturn(method, cancellationToken);
                    if (returnLocation is object)
                    {
                        return new[] { returnLocation };
                    }
                }
            }

            var location = TryGetThreadDependencyInfoLocation(containingSymbol, cancellationToken);
            if (location is object)
            {
                return new[] { location };
            }

            return Enumerable.Empty<Location>();
        }

        internal static class Scenario
        {
            public const string ContainingMethodShouldCaptureContext = nameof(ContainingMethodShouldCaptureContext);
            public const string ContainingMethodShouldBePerInstance = nameof(ContainingMethodShouldBePerInstance);
            public const string TargetMissingAttribute = nameof(TargetMissingAttribute);
        }

        private static class ScenarioProperties
        {
            public static readonly ImmutableDictionary<string, string> ContainingMethodShouldCaptureContext = ImmutableDictionary.Create<string, string>().Add(nameof(Scenario), nameof(ContainingMethodShouldCaptureContext));
            public static readonly ImmutableDictionary<string, string> ContainingMethodShouldBePerInstance = ImmutableDictionary.Create<string, string>().Add(nameof(Scenario), nameof(ContainingMethodShouldBePerInstance));
            public static readonly ImmutableDictionary<string, string> TargetMissingAttribute = ImmutableDictionary.Create<string, string>().Add(nameof(Scenario), nameof(TargetMissingAttribute));

            public static ImmutableDictionary<string, string> WithCapturesContext(ImmutableDictionary<string, string> properties)
                => properties.SetItem(nameof(ContextDependency), nameof(ContextDependency.Context));

            public static ImmutableDictionary<string, string> WithAlwaysCompleted(ImmutableDictionary<string, string> properties)
                => properties.SetItem("AlwaysCompleted", bool.TrueString);

            public static ImmutableDictionary<string, string> WithPerInstance(ImmutableDictionary<string, string> properties)
                => properties.SetItem("PerInstance", bool.TrueString);
        }
    }
}

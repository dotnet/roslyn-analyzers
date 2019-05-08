﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

        protected override void HandleCompilationStart(CompilationStartAnalysisContext context, INamedTypeSymbol threadDependencyAttribute)
        {
            context.RegisterOperationBlockStartAction(HandleOperationBlockStart);
        }

        private void HandleOperationBlockStart(OperationBlockStartAnalysisContext context)
        {
            var threadDependencyInfo = GetThreadDependencyInfo(context.OwningSymbol);
            if (!threadDependencyInfo.IsExplicit || !threadDependencyInfo.Verified)
            {
                return;
            }

            context.RegisterOperationAction(context => HandleReturnOperation(context, threadDependencyInfo), OperationKind.Return);
            context.RegisterOperationAction(ctx => HandleAwaitOperation(ctx, threadDependencyInfo), OperationKind.Await);
        }

        private void HandleReturnOperation(OperationAnalysisContext context, ThreadDependencyInfo threadDependencyInfo)
        {
            var returnOperation = (IReturnOperation)context.Operation;
            if (returnOperation.ReturnedValue is null || returnOperation.IsImplicit)
            {
                return;
            }

            var valueThreadDependencyInfo = GetThreadDependencyInfo(returnOperation.ReturnedValue, captureContextUnlessConfigured: false);
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

                if (IsReceiverMarkedPerInstance(returnOperation.ReturnedValue))
                {
                    propertiesOverride = ScenarioProperties.WithPerInstance(propertiesOverride);
                }
            }

            if (!valueThreadDependencyInfo.AlwaysCompleted)
            {
                if (threadDependencyInfo.AlwaysCompleted)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: propertiesOverride));
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
                if (properties is null && !IsReceiverMarkedPerInstance(returnOperation.ReturnedValue))
                {
                    var receiverOperation = GetReceiver(returnOperation.ReturnedValue);
                    if (receiverOperation is object && !HasExplicitThreadDependencyInfo(receiverOperation))
                    {
                        locationSyntax = receiverOperation.Syntax;
                        properties = ScenarioProperties.TargetMissingAttribute;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, locationSyntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, returnOperation, context.CancellationToken), properties: properties));
                return;
            }
        }

        private void HandleAwaitOperation(OperationAnalysisContext context, ThreadDependencyInfo threadDependencyInfo)
        {
            var awaitOperation = (IAwaitOperation)context.Operation;
            var valueThreadDependencyInfo = GetThreadDependencyInfo(awaitOperation.Operation, captureContextUnlessConfigured: true);

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

                if (IsReceiverMarkedPerInstance(awaitOperation.Operation))
                {
                    propertiesOverride = ScenarioProperties.WithPerInstance(propertiesOverride);
                }
            }

            if (!valueThreadDependencyInfo.AlwaysCompleted)
            {
                if (threadDependencyInfo.AlwaysCompleted)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: propertiesOverride));
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

            if (valueThreadDependencyInfo.PerInstance && !threadDependencyInfo.PerInstance)
            {
                var properties = propertiesOverride;
                var locationSyntax = context.Operation.Syntax;
                if (properties is null && !IsReceiverMarkedPerInstance(awaitOperation.Operation))
                {
                    var receiverOperation = GetReceiver(awaitOperation.Operation);
                    if (receiverOperation is object && !HasExplicitThreadDependencyInfo(receiverOperation))
                    {
                        locationSyntax = receiverOperation.Syntax;
                        properties = ScenarioProperties.TargetMissingAttribute;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, locationSyntax.GetLocation(), GetAdditionalLocations(context.ContainingSymbol, awaitOperation, context.CancellationToken), properties: properties));
                return;
            }
        }

        private ThreadDependencyInfo GetThreadDependencyInfo(IOperation operation, bool captureContextUnlessConfigured)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfoForReturn(conversion.OperatorMethod);
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    var instanceDependencyInfo = GetThreadDependencyInfo(invocation.Instance, captureContextUnlessConfigured: false);
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
                    var targetDependencyInfo = GetThreadDependencyInfoForReturn(invocation.TargetMethod);
                    if (targetDependencyInfo.PerInstance)
                    {
                        var instanceDependencyInfo = GetThreadDependencyInfo(invocation.Instance, captureContextUnlessConfigured: false);
                        if (instanceDependencyInfo.IsExplicit && !instanceDependencyInfo.MayHaveMainThreadDependency)
                        {
                            targetDependencyInfo = targetDependencyInfo.WithPerInstance(false);
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

            if (operation is IParameterReferenceOperation)
            {
                return operation;
            }

            return null;
        }

        private bool IsReceiverMarkedPerInstance(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfo(operation, captureContextUnlessConfigured: false).PerInstance;
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    return IsReceiverMarkedPerInstance(invocation.Instance);
                }
                else
                {
                    return HasExplicitThreadDependencyInfo(invocation.Instance);
                }
            }

            if (operation is IParameterReferenceOperation)
            {
                return false;
            }

            return false;
        }

        private bool HasExplicitThreadDependencyInfo(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfo(operation, captureContextUnlessConfigured: false).IsExplicit;
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    return HasExplicitThreadDependencyInfo(invocation.Instance);
                }
                else
                {
                    return GetThreadDependencyInfo(invocation.Instance, captureContextUnlessConfigured: false).IsExplicit;
                }
            }

            return GetThreadDependencyInfo(operation, captureContextUnlessConfigured: false).IsExplicit;
        }

        private IEnumerable<Location> GetAdditionalLocations(ISymbol containingSymbol, IOperation operation, CancellationToken cancellationToken)
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
                => properties.SetItem("AlwaysCompleted", "true");

            public static ImmutableDictionary<string, string> WithPerInstance(ImmutableDictionary<string, string> properties)
                => properties.SetItem("PerInstance", bool.TrueString);
        }
    }
}

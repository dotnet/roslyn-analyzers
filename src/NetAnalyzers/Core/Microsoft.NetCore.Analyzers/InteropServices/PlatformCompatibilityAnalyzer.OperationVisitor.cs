// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    public sealed partial class PlatformCompatibilityAnalyzer
    {
        private sealed class OperationVisitor : GlobalFlowStateDataFlowOperationVisitor
        {
            private readonly ImmutableArray<IMethodSymbol> _platformCheckMethods;
            private readonly INamedTypeSymbol? _osPlatformType;

            public OperationVisitor(
                ImmutableArray<IMethodSymbol> platformCheckMethods,
                INamedTypeSymbol? osPlatformType,
                GlobalFlowStateAnalysisContext analysisContext)
                : base(analysisContext, hasPredicatedGlobalState: true)
            {
                _platformCheckMethods = platformCheckMethods;
                _osPlatformType = osPlatformType;
            }

            internal static bool TryParseGuardAttrbiutes(ISymbol symbol, ref GlobalFlowStateAnalysisValueSet value)
            {
                if (HasAnyGuardAttribute(symbol))
                {
                    using var infosBuilder = ArrayBuilder<PlatformMethodValue>.GetInstance();
                    if (PlatformMethodValue.TryDecode(symbol.GetAttributes(), infosBuilder))
                    {
                        for (var i = 0; i < infosBuilder.Count; i++)
                        {
                            var newValue = GlobalFlowStateAnalysisValueSet.Create(infosBuilder[i]);
                            value = i == 0 ? newValue : infosBuilder[i].Negated ? value.WithAdditionalAnalysisValues(newValue, false) :
                                GlobalFlowStateAnalysis.GlobalFlowStateAnalysisValueSetDomain.Instance.Merge(value, newValue);
                        }

                        return true;
                    }

                    value = GlobalFlowStateAnalysisValueSet.Unknown;
                }

                return false;

                static bool HasAnyGuardAttribute(ISymbol symbol) =>
                    symbol.GetAttributes().Any(a => a.AttributeClass.Name is SupportedOSPlatformGuardAttribute or UnsupportedOSPlatformGuardAttribute);
            }

            public override GlobalFlowStateAnalysisValueSet VisitInvocation_NonLambdaOrDelegateOrLocalFunction(
                IMethodSymbol method,
                IOperation? visitedInstance,
                ImmutableArray<IArgumentOperation> visitedArguments,
                bool invokedAsDelegate,
                IOperation originalOperation,
                GlobalFlowStateAnalysisValueSet defaultValue)
            {
                var value = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(method, visitedInstance, visitedArguments, invokedAsDelegate, originalOperation, defaultValue);

                if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
                {
                    if (_platformCheckMethods.Contains(method.OriginalDefinition))
                    {
                        using var infosBuilder = ArrayBuilder<PlatformMethodValue>.GetInstance();
                        if (PlatformMethodValue.TryDecode(method, visitedArguments, DataFlowAnalysisContext.ValueContentAnalysisResult, _osPlatformType, infosBuilder))
                        {
                            for (var i = 0; i < infosBuilder.Count; i++)
                            {
                                var newValue = GlobalFlowStateAnalysisValueSet.Create(infosBuilder[i]);
                                value = i == 0 ? newValue : GlobalFlowStateAnalysis.GlobalFlowStateAnalysisValueSetDomain.Instance.Merge(value, newValue);
                            }

                            return value;
                        }

                        return GlobalFlowStateAnalysisValueSet.Unknown;
                    }
                    else if (TryParseGuardAttrbiutes(method, ref value))
                    {
                        return value;
                    }
                }

                return value;
            }

            public override GlobalFlowStateAnalysisValueSet VisitFieldReference(IFieldReferenceOperation operation, object? argument)
            {
                var value = base.VisitFieldReference(operation, argument);

                if (operation.Type.SpecialType == SpecialType.System_Boolean &&
                    TryParseGuardAttrbiutes(operation.Field, ref value))
                {
                    return value;
                }

                return ComputeAnalysisValueForReferenceOperation(operation, value);
            }

            public override GlobalFlowStateAnalysisValueSet VisitPropertyReference(IPropertyReferenceOperation operation, object? argument)
            {
                var value = base.VisitPropertyReference(operation, argument);
                if (operation.Type.SpecialType == SpecialType.System_Boolean &&
                    TryParseGuardAttrbiutes(operation.Property, ref value))
                {
                    return value;
                }

                return ComputeAnalysisValueForReferenceOperation(operation, value);
            }
        }
    }
}

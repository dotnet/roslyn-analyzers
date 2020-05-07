// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CAxxxx: Do not call Enumerable.Cast that will fail at runtime
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallEnumerableCastThatWillFailAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA9999";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerableCastThatWillFailTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerableCastThatWillFailMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: null,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        private static readonly ImmutableDictionary<string, string> methodMetadataNames = new Dictionary<string, string>
        {
            ["OfType"] = "System.Linq.Enumerable.OfType`1",
            ["Cast"] = "System.Linq.Enumerable.Cast`1",
        }.ToImmutableDictionary();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var enumerableType = compilationStartContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable);
                var genericIEnumerableType = compilationStartContext.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIEnumerable1);

                if (enumerableType == null || genericIEnumerableType == null)
                {
                    return;
                }

                compilationStartContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;

                    var targetMethod = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;
                    if (targetMethod == null || !methodMetadataNames.TryGetValue(targetMethod.Name, out string metadataName))
                    {
                        return;
                    }

                    if (!invocation.TargetMethod.TypeParameters.HasExactly(1))
                    {
                        return;
                    }

                    var numberOfArguments = invocation.IsExtensionMethodAndHasNoInstance() ? 1 : 0;
                    if (!invocation.Arguments.HasExactly(numberOfArguments))
                    {
                        return;
                    }

                    var arg = invocation.IsExtensionMethodAndHasNoInstance()
                            ? invocation.Arguments[0].Value
                            : invocation.Instance;

                    if (!(arg is IConversionOperation conversionOperation))
                    {
                        return;
                    }

                    // should explicit conversion supress?
                    //if (!conversionOperation.IsImplicit)
                    //return;

                    var ienumerable = conversionOperation.Operand.Type.AllInterfaces.SingleOrDefault(t => t.OriginalDefinition.Equals(genericIEnumerableType));
                    if (ienumerable is null)
                    {
                        return;
                    }

                    var castFrom = ienumerable.TypeArguments.Single();
                    var castTo = invocation.TargetMethod.TypeArguments.Single();

                    if (CastWillAlwaysFail(castFrom, castTo))
                    {
                        operationContext.ReportDiagnostic(invocation.CreateDiagnostic(Rule, castFrom.ToDisplayString(), castTo.ToDisplayString()));
                    }

                }, OperationKind.Invocation);
            });

            static bool CastWillAlwaysFail(ITypeSymbol castFrom, ITypeSymbol castTo)
            {
                if (castTo.SpecialType == SpecialType.System_Object)
                {
                    return false;
                }

                if (castFrom.Equals(castTo))
                {
                    return false;
                }

                switch (castFrom.OriginalDefinition.TypeKind, castTo.OriginalDefinition.TypeKind)
                {
                    case (TypeKind.Class, TypeKind.Class):
                    case (TypeKind.Struct, TypeKind.Struct):
                    case (TypeKind.Struct, TypeKind.Enum):

                        if (castFrom.DerivesFrom(castTo))
                        {
                            return false;
                        }

                        if (castTo.DerivesFrom(castFrom))
                        {
                            return false;
                        }

                        return true;

                    case (TypeKind.Interface, TypeKind.Class):
                        return castTo.IsSealed && !castTo.AllInterfaces.Contains(castFrom);

                    case (TypeKind.Class, TypeKind.Interface):
                        return castFrom.IsSealed && !castFrom.AllInterfaces.Contains(castTo);

                    case (TypeKind.Enum, TypeKind.Enum):
                        // todo
                        return false;

                    case (_, TypeKind.Enum):
                    case (TypeKind.Class, TypeKind.Struct):
                    case (TypeKind.Struct, TypeKind.Class):
                        return true;

                    default:
                        return false; // we don't *know* it'll fail...
                }
            }
        }
    }
}
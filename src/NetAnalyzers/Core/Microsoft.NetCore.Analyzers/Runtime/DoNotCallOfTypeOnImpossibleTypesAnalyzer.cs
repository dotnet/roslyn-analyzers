// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CAxxxx: Do not call ToImmutableCollection on an ImmutableCollection value
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallOfTypeOnImpossibleTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA9999";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallToImmutableCollectionOnAnImmutableCollectionValueTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallToImmutableCollectionOnAnImmutableCollectionValueMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: null,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        private static readonly ImmutableDictionary<string, string> OfTypeMetadataNames = new Dictionary<string, string>
        {
            ["OfType"] = "System.Linq.Enumerable.OfType`1",
            ["Cast"] = "System.Linq.Enumerable.Cast`1",
        }.ToImmutableDictionary();

        public static ImmutableArray<string> ToOfTypeMethodNames => OfTypeMetadataNames.Keys.ToImmutableArray();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                // var compilation = compilationStartContext.Compilation;
                //  var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);

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
                    if (targetMethod == null || !OfTypeMetadataNames.TryGetValue(targetMethod.Name, out string metadataName))
                    {
                        return;
                    }

                    if (!invocation.TargetMethod.TypeParameters.HasExactly(1))
                    {
                        return;
                    }

                    var argumentsToSkip = invocation.IsExtensionMethodAndHasNoInstance() ? 1 : 0;
                    if (invocation.Arguments.Skip(argumentsToSkip).Any(arg => arg.ArgumentKind == ArgumentKind.Explicit))
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

                    //if (!conversionOperation.IsImplicit)
                    //return;

                    var ienumerable = conversionOperation.Operand.Type.AllInterfaces.SingleOrDefault(t => t.OriginalDefinition.Equals(genericIEnumerableType));
                    if (ienumerable is null)
                    {
                        return;
                    }

                    var castFrom = ienumerable.TypeArguments.Single();

                    if (castFrom.TypeKind == TypeKind.Interface)
                        return;

                    var castToType = invocation.TargetMethod.TypeArguments.Single();

                    if (castToType.TypeKind == TypeKind.Interface && !castFrom.IsSealed)
                    {
                        return;
                    }

                    if (castToType.DerivesFrom(castFrom))
                    {
                        return;
                    }

                    if (castFrom.DerivesFrom(castToType))
                    {
                        return;
                    }

                    operationContext.ReportDiagnostic(invocation.CreateDiagnostic(Rule, castFrom.MetadataName, castToType.MetadataName));
                }, OperationKind.Invocation);
            });
        }
    }
}

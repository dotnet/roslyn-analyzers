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
    /// CAxxxx: Do not call Enumerable.Cast with incompatible types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCallEnumerableCastThatWillFailAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2250";
        private static readonly LocalizableString s_localizableCastTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerableCastThatWillFailTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCastMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerableCastThatWillFailMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableCastDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerableCastThatWillFailDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableOfTypeTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerablOfTypeThatWillNeverReturnAnyValuesTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableOfTypeMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerablOfTypeThatWillNeverReturnAnyValuesMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableOfTypeDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCallEnumerablOfTypeThatWillNeverReturnAnyValuesMessageDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor CastRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableCastTitle,
                                                                             s_localizableCastMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             RuleLevel.IdeSuggestion,
                                                                             s_localizableCastDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        internal static DiagnosticDescriptor OfTypeRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableOfTypeTitle,
                                                                             s_localizableOfTypeMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableOfTypeDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        private static readonly ImmutableDictionary<string, DiagnosticDescriptor> methodMetadataNames = new Dictionary<string, DiagnosticDescriptor>
        {
            ["OfType"] = OfTypeRule,
            ["Cast"] = CastRule,
        }.ToImmutableDictionary();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => methodMetadataNames.Values.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable, out var enumerableType)
                 || !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIEnumerable1, out var genericIEnumerableType))
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;

                    var targetMethod = invocation.TargetMethod.OriginalDefinition;
                    if (!methodMetadataNames.TryGetValue(targetMethod.Name, out var rule))
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

                    if (arg is not IConversionOperation conversionOperation)
                    {
                        return;
                    }

                    var ienumerable = conversionOperation.Operand.Type.AllInterfaces.SingleOrDefault(t => t.OriginalDefinition.Equals(genericIEnumerableType));
                    if (ienumerable is null)
                    {
                        return;
                    }

                    var castFrom = ienumerable.TypeArguments.Single();
                    var castTo = invocation.TargetMethod.TypeArguments.Single();

                    if (CastWillAlwaysFail(castFrom, castTo))
                    {
                        context.ReportDiagnostic(invocation.CreateDiagnostic(rule, castFrom.ToDisplayString(), castTo.ToDisplayString()));
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
                    case (TypeKind.Enum, TypeKind.Struct):
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

                    case (TypeKind.Enum, TypeKind.Enum)
                    when castFrom.OriginalDefinition is INamedTypeSymbol fromEnum
                      && castTo.OriginalDefinition is INamedTypeSymbol toEnum
                      && fromEnum.EnumUnderlyingType.Equals(toEnum.EnumUnderlyingType):
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

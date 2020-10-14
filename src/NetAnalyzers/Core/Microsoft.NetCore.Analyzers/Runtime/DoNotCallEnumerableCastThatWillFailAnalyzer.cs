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
    /// CA2250: Do not call Enumerable.Cast or Enumerable.OfType with incompatible types.
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

        private static readonly ImmutableArray<(string MethodName, DiagnosticDescriptor Rule)> methodMetadataNames = ImmutableArray.Create(
            ("OfType", OfTypeRule),
            ("Cast", CastRule)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(OfTypeRule, CastRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable, out var enumerableType))
                {
                    return;
                }

                var methodRuleDictionary = methodMetadataNames
                    .SelectMany(m => enumerableType.GetMembers(m.MethodName)
                                                   .OfType<IMethodSymbol>()
                                                   .Where(method => method.TypeParameters.HasExactly(1)
                                                                 && method.Parameters.HasExactly(1))
                                                   .Select(method => (method, m.Rule)))
                    .ToImmutableDictionary(key => key.method, v => v.Rule, SymbolEqualityComparer.Default);

                if (methodRuleDictionary.IsEmpty)
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;

                    var targetMethod = (invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod).OriginalDefinition;

                    if (!methodRuleDictionary.TryGetValue(targetMethod, out var rule))
                    {
                        return;
                    }

                    var instanceArg = invocation.GetInstance(); // "this" argument of an extension method

                    // because the type of the parameter is actually the non-generic IEnumerable,
                    // we have to reach back through conversion operator(s) to get the "real" type of the argument
                    if (instanceArg is not IConversionOperation conversionOperation)
                    {
                        return;
                    }

                    var argIEnumerableType = conversionOperation.Operand.Type.AllInterfaces.SingleOrDefault(t => t.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
                    if (argIEnumerableType is null)
                    {
                        return;
                    }

                    var castFrom = argIEnumerableType.TypeArguments[0];
                    var castTo = invocation.TargetMethod.TypeArguments[0];

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
                        return !castFrom.DerivesFrom(castTo)
                            && !castTo.DerivesFrom(castFrom);

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

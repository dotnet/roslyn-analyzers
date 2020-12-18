// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class EquatableAnalyzer : DiagnosticAnalyzer
    {
        internal const string ImplementIEquatableRuleId = "CA1066";
        internal const string OverrideObjectEqualsRuleId = "CA1067";
        internal const string OverrideBaseClassEqualsRuleId = "CA1071";

        private static readonly LocalizableString s_localizableTitleImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static readonly DiagnosticDescriptor ImplementIEquatableDescriptor = DiagnosticDescriptorHelper.Create(
            ImplementIEquatableRuleId,
            s_localizableTitleImplementIEquatable,
            s_localizableMessageImplementIEquatable,
            DiagnosticCategory.Design,
            RuleLevel.Disabled,
            description: s_localizableDescriptionImplementIEquatable,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly LocalizableString s_localizableTitleOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static readonly DiagnosticDescriptor OverridesObjectEqualsDescriptor = DiagnosticDescriptorHelper.Create(
            OverrideObjectEqualsRuleId,
            s_localizableTitleOverridesObjectEquals,
            s_localizableMessageOverridesObjectEquals,
            DiagnosticCategory.Design,
            RuleLevel.BuildWarningCandidate,
            description: s_localizableDescriptionOverridesObjectEquals,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly LocalizableString s_localizableTitleOverridesBaseClassEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideBaseClassEqualsTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOverridesBaseClassEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideBaseClassEqualsMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionOverridesBaseClassEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideBaseClassEqualsDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static readonly DiagnosticDescriptor OverridesBaseClassEqualsDescriptor = DiagnosticDescriptorHelper.Create(
            OverrideBaseClassEqualsRuleId,
            s_localizableTitleOverridesBaseClassEquals,
            s_localizableMessageOverridesBaseClassEquals,
            DiagnosticCategory.Design,
            RuleLevel.BuildWarningCandidate,
            description: s_localizableDescriptionOverridesBaseClassEquals,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ImplementIEquatableDescriptor, OverridesObjectEqualsDescriptor, OverridesBaseClassEqualsDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol? equatableType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIEquatable1);
            if (equatableType != null)
            {
                context.RegisterSymbolAction(c => AnalyzeSymbol(c, equatableType), SymbolKind.NamedType);
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol equatableType)
        {
            if (context.Symbol is not INamedTypeSymbol namedType
                || (namedType.TypeKind != TypeKind.Struct && namedType.TypeKind != TypeKind.Class)
                || (namedType.TypeKind == TypeKind.Struct && namedType.IsRefLikeType))
            {
                return;
            }

            // Check if base class implements IEquatable<baseclass>
            INamedTypeSymbol baseType = namedType.BaseType;

            INamedTypeSymbol baseEquatable = equatableType.Construct(baseType);
            bool baseClassImplementsEquatable = baseType
                .AllInterfaces
                .Any(x => x.Equals(baseEquatable));
            bool overridesBaseEquatable;

            if (baseClassImplementsEquatable)
            {
                // This class must override the base class IEquatable method.
                overridesBaseEquatable = namedType.OverridesBaseClassEquals(baseType);
            }
            else
            {
                // This class must override object.Equals
                overridesBaseEquatable = namedType.OverridesEquals();
            }

            INamedTypeSymbol constructedEquatable = equatableType.Construct(namedType);
            INamedTypeSymbol implementation = namedType
                .AllInterfaces
                .FirstOrDefault(x => x.Equals(constructedEquatable));
            bool implementsEquatable = implementation != null;

            if (implementsEquatable)
            {
                // Bail out for following cases:
                // 1. There is no method implementing IEquatable.Equals method, indicating compiler error.
                // 2. Base type is implementing the IEquatable for this type, and hence is responsible for overriding object Equals.
                //    For example, we should not flag type B below as IEquatable<B> is implemented by its base type:
                //       class B : A<B> { }
                //       class A<T> : IEquatable<T>
                //          where T: A<T>
                //       { ... }
                if (constructedEquatable.GetMembers(WellKnownMemberNames.ObjectEquals).FirstOrDefault() is not IMethodSymbol equatableEqualsMethod ||
                    !Equals(namedType, namedType.FindImplementationForInterfaceMember(equatableEqualsMethod)?.ContainingType))
                {
                    return;
                }
            }

            if (overridesBaseEquatable && !implementsEquatable && namedType.TypeKind == TypeKind.Struct)
            {
                context.ReportDiagnostic(namedType.CreateDiagnostic(ImplementIEquatableDescriptor, namedType));
            }

            if (!overridesBaseEquatable && implementsEquatable)
            {
                if (baseClassImplementsEquatable)
                {
                    context.ReportDiagnostic(namedType.CreateDiagnostic(OverridesBaseClassEqualsDescriptor, namedType, baseType));
                }
                else
                {
                    context.ReportDiagnostic(namedType.CreateDiagnostic(OverridesObjectEqualsDescriptor, namedType));
                }
            }
        }
    }
}

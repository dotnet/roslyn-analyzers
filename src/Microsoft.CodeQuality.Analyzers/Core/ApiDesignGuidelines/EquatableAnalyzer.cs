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

        private static readonly LocalizableString s_localizableTitleImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly DiagnosticDescriptor s_implementIEquatableDescriptor = new DiagnosticDescriptor(
            ImplementIEquatableRuleId,
            s_localizableTitleImplementIEquatable,
            s_localizableMessageImplementIEquatable,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescriptionImplementIEquatable,
            helpLinkUri: "http://go.microsoft.com/fwlink/?LinkId=734907");

        private static readonly LocalizableString s_localizableTitleOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly DiagnosticDescriptor s_overridesObjectEqualsDescriptor = new DiagnosticDescriptor(
            OverrideObjectEqualsRuleId,
            s_localizableTitleOverridesObjectEquals,
            s_localizableMessageOverridesObjectEquals,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescriptionOverridesObjectEquals,
            helpLinkUri: "http://go.microsoft.com/fwlink/?LinkId=734909");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_implementIEquatableDescriptor, s_overridesObjectEqualsDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol objectType = context.Compilation.GetSpecialType(SpecialType.System_Object);
            INamedTypeSymbol equatableType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIEquatable1);
            if (objectType != null && equatableType != null)
            {
                context.RegisterSymbolAction(c => AnalyzeSymbol(c, equatableType), SymbolKind.NamedType);
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol equatableType)
        {
            if (!(context.Symbol is INamedTypeSymbol namedType) || !(namedType.TypeKind == TypeKind.Struct || namedType.TypeKind == TypeKind.Class))
            {
                return;
            }

            bool overridesObjectEquals = namedType.OverridesEquals();

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
                if (!(constructedEquatable.GetMembers("Equals").FirstOrDefault() is IMethodSymbol equatableEqualsMethod) ||
                    !Equals(namedType, namedType.FindImplementationForInterfaceMember(equatableEqualsMethod)?.ContainingType))
                {
                    return;
                }
            }

            if (overridesObjectEquals && !implementsEquatable && namedType.TypeKind == TypeKind.Struct)
            {
                context.ReportDiagnostic(namedType.CreateDiagnostic(s_implementIEquatableDescriptor, namedType));
            }

            if (!overridesObjectEquals && implementsEquatable)
            {
                context.ReportDiagnostic(namedType.CreateDiagnostic(s_overridesObjectEqualsDescriptor, namedType));
            }
        }
    }
}

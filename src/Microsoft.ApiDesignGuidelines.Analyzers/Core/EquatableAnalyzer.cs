// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class EquatableAnalyzer : DiagnosticAnalyzer
    {
        private const string IEquatableMetadataName = "System.IEquatable`1";
        internal const string ImplementIEquatableRuleId = "CA1066";
        internal const string OverrideObjectEqualsRuleId = "CA1067";

        private static readonly LocalizableString s_localizableTitleImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageImplementIEquatable = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly DiagnosticDescriptor s_implementIEquatableDescriptor = new DiagnosticDescriptor(
            ImplementIEquatableRuleId,
            s_localizableTitleImplementIEquatable,
            s_localizableMessageImplementIEquatable,
            DiagnosticCategory.Design,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly LocalizableString s_localizableTitleOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOverridesObjectEquals = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideObjectEqualsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly DiagnosticDescriptor s_overridesObjectEqualsDescriptor = new DiagnosticDescriptor(
            OverrideObjectEqualsRuleId,
            s_localizableTitleOverridesObjectEquals,
            s_localizableMessageOverridesObjectEquals,
            DiagnosticCategory.Design,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(s_implementIEquatableDescriptor, s_overridesObjectEqualsDescriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(InitializeCore);
        }

        private void InitializeCore(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol objectType = context.Compilation.GetSpecialType(SpecialType.System_Object);
            INamedTypeSymbol equatableType = context.Compilation.GetTypeByMetadataName(IEquatableMetadataName);
            if (objectType != null && equatableType != null)
            {
                context.RegisterSymbolAction(c => AnalyzeSymbol(c, objectType, equatableType), SymbolKind.NamedType);
            }
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol objectType, INamedTypeSymbol equatableType)
        {
            var namedType = context.Symbol as INamedTypeSymbol;
            if (namedType == null || !(namedType.TypeKind == TypeKind.Struct || namedType.TypeKind == TypeKind.Class))
            {
                return;
            }

            bool overridesObjectEquals = namedType.OverridesEquals();

            INamedTypeSymbol constructedEquatable = equatableType.Construct(namedType);
            INamedTypeSymbol implementation = namedType
                .Interfaces
                .Where(x => x.Equals(constructedEquatable))
                .FirstOrDefault();
            bool implementsEquatable = implementation != null;

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

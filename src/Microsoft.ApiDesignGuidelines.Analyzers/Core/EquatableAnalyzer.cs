// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
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
            var objectType = context.Compilation.GetSpecialType(SpecialType.System_Object);
            var equatableType = context.Compilation.GetTypeByMetadataName(IEquatableMetadataName);
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

            var methodSymbol = namedType
                .GetMembers("Equals")
                .OfType<IMethodSymbol>()
                .Where(m => IsObjectEqualsOverride(m, objectType))
                .FirstOrDefault();
            var overridesObjectEquals = methodSymbol != null;

            var constructedEquatable = equatableType.Construct(namedType);
            var implementation = namedType
                .Interfaces
                .Where(x => x.Equals(constructedEquatable))
                .FirstOrDefault();
            var implementsEquatable = implementation != null;

            if (overridesObjectEquals && !implementsEquatable && namedType.TypeKind == TypeKind.Struct)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_implementIEquatableDescriptor, methodSymbol.Locations[0], namedType));
            }

            if (!overridesObjectEquals && implementsEquatable)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_overridesObjectEqualsDescriptor, namedType.Locations[0], namedType));
            }
        }

        private bool IsObjectEqualsOverride(IMethodSymbol methodSymbol, INamedTypeSymbol objectType)
        {
            Debug.Assert(methodSymbol != null);
            if (methodSymbol == null)
            {
                return false;
            }

            if (!methodSymbol.IsOverride)
            {
                return false;
            }

            if (methodSymbol.Parameters.Length != 1 ||
                methodSymbol.Parameters[0]?.Type?.Equals(objectType) != true)
            {
                return false;
            }

            if (methodSymbol.ReturnType?.SpecialType != SpecialType.System_Boolean)
            {
                return false;
            }

            do
            {
                methodSymbol = methodSymbol.OverriddenMethod;
            }
            while (methodSymbol?.IsOverride == true);

            return methodSymbol.ContainingType?.Equals(objectType) == true;
        }
    }
}

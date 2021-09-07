// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class DoNotPassMutableValueTypesByValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string ParametersRuleId = "CA2019";
        private static readonly LocalizableString s_parametersTitle = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_Parameters_Title), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_parametersMessage = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_Parameters_Message), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_parametersDescription = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_Parameters_Description), Resx.ResourceManager, typeof(Resx));

        internal static readonly DiagnosticDescriptor ParametersRule = DiagnosticDescriptorHelper.Create(
            ParametersRuleId,
            s_parametersTitle,
            s_parametersMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_parametersDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal const string ReturnValuesRuleId = "CA2020";
        private static readonly LocalizableString s_returnValuesTitle = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_ReturnValues_Title), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_returnValuesMessage = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_ReturnValues_Message), Resx.ResourceManager, typeof(Resx));
        private static readonly LocalizableString s_returnValuesDescription = new LocalizableResourceString(nameof(Resx.DoNotPassMutableValueTypesByValue_ReturnValues_Description), Resx.ResourceManager, typeof(Resx));

        internal static readonly DiagnosticDescriptor ReturnValuesRule = DiagnosticDescriptorHelper.Create(
            ReturnValuesRuleId,
            s_returnValuesTitle,
            s_returnValuesMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_returnValuesDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly ImmutableHashSet<string> _wellKnownMutableValueTypes = ImmutableHashSet.Create(
            WellKnownTypeNames.SystemTextJsonUtf8JsonReader,
            WellKnownTypeNames.SystemThreadingSpinLock);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ParametersRule, ReturnValuesRule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbols, SymbolKind.Parameter, SymbolKind.Method, SymbolKind.Property);
        }

        private protected abstract IEnumerable<Location> GetMethodReturnTypeLocations(IMethodSymbol methodSymbol, CancellationToken token);

        private protected abstract IEnumerable<Location> GetPropertyReturnTypeLocations(IPropertySymbol propertySymbol, CancellationToken token);

        private void AnalyzeSymbols(SymbolAnalysisContext context)
        {
            switch (context.Symbol)
            {
                case IParameterSymbol parameterSymbol:
                    AnalyzeParameter(parameterSymbol);
                    break;
                case IMethodSymbol methodSymbol:
                    AnalyzeMethod(methodSymbol);
                    break;
                case IPropertySymbol propertySymbol:
                    AnalyzeProperty(propertySymbol);
                    break;
            }
            return;

            //  Local functions

            //  We flag any parameter that:
            //  1) Is not a ref or out parameter
            //  2) Is in source code
            //  3) Who's type is a mutable value type
            //      a) Is on our hard-coded list of well-known mutable value types
            //      b) Is added as a mutable value type via .editorconfig
            //      c) Is a suspected struct enumerator type, which is any type that:
            //          - Is a value type
            //          - Is not an enum
            //          - Is a nested type
            //          - Name ends with "Enumerator"
            void AnalyzeParameter(IParameterSymbol parameterSymbol)
            {
                if (parameterSymbol.RefKind is RefKind.Ref or RefKind.Out || !parameterSymbol.IsInSource())
                    return;

                if (IsMutableValueType(parameterSymbol.Type, context, ParametersRule) != MutableValueTypeKind.None)
                {
                    foreach (var syntaxReference in parameterSymbol.DeclaringSyntaxReferences)
                    {
                        var location = Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);
                        var diagnostic = location.CreateDiagnostic(ParametersRule, parameterSymbol.Type.ToDisplayString(SymbolDisplayFormats.QualifiedTypeAndNamespaceSymbolDisplayFormat));
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            //  Flag any method with a mutable return type that doesn't return by reference.
            void AnalyzeMethod(IMethodSymbol methodSymbol)
            {
                if (methodSymbol.IsAccessorMethod())
                    return;
                var mutableTypeKind = IsMutableValueType(methodSymbol.ReturnType, context, ReturnValuesRule);

                if (mutableTypeKind != MutableValueTypeKind.None && !methodSymbol.ReturnsByRef)
                {
                    if (mutableTypeKind == MutableValueTypeKind.Enumerator && string.Equals(methodSymbol.Name, "GetEnumerator", StringComparison.Ordinal))
                        return;

                    var locations = GetMethodReturnTypeLocations(methodSymbol, context.CancellationToken);
                    var diagnostic = locations.First().CreateDiagnostic(ReturnValuesRule, ImmutableArray.CreateRange(locations.Skip(1)), null, methodSymbol.ReturnType.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }

            //  Flag any property with a mutable type that doesn't return by reference.
            void AnalyzeProperty(IPropertySymbol propertySymbol)
            {
                if (IsMutableValueType(propertySymbol.Type, context, ReturnValuesRule) != MutableValueTypeKind.None && !propertySymbol.ReturnsByRef)
                {
                    var locations = GetPropertyReturnTypeLocations(propertySymbol, context.CancellationToken);
                    var diagnostic = locations.First().CreateDiagnostic(ReturnValuesRule, ImmutableArray.CreateRange(locations.Skip(1)), null, propertySymbol.Type.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private enum MutableValueTypeKind
        {
            None,
            WellKnown,
            UserDefined,
            Enumerator
        }

        //  Returns true for any type that satisfies any of the following:
        //  - Is on our hard-coded list of well-known mutable value types
        //  - Is added as a mutable value type via .editorconfig
        //  - Is a suspected struct enumerator type, which is any type that satisfies all of:
        //      a) Is a value type
        //      b) Is not an enum
        //      c) Is a nested type
        //      d) Name ends with "Enumerator"
        private static MutableValueTypeKind IsMutableValueType(ITypeSymbol typeSymbol, SymbolAnalysisContext context, DiagnosticDescriptor rule)
        {
            if (_wellKnownMutableValueTypes.Contains(typeSymbol.ToString()))
                return MutableValueTypeKind.WellKnown;
            if (IsStructEnumeratorType(typeSymbol))
                return MutableValueTypeKind.Enumerator;
            if (context.Symbol.DeclaringSyntaxReferences.Any(syntaxReference => context.Options.GetAdditionalMutableValueTypesOption(rule, syntaxReference.SyntaxTree, context.Compilation).Contains(typeSymbol)))
                return MutableValueTypeKind.UserDefined;

            return MutableValueTypeKind.None;

            //  Local functions

            static bool IsStructEnumeratorType(ITypeSymbol typeSymbol)
            {
                return typeSymbol.IsValueType && typeSymbol.BaseType.SpecialType != SpecialType.System_Enum &&
                    typeSymbol.ContainingSymbol is ITypeSymbol &&
                    typeSymbol.ToString().EndsWith("Enumerator", StringComparison.Ordinal);
            }
        }
    }
}

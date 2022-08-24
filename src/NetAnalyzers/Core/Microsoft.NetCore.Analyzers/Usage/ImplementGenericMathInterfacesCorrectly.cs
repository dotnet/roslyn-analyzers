// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Usage
{
    using static MicrosoftNetCoreAnalyzersResources;

    public abstract class ImplementGenericMathInterfacesCorrectly : DiagnosticAnalyzer
    {
        private const string RuleId = "CA2260";

        internal static readonly DiagnosticDescriptor GMInterfacesRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(ImplementGenericMathInterfacesCorrectlyTitle)),
            CreateLocalizableResourceString(nameof(ImplementGenericMathInterfacesCorrectlyMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(ImplementGenericMathInterfacesCorrectlyDesciption)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        private static readonly ImmutableArray<string> s_knownInterfaces = ImmutableArray.Create("IParsable`1", "ISpanParsable`1", "IAdditionOperators`3", "IAdditiveIdentity`2",
            "IBinaryFloatingPointIeee754`1", "IBinaryInteger`1", "IBinaryNumber`1", "IBitwiseOperators`3", "IComparisonOperators`2", "IDecrementOperators`1", "IDivisionOperators`3",
            "IEqualityOperators`2", "IExponentialFunctions`1", "IFloatingPointIeee754`1", "IFloatingPoint`1", "IHyperbolicFunctions`1", "IIncrementOperators`1", "ILogarithmicFunctions`1",
            "IMinMaxValue`1", "IModulusOperators`3", "IMultiplicativeIdentity`2", "IMultiplyOperators`3", "INumberBase`1", "INumber`1", "IPowerFunctions`1", "IRootFunctions`1", "IShiftOperators`2",
            "ISignedNumber`1", "ISubtractionOperators`3", "ITrigonometricFunctions`1", "IUnaryNegationOperators`2", "IUnaryPlusOperators`2", "IUnsignedNumber`1", "IFloatingPointConstants`1");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GMInterfacesRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIParsable1, out var iParsableInterface) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemNumericsIDecrementOperators1, out var iBinaryInteger1))
                {
                    return;
                }

                context.RegisterSymbolAction(context =>
                {
                    if (context.Symbol is INamedTypeSymbol ntSymbol && !ntSymbol.IsGenericType)
                    {
                        AnalyzeSymbol(context, ntSymbol, iParsableInterface.ContainingNamespace, iBinaryInteger1.ContainingNamespace);
                    }
                }, SymbolKind.NamedType);
            });
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol symbol, INamespaceSymbol systemNS, INamespaceSymbol systemNumericsNS)
        {
            if (!CheckInterfacesForViolation(symbol))
            {
                INamedTypeSymbol? baseType = symbol.BaseType;
                if (baseType != null && baseType.IsGenericType)
                {
                    CheckInterfacesForViolation(baseType);
                }
            }

            bool CheckInterfacesForViolation(INamedTypeSymbol lookup)
            {
                foreach (INamedTypeSymbol anInterface in lookup.Interfaces)
                {
                    if (anInterface.IsGenericType)
                    {
                        if (IsKnownInterface(anInterface, systemNS, systemNumericsNS) &&
                            FirstTypeParameterNameIsNotTheSymbolName(symbol, anInterface))
                        {
                            SyntaxNode? typeParameter = FindTheTypeArgumentOfTheInterfaceFromTypeDeclaration(symbol, symbol.Equals(lookup, SymbolEqualityComparer.Default) ? anInterface : lookup);
                            if (typeParameter != null)
                            {
                                context.ReportDiagnostic(typeParameter.CreateDiagnostic(GMInterfacesRule, anInterface.Name, symbol.Name, GetTSelfParameterName(lookup, anInterface)));
                            }
                            else
                            {
                                context.ReportDiagnostic(symbol.CreateDiagnostic(GMInterfacesRule, anInterface.Name, symbol.Name, GetTSelfParameterName(lookup, anInterface)));
                            }

                            return true;
                        }
                        else if (CheckInterfacesForViolation(anInterface))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            string GetTSelfParameterName(INamedTypeSymbol symbol, INamedTypeSymbol anInterface) =>
                symbol.IsGenericType ? symbol.OriginalDefinition.TypeParameters[0].Name : anInterface.OriginalDefinition.TypeParameters[0].Name;
        }

        protected abstract SyntaxNode? FindTheTypeArgumentOfTheInterfaceFromTypeDeclaration(ISymbol typeSymbol, ISymbol theInterfaceSymbol);

        private bool IsKnownInterface(INamedTypeSymbol anInterface, INamespaceSymbol systemNS, INamespaceSymbol systemNumericsNS)
        {
            var iNamespace = anInterface.ContainingNamespace;

            return s_knownInterfaces.Contains(anInterface.MetadataName) &&
                   (iNamespace.Equals(systemNS, SymbolEqualityComparer.Default) ||
                    iNamespace.Equals(systemNumericsNS, SymbolEqualityComparer.Default));
        }

        private bool FirstTypeParameterNameIsNotTheSymbolName(INamedTypeSymbol symbol, INamedTypeSymbol anInterface) =>
            anInterface.TypeArguments[0].Name != symbol.Name;
    }
}

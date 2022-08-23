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
        private const string TSelf = nameof(TSelf);

        internal static readonly DiagnosticDescriptor CRTPRule = DiagnosticDescriptorHelper.Create(
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
            "ISignedNumber`1", "ISubtractionOperators`3", "ITrigonometricFunctions`1", "IUnaryNegationOperators`2", "IUnaryPlusOperators`2", "IUnsignedNumber`1");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CRTPRule);

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
                    if (context.Symbol is INamedTypeSymbol ntSymbol)
                    {
                        AnalyzeSymbol(context, ntSymbol, iParsableInterface.ContainingNamespace, iBinaryInteger1.ContainingNamespace);
                    }
                }, SymbolKind.NamedType);
            });
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol symbol, INamespaceSymbol systemNS, INamespaceSymbol systemNumericsNS)
        {
            foreach (INamedTypeSymbol anInterface in symbol.Interfaces)
            {
                if (IsKnownInterface(anInterface, systemNS, systemNumericsNS) &&
                    IsCRTPNotUsedCorrectly(symbol, anInterface, out int parameterLocation))
                {
                    SyntaxNode? typeParameter = FindTheTypeArgumentOfTheInterfaceFromTypeDeclaration(symbol, anInterface, parameterLocation);
                    if (typeParameter != null)
                    {
                        context.ReportDiagnostic(typeParameter.CreateDiagnostic(CRTPRule, anInterface.Name, symbol.Name));
                    }
                    else
                    {
                        context.ReportDiagnostic(symbol.CreateDiagnostic(CRTPRule, anInterface.Name, symbol.Name));
                    }
                }
            }

            INamedTypeSymbol? baseType = symbol.BaseType;
            if (baseType != null)
            {
                // Should check base types?
            }
        }
        protected abstract SyntaxNode? FindTheTypeArgumentOfTheInterfaceFromTypeDeclaration(ISymbol typeSymbol, ISymbol theInterfaceSymbol, int parameterLocation);

        private bool IsKnownInterface(INamedTypeSymbol anInterface, INamespaceSymbol systemNS, INamespaceSymbol systemNumericsNS)
        {
            var iNamespace = anInterface.ContainingNamespace;

            return s_knownInterfaces.Contains(anInterface.MetadataName) &&
                   (iNamespace.Equals(systemNS, SymbolEqualityComparer.Default) ||
                    iNamespace.Equals(systemNumericsNS, SymbolEqualityComparer.Default));
        }

        private bool IsCRTPNotUsedCorrectly(INamedTypeSymbol symbol, INamedTypeSymbol anInterface, out int location)
        {
            if (!symbol.IsGenericType)
            {
                location = GetTSelfTypeParameterLocation(anInterface.TypeParameters);
                RoslynDebug.Assert(location > -1 && anInterface.TypeArguments.Length > location);

                return anInterface.TypeArguments[location].Name != symbol.Name;
            }

            location = -1;
            return false;
        }

        private int GetTSelfTypeParameterLocation(ImmutableArray<ITypeParameterSymbol> typeParameters)
        {
            int i = 0;
            foreach (var tp in typeParameters)
            {
                if (tp.Name is TSelf)
                    return i;
                i++;
            }

            return -1;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    //  Visual Basic does not allow implicit conversions from char to int
#pragma warning disable RS1004 // Recommend adding language support to diagnostic analyzer
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1004 // Recommend adding language support to diagnostic analyzer
    public sealed class SuspiciousCastFromCharToIntAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2019";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntCodeFixTitle)),
            Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var symbols = new RequiredSymbols(context.Compilation);
            var stringSplitMethods = GetProblematicStringSplitMethods(context.Compilation, symbols);

            //  Report implicit char-to-int conversions in calls to certain overloads of string.Split
            context.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;

                if (stringSplitMethods.Contains(invocation.TargetMethod))
                {
                    foreach (var argument in invocation.Arguments)
                    {
                        if (IsImplicitCharToIntConversion(argument))
                        {
                            context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                        }
                    }
                }
            }, OperationKind.Invocation);

            if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out var stringBuilderType))
                return;

            //  Report implicit char-to-int conversions in calls to all StringBuilder constructors
            context.RegisterOperationAction(context =>
            {
                var creation = (IObjectCreationOperation)context.Operation;

                if (!creation.Constructor.ContainingType.Equals(stringBuilderType, SymbolEqualityComparer.Default))
                    return;

                foreach (var argument in creation.Arguments)
                {
                    if (IsImplicitCharToIntConversion(argument))
                    {
                        context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                    }
                }
            }, OperationKind.ObjectCreation);

            return;

            //  Local functions

            static bool IsImplicitCharToIntConversion(IArgumentOperation argument)
            {
                return argument.Value is IConversionOperation conversion &&
                    conversion.IsImplicit &&
                    conversion.Operand.Type.SpecialType is SpecialType.System_Char &&
                    argument.Parameter.Type.SpecialType is SpecialType.System_Int32;
            }
        }

        //  Property bag
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal readonly struct RequiredSymbols
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public RequiredSymbols(Compilation compilation)
            {
                CharType = compilation.GetSpecialType(SpecialType.System_Char);
                Int32Type = compilation.GetSpecialType(SpecialType.System_Int32);
                StringType = compilation.GetSpecialType(SpecialType.System_String);
            }

            public ITypeSymbol CharType { get; }
            public ITypeSymbol Int32Type { get; }
            public ITypeSymbol StringType { get; }
        }

        /// <summary>
        /// Gets the overloads of string.Split that may invite unintentional casts from <see cref="char"/> to <see cref="int"/>.
        /// </summary>
        private static ImmutableHashSet<IMethodSymbol> GetProblematicStringSplitMethods(Compilation compilation, RequiredSymbols symbols)
        {
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                return ImmutableHashSet<IMethodSymbol>.Empty;

            var builder = ImmutableHashSet.CreateBuilder<IMethodSymbol>(SymbolEqualityComparer.Default);
            var splitMembers = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>();

            AddIfNotNull(splitMembers.GetFirstOrDefaultMemberWithParameterTypes(symbols.CharType, symbols.Int32Type, stringSplitOptionsType));
            AddIfNotNull(splitMembers.GetFirstOrDefaultMemberWithParameterTypes(symbols.StringType, symbols.Int32Type, stringSplitOptionsType));

            return builder.ToImmutable();

            //  Local functions

            void AddIfNotNull(IMethodSymbol? method)
            {
                if (method is not null)
                    builder!.Add(method);
            }
        }
    }
}

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
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SuspiciousCastFromCharToIntAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3200";

        private static readonly LocalizableString s_localizableTitle = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntTitle));
        private static readonly LocalizableString s_localizableMessage = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntMessage));
        private static readonly LocalizableString s_localizableDescription = Resx.CreateLocalizableResourceString(nameof(Resx.SuspiciousCastFromCharToIntDescription));

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Correctness,
            RuleLevel.BuildWarning,
            s_localizableDescription,
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
            if (!RequiredSymbols.TryGetSymbols(context.Compilation, out var symbols))
                return;

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

            bool IsImplicitCharToIntConversion(IArgumentOperation argument)
            {
                return argument.Value is IConversionOperation conversion &&
                    conversion.IsImplicit &&
                    conversion.Operand.Type.Equals(symbols.CharType, SymbolEqualityComparer.Default) &&
                    argument.Parameter.Type.Equals(symbols.Int32Type, SymbolEqualityComparer.Default);
            }
        }

        //  Property bag
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal readonly struct RequiredSymbols
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private RequiredSymbols(ITypeSymbol charType, ITypeSymbol int32Type, ITypeSymbol stringType)
            {
                CharType = charType;
                Int32Type = int32Type;
                StringType = stringType;
            }

            public static bool TryGetSymbols(Compilation compilation, out RequiredSymbols result)
            {
                var charType = compilation.GetSpecialType(SpecialType.System_Char);
                var int32Type = compilation.GetSpecialType(SpecialType.System_Int32);
                var stringType = compilation.GetSpecialType(SpecialType.System_String);

                if (charType is not null && int32Type is not null && stringType is not null)
                {
                    result = new RequiredSymbols(charType, int32Type, stringType);
                    return true;
                }

                result = default;
                return false;
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

#if false
        private abstract class OperationAnalyzer
        {
            protected OperationAnalyzer(IMethodSymbol invokedMethod)
            {
                InvokedMethod = invokedMethod;
            }

            public IMethodSymbol InvokedMethod { get; }

            public abstract void AnalyzeOperation(OperationAnalysisContext context);
        }

        /// <summary>
        /// Analyze string.Split(char, int, StringSplitOptions) calls.
        /// </summary>
        private sealed class StringSplit_CharInt32StringSplitOptions : OperationAnalyzer
        {
            private StringSplit_CharInt32StringSplitOptions(IMethodSymbol stringSplit) : base(stringSplit) { }

            public static StringSplit_CharInt32StringSplitOptions? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.CharType, symbols.Int32Type, stringSplitOptionsType);

                return splitMethod is not null ? new StringSplit_CharInt32StringSplitOptions(splitMethod) : null;
            }

            public override void AnalyzeOperation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;
                var argument = invocation.Arguments.First(x => x.Parameter.Ordinal == 1);

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType is SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }
        }

        /// <summary>
        /// Analyze string.Split(string, int, StringSplitOptions) calls
        /// </summary>
        private sealed class StringSplit_StringInt32StringSplitOptions : OperationAnalyzer
        {
            private StringSplit_StringInt32StringSplitOptions(IMethodSymbol stringSplit) : base(stringSplit) { }

            public static StringSplit_StringInt32StringSplitOptions? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemStringSplitOptions, out var stringSplitOptionsType))
                    return null;

                var splitMethod = symbols.StringType.GetMembers(nameof(string.Split)).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.StringType, symbols.Int32Type, stringSplitOptionsType);

                return splitMethod is not null ? new StringSplit_StringInt32StringSplitOptions(splitMethod) : null;
            }

            public override void AnalyzeOperation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;
                var argument = invocation.Arguments.First(x => x.Parameter.Ordinal == 1);

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType is SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }
        }

        /// <summary>
        /// Analyze new StringBuilder(int) calls.
        /// </summary>
        private sealed class StringBuilderCtor_Int32 : OperationAnalyzer
        {
            private StringBuilderCtor_Int32(IMethodSymbol ctor) : base(ctor) { }

            public static StringBuilderCtor_Int32? Create(Compilation compilation, RequiredSymbols symbols)
            {
                if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemTextStringBuilder, out var stringBuilderType))
                    return null;

                var ctor = stringBuilderType.GetMembers(WellKnownMemberNames.InstanceConstructorName).OfType<IMethodSymbol>()
                    .GetFirstOrDefaultMemberWithParameterTypes(symbols.Int32Type);

                return ctor is not null ? new StringBuilderCtor_Int32(ctor) : null;
            }

            public override void AnalyzeOperation(OperationAnalysisContext context)
            {
                var objectCreation = (IObjectCreationOperation)context.Operation;
                var argument = objectCreation.Arguments[0];

                if (argument.Value is IConversionOperation conversion && conversion.IsImplicit && conversion.Operand.Type.SpecialType is SpecialType.System_Char)
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(Rule));
                }
            }
        }

        private static ImmutableDictionary<IMethodSymbol, OperationAnalyzer> CreateAnalyzerCache(Compilation compilation)
        {
            if (!RequiredSymbols.TryGetSymbols(compilation, out var symbols))
                return ImmutableDictionary<IMethodSymbol, OperationAnalyzer>.Empty;

            var builder = ImmutableDictionary.CreateBuilder<IMethodSymbol, OperationAnalyzer>();

            AddIfNotNull(StringSplit_CharInt32StringSplitOptions.Create(compilation, symbols));
            AddIfNotNull(StringSplit_StringInt32StringSplitOptions.Create(compilation, symbols));
            AddIfNotNull(StringBuilderCtor_Int32.Create(compilation, symbols));

            return builder.ToImmutable();

            //  Local functions

            void AddIfNotNull(OperationAnalyzer? analyzer)
            {
                if (analyzer is not null)
                    builder!.Add(analyzer.InvokedMethod, analyzer);
            }
        }
#endif
    }
}

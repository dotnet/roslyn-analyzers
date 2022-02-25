// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;
    public abstract partial class ConstantExpectedAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA1860 = nameof(CA1860);
        internal const string CA1861 = nameof(CA1861);
        protected static readonly string ConstantExpectedAttribute = nameof(ConstantExpectedAttribute);
        protected static readonly string ConstantExpected = nameof(ConstantExpected);
        private static readonly LocalizableString s_localizableApplicationTitle = CreateLocalizableResourceString(nameof(ConstantExpectedApplicationTitle));
        private static readonly LocalizableString s_localizableApplicationDescription = CreateLocalizableResourceString(nameof(ConstantExpectedApplicationDescription));
        private static readonly LocalizableString s_localizableUsageTitle = CreateLocalizableResourceString(nameof(ConstantExpectedUsageTitle));
        private static readonly LocalizableString s_localizableUsageDescription = CreateLocalizableResourceString(nameof(ConstantExpectedUsageDescription));

        internal static readonly DiagnosticDescriptor InvalidTypeRule = DiagnosticDescriptorHelper.Create(
            CA1860,
            s_localizableApplicationTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedInvalidTypeMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildError,
            description: s_localizableApplicationDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor IncompatibleConstantTypeRule = DiagnosticDescriptorHelper.Create(
            CA1860,
            s_localizableApplicationTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedIncompatibleConstantTypeMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildError,
            description: s_localizableApplicationDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor IncompatibleConstantForMinMaxRule = DiagnosticDescriptorHelper.Create(
            CA1860,
            s_localizableApplicationTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedIncompatibleConstantMinMaxMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildError,
            description: s_localizableApplicationDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor InvalidBoundsRule = DiagnosticDescriptorHelper.Create(
            CA1860,
            s_localizableApplicationTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedInvalidBoundsMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildError,
            description: s_localizableApplicationDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor InvertedRangeRule = DiagnosticDescriptorHelper.Create(
            CA1860,
            s_localizableApplicationTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedInvertedRangeMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildError,
            description: s_localizableApplicationDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ConstantOutOfBoundsRule = DiagnosticDescriptorHelper.Create(
            CA1861,
            s_localizableUsageTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedOutOfBoundsMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableUsageDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ConstantNotConstantRule = DiagnosticDescriptorHelper.Create(
            CA1861,
            s_localizableUsageTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedNotConstantMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableUsageDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor AttributeOutOfBoundsRule = DiagnosticDescriptorHelper.Create(
            CA1861,
            s_localizableUsageTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedAttributeOutOfBoundsMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableUsageDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor AttributeNotSameTypeRule = DiagnosticDescriptorHelper.Create(
            CA1861,
            s_localizableUsageTitle,
            CreateLocalizableResourceString(nameof(ConstantExpectedAttributeNotSameTypeMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableUsageDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            InvalidTypeRule, IncompatibleConstantTypeRule, IncompatibleConstantForMinMaxRule, InvalidBoundsRule, InvertedRangeRule,
            ConstantOutOfBoundsRule, ConstantNotConstantRule, AttributeOutOfBoundsRule, AttributeNotSameTypeRule);

        protected abstract DiagnosticHelper Helper { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context => OnCompilationStart(context));
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterOperationAction(OnInvocation, OperationKind.Invocation);
            RegisterAttributeSyntax(context);
            return;
        }

        private static void OnInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            foreach (var argument in invocation.Arguments)
            {
                if (!TryCreateConstantExpectedParameter(argument.Parameter, out var argConstantParameter))
                {
                    continue;
                }
                var v = argument.Value.WalkDownConversion();
                if (v is IParameterReferenceOperation parameterReference &&
                    TryCreateConstantExpectedParameter(parameterReference.Parameter, out var currConstantParameter))
                {
                    if (!argConstantParameter.ValidateParameterIsWithinRange(currConstantParameter, out var parameterCheckDiagnostic))
                    {
                        context.ReportDiagnostic(parameterCheckDiagnostic);
                    }
                    continue;
                }
                var constantValue = v.ConstantValue;
                if (!constantValue.HasValue)
                {
                    var location = argument.Syntax.GetLocation();
                    context.ReportDiagnostic(Diagnostic.Create(ConstantNotConstantRule, location));
                    continue;
                }

                var rawValue = constantValue.Value;
                if (!argConstantParameter.ValidateValue(argument, rawValue, out var valueDiagnostic))
                {
                    context.ReportDiagnostic(valueDiagnostic);
                }
            }
        }

        protected abstract void RegisterAttributeSyntax(CompilationStartAnalysisContext context);

        protected void OnParameterWithConstantExpectedAttribute(IParameterSymbol parameter, Action<Diagnostic> reportAction)
        {
            if (!ValidateConstantExpectedParameter(parameter, out ImmutableArray<Diagnostic> diagnostics))
            {
                foreach (var diagnostic in diagnostics)
                {
                    reportAction(diagnostic);
                }
            }
        }

        private static bool TryCreateConstantExpectedParameter(IParameterSymbol parameterSymbol, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
        {
            if (!TryGetConstantExpectedAttributeData(parameterSymbol, out var attributeData))
            {
                parameter = null;
                return false;
            }

            switch (parameterSymbol.Type.SpecialType)
            {
                case SpecialType.System_Char:
                    return UnmanagedHelper<char>.TryCreate(parameterSymbol, attributeData, char.MinValue, char.MaxValue, out parameter);
                case SpecialType.System_Byte:
                    return UnmanagedHelper<ulong>.TryCreate(parameterSymbol, attributeData, byte.MinValue, byte.MaxValue, out parameter);
                case SpecialType.System_UInt16:
                    return UnmanagedHelper<ulong>.TryCreate(parameterSymbol, attributeData, ushort.MinValue, ushort.MaxValue, out parameter);
                case SpecialType.System_UInt32:
                    return UnmanagedHelper<ulong>.TryCreate(parameterSymbol, attributeData, uint.MinValue, uint.MaxValue, out parameter);
                case SpecialType.System_UInt64:
                    return UnmanagedHelper<ulong>.TryCreate(parameterSymbol, attributeData, ulong.MinValue, ulong.MaxValue, out parameter);
                case SpecialType.System_UIntPtr:
                    return UnmanagedHelper<ulong>.TryCreate(parameterSymbol, attributeData, uint.MinValue, uint.MaxValue, out parameter);
                case SpecialType.System_SByte:
                    return UnmanagedHelper<long>.TryCreate(parameterSymbol, attributeData, sbyte.MinValue, sbyte.MaxValue, out parameter);
                case SpecialType.System_Int16:
                    return UnmanagedHelper<long>.TryCreate(parameterSymbol, attributeData, short.MinValue, short.MaxValue, out parameter);
                case SpecialType.System_Int32:
                    return UnmanagedHelper<long>.TryCreate(parameterSymbol, attributeData, int.MinValue, int.MaxValue, out parameter);
                case SpecialType.System_Int64:
                    return UnmanagedHelper<long>.TryCreate(parameterSymbol, attributeData, long.MinValue, long.MaxValue, out parameter);
                case SpecialType.System_IntPtr:
                    return UnmanagedHelper<long>.TryCreate(parameterSymbol, attributeData, int.MinValue, int.MaxValue, out parameter);
                case SpecialType.System_Single:
                    return UnmanagedHelper<float>.TryCreate(parameterSymbol, attributeData, float.MinValue, float.MaxValue, out parameter);
                case SpecialType.System_Double:
                    return UnmanagedHelper<double>.TryCreate(parameterSymbol, attributeData, double.MinValue, double.MaxValue, out parameter);
                case SpecialType.System_Boolean:
                    return UnmanagedHelper<bool>.TryCreate(parameterSymbol, attributeData, false, true, out parameter);
                case SpecialType.System_String:
                    return StringConstantExpectedParameter.TryCreate(parameterSymbol, attributeData, out parameter);
                default:
                    parameter = null;
                    return false;
            }
        }

        private bool ValidateConstantExpectedParameter(IParameterSymbol parameterSymbol, out ImmutableArray<Diagnostic> diagnostics)
        {
            if (!TryGetConstantExpectedAttributeData(parameterSymbol, out var attributeData))
            {
                diagnostics = ImmutableArray<Diagnostic>.Empty;
                return false;
            }

            switch (parameterSymbol.Type.SpecialType)
            {
                case SpecialType.System_Char:
                    return UnmanagedHelper<char>.Validate(parameterSymbol, attributeData, char.MinValue, char.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Byte:
                    return UnmanagedHelper<ulong>.Validate(parameterSymbol, attributeData, byte.MinValue, byte.MaxValue, Helper, out diagnostics);
                case SpecialType.System_UInt16:
                    return UnmanagedHelper<ulong>.Validate(parameterSymbol, attributeData, ushort.MinValue, ushort.MaxValue, Helper, out diagnostics);
                case SpecialType.System_UInt32:
                    return UnmanagedHelper<ulong>.Validate(parameterSymbol, attributeData, uint.MinValue, uint.MaxValue, Helper, out diagnostics);
                case SpecialType.System_UInt64:
                    return UnmanagedHelper<ulong>.Validate(parameterSymbol, attributeData, ulong.MinValue, ulong.MaxValue, Helper, out diagnostics);
                case SpecialType.System_UIntPtr:
                    return UnmanagedHelper<ulong>.Validate(parameterSymbol, attributeData, uint.MinValue, uint.MaxValue, Helper, out diagnostics);
                case SpecialType.System_SByte:
                    return UnmanagedHelper<long>.Validate(parameterSymbol, attributeData, sbyte.MinValue, sbyte.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Int16:
                    return UnmanagedHelper<long>.Validate(parameterSymbol, attributeData, short.MinValue, short.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Int32:
                    return UnmanagedHelper<long>.Validate(parameterSymbol, attributeData, int.MinValue, int.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Int64:
                    return UnmanagedHelper<long>.Validate(parameterSymbol, attributeData, long.MinValue, long.MaxValue, Helper, out diagnostics);
                case SpecialType.System_IntPtr:
                    return UnmanagedHelper<long>.Validate(parameterSymbol, attributeData, int.MinValue, int.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Single:
                    return UnmanagedHelper<float>.Validate(parameterSymbol, attributeData, float.MinValue, float.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Double:
                    return UnmanagedHelper<double>.Validate(parameterSymbol, attributeData, double.MinValue, double.MaxValue, Helper, out diagnostics);
                case SpecialType.System_Boolean:
                    return UnmanagedHelper<bool>.Validate(parameterSymbol, attributeData, false, true, Helper, out diagnostics);
                case SpecialType.System_String:
                    return StringConstantExpectedParameter.Validate(attributeData, out diagnostics);
                case SpecialType.None when parameterSymbol.Type.TypeKind == TypeKind.TypeParameter:
                    return ValidateGenericConstantCase(attributeData, out diagnostics);
                default:
                    diagnostics = Helper.ParameterIsInvalid(parameterSymbol.Type.ToDisplayString(), attributeData.ApplicationSyntaxReference.GetSyntax());
                    return false;
            }
        }
        private bool ValidateGenericConstantCase(AttributeData attributeData, out ImmutableArray<Diagnostic> diagnostics)
        {
            var (min, max) = GetAttributeConstants(attributeData);
            // Ensure min max is unassigned/null
            if (min is not null || max is not null)
            {
                diagnostics = ImmutableArray.Create(
                    Diagnostic.Create(IncompatibleConstantForMinMaxRule,
                    attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation(),
                    "generic"));
                return false;
            }
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return true;
        }

        private static bool TryGetConstantExpectedAttributeData(IParameterSymbol parameter, [NotNullWhen(true)] out AttributeData? attributeData)
        {
            AttributeData? constantExpectedAttributeData = parameter.GetAttributes()
                .FirstOrDefault(attrData => IsConstantExpectedAttribute(attrData.AttributeClass));
            attributeData = constantExpectedAttributeData;
            return constantExpectedAttributeData is not null;
        }

        private static bool IsConstantExpectedAttribute(INamedTypeSymbol namedType)
        {
            return namedType.Name.Equals(ConstantExpectedAttribute, StringComparison.Ordinal) &&
                   namedType.GetMembers().OfType<IPropertySymbol>()
                       .All(s => s.Name.Equals("Min", StringComparison.Ordinal) ||
                                 s.Name.Equals("Max", StringComparison.Ordinal));
        }

        private abstract class ConstantExpectedParameter
        {
            protected ConstantExpectedParameter(IParameterSymbol parameter, SyntaxNode attributeSyntax)
            {
                Parameter = parameter;
                AttributeSyntax = attributeSyntax;
            }

            public IParameterSymbol Parameter { get; }
            public SyntaxNode AttributeSyntax { get; }

            public abstract bool ValidateValue(IArgumentOperation argument, object? constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics);
            public abstract bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, [NotNullWhen(false)] out Diagnostic? validationDiagnostics);
        }

        private sealed class StringConstantExpectedParameter : ConstantExpectedParameter
        {
            public StringConstantExpectedParameter(IParameterSymbol parameter, SyntaxNode attributeSyntax) : base(parameter, attributeSyntax) { }

            public override bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
            {
                if (subsetCandidate is not StringConstantExpectedParameter)
                {
                    validationDiagnostics = Diagnostic.Create(AttributeNotSameTypeRule, subsetCandidate.AttributeSyntax.GetLocation(), Parameter.Type.ToDisplayString());
                    return false;
                }
                validationDiagnostics = null;
                return true;
            }

            public override bool ValidateValue(IArgumentOperation argument, object? constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
            {
                if (constant is not string and not null)
                {
                    validationDiagnostics = argument.CreateDiagnostic(ConstantNotConstantRule);
                    return false;
                }
                validationDiagnostics = null;
                return true;
            }

            public static bool TryCreate(IParameterSymbol parameterSymbol, AttributeData attributeData, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
            {
                if (!IsMinMaxValid(attributeData))
                {
                    parameter = null;
                    return false;
                }
                parameter = new StringConstantExpectedParameter(parameterSymbol, attributeData.ApplicationSyntaxReference.GetSyntax());
                return true;
            }

            public static bool Validate(AttributeData attributeData, out ImmutableArray<Diagnostic> diagnostics)
            {
                if (!IsMinMaxValid(attributeData))
                {
                    diagnostics = ImmutableArray.Create(Diagnostic.Create(IncompatibleConstantForMinMaxRule, attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation(), "string"));
                    return false;
                }
                diagnostics = ImmutableArray<Diagnostic>.Empty;
                return true;
            }

            private static bool IsMinMaxValid(AttributeData attributeData)
            {
                (object? min, object? max) = GetAttributeConstants(attributeData);

                return min is null && max is null;
            }
        }

        public static (object? MinConstant, object? MaxConstant) GetAttributeConstants(AttributeData attributeData)
        {
            object? minConstant = null;
            object? maxConstant = null;

            foreach (var namedArg in attributeData.NamedArguments)
            {
                if (namedArg.Key.Equals("Min", StringComparison.Ordinal))
                {
                    minConstant = ToObject(namedArg.Value);
                }
                else if (namedArg.Key.Equals("Max", StringComparison.Ordinal))
                {
                    maxConstant = ToObject(namedArg.Value);
                }
            }

            return (minConstant, maxConstant);
            static object? ToObject(TypedConstant typedConstant)
            {
                if (typedConstant.IsNull)
                {
                    return null;
                }
                return typedConstant.Kind == TypedConstantKind.Array ? typedConstant.Values : typedConstant.Value;
            }
        }

        protected abstract class DiagnosticHelper
        {
            public abstract Location? GetMinLocation(SyntaxNode attributeSyntax);
            public abstract Location? GetMaxLocation(SyntaxNode attributeSyntax);

            public ImmutableArray<Diagnostic> ParameterIsInvalid(string expectedTypeName, SyntaxNode attributeSyntax) => ImmutableArray.Create(Diagnostic.Create(InvalidTypeRule, attributeSyntax.GetLocation(), expectedTypeName));

            public Diagnostic MinIsIncompatible(string expectedTypeName, SyntaxNode attributeSyntax) => Diagnostic.Create(IncompatibleConstantTypeRule, GetMinLocation(attributeSyntax)!, "Min", expectedTypeName);

            public Diagnostic MaxIsIncompatible(string expectedTypeName, SyntaxNode attributeSyntax) => Diagnostic.Create(IncompatibleConstantTypeRule, GetMaxLocation(attributeSyntax)!, "Max", expectedTypeName);

            public Diagnostic MinIsOutOfRange(SyntaxNode attributeSyntax, string typeMinValue, string typeMaxValue) => Diagnostic.Create(InvalidBoundsRule, GetMinLocation(attributeSyntax)!, "Min", typeMinValue, typeMaxValue);

            public Diagnostic MaxIsOutOfRange(SyntaxNode attributeSyntax, string typeMinValue, string typeMaxValue) => Diagnostic.Create(InvalidBoundsRule, GetMaxLocation(attributeSyntax)!, "Max", typeMinValue, typeMaxValue);

            public static Diagnostic MinMaxIsInverted(SyntaxNode attributeSyntax) => Diagnostic.Create(InvertedRangeRule, attributeSyntax.GetLocation());

            public ImmutableArray<Diagnostic> GetError(ErrorKind errorFlags, IParameterSymbol parameterSymbol, SyntaxNode attributeSyntax, string typeMinValue, string typeMaxValue)
            {
                switch (errorFlags)
                {
                    case ErrorKind.MinIsIncompatible:
                        return ImmutableArray.Create(MinIsIncompatible(parameterSymbol.Type.ToDisplayString(), attributeSyntax));
                    case ErrorKind.MaxIsIncompatible:
                        return ImmutableArray.Create(MaxIsIncompatible(parameterSymbol.Type.ToDisplayString(), attributeSyntax));
                    case ErrorKind.MinIsIncompatible | ErrorKind.MaxIsIncompatible:
                        var expectedTypeName = parameterSymbol.Type.ToDisplayString();
                        return ImmutableArray.Create(MinIsIncompatible(expectedTypeName, attributeSyntax), MaxIsIncompatible(expectedTypeName, attributeSyntax));
                    case ErrorKind.MinIsOutOfRange:
                        return ImmutableArray.Create(MinIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue));
                    case ErrorKind.MaxIsOutOfRange:
                        return ImmutableArray.Create(MaxIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue));
                    case ErrorKind.MinIsOutOfRange | ErrorKind.MaxIsOutOfRange:
                        return ImmutableArray.Create(MinIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue), MaxIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue));
                    case ErrorKind.MinIsOutOfRange | ErrorKind.MaxIsIncompatible:
                        return ImmutableArray.Create(MinIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue), MaxIsIncompatible(parameterSymbol.Type.ToDisplayString(), attributeSyntax));
                    case ErrorKind.MinIsIncompatible | ErrorKind.MaxIsOutOfRange:
                        return ImmutableArray.Create(MinIsIncompatible(parameterSymbol.Type.ToDisplayString(), attributeSyntax), MaxIsOutOfRange(attributeSyntax, typeMinValue, typeMaxValue));
                    case ErrorKind.MinMaxInverted:
                        return ImmutableArray.Create(MinMaxIsInverted(attributeSyntax));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(errorFlags));
                }
            }
        }

        [Flags]
        protected enum ErrorKind
        {
            None = 0,
            /// <summary>
            /// mutually exclusive with MinIsIncompatible and MinMaxInverted
            /// </summary>
            MinIsOutOfRange = 1,
            /// <summary>
            /// mutually exclusive with MinIsOutOfRange and MinMaxInverted
            /// </summary>
            MinIsIncompatible = 1 << 2,
            /// <summary>
            /// mutually exclusive with MaxIsIncompatible and MinMaxInverted
            /// </summary>
            MaxIsOutOfRange = 1 << 3,
            /// <summary>
            /// mutually exclusive with MaxIsOutOfRange and MinMaxInverted
            /// </summary>
            MaxIsIncompatible = 1 << 4,
            /// <summary>
            /// mutually exclusive
            /// </summary>
            MinMaxInverted = 1 << 5,
        }
    }
}

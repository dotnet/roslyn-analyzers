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
        protected static readonly string ConstantExpectedAttribute = nameof(ConstantExpectedAttribute);
        protected static readonly string ConstantExpected = nameof(ConstantExpected);
        private static readonly LocalizableString s_localizableApplicationTitle = CreateLocalizableResourceString(nameof(ConstantExpectedApplicationTitle));
        private static readonly LocalizableString s_localizableApplicationDescription = CreateLocalizableResourceString(nameof(ConstantExpectedApplicationDescription));
        private static readonly LocalizableString s_localizableUsageTitle = CreateLocalizableResourceString(nameof(ConstantExpectedUsageTitle));
        private static readonly LocalizableString s_localizableUsageDescription = CreateLocalizableResourceString(nameof(ConstantExpectedUsageDescription));

        internal static class CA1860
        {
            internal const string Id = nameof(CA1860);
            internal const RuleLevel Level = RuleLevel.BuildError;
            internal static readonly DiagnosticDescriptor UnsupportedTypeRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableApplicationTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedNotSupportedMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableApplicationDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor IncompatibleConstantTypeRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableApplicationTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedIncompatibleConstantTypeMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableApplicationDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor InvalidBoundsRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableApplicationTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedInvalidBoundsMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableApplicationDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor InvertedRangeRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableApplicationTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedInvertedRangeMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableApplicationDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);
        }

        internal static class CA1861
        {
            internal const string Id = nameof(CA1861);
            internal const RuleLevel Level = RuleLevel.BuildWarning;

            internal static readonly DiagnosticDescriptor ConstantOutOfBoundsRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableUsageTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedOutOfBoundsMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableUsageDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor ConstantNotConstantRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableUsageTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedNotConstantMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableUsageDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor ConstantInvalidConstantRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableUsageTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedInvalidMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableUsageDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);

            internal static readonly DiagnosticDescriptor AttributeExpectedRule = DiagnosticDescriptorHelper.Create(
                Id,
                s_localizableUsageTitle,
                CreateLocalizableResourceString(nameof(ConstantExpectedAttributExpectedMessage)),
                DiagnosticCategory.Performance,
                Level,
                description: s_localizableUsageDescription,
                isPortedFxCopRule: false,
                isDataflowRule: false);
        }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            CA1860.UnsupportedTypeRule, CA1860.IncompatibleConstantTypeRule,
            CA1860.InvalidBoundsRule, CA1860.InvertedRangeRule,
            CA1861.ConstantOutOfBoundsRule, CA1861.ConstantInvalidConstantRule,
            CA1861.ConstantNotConstantRule, CA1861.AttributeExpectedRule);

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
            context.RegisterSymbolAction(context => OnMethodSymbol(context), SymbolKind.Method);
            RegisterAttributeSyntax(context);
        }

        private static void OnMethodSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            if (TryGetMethodInterface(methodSymbol, out var interfaceMethodSymbol))
            {
                CheckAttribute(context, methodSymbol.Parameters, interfaceMethodSymbol.Parameters);
            }
            else if (methodSymbol.OverriddenMethod is not null)
            {
                CheckAttribute(context, methodSymbol.Parameters, methodSymbol.OverriddenMethod.Parameters);
            }

            static void CheckAttribute(SymbolAnalysisContext context, ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IParameterSymbol> baseParameters)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (!IsConstantCompatible(parameter.Type))
                    {
                        continue;
                    }
                    var baseParameter = baseParameters[i];
                    if (HasConstantExpectedAttributeData(baseParameter) && !HasConstantExpectedAttributeData(parameter))
                    {
                        // mark the parameter including the type and name
                        var diagnostic = parameter.DeclaringSyntaxReferences[0].GetSyntax().CreateDiagnostic(CA1861.AttributeExpectedRule);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
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
                    if (!argConstantParameter.ValidateParameterIsWithinRange(currConstantParameter, argument, out var parameterCheckDiagnostic))
                    {
                        context.ReportDiagnostic(parameterCheckDiagnostic);
                    }
                    continue;
                }
                var constantValue = v.ConstantValue;
                if (!constantValue.HasValue)
                {
                    var location = argument.Syntax.GetLocation();
                    context.ReportDiagnostic(Diagnostic.Create(CA1861.ConstantNotConstantRule, location));
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
                    return Validate(parameterSymbol, attributeData, Helper, out diagnostics);
                case SpecialType.None when parameterSymbol.Type.TypeKind == TypeKind.TypeParameter:
                    return Validate(parameterSymbol, attributeData, Helper, out diagnostics);
                default:
                    diagnostics = Helper.ParameterIsInvalid(parameterSymbol.Type.ToDisplayString(), attributeData.ApplicationSyntaxReference.GetSyntax());
                    return false;
            }
        }

        private static bool TryGetConstantExpectedAttributeData(IParameterSymbol parameter, [NotNullWhen(true)] out AttributeData? attributeData)
        {
            AttributeData? constantExpectedAttributeData = parameter.GetAttributes()
                .FirstOrDefault(attrData => IsConstantExpectedAttribute(attrData.AttributeClass));
            attributeData = constantExpectedAttributeData;
            return constantExpectedAttributeData is not null;
        }

        private static bool HasConstantExpectedAttributeData(IParameterSymbol parameter)
        {
            return parameter.GetAttributes()
                .Any(attrData => IsConstantExpectedAttribute(attrData.AttributeClass));
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
            protected ConstantExpectedParameter(IParameterSymbol parameter)
            {
                Parameter = parameter;
            }

            public IParameterSymbol Parameter { get; }

            public abstract bool ValidateValue(IArgumentOperation argument, object? constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics);
            public abstract bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, IArgumentOperation argument, [NotNullWhen(false)] out Diagnostic? validationDiagnostics);
        }

        private sealed class StringConstantExpectedParameter : ConstantExpectedParameter
        {
            public StringConstantExpectedParameter(IParameterSymbol parameter) : base(parameter) { }

            public override bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, IArgumentOperation argument, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
            {
                if (subsetCandidate is not StringConstantExpectedParameter)
                {
                    validationDiagnostics = Diagnostic.Create(CA1861.ConstantInvalidConstantRule, argument.Syntax.GetLocation(), Parameter.Type.ToDisplayString());
                    return false;
                }
                validationDiagnostics = null;
                return true;
            }

            public override bool ValidateValue(IArgumentOperation argument, object? constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
            {
                if (constant is not string and not null)
                {
                    validationDiagnostics = argument.CreateDiagnostic(CA1861.ConstantInvalidConstantRule, Parameter.Type.ToDisplayString());
                    return false;
                }
                validationDiagnostics = null;
                return true;
            }

            public static bool TryCreate(IParameterSymbol parameterSymbol, AttributeData attributeData, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
            {
                if (!IsMinMaxValid(attributeData, out _))
                {
                    parameter = null;
                    return false;
                }
                parameter = new StringConstantExpectedParameter(parameterSymbol);
                return true;
            }
        }

        private static bool Validate(IParameterSymbol parameterSymbol, AttributeData attributeData, DiagnosticHelper helper, out ImmutableArray<Diagnostic> diagnostics)
        {
            if (!IsMinMaxValid(attributeData, out ErrorKind errorFlags))
            {
                diagnostics = helper.GetError(errorFlags, parameterSymbol, attributeData.ApplicationSyntaxReference.GetSyntax(), "null", "null");
                return false;
            }
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return true;
        }

        private static bool IsMinMaxValid(AttributeData attributeData, out ErrorKind errorFlags)
        {
            (object? min, object? max) = GetAttributeConstants(attributeData);
            errorFlags = 0;
            if (min is not null)
            {
                errorFlags |= ErrorKind.MinIsIncompatible;
            }
            if (max is not null)
            {
                errorFlags |= ErrorKind.MaxIsIncompatible;
            }
            return min is null && max is null;
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

        private static bool TryGetMethodInterface(IMethodSymbol methodSymbol, [NotNullWhen(true)] out IMethodSymbol? interfaceMethodSymbol)
        {
            var explicitInterface = methodSymbol.ExplicitInterfaceImplementations
                .FirstOrDefault(exInterface => methodSymbol.IsImplementationOfInterfaceMember(exInterface));
            if (explicitInterface is not null)
            {
                interfaceMethodSymbol = explicitInterface;
                return true;
            }

            if (methodSymbol.ContainingType is not null)
            {
                foreach (INamedTypeSymbol interfaceSymbol in methodSymbol.ContainingType.AllInterfaces)
                {
                    foreach (var interfaceMember in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (methodSymbol.IsImplementationOfInterfaceMember(interfaceMember))
                        {
                            interfaceMethodSymbol = interfaceMember;
                            return true;
                        }
                    }
                }
            }

            interfaceMethodSymbol = null;
            return false;
        }

        private static bool IsConstantCompatible(ITypeSymbol type)
        {
            return type.SpecialType switch
            {
                SpecialType.System_Char => true,
                SpecialType.System_Byte => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_UInt64 => true,
                SpecialType.System_UIntPtr => true,
                SpecialType.System_SByte => true,
                SpecialType.System_Int16 => true,
                SpecialType.System_Int32 => true,
                SpecialType.System_Int64 => true,
                SpecialType.System_IntPtr => true,
                SpecialType.System_Single => true,
                SpecialType.System_Double => true,
                SpecialType.System_Boolean => true,
                SpecialType.System_String => true,
                SpecialType.None when type.TypeKind == TypeKind.TypeParameter => true,
                _ => false,
            };
        }

        protected abstract class DiagnosticHelper
        {
            public abstract Location? GetMinLocation(SyntaxNode attributeSyntax);
            public abstract Location? GetMaxLocation(SyntaxNode attributeSyntax);

            public ImmutableArray<Diagnostic> ParameterIsInvalid(string expectedTypeName, SyntaxNode attributeSyntax) => ImmutableArray.Create(Diagnostic.Create(CA1860.UnsupportedTypeRule, attributeSyntax.GetLocation(), expectedTypeName));

            public Diagnostic MinIsIncompatible(string expectedTypeName, SyntaxNode attributeSyntax) => Diagnostic.Create(CA1860.IncompatibleConstantTypeRule, GetMinLocation(attributeSyntax)!, "Min", expectedTypeName);

            public Diagnostic MaxIsIncompatible(string expectedTypeName, SyntaxNode attributeSyntax) => Diagnostic.Create(CA1860.IncompatibleConstantTypeRule, GetMaxLocation(attributeSyntax)!, "Max", expectedTypeName);

            public Diagnostic MinIsOutOfRange(SyntaxNode attributeSyntax, string typeMinValue, string typeMaxValue) => Diagnostic.Create(CA1860.InvalidBoundsRule, GetMinLocation(attributeSyntax)!, "Min", typeMinValue, typeMaxValue);

            public Diagnostic MaxIsOutOfRange(SyntaxNode attributeSyntax, string typeMinValue, string typeMaxValue) => Diagnostic.Create(CA1860.InvalidBoundsRule, GetMaxLocation(attributeSyntax)!, "Max", typeMinValue, typeMaxValue);

            public static Diagnostic MinMaxIsInverted(SyntaxNode attributeSyntax) => Diagnostic.Create(CA1860.InvertedRangeRule, attributeSyntax.GetLocation());

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

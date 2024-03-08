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

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferConvertToHexStringOverBitConverterAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1872";

        private const string Empty = nameof(Empty);
        private const string Replace = nameof(Replace);
        private const string ToHexString = nameof(ToHexString);
        private const string ToLower = nameof(ToLower);
        private const string ToLowerInvariant = nameof(ToLowerInvariant);

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(PreferConvertToHexStringOverBitConverterTitle)),
            CreateLocalizableResourceString(nameof(PreferConvertToHexStringOverBitConverterMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferConvertToHexStringOverBitConverterDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!RequiredSymbols.TryGetSymbols(context.Compilation, out var symbols))
            {
                return;
            }

            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);

            void AnalyzeInvocation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;

                if (!symbols.IsAnyStringReplaceMethod(invocation.TargetMethod) ||
                    invocation.GetInstance() is not IInvocationOperation instanceInvocation ||
                    !symbols.TryGetBitConverterToStringInvocation(instanceInvocation, out var bitConverterInvocation, out var _) ||
                    !HasStringReplaceArgumentsToRemoveHyphen(invocation.Arguments))
                {
                    return;
                }

                // Null forgiving operator is okay because we would have bailed out if the required Convert.ToHexString method was null.
                context.ReportDiagnostic(invocation.CreateDiagnostic(
                    Rule,
                    bitConverterInvocation.TargetMethod.Parameters.Length == 1
                        ? symbols.ConvertToHexString!.ToDisplayString()
                        : symbols.ConvertToHexStringStartLength!.ToDisplayString(),
                    bitConverterInvocation.TargetMethod.ToDisplayString()));

                bool HasStringReplaceArgumentsToRemoveHyphen(ImmutableArray<IArgumentOperation> arguments)
                {
                    var argumentsInParameterOrder = arguments.GetArgumentsInParameterOrder();
                    var oldValue = argumentsInParameterOrder[0].Value;
                    var newValue = argumentsInParameterOrder[1].Value;

                    bool oldValueIsConstantHyphenString = oldValue.ConstantValue.HasValue &&
                        oldValue.ConstantValue.Value is string oldValueString &&
                        oldValueString.Equals("-", StringComparison.Ordinal);

                    bool newValueIsConstantNullOrEmptyString = newValue.ConstantValue.HasValue &&
                        newValue.ConstantValue.Value is string newValueString &&
                        string.IsNullOrEmpty(newValueString);

                    return oldValueIsConstantHyphenString &&
                        (newValueIsConstantNullOrEmptyString || newValue.HasNullConstantValue() || symbols.IsStringEmptyField(newValue));
                }
            }
        }

        // Internal as this is also used by the fixer.
        internal sealed class RequiredSymbols
        {
            private RequiredSymbols(
                ImmutableArray<IMethodSymbol> stringReplaceMethods,
                ImmutableArray<IMethodSymbol> stringToLowerMethods,
                ImmutableArray<IMethodSymbol> bitConverterToStringMethods,
                IFieldSymbol? stringEmptyField,
                IMethodSymbol? convertToHexString,
                IMethodSymbol? convertToHexStringStartLength)
            {
                _stringReplaceMethods = stringReplaceMethods;
                _stringToLowerMethods = stringToLowerMethods;
                _bitConverterToStringMethods = bitConverterToStringMethods;
                _stringEmptyField = stringEmptyField;
                ConvertToHexString = convertToHexString;
                ConvertToHexStringStartLength = convertToHexStringStartLength;
            }

            public static bool TryGetSymbols(Compilation compilation, [NotNullWhen(true)] out RequiredSymbols? symbols)
            {
                symbols = default;

                var bitConverterType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBitConverter);
                var convertType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemConvert);

                // Bail out if we do not have a BitConverter or Convert type.
                if (bitConverterType is null || convertType is null)
                {
                    return false;
                }

                var byteArrayType = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Byte));
                var int32Type = compilation.GetSpecialType(SpecialType.System_Int32);
                var stringType = compilation.GetSpecialType(SpecialType.System_String);
                var rosType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1);

                var stringReplaceMethods = stringType.GetMembers(Replace)
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(FilterStringReplaceMethods);
                var stringToLowerMethods = stringType.GetMembers(ToLower)
                    .AddRange(stringType.GetMembers(ToLowerInvariant))
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();
                var bitConverterToStringMethods = bitConverterType.GetMembers(WellKnownMemberNames.ObjectToString)
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(FilterBitConverterToStringMethods);
                var stringEmptyField = stringType.GetMembers(Empty)
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault();

                var bitConverterToString = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType);
                var bitConverterToStringStart = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type);
                var bitConverterToStringStartLength = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type, int32Type);

                var convertToHexStringMethods = convertType.GetMembers(ToHexString).OfType<IMethodSymbol>();
                var convertToHexString = convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType);
                var convertToHexStringRos = rosType is not null ? convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(rosType) : null;
                var convertToHexStringStartLength = convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type, int32Type);

                // BitConverter.ToString(data) needs Convert.ToHexString(data)
                bool hasMatchingMethodsNoAdditionalParams = bitConverterToString is not null && convertToHexString is not null;

                // BitConverter.ToString(data, start) needs ROS overload Convert.ToHexString(bytes)
                bool hasMatchingMethodsStart = bitConverterToStringStart is not null && convertToHexStringRos is not null;

                // BitConverter.ToString(data, start, length) needs Convert.ToHexString(data, start, length)
                bool hasMatchingMethodsStartLength = bitConverterToStringStartLength is not null && convertToHexStringStartLength is not null;

                // Bail out if we do not have a matching pair of BitConverter.ToString and Convert.ToHexString methods or no string.Replace method.
                if ((!hasMatchingMethodsNoAdditionalParams && !hasMatchingMethodsStart && !hasMatchingMethodsStartLength) ||
                    stringReplaceMethods.IsEmpty)
                {
                    return false;
                }

                symbols = new RequiredSymbols(stringReplaceMethods, stringToLowerMethods, bitConverterToStringMethods,
                    stringEmptyField, convertToHexString, convertToHexStringStartLength);

                return true;

                bool FilterStringReplaceMethods(IMethodSymbol stringReplaceMethod)
                {
                    return stringReplaceMethod.Parameters.Length >= 2 &&
                        SymbolEqualityComparer.Default.Equals(stringReplaceMethod.Parameters[0].Type, stringType) &&
                        SymbolEqualityComparer.Default.Equals(stringReplaceMethod.Parameters[1].Type, stringType);
                }

                bool FilterBitConverterToStringMethods(IMethodSymbol bitConverterToStringMethod)
                {
                    return bitConverterToStringMethod.Parameters.Length switch
                    {
                        1 => SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[0].Type, byteArrayType),
                        2 =>
                            SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[0].Type, byteArrayType) &&
                            SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[1].Type, int32Type),
                        3 =>
                            SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[0].Type, byteArrayType) &&
                            SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[1].Type, int32Type) &&
                            SymbolEqualityComparer.Default.Equals(bitConverterToStringMethod.Parameters[2].Type, int32Type),
                        _ => false
                    };
                }
            }

            /// <summary>
            /// Attempts to get a BitConverter.ToString invocation (and optionally a string.ToLower* invocation) from <paramref name="invocation"/> and its instance:
            ///   1. Checks if <paramref name="invocation"/> is a BitConverter.ToString invocation.
            ///   2. Checks if <paramref name="invocation"/> is a string.ToLower* invocation.
            ///      If it is, check if the instance of <paramref name="invocation"/> is a BitConverter.ToString invocation.
            /// </summary>
            /// <param name="invocation">The invocation and its instance to check</param>
            /// <param name="bitConverterToStringInvocation">The extracted BitConverter.ToString invocation, or null if unsuccessful</param>
            /// <param name="toLowerInvocation">The optionally extracted string.ToLower* invocation</param>
            /// <returns>true if a BitConverter.ToString invocation could be extracted</returns>
            public bool TryGetBitConverterToStringInvocation(
                IInvocationOperation invocation,
                [NotNullWhen(true)] out IInvocationOperation? bitConverterToStringInvocation,
                out IInvocationOperation? toLowerInvocation)
            {
                bitConverterToStringInvocation = default;
                toLowerInvocation = default;

                if (IsAnyBitConverterToStringMethod(invocation.TargetMethod))
                {
                    bitConverterToStringInvocation = invocation;

                    return true;
                }

                // Check if target method is a string.ToLower* method.
                // If so, check if the instance of the string.ToLower* invocation is a BitConverter.ToString method.
                if (!IsAnyStringToLowerMethod(invocation.TargetMethod) ||
                    invocation.GetInstance() is not IInvocationOperation instanceInvocation ||
                    !IsAnyBitConverterToStringMethod(instanceInvocation.TargetMethod))
                {
                    return false;
                }

                bitConverterToStringInvocation = instanceInvocation;
                toLowerInvocation = invocation;

                return true;
            }

            public bool IsAnyStringReplaceMethod(IMethodSymbol method)
            {
                return _stringReplaceMethods.Any(m => SymbolEqualityComparer.Default.Equals(m, method));
            }

            public bool IsAnyStringToLowerMethod(IMethodSymbol method)
            {
                return _stringToLowerMethods.Any(m => SymbolEqualityComparer.Default.Equals(m, method));
            }

            public bool IsAnyBitConverterToStringMethod(IMethodSymbol method)
            {
                return _bitConverterToStringMethods.Any(m => SymbolEqualityComparer.Default.Equals(m, method));
            }

            public bool IsStringEmptyField(IOperation operation)
            {
                return operation is IFieldReferenceOperation fieldReferenceOperation &&
                    SymbolEqualityComparer.Default.Equals(fieldReferenceOperation.Field, _stringEmptyField);
            }

            public IMethodSymbol? ConvertToHexString { get; }
            public IMethodSymbol? ConvertToHexStringStartLength { get; }

            private readonly ImmutableArray<IMethodSymbol> _stringReplaceMethods;
            private readonly ImmutableArray<IMethodSymbol> _stringToLowerMethods;
            private readonly ImmutableArray<IMethodSymbol> _bitConverterToStringMethods;
            private readonly IFieldSymbol? _stringEmptyField;
        }
    }
}

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
        private const string ToHexStringLower = nameof(ToHexStringLower);
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

                if (!symbols.TryGetBitConverterToStringInvocationAndReplacement(invocation, out var bitConverterInvocation, out var convertToHexStringMethod, out var _))
                {
                    return;
                }

                context.ReportDiagnostic(invocation.CreateDiagnostic(
                    Rule,
                    convertToHexStringMethod.ToDisplayString(),
                    bitConverterInvocation.TargetMethod.ToDisplayString()));
            }
        }

        // Internal as this is also used by the fixer.
        internal sealed class RequiredSymbols
        {
            private RequiredSymbols(
                ImmutableArray<IMethodSymbol> stringReplaceMethods,
                ImmutableArray<IMethodSymbol> stringToLowerMethods,
                IFieldSymbol? stringEmptyField,
                ImmutableDictionary<IMethodSymbol, IMethodSymbol> bitConverterReplacements,
                ImmutableDictionary<IMethodSymbol, IMethodSymbol> bitConverterReplacementsToLower)
            {
                _stringReplaceMethods = stringReplaceMethods;
                _stringToLowerMethods = stringToLowerMethods;
                _stringEmptyField = stringEmptyField;
                _bitConverterReplacements = bitConverterReplacements;
                _bitConverterReplacementsToLower = bitConverterReplacementsToLower;
            }

            public static bool TryGetSymbols(Compilation compilation, [NotNullWhen(true)] out RequiredSymbols? symbols)
            {
                symbols = default;

                var bitConverterType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemBitConverter);
                var convertType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemConvert);

                if (bitConverterType is null || convertType is null)
                {
                    return false;
                }

                var byteType = compilation.GetSpecialType(SpecialType.System_Byte);
                var byteArrayType = compilation.CreateArrayTypeSymbol(byteType);
                var int32Type = compilation.GetSpecialType(SpecialType.System_Int32);
                var stringType = compilation.GetSpecialType(SpecialType.System_String);
                var rosByteType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1)?.Construct(byteType);

                var stringReplaceMethods = stringType.GetMembers(Replace)
                    .OfType<IMethodSymbol>()
                    .WhereAsArray(FilterStringReplaceMethods);

                if (stringReplaceMethods.IsEmpty)
                {
                    return false;
                }

                var stringToLowerMethods = stringType.GetMembers(ToLower)
                    .AddRange(stringType.GetMembers(ToLowerInvariant))
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();
                var stringEmptyField = stringType.GetMembers(Empty)
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault();

                var bitConverterToStringMethods = bitConverterType.GetMembers(WellKnownMemberNames.ObjectToString)
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();

                var bitConverterToString = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType);
                var bitConverterToStringStart = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type);
                var bitConverterToStringStartLength = bitConverterToStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type, int32Type);

                var convertToHexStringMethods = convertType.GetMembers(ToHexString)
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();

                var convertToHexString = convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType);
                var convertToHexStringRos = rosByteType is not null ? convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(rosByteType) : null;
                var convertToHexStringStartLength = convertToHexStringMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type, int32Type);

                var bitConverterReplacementsBuilder = ImmutableDictionary.CreateBuilder<IMethodSymbol, IMethodSymbol>();
                // BitConverter.ToString(data).Replace("-", "") => Convert.ToHexString(data)
                bitConverterReplacementsBuilder.AddKeyValueIfNotNull(bitConverterToString, convertToHexString);
                // BitConverter.ToString(data, start).Replace("-", "") => Convert.ToHexString(data.AsSpan().Slice(start))
                bitConverterReplacementsBuilder.AddKeyValueIfNotNull(bitConverterToStringStart, convertToHexStringRos);
                // BitConverter.ToString(data, start, length).Replace("-", "") => Convert.ToHexString(data, start, length)
                bitConverterReplacementsBuilder.AddKeyValueIfNotNull(bitConverterToStringStartLength, convertToHexStringStartLength);
                var bitConverterReplacements = bitConverterReplacementsBuilder.ToImmutableDictionary();

                // Bail out if we have no valid replacement pair from BitConverter to Convert.ToHexString.
                if (bitConverterReplacements.IsEmpty)
                {
                    return false;
                }

                var convertToHexStringLowerMethods = convertType.GetMembers(ToHexStringLower)
                    .OfType<IMethodSymbol>()
                    .ToImmutableArray();

                var convertToHexStringLower = convertToHexStringLowerMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType);
                var convertToHexStringLowerRos = rosByteType is not null ? convertToHexStringLowerMethods.GetFirstOrDefaultMemberWithParameterTypes(rosByteType) : null;
                var convertToHexStringLowerStartLength = convertToHexStringLowerMethods.GetFirstOrDefaultMemberWithParameterTypes(byteArrayType, int32Type, int32Type);

                // The following replacements are optional: Convert.ToHexStringLower is available as of .NET 9.
                var bitConverterReplacementsToLowerBuilder = ImmutableDictionary.CreateBuilder<IMethodSymbol, IMethodSymbol>();
                // BitConverter.ToString(data).Replace("-", "").ToLower() => Convert.ToHexStringLower(data)
                bitConverterReplacementsToLowerBuilder.AddKeyValueIfNotNull(bitConverterToString, convertToHexStringLower);
                // BitConverter.ToString(data, start).Replace("-", "").ToLower() => Convert.ToHexStringLower(data.AsSpan().Slice(start))
                bitConverterReplacementsToLowerBuilder.AddKeyValueIfNotNull(bitConverterToStringStart, convertToHexStringLowerRos);
                // BitConverter.ToString(data, start, length).Replace("-", "").ToLower() => Convert.ToHexStringLower(data, start, length)
                bitConverterReplacementsToLowerBuilder.AddKeyValueIfNotNull(bitConverterToStringStartLength, convertToHexStringLowerStartLength);
                var bitConverterReplacementsToLower = bitConverterReplacementsToLowerBuilder.ToImmutableDictionary();

                symbols = new RequiredSymbols(stringReplaceMethods, stringToLowerMethods,
                    stringEmptyField, bitConverterReplacements, bitConverterReplacementsToLower);

                return true;

                bool FilterStringReplaceMethods(IMethodSymbol stringReplaceMethod)
                {
                    return stringReplaceMethod.Parameters.Length >= 2 &&
                        SymbolEqualityComparer.Default.Equals(stringReplaceMethod.Parameters[0].Type, stringType) &&
                        SymbolEqualityComparer.Default.Equals(stringReplaceMethod.Parameters[1].Type, stringType);
                }
            }

            /// <summary>
            /// Attempts to get a BitConverter.ToString invocation, its appropriate replacement method and optionally a string.ToLower* invocation:
            ///   1. Check if invocation is a string.ToLower* invocation. If so, continue the next steps with its instance.
            ///   2. Check if invocation is a string.Replace("-", "") invocation. Abort if not. Continue the next steps with its instance.
            ///   3. Check if invocation is a string.ToLower* invocation. If so, continue the next steps with its instance.
            ///   4. Check if invocation is a BitConverter.ToString invocation. If not, abort.
            /// Note that <paramref name="toLowerInvocation"/> is only set if a string.ToLower invocation is found AND the method Convert.ToHexStringLower is not available.
            /// </summary>
            /// <param name="invocation">The starting invocation</param>
            /// <param name="bitConverterToStringInvocation">The extracted BitConverter.ToString invocation, or null if unsuccessful</param>
            /// <param name="replacementMethod">The Convert.ToHexString* method to use as a replacement</param>
            /// <param name="toLowerInvocation">The optionally extracted string.ToLower* invocation</param>
            /// <returns>true if a BitConverter.ToString invocation and its replacement could be extracted</returns>
            public bool TryGetBitConverterToStringInvocationAndReplacement(
                IInvocationOperation invocation,
                [NotNullWhen(true)] out IInvocationOperation? bitConverterToStringInvocation,
                [NotNullWhen(true)] out IMethodSymbol? replacementMethod,
                out IInvocationOperation? toLowerInvocation)
            {
                bitConverterToStringInvocation = default;
                replacementMethod = default;
                toLowerInvocation = default;

                // This check is to prevent two diagnostics when encountering: BitConverter.ToString(data).Replace("-", "").ToLower();
                // Without this check, this case would report one diagnostic when analyzing Replace and one when analyzing ToLower.
                if (invocation.Parent is IInvocationOperation parentInvocation &&
                    IsAnyStringToLowerMethod(parentInvocation.TargetMethod))
                {
                    return false;
                }

                if (IsAnyStringToLowerMethod(invocation.TargetMethod))
                {
                    toLowerInvocation = invocation;

                    if (!TryGetInstanceInvocation(invocation, out invocation!))
                    {
                        return false;
                    }
                }

                if (!IsAnyStringReplaceMethod(invocation.TargetMethod) ||
                    !HasStringReplaceArgumentsToRemoveHyphen(invocation.Arguments) ||
                    !TryGetInstanceInvocation(invocation, out invocation!))
                {
                    return false;
                }

                if (IsAnyStringToLowerMethod(invocation.TargetMethod))
                {
                    toLowerInvocation = invocation;

                    if (!TryGetInstanceInvocation(invocation, out invocation!))
                    {
                        return false;
                    }
                }

                // Check if there is a valid replacement that uses Convert.ToHexStringLower or Convert.ToHexString.
                if (toLowerInvocation is not null &&
                    TryGetBitConverterReplacementToLower(invocation.TargetMethod, out var replacementToLower))
                {
                    bitConverterToStringInvocation = invocation;
                    replacementMethod = replacementToLower;
                    // Reset toLowerInvocation as we no longer need a ToLower after using ToHexStringLower
                    toLowerInvocation = default;

                    return true;
                }
                else if (TryGetBitConverterReplacement(invocation.TargetMethod, out var replacement))
                {
                    bitConverterToStringInvocation = invocation;
                    replacementMethod = replacement;

                    return true;
                }

                return false;

                static bool TryGetInstanceInvocation(IInvocationOperation invocation, [NotNullWhen(true)] out IInvocationOperation? instanceInvocation)
                {
                    instanceInvocation = invocation.GetInstance() as IInvocationOperation;

                    return instanceInvocation is not null;
                }
            }

            private bool IsAnyStringReplaceMethod(IMethodSymbol? method)
            {
                return _stringReplaceMethods.Any(m => SymbolEqualityComparer.Default.Equals(m, method));
            }

            private bool IsAnyStringToLowerMethod(IMethodSymbol? method)
            {
                return _stringToLowerMethods.Any(m => SymbolEqualityComparer.Default.Equals(m, method));
            }

            private bool IsStringEmptyField(IOperation operation)
            {
                return operation is IFieldReferenceOperation fieldReferenceOperation &&
                    SymbolEqualityComparer.Default.Equals(fieldReferenceOperation.Field, _stringEmptyField);
            }

            private bool HasStringReplaceArgumentsToRemoveHyphen(ImmutableArray<IArgumentOperation> arguments)
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
                    (newValueIsConstantNullOrEmptyString || newValue.HasNullConstantValue() || IsStringEmptyField(newValue));
            }

            private bool TryGetBitConverterReplacement(IMethodSymbol method, [NotNullWhen(true)] out IMethodSymbol? replacement)
            {
                replacement = _bitConverterReplacements.GetValueOrDefault(method);

                return replacement is not null;
            }

            private bool TryGetBitConverterReplacementToLower(IMethodSymbol method, [NotNullWhen(true)] out IMethodSymbol? replacement)
            {
                replacement = _bitConverterReplacementsToLower.GetValueOrDefault(method);

                return replacement is not null;
            }

            private readonly ImmutableArray<IMethodSymbol> _stringReplaceMethods;
            private readonly ImmutableArray<IMethodSymbol> _stringToLowerMethods;
            private readonly IFieldSymbol? _stringEmptyField;
            private readonly ImmutableDictionary<IMethodSymbol, IMethodSymbol> _bitConverterReplacements;
            private readonly ImmutableDictionary<IMethodSymbol, IMethodSymbol> _bitConverterReplacementsToLower;
        }
    }
}

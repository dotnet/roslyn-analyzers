﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract partial class ConstantExpectedAnalyzer
    {
        private sealed class UnmanagedHelper<T> where T : unmanaged
        {
            private static readonly UnmanagedHelper<T>.ConstantExpectedParameterFactory? _instance;
            private static UnmanagedHelper<T>.ConstantExpectedParameterFactory Instance => _instance ?? throw new InvalidOperationException("unsupported type");

            static UnmanagedHelper()
            {
                if (typeof(T) == typeof(long))
                {
                    var helper = new UnmanagedHelper<long>.TransformHelper(TryConvertInt64, TryTransformInt64);
                    _instance = new UnmanagedHelper<T>.ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var helper = new UnmanagedHelper<ulong>.TransformHelper(TryConvertUInt64, TryTransformUInt64);
                    _instance = new ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(float))
                {
                    var helper = new UnmanagedHelper<float>.TransformHelper(TryConvertSingle, TryTransformSingle);
                    _instance = new ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(double))
                {
                    var helper = new UnmanagedHelper<double>.TransformHelper(TryConvertDouble, TryTransformDouble);
                    _instance = new ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(char))
                {
                    var helper = new UnmanagedHelper<char>.TransformHelper(TryConvertChar, TryTransformChar);
                    _instance = new ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(bool))
                {
                    var helper = new UnmanagedHelper<bool>.TransformHelper(TryConvertBoolean, TryTransformBoolean);
                    _instance = new ConstantExpectedParameterFactory((UnmanagedHelper<T>.TransformHelper)(object)helper);
                }
            }

            public static bool TryCreate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
                => Instance.TryCreate(parameterSymbol, attributeData, typeMin, typeMax, out parameter);
            public static bool Validate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, DiagnosticHelper diagnosticHelper, out ImmutableArray<Diagnostic> diagnostics)
                => Instance.Validate(parameterSymbol, attributeData, typeMin, typeMax, diagnosticHelper, out diagnostics);

            public delegate bool TryTransform(object constant, out T value, out bool isInvalid);
            public delegate bool TryConvert(object? constant, out T value);
            public sealed class TransformHelper
            {
                private readonly TryTransform _tryTransform;
                private readonly TryConvert _convert;

                public TransformHelper(TryConvert convert, TryTransform tryTransform)
                {
                    _convert = convert;
                    _tryTransform = tryTransform;
                }

                public bool IsLessThan(T operand1, T operand2) => Comparer<T>.Default.Compare(operand1, operand2) < 0;
                public bool TryTransformMin(object constant, out T value, ref ErrorKind errorFlags)
                {
                    if (_tryTransform(constant, out value, out bool isInvalid))
                    {
                        return true;
                    }

                    if (isInvalid)
                    {
                        errorFlags |= ErrorKind.MinIsIncompatible;
                    }
                    else
                    {
                        errorFlags |= ErrorKind.MinIsOutOfRange;
                    }
                    return false;
                }

                public bool TryTransformMax(object constant, out T value, ref ErrorKind errorFlags)
                {
                    if (_tryTransform(constant, out value, out bool isInvalid))
                    {
                        return true;
                    }

                    if (isInvalid)
                    {
                        errorFlags |= ErrorKind.MaxIsIncompatible;
                    }
                    else
                    {
                        errorFlags |= ErrorKind.MaxIsOutOfRange;
                    }
                    return false;
                }
                public bool TryConvert(object? val, out T value) => _convert(val, out value);
            }

            public sealed class ConstantExpectedParameterFactory
            {
                private readonly TransformHelper _helper;

                public ConstantExpectedParameterFactory(TransformHelper helper)
                {
                    _helper = helper;
                }
                public bool Validate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, DiagnosticHelper diagnosticHelper, out ImmutableArray<Diagnostic> diagnostics)
                {
                    if (!IsValidMinMax(attributeData, typeMin, typeMax, out _, out _, out ErrorKind errorFlags))
                    {
                        diagnostics = diagnosticHelper.GetError(errorFlags, parameterSymbol, attributeData.ApplicationSyntaxReference.GetSyntax(), typeMin.ToString(), typeMax.ToString());
                        return false;
                    }

                    diagnostics = ImmutableArray<Diagnostic>.Empty;
                    return true;
                }

                public bool TryCreate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
                {
                    if (!IsValidMinMax(attributeData, typeMin, typeMax, out T minValue, out T maxValue, out _))
                    {
                        parameter = null;
                        return false;
                    }

                    parameter = new UnmanagedConstantExpectedParameter(parameterSymbol, attributeData.ApplicationSyntaxReference.GetSyntax(), minValue, maxValue, _helper);
                    return true;
                }

                private bool IsValidMinMax(AttributeData attributeData, T typeMin, T typeMax, out T minValue, out T maxValue, out ErrorKind errorFlags)
                {
                    minValue = typeMin;
                    maxValue = typeMax;
                    (object? min, object? max) = GetAttributeConstants(attributeData);
                    errorFlags = ErrorKind.None;
                    if (min is not null && _helper.TryTransformMin(min, out minValue, ref errorFlags))
                    {
                        if (_helper.IsLessThan(minValue, typeMin) || _helper.IsLessThan(typeMax, minValue))
                        {
                            errorFlags |= ErrorKind.MinIsOutOfRange;
                        }
                    }

                    if (max is not null && _helper.TryTransformMax(max, out maxValue, ref errorFlags))
                    {
                        if (_helper.IsLessThan(maxValue, typeMin) || _helper.IsLessThan(typeMax, maxValue))
                        {
                            errorFlags |= ErrorKind.MaxIsOutOfRange;
                        }
                    }

                    if (errorFlags != ErrorKind.None)
                    {
                        return false;
                    }

                    if (_helper.IsLessThan(maxValue, minValue))
                    {
                        errorFlags = ErrorKind.MinMaxInverted;
                        return false;
                    }
                    return true;
                }
            }

            public sealed class UnmanagedConstantExpectedParameter : ConstantExpectedParameter
            {
                private readonly TransformHelper _helper;
                public UnmanagedConstantExpectedParameter(IParameterSymbol parameter, SyntaxNode attributeSyntax, T min, T max, TransformHelper helper) : base(parameter, attributeSyntax)
                {
                    Min = min;
                    Max = max;
                    _helper = helper;
                }

                public T Min { get; }
                public T Max { get; }

                public override bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
                {
                    if (Parameter.Type.SpecialType != subsetCandidate.Parameter.Type.SpecialType ||
                        subsetCandidate is not UnmanagedConstantExpectedParameter subsetCandidateTParameter)
                    {
                        validationDiagnostics = Diagnostic.Create(AttributeNotSameTypeRule, subsetCandidate.AttributeSyntax.GetLocation(), Parameter.Type.ToDisplayString());
                        return false;
                    }

                    if (!_helper.IsLessThan(subsetCandidateTParameter.Min, Min) && !_helper.IsLessThan(Max, subsetCandidateTParameter.Max))
                    {
                        //within range
                        validationDiagnostics = null;
                        return true;
                    }
                    validationDiagnostics = Diagnostic.Create(AttributeOutOfBoundsRule, subsetCandidateTParameter.AttributeSyntax.GetLocation(), Min.ToString(), Max.ToString());
                    return false;
                }

                public override bool ValidateValue(IArgumentOperation argument, object? constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
                {
                    if (_helper.TryConvert(constant, out T value))
                    {
                        if (!_helper.IsLessThan(value, Min) && !_helper.IsLessThan(Max, value))
                        {
                            validationDiagnostics = null;
                            return true;
                        }
                    }

                    validationDiagnostics = argument.CreateDiagnostic(ConstantOutOfBoundsRule, Min.ToString(), Max.ToString());
                    return false;
                }
            }
        }

        private static bool TryConvertSignedInteger(object constant, out long integer)
        {
            try
            {
                if (constant is string or bool)
                {
                    integer = default;
                    return false;
                }
                integer = Convert.ToInt64(constant);
            }
            catch
            {
                integer = default;
                return false;
            }
            return true;
        }
        private static bool TryConvertUnsignedInteger(object constant, out ulong integer)
        {
            try
            {
                if (constant is string or bool)
                {
                    integer = default;
                    return false;
                }
                integer = Convert.ToUInt64(constant);
            }
            catch
            {
                integer = default;
                return false;
            }
            return true;
        }

        private static bool TryConvertInt64(object? constant, out long value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = Convert.ToInt64(constant);
            return true;
        }

        private static bool TryTransformInt64(object constant, out long value, out bool isInvalid)
        {
            bool isValidSigned = TryConvertSignedInteger(constant, out value);
            isInvalid = false;
            if (!isValidSigned)
            {
                bool isValidUnsigned = TryConvertUnsignedInteger(constant, out _);
                if (!isValidUnsigned)
                {
                    isInvalid = true;
                }
            }

            return isValidSigned;
        }
        private static bool TryConvertUInt64(object? constant, out ulong value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = Convert.ToUInt64(constant);
            return true;
        }
        private static bool TryTransformUInt64(object constant, out ulong value, out bool isInvalid)
        {
            bool isValidUnsigned = TryConvertUnsignedInteger(constant, out value);
            isInvalid = false;
            if (!isValidUnsigned)
            {
                bool isValidSigned = TryConvertSignedInteger(constant, out _);
                if (!isValidSigned)
                {
                    isInvalid = true;
                }
            }
            return isValidUnsigned;
        }

        private static bool TryTransformChar(object constant, out char value, out bool isInvalid)
        {
            try
            {
                if (constant is string or bool)
                {
                    value = default;
                    isInvalid = true;
                    return false;
                }
                value = Convert.ToChar(constant);
            }
            catch
            {
                value = default;
                isInvalid = true;
                return false;
            }
            isInvalid = false;
            return true;
        }
        private static bool TryConvertChar(object? constant, out char value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = Convert.ToChar(constant);
            return true;
        }

        private static bool TryTransformBoolean(object constant, out bool value, out bool isInvalid)
        {
            if (constant is bool b)
            {
                value = b;
                isInvalid = false;
                return true;
            }
            isInvalid = true;
            value = default;
            return false;
        }
        private static bool TryConvertBoolean(object? constant, out bool value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = (bool)constant;
            return true;
        }

        private static bool TryTransformSingle(object constant, out float value, out bool isInvalid)
        {
            try
            {
                if (constant is string or bool)
                {
                    value = default;
                    isInvalid = true;
                    return false;
                }
                value = Convert.ToSingle(constant);
            }
            catch
            {
                value = default;
                isInvalid = true;
                return false;
            }
            isInvalid = false;
            return true;
        }
        private static bool TryConvertSingle(object? constant, out float value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = Convert.ToSingle(constant);
            return true;
        }

        private static bool TryTransformDouble(object constant, out double value, out bool isInvalid)
        {
            try
            {
                if (constant is string or bool)
                {
                    value = default;
                    isInvalid = true;
                    return false;
                }
                value = Convert.ToDouble(constant);
            }
            catch
            {
                value = default;
                isInvalid = true;
                return false;
            }
            isInvalid = false;
            return true;
        }
        private static bool TryConvertDouble(object? constant, out double value)
        {
            if (constant is null)
            {
                value = default;
                return false;
            }
            value = Convert.ToDouble(constant);
            return true;
        }
    }
}

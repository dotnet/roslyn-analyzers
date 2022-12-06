﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract partial class ConstantExpectedAnalyzer
    {
        private sealed class UnmanagedHelper<T> where T : unmanaged
        {
            private static readonly ConstantExpectedParameterFactory? _instance;
            private static ConstantExpectedParameterFactory Instance => _instance ?? throw new InvalidOperationException("unsupported type");

            static UnmanagedHelper()
            {
                if (typeof(T) == typeof(long))
                {
                    var helper = new UnmanagedHelper<long>.TransformHelper(TryTransformInt64);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    var helper = new UnmanagedHelper<ulong>.TransformHelper(TryTransformUInt64);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(float))
                {
                    var helper = new UnmanagedHelper<float>.TransformHelper(TryTransformSingle);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(double))
                {
                    var helper = new UnmanagedHelper<double>.TransformHelper(TryTransformDouble);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(char))
                {
                    var helper = new UnmanagedHelper<char>.TransformHelper(TryTransformChar);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
                else if (typeof(T) == typeof(bool))
                {
                    var helper = new UnmanagedHelper<bool>.TransformHelper(TryTransformBoolean);
                    _instance = new ConstantExpectedParameterFactory((TransformHelper)(object)helper);
                }
            }

            public static bool TryCreate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, [NotNullWhen(true)] out ConstantExpectedParameter? parameter)
                => Instance.TryCreate(parameterSymbol, attributeData, typeMin, typeMax, out parameter);
            public static bool Validate(IParameterSymbol parameterSymbol, AttributeData attributeData, T typeMin, T typeMax, DiagnosticHelper diagnosticHelper, out ImmutableArray<Diagnostic> diagnostics)
                => Instance.Validate(parameterSymbol, attributeData, typeMin, typeMax, diagnosticHelper, out diagnostics);

            public delegate bool TryTransform(object constant, out T value, out bool isInvalid);
            public sealed class TransformHelper
            {
                private readonly TryTransform _tryTransform;

                public TransformHelper(TryTransform tryTransform)
                {
                    _tryTransform = tryTransform;
                }

                public bool IsLessThan(T operand1, T operand2) => Comparer<T>.Default.Compare(operand1, operand2) < 0;
                public bool TryTransformMin(object constant, out T value, ref ErrorKind errorFlags)
                {
                    if (_tryTransform(constant, out value, out bool isInvalid))
                    {
                        return true;
                    }

                    errorFlags |= isInvalid ? ErrorKind.MinIsIncompatible : ErrorKind.MinIsOutOfRange;
                    return false;
                }

                public bool TryTransformMax(object constant, out T value, ref ErrorKind errorFlags)
                {
                    if (_tryTransform(constant, out value, out bool isInvalid))
                    {
                        return true;
                    }

                    errorFlags |= isInvalid ? ErrorKind.MaxIsIncompatible : ErrorKind.MaxIsOutOfRange;
                    return false;
                }
                public bool TryConvert(object val, out T value) => _tryTransform(val, out value, out _);
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

                    parameter = new UnmanagedConstantExpectedParameter(parameterSymbol, minValue, maxValue, _helper);
                    return true;
                }

                private bool IsValidMinMax(AttributeData attributeData, T typeMin, T typeMax, out T minValue, out T maxValue, out ErrorKind errorFlags)
                {
                    minValue = typeMin;
                    maxValue = typeMax;
                    var ac = AttributeConstant.Get(attributeData);
                    errorFlags = ErrorKind.None;
                    if (ac.Min is not null && _helper.TryTransformMin(ac.Min, out minValue, ref errorFlags))
                    {
                        if (_helper.IsLessThan(minValue, typeMin) || _helper.IsLessThan(typeMax, minValue))
                        {
                            errorFlags |= ErrorKind.MinIsOutOfRange;
                        }
                    }

                    if (ac.Max is not null && _helper.TryTransformMax(ac.Max, out maxValue, ref errorFlags))
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
                public UnmanagedConstantExpectedParameter(IParameterSymbol parameter, T min, T max, TransformHelper helper) : base(parameter)
                {
                    Min = min;
                    Max = max;
                    _helper = helper;
                }

                public T Min { get; }
                public T Max { get; }

                public override bool ValidateParameterIsWithinRange(ConstantExpectedParameter subsetCandidate, IArgumentOperation argument, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
                {
                    if (Parameter.Type.SpecialType != subsetCandidate.Parameter.Type.SpecialType ||
                        subsetCandidate is not UnmanagedConstantExpectedParameter subsetCandidateTParameter)
                    {
                        validationDiagnostics = CreateConstantInvalidConstantRuleDiagnostic(argument);
                        return false;
                    }

                    if (!_helper.IsLessThan(subsetCandidateTParameter.Min, Min) && !_helper.IsLessThan(Max, subsetCandidateTParameter.Max))
                    {
                        //within range
                        validationDiagnostics = null;
                        return true;
                    }
                    validationDiagnostics = CreateConstantOutOfBoundsRuleDiagnostic(argument, Min.ToString(), Max.ToString());
                    return false;
                }

                public override bool ValidateValue(IArgumentOperation argument, Optional<object> constant, [NotNullWhen(false)] out Diagnostic? validationDiagnostics)
                {
                    if (!ValidateConstant(argument, constant, out validationDiagnostics))
                    {
                        return false;
                    }

                    if (constant.Value is not null && _helper.TryConvert(constant.Value, out T value))
                    {
                        if (!_helper.IsLessThan(value, Min) && !_helper.IsLessThan(Max, value))
                        {
                            validationDiagnostics = null;
                            return true;
                        }
                        validationDiagnostics = CreateConstantOutOfBoundsRuleDiagnostic(argument, Min.ToString(), Max.ToString());
                        return false;
                    }
                    validationDiagnostics = CreateConstantInvalidConstantRuleDiagnostic(argument);
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

        private static bool TryTransformInt64(object constant, out long value, out bool isInvalid)
        {
            bool isValidSigned = TryConvertSignedInteger(constant, out value);
            isInvalid = false;
            if (isValidSigned)
            {
                return isValidSigned;
            }
            if (!TryConvertUnsignedInteger(constant, out _))
            {
                isInvalid = true;
            }
            return isValidSigned;
        }
        private static bool TryTransformUInt64(object constant, out ulong value, out bool isInvalid)
        {
            bool isValidUnsigned = TryConvertUnsignedInteger(constant, out value);
            isInvalid = false;
            if (isValidUnsigned)
            {
                return isValidUnsigned;
            }
            if (!TryConvertSignedInteger(constant, out _))
            {
                isInvalid = true;
            }
            return isValidUnsigned;
        }

        private static bool TryTransformChar(object constant, out char value, out bool isInvalid)
        {
            try
            {
                if (constant is string or bool)
                {
                    return Invalid(out value, out isInvalid);
                }
                value = Convert.ToChar(constant);
            }
            catch
            {
                return Invalid(out value, out isInvalid);
            }
            isInvalid = false;
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
            return Invalid(out value, out isInvalid);
        }

        private static bool TryTransformSingle(object constant, out float value, out bool isInvalid)
        {
            if (constant is string or bool)
            {
                return Invalid(out value, out isInvalid);
            }
            value = Convert.ToSingle(constant);
            isInvalid = false;
            return true;
        }

        private static bool TryTransformDouble(object constant, out double value, out bool isInvalid)
        {
            if (constant is string or bool)
            {
                return Invalid(out value, out isInvalid);
            }
            value = Convert.ToDouble(constant);
            isInvalid = false;
            return true;
        }

        private static bool Invalid<T>(out T value, out bool isInvalid) where T : unmanaged
        {
            value = default;
            isInvalid = true;
            return false;
        }
    }
}

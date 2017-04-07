﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace Analyzer.Utilities.Extensions
{
    public static class IOperationExtensions
    {
        /// <summary>
        /// Gets the receiver type for an invocation expression (i.e. type of 'A' in invocation 'A.B()')
        /// If the invocation actually involves a conversion from A to some other type, say 'C', on which B is invoked,
        /// then this method returns type A if <paramref name="beforeConversion"/> is true, and C if false.
        /// </summary>
        public static INamedTypeSymbol GetReceiverType(this IInvocationExpression invocation, Compilation compilation, bool beforeConversion, CancellationToken cancellationToken)
        {
            if (invocation.Instance != null)
            {
                return beforeConversion ?
                    GetReceiverType(invocation.Instance.Syntax, compilation, cancellationToken) :
                    invocation.Instance.Type as INamedTypeSymbol;
            }
            else if (invocation.TargetMethod.IsExtensionMethod && invocation.TargetMethod.Parameters.Length > 0)
            {
                var firstArg = invocation.ArgumentsInParameterOrder.FirstOrDefault();
                if (firstArg != null)
                {
                    return beforeConversion ?
                        GetReceiverType(firstArg.Value.Syntax, compilation, cancellationToken) :
                        firstArg.Type as INamedTypeSymbol;
                }
                else if (invocation.TargetMethod.Parameters[0].IsParams)
                {
                    return invocation.TargetMethod.Parameters[0].Type as INamedTypeSymbol;
                }
            }

            return null;
        }

        private static INamedTypeSymbol GetReceiverType(SyntaxNode receiverSyntax, Compilation compilation, CancellationToken cancellationToken)
        {
            var model = compilation.GetSemanticModel(receiverSyntax.SyntaxTree);
            var typeInfo = model.GetTypeInfo(receiverSyntax, cancellationToken);
            return typeInfo.Type as INamedTypeSymbol;
        }

        public static bool HasConstantValue(this IOperation operation, string comparand, StringComparison comparison)
        {
            var constantValue = operation.ConstantValue;
            if (!constantValue.HasValue)
            {
                return false;
            }

            if (operation.Type == null || operation.Type.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            return string.Equals((string)constantValue.Value, comparand, comparison);
        }

        public static bool HasNullConstantValue(this IOperation operation)
        {
            return operation.ConstantValue.HasValue && operation.ConstantValue.Value == null;
        }

        public static bool HasConstantValue(this IOperation operation, long comparand)
        {
            return operation.HasConstantValue(unchecked((ulong)(comparand)));
        }

        public static bool HasConstantValue(this IOperation operation, ulong comparand)
        {
            var constantValue = operation.ConstantValue;
            if (!constantValue.HasValue)
            {
                return false;
            }

            if (operation.Type == null || operation.Type.IsErrorType())
            {
                return false;
            }

            if (operation.Type.IsPrimitiveType())
            {
                return HasConstantValue(constantValue, operation.Type, comparand);
            }

            if (operation.Type.TypeKind == TypeKind.Enum)
            {
                var enumUnderlyingType = ((INamedTypeSymbol)operation.Type).EnumUnderlyingType;
                return enumUnderlyingType != null &&
                    enumUnderlyingType.IsPrimitiveType() &&
                    HasConstantValue(constantValue, enumUnderlyingType, comparand);
            }

            return false;
        }

        private static bool HasConstantValue(Optional<object> constantValue, ITypeSymbol constantValueType, ulong comparand)
        {
            if (constantValueType.SpecialType == SpecialType.System_Double || constantValueType.SpecialType == SpecialType.System_Single)
            {
                return (double)constantValue.Value == comparand;
            }

            return DiagnosticHelpers.TryConvertToUInt64(constantValue.Value, constantValueType.SpecialType, out ulong convertedValue) &&
    convertedValue == comparand;
        }
    }
}

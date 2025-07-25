// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    /// <summary>
    /// CA1516: Use cross-platform intrinsics
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpUseCrossPlatformIntrinsicsAnalyzer : UseCrossPlatformIntrinsicsAnalyzer
    {
        protected override bool IsSupported(IInvocationOperation invocation, RuleKind ruleKind)
        {
            if (invocation.Syntax is not InvocationExpressionSyntax)
            {
                return false;
            }

            // We need to validate that the invocation is the expected syntax kind and
            // that the diagnostic is valid to report for its shape. We should've already
            // validated that the number of arguments and their types match the expected.

            return ruleKind switch
            {
                RuleKind.op_Addition or
                RuleKind.op_BitwiseAnd or
                RuleKind.op_BitwiseOr or
                RuleKind.op_ExclusiveOr or
                RuleKind.op_Multiply or
                RuleKind.op_Subtraction => IsValidBinaryOperatorMethodInvocation(invocation, isCommutative: true),

                RuleKind.op_Division => IsValidBinaryOperatorMethodInvocation(invocation, isCommutative: false),

                RuleKind.op_LeftShift or
                RuleKind.op_RightShift or
                RuleKind.op_UnsignedRightShift => IsValidShiftOperatorMethodInvocation(invocation),

                RuleKind.op_OnesComplement or
                RuleKind.op_UnaryNegation => IsValidUnaryOperatorMethodInvocation(invocation),

                _ => false,
            };

            static bool IsValidBinaryOperatorMethodInvocation(IInvocationOperation invocation, bool isCommutative)
            {
                Debug.Assert(invocation.Arguments.Length == 2);
                Debug.Assert(SymbolEqualityComparer.Default.Equals(invocation.Type, invocation.Arguments[0].Type));
                Debug.Assert(SymbolEqualityComparer.Default.Equals(invocation.Type, invocation.Arguments[1].Type));

                return isCommutative || (invocation.Arguments[0].Parameter?.Ordinal == 0);
            }

            static bool IsValidShiftOperatorMethodInvocation(IInvocationOperation invocation)
            {
                Debug.Assert(invocation.Arguments.Length == 2);
                Debug.Assert(SymbolEqualityComparer.Default.Equals(invocation.Type, invocation.Arguments[0].Type));
                Debug.Assert(invocation.Arguments[1].Type?.SpecialType is SpecialType.System_Byte or SpecialType.System_Int32);

                return invocation.Arguments[0].Parameter?.Ordinal == 0;
            }

            static bool IsValidUnaryOperatorMethodInvocation(IInvocationOperation invocation)
            {
                Debug.Assert(invocation.Arguments.Length == 1);
                Debug.Assert(SymbolEqualityComparer.Default.Equals(invocation.Type, invocation.Arguments[0].Type));

                return true;
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#if HAS_IOPERATION

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.Extensions
{
    internal static class OperationBlockAnalysisContextExtension
    {
#pragma warning disable RS1012 // Start action has no registered actions.
        public static bool IsMethodNotImplementedOrSupported(this OperationBlockStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            var methodBlock = FindMethodBlockOperation(context);

            if (methodBlock != null &&
                IsSingleStatementBody(methodBlock) &&
                methodBlock.Operations[0].GetTopmostExplicitDescendants() is { } descendants &&
                descendants.Length == 1 &&
                descendants[0] is IThrowOperation throwOperation &&
                throwOperation.GetThrownExceptionType() is ITypeSymbol createdExceptionType)
            {
                if (Equals(context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemNotImplementedException), createdExceptionType.OriginalDefinition)
                    || Equals(context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemNotSupportedException), createdExceptionType.OriginalDefinition))
                {
                    return true;
                }
            }

            return false;

            static bool IsSingleStatementBody(IBlockOperation body)
            {
                // Note that VB method bodies with 1 action have 3 operations.
                // The first is the actual operation, the second is a label statement, and the third is a return
                // statement. The last two are implicit in these scenarios.

                return body.Operations.Length == 1 ||
                    (body.Operations.Length == 3 && body.Syntax.Language == LanguageNames.VisualBasic &&
                     body.Operations[1] is ILabeledOperation labeledOp && labeledOp.IsImplicit &&
                     body.Operations[2] is IReturnOperation returnOp && returnOp.IsImplicit);
            }
        }

#pragma warning disable RS1012 // Start action has no registered actions.
        public static bool IsEmptyMethod(this OperationBlockStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            var methodBlock = FindMethodBlockOperation(context);

            return methodBlock != null &&
                (methodBlock.Operations.Length == 0 || IsVisualBasicEmptyMethod(methodBlock));

            static bool IsVisualBasicEmptyMethod(IBlockOperation methodBlock)
                => methodBlock.Operations.Length == 2 &&
                methodBlock.Syntax.Language == LanguageNames.VisualBasic &&
                methodBlock.Operations[0] is ILabeledOperation labeledOp && labeledOp.IsImplicit &&
                methodBlock.Operations[1] is IReturnOperation returnOp && returnOp.IsImplicit;
        }

#pragma warning disable RS1012 // Start action has no registered actions.
        public static IBlockOperation? FindMethodBlockOperation(this OperationBlockStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            var operationBlocks = context.OperationBlocks.WhereAsArray(operation => !operation.IsOperationNoneRoot());

            if (operationBlocks.Length == 1 && operationBlocks[0].Kind == OperationKind.Block)
            {
                return (IBlockOperation)operationBlocks[0];
            }
            else if (operationBlocks.Length > 1)
            {
                foreach (var block in operationBlocks)
                {
                    if (block.Kind == OperationKind.Block)
                    {
                        return (IBlockOperation)block;
                    }
                }
            }

            return null;
        }
    }
}

#endif

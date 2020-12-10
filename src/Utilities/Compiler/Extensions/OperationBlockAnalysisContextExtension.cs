// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#if HAS_IOPERATION

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.Extensions
{
    internal static class OperationBlockAnalysisContextExtension
    {
        public static bool IsMethodNotImplementedOrSupported(this OperationBlockStartAnalysisContext context)
            => BlockContainsOnlyGivenStatements(context, 1,
                operations =>
                {
                    operations = operations[0].GetTopmostExplicitDescendants();

                    if (operations.Length != 1
                        || operations[0] is not IThrowOperation throwOperation
                        || throwOperation.GetThrownExceptionType() is not ITypeSymbol createdExceptionType)
                    {
                        return false;
                    }

                    RoslynDebug.Assert(createdExceptionType != null);

                    return
                        Equals(createdExceptionType.OriginalDefinition,
                            context.Compilation.GetOrCreateTypeByMetadataName(
                                WellKnownTypeNames.SystemNotImplementedException))
                        || Equals(createdExceptionType.OriginalDefinition,
                            context.Compilation.GetOrCreateTypeByMetadataName(
                                WellKnownTypeNames.SystemNotSupportedException));
                });

        public static bool IsEmptyBlock(this OperationBlockStartAnalysisContext context)
            => BlockContainsOnlyGivenStatements(context, 0, operations => true);

#pragma warning disable RS1012 // Start action has no registered actions
        private static bool BlockContainsOnlyGivenStatements(this OperationBlockStartAnalysisContext context,
            int expectedOperationCount, Func<ImmutableArray<IOperation>, bool> areExpectedOperations)
#pragma warning restore RS1012 // Start action has no registered actions
        {
            // Note that VB method bodies with X statements have X + 2 operations.
            // The first X operations are the actual operation, and the last two are
            // a label statement and a return statement. The last two are implicit in these scenarios.

            var operationBlocks = context.OperationBlocks.WhereAsArray(operation => !operation.IsOperationNoneRoot());

            IBlockOperation? methodBlock = null;
            if (operationBlocks.Length == 1 && operationBlocks[0].Kind == OperationKind.Block)
            {
                methodBlock = (IBlockOperation)operationBlocks[0];
            }
            else if (operationBlocks.Length > 1)
            {
                foreach (var block in operationBlocks)
                {
                    if (block.Kind == OperationKind.Block)
                    {
                        methodBlock = (IBlockOperation)block;
                        break;
                    }
                }
            }

            return methodBlock != null
                && HasEnoughOperations(methodBlock, expectedOperationCount)
                && areExpectedOperations(methodBlock.Operations);

            static bool HasEnoughOperations(IBlockOperation body, int expectedOperationCount)
            {
                if (body.Operations.Length == expectedOperationCount)
                {
                    return true;
                }

                return body.Operations.Length == expectedOperationCount + 2
                    && body.Syntax.Language == LanguageNames.VisualBasic
                    && body.Operations[^2] is ILabeledOperation labeledOp
                    && labeledOp.IsImplicit
                    && body.Operations[^1] is IReturnOperation returnOp
                    && returnOp.IsImplicit;
            }
        }
    }
}

#endif

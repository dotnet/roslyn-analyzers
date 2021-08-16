// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#if HAS_IOPERATION

using System;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.Lightup
{
    internal static class IForEachLoopOperationExtensions
    {
        private static readonly Func<IForEachLoopOperation, bool> s_isAsynchronous
            = LightupHelpers.CreateOperationPropertyAccessor<IForEachLoopOperation, bool>(typeof(IForEachLoopOperation), nameof(IsAsynchronous), fallbackResult: false);

        public static bool IsAsynchronous(this IForEachLoopOperation forEachLoopOperation)
            => s_isAsynchronous(forEachLoopOperation);
    }
}

#endif

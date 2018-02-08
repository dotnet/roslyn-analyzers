// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    /// <summary>
    /// Abstract null value for <see cref="AnalysisEntity"/>/<see cref="IOperation"/> tracked by <see cref="NullAnalysis"/>.
    /// </summary>
    internal enum NullAbstractValue
    {
        Undefined,
        Null,
        NotNull,
        MaybeNull
    }
}

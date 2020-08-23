﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis
{
    /// <summary>
    /// Abstract analysis value for <see cref="GlobalFlowStateAnalysis"/>.
    /// </summary>
    internal interface IAbstractAnalysisValue : IEquatable<IAbstractAnalysisValue>
    {
        /// <summary>
        /// Return negated value if the analysis value is a predicated value.
        /// Otherwise, return the current instance itself.
        /// </summary>
        IAbstractAnalysisValue GetNegatedValue();

        /// <summary>
        /// String representation of the abstract value.
        /// </summary>
        string ToString();
    }
}

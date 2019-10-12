// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class Domain : AbstractAnalysisDomain<Data>
        {
            public override Data Clone(Data value)
            {
                return new Data(value);
            }

            public override int Compare(Data oldValue, Data newValue)
            {
                return oldValue.CompareTo(newValue);
            }

            public override bool Equals(Data value1, Data value2)
            {
                return value1.Equals(value2);
            }

            public override Data Merge(Data value1, Data value2)
            {
                return Data.Merge(value1, value2);
            }
        }
    }
}

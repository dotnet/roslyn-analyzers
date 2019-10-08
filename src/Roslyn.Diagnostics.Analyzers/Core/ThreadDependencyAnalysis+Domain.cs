// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class Domain : AbstractAnalysisDomain<Data>
        {
            public override Data Clone(Data value)
            {
                throw new NotImplementedException();
            }

            public override int Compare(Data oldValue, Data newValue)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(Data value1, Data value2)
            {
                throw new NotImplementedException();
            }

            public override Data Merge(Data value1, Data value2)
            {
                throw new NotImplementedException();
            }
        }
    }
}

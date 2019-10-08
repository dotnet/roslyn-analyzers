// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class ValueDomain : AbstractValueDomain<Value>
        {
            internal static readonly ValueDomain Default = new ValueDomain();

            private ValueDomain()
            {
            }

            public override Value UnknownOrMayBeValue { get => throw new NotImplementedException(); }
            public override Value Bottom { get => throw new NotImplementedException(); }

            public override int Compare(Value oldValue, Value newValue, bool assertMonotonicity)
            {
                throw new NotImplementedException();
            }

            public override Value Merge(Value value1, Value value2)
            {
                throw new NotImplementedException();
            }
        }
    }
}

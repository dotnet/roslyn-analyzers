// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class ValueDomain : AbstractValueDomain<Value>
        {
            internal static readonly ValueDomain Default = new ValueDomain();

            private ValueDomain()
            {
            }

            public override Value UnknownOrMayBeValue => new Value(YieldKind.Unknown, alwaysComplete: null);
            public override Value Bottom => new Value(YieldKind.NotYielded, alwaysComplete: true);

            public override int Compare(Value oldValue, Value newValue, bool assertMonotonicity)
            {
                throw new NotImplementedException();
            }

            public override Value Merge(Value value1, Value value2)
            {
                var yieldKind = Merge(value1.YieldKind, value2.YieldKind);
                var alwaysComplete = value1.AlwaysComplete & value2.AlwaysComplete;
                return new Value(yieldKind, alwaysComplete);
            }

            internal static YieldKind Merge(YieldKind value1, YieldKind value2)
            {
                if (value1 == YieldKind.Unknown)
                {
                    return value2;
                }
                else if (value2 == YieldKind.Unknown)
                {
                    return value1;
                }
                else if (value1 == value2)
                {
                    return value1;
                }
                else
                {
                    return YieldKind.MaybeYielded;
                }
            }
        }
    }
}

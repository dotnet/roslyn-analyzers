// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class Data : AbstractAnalysisData, IComparable<Data>, IEquatable<Data>
        {
            internal Data(YieldKind yieldKind)
            {
                YieldKind = yieldKind;
            }

            internal Data(Data value)
            {
                YieldKind = value.YieldKind;
            }

            public YieldKind YieldKind { get; set; }

            public static Data Merge(Data value1, Data value2)
            {
                return new Data(ValueDomain.Merge(value1.YieldKind, value2.YieldKind));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Data);
            }

            public override int GetHashCode()
            {
                return (int)YieldKind;
            }

            public int CompareTo(Data other)
            {
                return YieldKind.CompareTo(other.YieldKind);
            }

            public bool Equals(Data other)
            {
                return other is object
                    && YieldKind == other.YieldKind;
            }
        }
    }
}

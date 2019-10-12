// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    internal partial class ThreadDependencyAnalysis
    {
        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        internal class Value
        {
            public Value(YieldKind yieldKind, bool? alwaysComplete)
            {
                YieldKind = yieldKind;
                AlwaysComplete = alwaysComplete;
            }

            public YieldKind YieldKind { get; }

            public bool? AlwaysComplete { get; }

            private string GetDebuggerDisplay()
            {
                return $"{nameof(YieldKind)}={YieldKind}, {nameof(AlwaysComplete)}={AlwaysComplete}";
            }
        }

        internal enum YieldKind
        {
            Unknown,

            NotYielded,

            Yielded,

            MaybeYielded,
        }
    }
}

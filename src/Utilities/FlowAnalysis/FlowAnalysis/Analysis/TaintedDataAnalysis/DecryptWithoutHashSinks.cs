// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    internal static class DecryptWithoutHashSinks
    {
        /// <summary>
        /// <see cref="SinkInfo"/>s for decrypt without hash sinks.
        /// </summary>
        public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

        static DecryptWithoutHashSinks()
        {
            PooledHashSet<SinkInfo> builder = PooledHashSet<SinkInfo>.GetInstance();

            builder.AddSinkInfo(
                WellKnownTypeNames.SystemSecurityCryptographyCryptoStream,
                SinkKind.DecryptWithoutHash,
                isInterface: false,
                isAnyStringParameterInConstructorASink: false,
                sinkProperties: null,
                sinkMethodParameters: null,
                sinkMethodParametersWithTaintedInstance: new[] {
                    ( "Write", new[] { "buffer" } ),
                });

            SinkInfos = builder.ToImmutableAndFree();
        }
    }
}

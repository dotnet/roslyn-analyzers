// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    internal static class DecryptWithoutHashSanitizers
    {
        /// <summary>
        /// <see cref="SanitizerInfo"/>s for decrypt without hash sanitizers.
        /// </summary>
        public static ImmutableHashSet<SanitizerInfo> SanitizerInfos { get; }

        /// <summary>
        /// Statically constructs.
        /// </summary>
        static DecryptWithoutHashSanitizers()
        {
            var builder = PooledHashSet<SanitizerInfo>.GetInstance();

            builder.AddSanitizerInfo(
                WellKnownTypeNames.SystemSecurityCryptographyHashAlgorithm,
                isInterface: false,
                isConstructorSanitizing: false,
                sanitizingMethods: null,
                sanitizingMethodsSpecifyTargets: new[] {
                    ("ComputeHash", (false, false, new[] { "buffer" })),
                });

            SanitizerInfos = builder.ToImmutableAndFree();
        }
    }
}

﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.InvocationCountAnalysis
{
    internal class InvocationCountAnalysisValue : CacheBasedEquatable<InvocationCountAnalysisValue>
    {
        public ImmutableDictionary<AnalysisEntity, TrackingInvocationSet> TrackedEntities { get; }

        public InvocationCountAnalysisValueKind Kind { get; }

        public static readonly InvocationCountAnalysisValue Empty = new(ImmutableDictionary<AnalysisEntity, TrackingInvocationSet>.Empty, InvocationCountAnalysisValueKind.Empty);
        public static readonly InvocationCountAnalysisValue Unknown = new(ImmutableDictionary<AnalysisEntity, TrackingInvocationSet>.Empty, InvocationCountAnalysisValueKind.Unknown);

        public InvocationCountAnalysisValue(
            ImmutableDictionary<AnalysisEntity, TrackingInvocationSet> trackedEntities,
            InvocationCountAnalysisValueKind kind)
        {
            TrackedEntities = trackedEntities;
            Kind = kind;
        }

        public static InvocationCountAnalysisValue Merge(InvocationCountAnalysisValue value1, InvocationCountAnalysisValue value2)
        {
            RoslynDebug.Assert(value1.Kind == InvocationCountAnalysisValueKind.Known && value2.Kind == InvocationCountAnalysisValueKind.Known);

            if (value1.TrackedEntities.Count == 0)
            {
                return value2;
            }

            if (value2.TrackedEntities.Count == 0)
            {
                return value1;
            }

            using var builder = PooledObjects.PooledDictionary<AnalysisEntity, TrackingInvocationSet>.GetInstance();
            foreach (var kvp in value2.TrackedEntities)
            {
                builder[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in value1.TrackedEntities)
            {
                var key = kvp.Key;
                var trackedEntities1 = kvp.Value;
                if (value2.TrackedEntities.TryGetValue(key, out var trackedEntities2))
                {
                    builder[key] = InvocationSetHelpers.Merge(trackedEntities1, trackedEntities2);
                }
                else
                {
                    builder[key] = kvp.Value;
                }
            }

            return new InvocationCountAnalysisValue(builder.ToImmutableDictionaryAndFree(), InvocationCountAnalysisValueKind.Known);
        }

        public static InvocationCountAnalysisValue Intersect(InvocationCountAnalysisValue value1, InvocationCountAnalysisValue value2)
        {
            using var builder = PooledObjects.PooledDictionary<AnalysisEntity, TrackingInvocationSet>.GetInstance();
            using var intersectedKeys = PooledObjects.PooledHashSet<AnalysisEntity>.GetInstance();

            foreach (var kvp in value1.TrackedEntities)
            {
                var key = kvp.Key;
                var invocationSet = kvp.Value;
                if (value2.TrackedEntities.TryGetValue(key, out var trackedEntities2))
                {
                    builder[key] = InvocationSetHelpers.Intersect(invocationSet, trackedEntities2);
                    intersectedKeys.Add(key);
                }
                else
                {
                    builder[key] = InvocationSetHelpers.Intersect(invocationSet, TrackingInvocationSet.Empty);
                }
            }

            foreach (var kvp in value2.TrackedEntities)
            {
                var key = kvp.Key;
                var invocationSet = kvp.Value;

                if (!intersectedKeys.Contains(kvp.Key))
                {
                    builder[key] = InvocationSetHelpers.Intersect(invocationSet, TrackingInvocationSet.Empty);
                }
            }

            return new InvocationCountAnalysisValue(builder.ToImmutableDictionaryAndFree(), InvocationCountAnalysisValueKind.Known);
        }

        protected override bool ComputeEqualsByHashCodeParts(CacheBasedEquatable<InvocationCountAnalysisValue> obj)
        {
            var other = (InvocationCountAnalysisValue)obj;
            return HashUtilities.Combine(TrackedEntities) == HashUtilities.Combine(other.TrackedEntities)
                && Kind.GetHashCode() == other.Kind.GetHashCode();
        }

        protected override void ComputeHashCodeParts(ref RoslynHashCode hashCode)
        {
            hashCode.Add(HashUtilities.Combine(TrackedEntities));
            hashCode.Add(Kind.GetHashCode());
        }
    }
}

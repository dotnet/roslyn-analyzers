// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    internal class SetAbstractDomain<T> : AbstractDomain<ImmutableHashSet<T>>
    {
        public static SetAbstractDomain<T> Default = new SetAbstractDomain<T>();

        public override ImmutableHashSet<T> Bottom => ImmutableHashSet<T>.Empty;

        public override int Compare(ImmutableHashSet<T> oldValue, ImmutableHashSet<T> newValue)
        {
            if (oldValue == null)
            {
                return newValue == null ? 0 : -1;
            }
            else if (newValue == null)
            {
                return 1;
            }

            if (ReferenceEquals(oldValue, newValue))
            {
                return 0;
            }

            int result;
            var isSubset = oldValue.IsSubsetOf(newValue);

            if (isSubset &&
                oldValue.Count == newValue.Count)
            {
                // oldValue == newValue
                result = 0;
            }
            else if (isSubset)
            {
                // oldValue < newValue
                result = -1;
            }
            else
            {
                // oldValue > newValue
                result = 1;
            }

            return result;
        }

        public override ImmutableHashSet<T> Merge(ImmutableHashSet<T> value1, ImmutableHashSet<T> value2)
        {
            if (value1 == null)
            {
                return value2;
            }
            else if (value2 == null)
            {
                return value1;
            }
            else if (value1.IsEmpty)
            {
                return value2;
            }
            else if (value2.IsEmpty || ReferenceEquals(value1, value2))
            {
                return value1;
            }

            var builder = ImmutableHashSet.CreateBuilder<T>();
            builder.UnionWith(value1);
            builder.UnionWith(value2);
            return builder.ToImmutable();
        }
    }
}